using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ApplicationApi.Controllers;

[ApiController]
[Route("")]
public class IndexController : ControllerBase
{
    [HttpPost]
    public JObject Post([FromBody] JObject body) 
        => body;
}