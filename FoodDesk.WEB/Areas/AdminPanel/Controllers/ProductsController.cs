using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDesk.WEB.Areas.AdminPanel.Models;
using FoodDesk.Persistence.Context;
using FoodDesk.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FoodDesk.WEB.Areas.AdminPanel.Controllers;

[Area("AdminPanel")]
[Authorize(Roles = "admin")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Discount = p.Discount,
                Rating = p.Rating,
                ImageUrl = p.ImageUrl,
                IsPopular = p.IsPopular,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .ToListAsync();

        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        ModelState.Remove("CategoryName");
        if (ModelState.IsValid)
        {
            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                Discount = model.Discount,
                Rating = model.Rating,
                ImageUrl = model.ImageUrl,
                IsPopular = model.IsPopular,
                CategoryId = model.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product created: {Name}", product.Name);
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Discount = product.Discount,
            Rating = product.Rating,
            ImageUrl = product.ImageUrl,
            IsPopular = product.IsPopular,
            CategoryId = product.CategoryId
        };

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductViewModel model)
    {
        ModelState.Remove("CategoryName");
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = model.Name;
            product.Price = model.Price;
            product.Discount = model.Discount;
            product.Rating = model.Rating;
            product.ImageUrl = model.ImageUrl;
            product.IsPopular = model.IsPopular;
            product.CategoryId = model.CategoryId;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product edited: {Name}", product.Name);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Product deleted: {Name}", product.Name);
        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
} 