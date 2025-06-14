using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDesk.Persistence.Context;
using FoodDesk.WEB.Areas.AdminPanel.Models;
using Microsoft.Extensions.Logging;
using FoodDesk.WEB.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FoodDesk.WEB.Areas.AdminPanel.Controllers;

[Area("AdminPanel")]
[Authorize(Roles = "admin")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrdersController> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersController(
        ApplicationDbContext context, 
        ILogger<OrdersController> logger,
        IHubContext<NotificationHub> hubContext,
        IDistributedCache cache)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
    }

    public async Task<IActionResult> Index()
    {
        var model = await GetOrdersFromCache();
        return View(model);
    }

    private async Task<OrdersViewModel> GetOrdersFromCache()
    {
        const string cacheKey = "admin_orders";
        var cachedOrders = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedOrders))
        {
            _logger.LogInformation("Orders retrieved from Redis cache");
            return JsonSerializer.Deserialize<OrdersViewModel>(cachedOrders, _jsonOptions);
        }

        _logger.LogInformation("Orders retrieved from database");
        var pendingOrders = await _context.Orders
            .Where(o => o.Status == "Pending")
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                RestaurantName = o.RestaurantName,
                DeliveryTime = o.DeliveryTime,
                Distance = o.Distance,
                Status = o.Status,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();

        var confirmedOrders = await _context.Orders
            .Where(o => o.Status == "Confirmed")
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                RestaurantName = o.RestaurantName,
                DeliveryTime = o.DeliveryTime,
                Distance = o.Distance,
                Status = o.Status,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();

        var deliveredOrders = await _context.Orders
            .Where(o => o.Status == "Delivered")
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                RestaurantName = o.RestaurantName,
                DeliveryTime = o.DeliveryTime,
                Distance = o.Distance,
                Status = o.Status,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();

        var model = new OrdersViewModel
        {
            PendingOrders = pendingOrders,
            ConfirmedOrders = confirmedOrders,
            DeliveredOrders = deliveredOrders
        };

        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5)); // Кэш на 5 минут

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(model, _jsonOptions),
            options);
        _logger.LogInformation("Orders saved to Redis cache");

        return model;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderDetails(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        var orderDetails = new OrderViewModel
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            RestaurantName = order.RestaurantName,
            DeliveryTime = order.DeliveryTime,
            Distance = order.Distance,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Items = order.OrderItems.Select(oi => new OrderItemViewModel
            {
                Name = oi.Product.Name,
                Quantity = oi.Quantity,
                Price = oi.Price,
                ImageUrl = oi.Product.ImageUrl
            }).ToList()
        };

        return PartialView("_OrderDetails", orderDetails);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        _logger.LogInformation("Order status update: OrderId={OrderId}, NewStatus={Status}", id, status);
        order.Status = status;
        await _context.SaveChangesAsync();

        // Инвалидируем кэш заказов
        await _cache.RemoveAsync("admin_orders");
        _logger.LogInformation("Orders cache invalidated after status update");

        // Отправляем уведомление клиенту
        string message = status switch
        {
            "Confirmed" => $"Ваш заказ #{id} был принят!",
            "Cancelled" => $"Ваш заказ #{id} был отменен.",
            "Delivered" => $"Ваш заказ #{id} был доставлен!",
            _ => $"Статус вашего заказа #{id} изменен на {status}"
        };

        await _hubContext.Clients.User(order.UserId).SendAsync("ReceiveOrderNotification", message);

        // Отправляем уведомление админам
        await _hubContext.Clients.Group("Admins").SendAsync("ReceiveAdminNotification", $"Заказ #{id} обновлен: {status}");

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var pendingOrders = await _context.Orders
            .Where(o => o.Status == "Pending")
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                RestaurantName = o.RestaurantName,
                DeliveryTime = o.DeliveryTime,
                Distance = o.Distance,
                Status = o.Status,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();

        var confirmedOrders = await _context.Orders
            .Where(o => o.Status == "Confirmed")
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                RestaurantName = o.RestaurantName,
                DeliveryTime = o.DeliveryTime,
                Distance = o.Distance,
                Status = o.Status,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();

        var deliveredOrders = await _context.Orders
            .Where(o => o.Status == "Delivered")
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                RestaurantName = o.RestaurantName,
                DeliveryTime = o.DeliveryTime,
                Distance = o.Distance,
                Status = o.Status,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();

        return Json(new { 
            pendingOrders = pendingOrders,
            confirmedOrders = confirmedOrders,
            deliveredOrders = deliveredOrders
        });
    }
}