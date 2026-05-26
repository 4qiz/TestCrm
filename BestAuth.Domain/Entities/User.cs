using Microsoft.AspNetCore.Identity;

namespace BestAuth.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public required string Name { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }

        public static User Create(string email, string userName, string name)
        {
            return new User { Email = email, UserName = userName, Name = name };
        }
    }

    public class Role : IdentityRole<Guid>
    {

    }
}
