using Newtonsoft.Json.Linq;

namespace RoundRobinApi.Interface;

public interface IRoundRobinCore
{
    Task<JObject> SendWithRetryAsync(JObject request);
    string[] GetAddresses(bool active);
    IDictionary<string, int> GetStats();
    IDictionary<string, int> GetErrors();
    void ReactivateAll();
}
