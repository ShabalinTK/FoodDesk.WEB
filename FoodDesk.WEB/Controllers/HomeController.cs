using System.Diagnostics;
using System.Text.Json;
using FoodDesk.WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDesk.Persistence.Context;

namespace FoodDesk.WEB.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            Categories = await _context.Categories.ToListAsync(),
            Products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync(),
            Orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync()
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private List<CartItem> GetCartFromSession()
    {
        var cartJson = HttpContext.Session.GetString("Cart");
        return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound();

        var cart = GetCartFromSession();
        var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);
        if (cartItem == null)
        {
            cart.Add(new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1,
                ImageUrl = product.ImageUrl
            });
        }
        else
        {
            cartItem.Quantity++;
        }

        SaveCart(cart);

        return Json(new { success = true, cart });
    }

    [HttpPost]
    public IActionResult UpdateCartItem(int productId, int quantity)
    {
        var cart = GetCartFromSession();
        var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);
        if (cartItem == null)
            return NotFound();

        if (quantity <= 0)
            cart.Remove(cartItem);
        else
            cartItem.Quantity = quantity;

        SaveCart(cart);

        return Json(new { success = true, cart });
    }

    [HttpGet]
    public IActionResult GetCart()
    {
        var cart = GetCartFromSession();
        return Json(new { success = true, cart });
    }
}