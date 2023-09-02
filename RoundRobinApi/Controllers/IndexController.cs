using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RoundRobinApi.Interface;

namespace RoundRobinApi.Controllers;

[ApiController]
[Route("")]
public class IndexController : ControllerBase
{
    private readonly IRoundRobinCore _roundRobinCore;

    public IndexController(IRoundRobinCore roundRobinCore)
    {
        _roundRobinCore = roundRobinCore;
    }

    [HttpPost]
    public Task<JObject> Post([FromBody] JObject body) 
        => _roundRobinCore.SendWithRetryAsync(body);

    [HttpGet("reactivate")]
    public void Reactivate()
        => _roundRobinCore.ReactivateAll();

    [HttpGet("errors")]
    public IDictionary<string, int> GetErrors()
        => _roundRobinCore.GetErrors();

    [HttpGet("stats")]
    public IDictionary<string, int> GetStats()
        => _roundRobinCore.GetStats();

    [HttpGet("active")]
    public string[] GetActive()
        => _roundRobinCore.GetAddresses(true);

    [HttpGet("inactive")]
    public string[] GetInactive()
        => _roundRobinCore.GetAddresses(false);
}
