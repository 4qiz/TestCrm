

namespace BestAuth.Domain.Requests
{
    public record UpdateUserRoleRequest
    {
        public required string RoleName { get; init; }
    }

    public record UserRolesResponse(Guid Id, string UserName, string Email, IReadOnlyCollection<string> Roles);
}
