using BestAuth.Application.Abstracts;
using BestAuth.Application.Constants;
using BestAuth.Domain.Entities;
using BestAuth.Domain.Exceptions;
using BestAuth.Domain.Requests;
using Microsoft.AspNetCore.Identity;

namespace BestAuth.Application.Services
{
    /// <summary>
    /// Authorization service
    /// </summary>
    public class AccountService(IAuthTokenProcessor tokenProcessor, UserManager<User> userManager, IUserRepository userRepository)
        : IAccountService
    {
        private readonly IAuthTokenProcessor _tokenProcessor = tokenProcessor;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IUserRepository _userRepository = userRepository;

        public async Task RegisterAsync(RegisterRequest request)
        {
            var userExists = await _userManager.FindByNameAsync(request.Login) != null;

            if (userExists)
            {
                throw new UserExistsException(request.Login);
            }

            var user = User.Create(request.Email ?? "", request.Login, request.Login);
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password);

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new RegistrationFailedException(result.Errors.Select(e => e.Description));
            }

            // Назначаем роль по умолчанию
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                throw new RegistrationFailedException(roleResult.Errors.Select(e => e.Description));
            }
        }

        public async Task LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Login);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new LoginFailedException(request.Login);
            }

            await CreateAuthSessionAsync(user);
        }

        public async Task RefreshToken(string? refreshToken, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new RefreshTokenException("Refresh token is missing");
            }

            var user = await _userRepository.GetByRefreshToken(refreshToken, ct)
                ?? throw new RefreshTokenException("Cant get user by refresh token");

            if (user.ExpiresAtUtc < DateTime.UtcNow)
            {
                throw new RefreshTokenException("Refresh expired");
            }

            await CreateAuthSessionAsync(user);
        }

        private async Task CreateAuthSessionAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var access = _tokenProcessor.GenerateAccessToken(user, roles);
            var refresh = _tokenProcessor.GenerateRefreshToken();

            user.RefreshToken = refresh.token;
            user.ExpiresAtUtc = refresh.expiresUtc;

            await _userManager.UpdateAsync(user);

            _tokenProcessor.WriteHttpOnlyCookie(CookieNames.Access, access.token, access.expiresUtc);
            _tokenProcessor.WriteHttpOnlyCookie(CookieNames.Refresh, refresh.token, refresh.expiresUtc);
        }
    }
}
