using BestAuth.Application.Abstracts;
using BestAuth.Application.Constants;
using BestAuth.Domain.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BestAuth.Api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController(IAccountService accountService) : Controller
    {
        private readonly IAccountService _accountService = accountService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            await _accountService.RegisterAsync(request);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            await _accountService.LoginAsync(request);
            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            var token = HttpContext.Request.Cookies[CookieNames.Refresh];
            await _accountService.RefreshToken(token, ct);
            return Ok();
        }
    }
}
