using BestAuth.Application.Abstracts;
using BestAuth.Domain.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BestAuth.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController(IUserService userService) : Controller
    {
        private readonly IUserService _userService = userService;

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(CancellationToken ct)
        {
            var users = await _userService.GetUsersWithRolesAsync(ct);
            return Ok(users);
        }

        [HttpPost("users/{userId:guid}/roles")]
        public async Task<IActionResult> AssignRole(Guid userId, UpdateUserRoleRequest request)
        {
            await _userService.AssignRoleAsync(userId, request.RoleName);
            return NoContent();
        }

        [HttpDelete("users/{userId:guid}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(Guid userId, string roleName)
        {
            await _userService.RemoveRoleAsync(userId, roleName);
            return NoContent();
        }
    }
}
