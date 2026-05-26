using BestAuth.Domain.Entities;
using BestAuth.Domain.Requests;

namespace BestAuth.Application.Abstracts
{
    public interface IRequestService
    {
        Task<IEnumerable<RequestDto>> GetRequestsAsync(string? clientName, RequestStatus? status, CancellationToken ct);
        Task<RequestDto> GetRequestByIdAsync(Guid id, CancellationToken ct);
        Task<RequestDto> CreateRequestAsync(CreateRequestRequest request, CancellationToken ct);
        Task<RequestDto> UpdateStatusAsync(Guid id, RequestStatus status, CancellationToken ct);
        Task DeleteRequestAsync(Guid id, CancellationToken ct);
    }
}
