using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BestAuth.Api.Hubs
{
    [Authorize]
    public class RequestsHub : Hub
    {
    }
}
