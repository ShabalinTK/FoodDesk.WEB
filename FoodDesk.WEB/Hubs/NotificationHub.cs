using Microsoft.AspNetCore.SignalR;
using FoodDesk.WEB.ViewModels;
using System.Collections.Concurrent;

namespace FoodDesk.WEB.Hubs
{
    public class NotificationHub : Hub
    {
        // Хранилище уведомлений в памяти
        private static readonly ConcurrentDictionary<string, List<NotificationViewModel>> _notifications = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string userId, string title, string message, string type = "info")
        {
            var notification = new NotificationViewModel
            {
                Id = Guid.NewGuid().GetHashCode(),
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Type = type
            };

            // Сохраняем уведомление
            var userNotifications = _notifications.GetOrAdd(userId, _ => new List<NotificationViewModel>());
            userNotifications.Add(notification);

            // Отправляем уведомление пользователю
            await Clients.Group(userId).SendAsync("ReceiveNotification", notification);
        }

        public async Task MarkAsRead(int notificationId)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return;

            if (_notifications.TryGetValue(userId, out var notifications))
            {
                var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    await Clients.Group(userId).SendAsync("NotificationMarkedAsRead", notificationId);
                }
            }
        }

        public async Task DeleteNotification(int notificationId)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return;

            if (_notifications.TryGetValue(userId, out var notifications))
            {
                var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notifications.Remove(notification);
                    await Clients.Group(userId).SendAsync("NotificationDeleted", notificationId);
                }
            }
        }

        public List<NotificationViewModel> GetUserNotifications()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return new List<NotificationViewModel>();

            return _notifications.GetOrAdd(userId, _ => new List<NotificationViewModel>())
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }
    }
} 