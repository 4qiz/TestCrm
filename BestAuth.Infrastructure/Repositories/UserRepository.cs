using BestAuth.Application.Abstracts;
using BestAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BestAuth.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<User?> GetByRefreshToken(string refreshToken, CancellationToken ct)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
        }
    }
}
