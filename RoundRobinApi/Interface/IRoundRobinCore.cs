using Newtonsoft.Json.Linq;

namespace RoundRobinApi.Interface;

public interface IRoundRobinCore
{
    Task<JObject> SendWithRetryAsync(JObject request);
}
