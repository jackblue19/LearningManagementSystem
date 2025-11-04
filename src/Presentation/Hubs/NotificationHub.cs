using Microsoft.AspNetCore.SignalR;

namespace Presentation.Hubs;

public class NotificationHub : Hub
{
    // Example: server-to-client notification method
    // Clients can implement a method like: receiveNotification(string title, string message)
    // Call from server: await Clients.User(userId).SendAsync("receiveNotification", title, message);
}
