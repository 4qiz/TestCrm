using BestAuth.Domain.Entities;

namespace BestAuth.Domain.Requests
{
    public record CreateRequestRequest
    {
        public required string ClientName { get; init; }
        public required string Phone { get; init; }
        public string? Comment { get; init; }
    }

    public record UpdateRequestStatusRequest
    {
        public required RequestStatus Status { get; init; }
    }

    public record RequestDto
    {
        public required Guid Id { get; init; }
        public required string ClientName { get; init; }
        public required string Phone { get; init; }
        public required RequestStatus Status { get; init; }
        public string? Comment { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
    }
}
