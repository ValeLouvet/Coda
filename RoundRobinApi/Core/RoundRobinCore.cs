using Newtonsoft.Json.Linq;
using RoundRobinApi.Domain;
using RoundRobinApi.Interface;
using System.Collections.Concurrent;

namespace RoundRobinApi.Core;

public class RoundRobinCore : IRoundRobinCore
{
    private readonly ConcurrentQueue<string> _addressesQueue;
    private readonly ConcurrentDictionary<string, int> _addressesErrorCount;
    private readonly HttpClient _httpClient;
    private readonly string[] _addresses;
    private const int _errorCountThreshold = 50;
    private const int _preQueueCount = 10;
    private const int _retryCount = 5;

    public RoundRobinCore(AddressesConfiguration config, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(config.Addresses);
        _httpClient = httpClient;
        _addresses = config.Addresses;
        if(_addressesErrorCount == null)
        {
            _addressesErrorCount = new ConcurrentDictionary<string, int>();
            for (int i = 0; i < _addresses.Length; i++)
            {
                _addressesErrorCount.TryAdd(_addresses[i], 0);
            }
        }

        if (_addressesQueue == null)
        {
            _addressesQueue = new ConcurrentQueue<string>();
        }

        if (_addressesQueue.IsEmpty)
        {
            AddAddressesToQueue(_addresses);
        }
    }

    public async Task<JObject> SendWithRetryAsync(JObject request)
    {
        string? currentAddress;
        while (_addressesQueue.TryDequeue(out currentAddress))
        {
            AddAddressesToQueue(_addresses);
        };

        var iteration = 0;
        var result = await SendAsync(request, currentAddress);
        while(iteration < _retryCount && !result.IsOk)
        {
            ++iteration;
            _addressesErrorCount.AddOrUpdate(currentAddress, 0, (key, currentValue) => currentValue + 1);
            result = await SendAsync(request, currentAddress);
        }

        if (result.IsOk)
        {
            return result.Value;
        }

        return await SendWithRetryAsync(request);
    }

    private void AddAddressesToQueue(string[] addresses)
    {
        for(var j = 0; j < _preQueueCount; j++)
        {
            for (int i = 0; i < addresses.Length; i++)
            {
                if(_addressesErrorCount.TryGetValue(addresses[i], out var errorCount) && errorCount < _errorCountThreshold)
                {
                    _addressesQueue.Enqueue(addresses[i]);
                }
            }
        }
    }

    private async Task<Result<JObject>> SendAsync(JObject request, string currentAddress)
    {
        var response = await _httpClient.PostAsync(currentAddress, JsonContent.Create(request));
        if (!response.IsSuccessStatusCode) return Result<JObject>.Fail();
        var result = await response.Content.ReadFromJsonAsync<JObject>();
        if (result == null) return Result<JObject>.Fail();
        return Result<JObject>.Success(result);
    }
}
