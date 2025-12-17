using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chattt.Hubs;

[Authorize]
public class ChatHub : Hub
{
}