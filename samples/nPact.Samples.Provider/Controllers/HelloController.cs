using Microsoft.AspNetCore.Mvc;

namespace nPact.Samples.Provider.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        [HttpGet("{name:alpha}")]
        public IActionResult SayHello([FromRoute] string name)
        {
            return Ok(new
            {
                message = $"Hello, {name}"
            });
        }
    }
}
