namespace BestAuth.Infrastructure.Options
{
    public class JwtOptions
    {
        public const string JwtOptionsKey = "Jwt";

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public int AccessExpireMinutes { get; set; }
        public int RefreshExpireDays { get; set; }

    }
}
