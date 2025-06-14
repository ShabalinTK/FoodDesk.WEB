using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDesk.Infrastructure.Identity;
using FoodDesk.Persistence.Context;
using FoodDesk.WEB.Models;
using FoodDesk.WEB.Hubs;
using Microsoft.AspNetCore.SignalR;
using FoodDesk.Domain.Entities;

namespace FoodDesk.WEB.Controllers;

[Authorize]
public class FoodOrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<NotificationHub> _hubContext;

    public FoodOrderController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _hubContext = hubContext;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new FoodOrderViewModel
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                RestaurantName = o.RestaurantName,
                DeliveryTime = o.DeliveryTime,
                Distance = o.Distance,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                Items = o.OrderItems.Select(oi => new FoodOrderItemViewModel
                {
                    Name = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    ImageUrl = oi.Product.ImageUrl
                }).ToList()
            })
            .ToListAsync();

        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var order = new Order
        {
            UserId = user.Id,
            RestaurantName = model.RestaurantName,
            DeliveryTime = model.DeliveryTime,
            Distance = model.Distance,
            Status = "Pending",
            TotalAmount = model.TotalAmount,
            CreatedAt = DateTime.UtcNow,
            OrderItems = model.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Отправляем уведомление админам о новом заказе
        await _hubContext.Clients.Group("Admins").SendAsync("ReceiveAdminNotification", $"Новый заказ #{order.Id} от {user.UserName}");

        // Отправляем уведомление клиенту
        await _hubContext.Clients.User(user.Id).SendAsync("ReceiveOrderNotification", $"Ваш заказ #{order.Id} успешно создан!");

        return Json(new { success = true, orderId = order.Id });
    }
}
