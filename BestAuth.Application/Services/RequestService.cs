using BestAuth.Application.Abstracts;
using BestAuth.Domain.Entities;
using BestAuth.Domain.Exceptions;
using BestAuth.Domain.Requests;

namespace BestAuth.Application.Services
{
    public class RequestService(IRequestRepository requestRepository) : IRequestService
    {
        private readonly IRequestRepository _requestRepository = requestRepository;

        public async Task<IEnumerable<RequestDto>> GetRequestsAsync(string? clientName, RequestStatus? status, CancellationToken ct)
        {
            var requests = await _requestRepository.GetListAsync(clientName, status, ct);
            return requests.Select(ToDto);
        }

        public async Task<RequestDto> GetRequestByIdAsync(Guid id, CancellationToken ct)
        {
            var request = await _requestRepository.GetByIdAsync(id, ct)
                ?? throw new RequestNotFoundException(id);

            return ToDto(request);
        }

        public async Task<RequestDto> CreateRequestAsync(CreateRequestRequest request, CancellationToken ct)
        {
            var entity = Request.Create(request.ClientName, request.Phone, request.Comment);
            var created = await _requestRepository.CreateAsync(entity, ct);
            return ToDto(created);
        }

        public async Task<RequestDto> UpdateStatusAsync(Guid id, RequestStatus status, CancellationToken ct)
        {
            var existing = await _requestRepository.GetByIdAsync(id, ct)
                ?? throw new RequestNotFoundException(id);

            await _requestRepository.UpdateStatusAsync(existing, status, ct);
            return ToDto(existing);
        }

        public async Task DeleteRequestAsync(Guid id, CancellationToken ct)
        {
            var existing = await _requestRepository.GetByIdAsync(id, ct)
                ?? throw new RequestNotFoundException(id);

            await _requestRepository.DeleteAsync(existing, ct);
        }

        private static RequestDto ToDto(Request request)
        {
            return new RequestDto
            {
                Id = request.Id,
                ClientName = request.ClientName,
                Phone = request.Phone,
                Status = request.Status,
                Comment = request.Comment,
                CreatedAtUtc = request.CreatedAtUtc
            };
        }
    }
}
