using BestAuth.Application.Abstracts;
using BestAuth.Api.Hubs;
using BestAuth.Domain.Entities;
using BestAuth.Domain.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BestAuth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestsController(IRequestService requestService, IHubContext<RequestsHub> hubContext) : Controller
    {
        private readonly IRequestService _requestService = requestService;
        private readonly IHubContext<RequestsHub> _hubContext = hubContext;

        [HttpGet]
        public async Task<IActionResult> GetRequests([FromQuery] string? clientName, [FromQuery] RequestStatus? status, CancellationToken ct)
        {
            var requests = await _requestService.GetRequestsAsync(clientName, status, ct);
            return Ok(requests);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRequest(Guid id, CancellationToken ct)
        {
            var request = await _requestService.GetRequestByIdAsync(id, ct);
            return Ok(request);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRequestRequest request, CancellationToken ct)
        {
            var created = await _requestService.CreateRequestAsync(request, ct);
            await _hubContext.Clients.All.SendAsync("RequestCreated", created, ct);
            return CreatedAtAction(nameof(GetRequest), new { id = created.Id }, created);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateRequestStatusRequest request, CancellationToken ct)
        {
            var updated = await _requestService.UpdateStatusAsync(id, request.Status, ct);
            await _hubContext.Clients.All.SendAsync("RequestUpdated", updated, ct);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _requestService.DeleteRequestAsync(id, ct);
            await _hubContext.Clients.All.SendAsync("RequestDeleted", id, ct);
            return NoContent();
        }
    }
}
