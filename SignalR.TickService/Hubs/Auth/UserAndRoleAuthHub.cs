
using Microsoft.AspNet.SignalR;

namespace SignalR.Tick.Hubs.Auth
{
    [Authorize(Users="User")]
    [Authorize(Roles="Admin")]
    public class UserAndRoleAuthHub : NoAuthHub
    {
    }
}