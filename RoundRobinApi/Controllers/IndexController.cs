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
}
