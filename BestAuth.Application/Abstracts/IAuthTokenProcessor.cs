using System;
using System.Collections.Generic;
using BestAuth.Domain.Entities;

namespace BestAuth.Application.Abstracts
{
    public interface IAuthTokenProcessor
    {
        (string token, DateTime expiresUtc) GenerateAccessToken(User user, IEnumerable<string> roles);

        (string token, DateTime expiresUtc) GenerateRefreshToken();

        void WriteHttpOnlyCookie(string cookieName, string value, DateTime expireUtc);
    }
}
