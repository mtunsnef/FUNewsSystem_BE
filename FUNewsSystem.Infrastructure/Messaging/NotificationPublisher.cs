using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Messaging
{
    public class NotificationPublisher : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[SignalR] Connected: ConnId={Context.ConnectionId}, UserId={Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }
    }
}
