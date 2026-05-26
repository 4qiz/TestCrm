using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BestAuth.Api.Hubs
{
    // /hubs/requests
    [Authorize]
    public class RequestsHub : Hub
    {
    }
}
