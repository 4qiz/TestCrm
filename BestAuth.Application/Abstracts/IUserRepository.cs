using BestAuth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BestAuth.Application.Abstracts
{
    public interface IUserRepository
    {
        Task<User?> GetByRefreshToken(string refreshToken, CancellationToken ct);
    }
}
