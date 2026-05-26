using BestAuth.Domain.Requests;

namespace BestAuth.Application.Abstracts
{
    public interface IAccountService
    {
        Task RegisterAsync(RegisterRequest request);

        Task LoginAsync(LoginRequest request);

        Task RefreshToken(string? refreshToken, CancellationToken ct);
    }
}
