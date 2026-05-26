using BestAuth.Application.Abstracts;
using BestAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BestAuth.Infrastructure.Repositories
{
    public class RequestRepository(AppDbContext context) : IRequestRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Request?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.Requests.FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<IEnumerable<Request>> GetListAsync(string? clientName, RequestStatus? status, CancellationToken ct)
        {
            var query = _context.Requests.AsQueryable();

            if (!string.IsNullOrWhiteSpace(clientName))
            {
                var normalizedName = clientName.Trim().ToLower();
                query = query.Where(r => r.ClientName.ToLower().Contains(normalizedName));
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync(ct);
        }

        public async Task<Request> CreateAsync(Request request, CancellationToken ct)
        {
            _context.Requests.Add(request);
            await _context.SaveChangesAsync(ct);
            return request;
        }

        public async Task UpdateStatusAsync(Request request, RequestStatus status, CancellationToken ct)
        {
            request.Status = status;
            _context.Requests.Update(request);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Request request, CancellationToken ct)
        {
            _context.Requests.Remove(request);
            await _context.SaveChangesAsync(ct);
        }
    }
}
