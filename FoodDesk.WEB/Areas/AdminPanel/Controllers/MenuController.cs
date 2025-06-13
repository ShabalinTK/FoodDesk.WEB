using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodDesk.Persistence.Context;
using FoodDesk.WEB.Areas.AdminPanel.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDesk.WEB.Areas.AdminPanel.Controllers;

[Area("AdminPanel")]
[Authorize(Roles = "admin")]
public class MenuController : Controller
{
    private readonly ApplicationDbContext _context;
    public MenuController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        // Категории
        var categories = await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl
            }).ToListAsync();

        // Топ-5 популярных блюд
        var top5 = await _context.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new {
                ProductId = g.Key,
                TotalSold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(5)
            .Join(_context.Products,
                g => g.ProductId,
                p => p.Id,
                (g, p) => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    Discount = p.Discount,
                    TotalSold = g.TotalSold
                })
            .ToListAsync();

        // Промо-товар с максимальной скидкой
        var promo = await _context.Products
            .OrderByDescending(p => p.Discount)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                Discount = p.Discount
            })
            .FirstOrDefaultAsync();

        var model = new MenuViewModel
        {
            Categories = categories,
            Top5PopularDishes = top5,
            PromoProduct = promo
        };
        return View(model);
    }
}
