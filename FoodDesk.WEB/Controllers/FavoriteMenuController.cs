using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDesk.Persistence.Context;
using FoodDesk.Domain.Entities;

namespace FoodDesk.WEB.Controllers;

[Authorize(Roles = "client")]
public class FavoriteMenuController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 8;

    public FavoriteMenuController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        var products = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(products);
    }
}
