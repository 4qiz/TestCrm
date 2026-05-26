namespace BestAuth.Domain.Requests
{
    public record LoginRequest
    {
        public required string Login { get; init; }
        public required string Password { get; init; }
    }

    public record RegisterRequest
    {
        public required string Login { get; init; }
        public string? Email { get; init; }
        public required string Password { get; init; }
    }
}
