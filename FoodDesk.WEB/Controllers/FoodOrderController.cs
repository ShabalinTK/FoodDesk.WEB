using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDesk.Infrastructure.Identity;
using FoodDesk.Persistence.Context;
using FoodDesk.WEB.Models;

namespace FoodDesk.WEB.Controllers;

[Authorize]
public class FoodOrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FoodOrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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
}
