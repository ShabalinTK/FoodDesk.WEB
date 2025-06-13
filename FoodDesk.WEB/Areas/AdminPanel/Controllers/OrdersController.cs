using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDesk.Persistence.Context;
using FoodDesk.WEB.Areas.AdminPanel.Models;
using Microsoft.Extensions.Logging;

namespace FoodDesk.WEB.Areas.AdminPanel.Controllers;

[Area("AdminPanel")]
[Authorize(Roles = "admin")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
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

        var model = new OrdersViewModel
        {
            PendingOrders = pendingOrders,
            ConfirmedOrders = confirmedOrders,
            DeliveredOrders = deliveredOrders
        };

        return View(model);
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
        return RedirectToAction(nameof(Index));
    }
}