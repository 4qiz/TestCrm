
using BestAuth.Domain.Requests;

namespace BestAuth.Application.Abstracts
{
    public interface IUserService
    {
        Task<IReadOnlyCollection<UserRolesResponse>> GetUsersWithRolesAsync(CancellationToken ct);

        Task AssignRoleAsync(Guid userId, string roleName);

        Task RemoveRoleAsync(Guid userId, string roleName);
    }
}
