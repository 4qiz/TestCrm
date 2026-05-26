using BestAuth.Domain.Entities;

namespace BestAuth.Application.Abstracts
{
    public interface IRequestRepository
    {
        Task<Request?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<Request>> GetListAsync(string? clientName, RequestStatus? status, CancellationToken ct);
        Task<Request> CreateAsync(Request request, CancellationToken ct);
        Task UpdateStatusAsync(Request request, RequestStatus status, CancellationToken ct);
        Task DeleteAsync(Request request, CancellationToken ct);
    }
}
