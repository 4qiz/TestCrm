using BestAuth.Application.Abstracts;
using BestAuth.Domain.Entities;
using BestAuth.Domain.Exceptions;
using BestAuth.Domain.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace BestAuth.Application.Services
{
    /// <summary>
    /// User management service
    /// </summary>
    public class UserService(UserManager<User> userManager, RoleManager<Role> roleManager) : IUserService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<Role> _roleManager = roleManager;

        public async Task<IReadOnlyCollection<UserRolesResponse>> GetUsersWithRolesAsync(CancellationToken ct)
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync(ct);
            var result = new List<UserRolesResponse>(users.Count);

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserRolesResponse(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, [.. roles]));
            }

            return result;
        }

        public async Task AssignRoleAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException(userId);

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                throw new RoleNotFoundException(roleName);
            }

            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                return;
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                throw new RoleAssignmentFailedException(result.Errors.Select(x => x.Description));
            }
        }

        public async Task RemoveRoleAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException(userId);

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                throw new RoleNotFoundException(roleName);
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                return;
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                throw new RoleAssignmentFailedException(result.Errors.Select(x => x.Description));
            }
        }
    }
}
