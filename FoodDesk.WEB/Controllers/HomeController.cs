using System.Diagnostics;
using System.Text.Json;
using FoodDesk.WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDesk.Persistence.Context;
using Microsoft.Extensions.Caching.Distributed;
using FoodDesk.Domain.Entities;

namespace FoodDesk.WEB.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public HomeController(
        ILogger<HomeController> logger, 
        ApplicationDbContext context,
        IDistributedCache cache)
    {
        _logger = logger;
        _context = context;
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
    }

    public async Task<IActionResult> Index()
    {
        // Получаем категории из кэша или из БД
        var categories = await GetCategoriesFromCache();
        
        // Получаем популярные продукты из кэша или из БД
        var popularProducts = await GetPopularProductsFromCache();

        var model = new HomeViewModel
        {
            Categories = categories,
            Products = popularProducts,
            Orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync()
        };
        return View(model);
    }

    private async Task<List<Category>> GetCategoriesFromCache()
    {
        const string cacheKey = "categories";
        var cachedCategories = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedCategories))
        {
            _logger.LogInformation("Categories retrieved from Redis cache");
            return JsonSerializer.Deserialize<List<Category>>(cachedCategories, _jsonOptions);
        }

        _logger.LogInformation("Categories retrieved from database");
        var categories = await _context.Categories.ToListAsync();
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromHours(24)); // Кэш на 24 часа

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(categories, _jsonOptions),
            options);
        _logger.LogInformation("Categories saved to Redis cache");

        return categories;
    }

    private async Task<List<Product>> GetPopularProductsFromCache()
    {
        const string cacheKey = "popular_products";
        var cachedProducts = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedProducts))
        {
            _logger.LogInformation("Popular products retrieved from Redis cache");
            return JsonSerializer.Deserialize<List<Product>>(cachedProducts, _jsonOptions);
        }

        _logger.LogInformation("Popular products retrieved from database");
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsPopular)
            .ToListAsync();

        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromHours(1)); // Кэш на 1 час

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(products, _jsonOptions),
            options);
        _logger.LogInformation("Popular products saved to Redis cache");

        return products;
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