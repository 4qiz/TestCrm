using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BestAuth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Authorize");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetForAdmin()
        {
            return Ok("Admin");
        }
    }
}
