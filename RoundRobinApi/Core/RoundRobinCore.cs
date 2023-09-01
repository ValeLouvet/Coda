using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoundRobinApi.Domain;
using RoundRobinApi.Interface;
using System.Collections.Concurrent;

namespace RoundRobinApi.Core;

public class RoundRobinCore : IRoundRobinCore
{
    private static ConcurrentQueue<string> _addressesQueue = new ConcurrentQueue<string>();
    private static ConcurrentDictionary<string, int> _addressesErrorCount = new ConcurrentDictionary<string, int>();
    private static ConcurrentDictionary<string, int> _addressesExecutionCount = new ConcurrentDictionary<string, int>();
    private readonly HttpClient _httpClient;
    private readonly string[] _addresses;
    private const int _errorCountThreshold = 50;
    private const int _preQueueCount = 100;
    private const int _retryCount = 5;
    private const int _queueThreshold = 50;

    public RoundRobinCore(AddressesConfiguration config, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(config.Addresses);
        _httpClient = httpClient;
        _addresses = config.Addresses;
        if(_addressesErrorCount.Count == 0)
        {
            for (int i = 0; i < _addresses.Length; i++)
            {
                _addressesErrorCount.TryAdd(_addresses[i], 0);
            }
        }

        if (_addressesExecutionCount.Count == 0)
        {
            for (int i = 0; i < _addresses.Length; i++)
            {
                _addressesExecutionCount.TryAdd(_addresses[i], 0);
            }
        }

        if (_addressesQueue.Count < _queueThreshold)
        {
            AddAddressesToQueue(_addresses);
        }
    }

    public IDictionary<string, int> GetStats()
        => _addressesExecutionCount;

    public string[] GetAddresses(bool active)
    {
        return active
            ? _addressesErrorCount.Where(x => x.Value < _errorCountThreshold).Select(x => x.Key).ToArray()
            : _addressesErrorCount.Where(x => x.Value >= _errorCountThreshold).Select(x => x.Key).ToArray();
    }

    public async Task<JObject> SendWithRetryAsync(JObject request)
    {
        string currentAddress;
        while (!_addressesQueue.TryDequeue(out currentAddress!))
        {
            AddAddressesToQueue(_addresses, 1);
        };

        var iteration = 0;
        _addressesExecutionCount.AddOrUpdate(currentAddress, 0, (key, currentValue) => currentValue + 1);
        var result = await SendAsync(request, currentAddress);
        while(iteration < _retryCount && !result.IsOk)
        {
            ++iteration;
            _addressesExecutionCount.AddOrUpdate(currentAddress, 0, (key, currentValue) => currentValue + 1);
            _addressesErrorCount.AddOrUpdate(currentAddress, 0, (key, currentValue) => currentValue + 1);
            result = await SendAsync(request, currentAddress);
        }

        if (result.IsOk)
        {
            return result.Value;
        }

        return await SendWithRetryAsync(request);
    }

    private void AddAddressesToQueue(string[] addresses, int preQueueCount = _preQueueCount)
    {
        for(var j = 0; j < preQueueCount; j++)
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
        try
        {
            var response = await _httpClient.PostAsync(currentAddress, JsonContent.Create(request));
            if (!response.IsSuccessStatusCode) return Result<JObject>.Fail();
            var result = await response.Content.ReadAsStringAsync();
            if (result == null) return Result<JObject>.Fail();
            return Result<JObject>.Success(JsonConvert.DeserializeObject<JObject>(result)!);
        }
        catch
        {
            return Result<JObject>.Fail();
        }
    }
}
