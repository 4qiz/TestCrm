namespace BestAuth.Domain.Exceptions
{
    public class UserExistsException(string userName) : Exception($"User {userName} exists");
    public class RegistrationFailedException(IEnumerable<string> errors)
        : Exception($"Registration failed with: {string.Join(Environment.NewLine, errors)}");
    public class LoginFailedException(string userName) : Exception($"Invalid username {userName} or password");
    public class RefreshTokenException(string message) : Exception(message);
    public class UserNotFoundException(Guid userId) : Exception($"User {userId} not found");
    public class RequestNotFoundException(Guid requestId) : Exception($"Request {requestId} not found");
    public class RoleNotFoundException(string roleName) : Exception($"Role {roleName} not found");
    public class RoleAssignmentFailedException(IEnumerable<string> errors)
        : Exception($"Role assignment failed with: {string.Join(Environment.NewLine, errors)}");
}
