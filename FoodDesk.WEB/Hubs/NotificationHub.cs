using Microsoft.AspNetCore.SignalR;
using FoodDesk.WEB.ViewModels;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace FoodDesk.WEB.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        // Хранилище уведомлений в памяти
        private static readonly ConcurrentDictionary<string, List<NotificationViewModel>> _notifications = new();

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                }

                if (Context.User?.IsInRole("Admin") == true)
                {
                    _logger.LogInformation("Admin user {UserId} connected", Context.UserIdentifier);
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                }
                else
                {
                    _logger.LogInformation("User {UserId} connected", Context.UserIdentifier);
                }
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for user {UserId}", Context.UserIdentifier);
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                }

                if (Context.User?.IsInRole("Admin") == true)
                {
                    _logger.LogInformation("Admin user {UserId} disconnected", Context.UserIdentifier);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
                }
                else
                {
                    _logger.LogInformation("User {UserId} disconnected", Context.UserIdentifier);
                }
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync for user {UserId}", Context.UserIdentifier);
                throw;
            }
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

        public async Task SendOrderNotification(string userId, string message)
        {
            try
            {
                _logger.LogInformation("Sending notification to user {UserId}: {Message}", userId, message);
                await Clients.User(userId).SendAsync("ReceiveOrderNotification", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                throw;
            }
        }

        public async Task SendAdminNotification(string message)
        {
            try
            {
                _logger.LogInformation("Sending admin notification: {Message}", message);
                await Clients.Group("Admins").SendAsync("ReceiveAdminNotification", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin notification");
                throw;
            }
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }
} 