using System.Text.Json;
using FoodDesk.Application.Interfaces.Services;
using FoodDesk.Domain.Entities;
using FoodDesk.Infrastructure.Identity;
using FoodDesk.Persistence.Context;
using FoodDesk.WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDesk.WEB.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public CheckoutController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task<IActionResult> Index()
    {
        var cart = GetCartFromSession();
        if (cart == null || !cart.Any())
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new CheckoutViewModel
        {
            FirstName = user.UserName?.Split(' ').FirstOrDefault() ?? "",
            LastName = user.UserName?.Split(' ').Skip(1).FirstOrDefault() ?? "",
            Email = user.Email ?? "",
            Phone = user.PhoneNumber ?? "",
            Address = user.Address ?? "",
            CartItems = cart,
            CartTotal = cart.Sum(item => item.Price * item.Quantity)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CartItems = GetCartFromSession();
            return View(model);
        }

        var cart = GetCartFromSession();
        if (cart == null || !cart.Any())
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Рассчитываем сумму заказа
        model.CartTotal = cart.Sum(item => item.Price * item.Quantity);
        var totalAmount = model.CartTotal + model.TipAmount;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                UserId = user.Id,
                TotalAmount = totalAmount,
                Status = "Preparing",
                CreatedAt = DateTime.UtcNow,
                RestaurantName = "FoodDesk Restaurant",
                DeliveryTime = 45,
                Distance = 5.0m
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderItems = cart.Select(item => new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price * item.Quantity
            }).ToList();

            _context.OrderItems.AddRange(orderItems);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Очищаем корзину
            HttpContext.Session.Remove("Cart");

            // Отправляем email с подтверждением
            var emailMessage = $@"Спасибо за ваш заказ!

Номер заказа: {order.Id}
Сумма заказа: ${model.CartTotal:F2}

Детали заказа:
{string.Join("\n", cart.Select(item => $"- {item.Name} x{item.Quantity} (${item.Price * item.Quantity:F2})"))}

Чаевые: ${model.TipAmount:F2}
Итого: ${totalAmount:F2}

Адрес доставки:
{model.Address}
{model.City}
{model.Country}

Статус заказа: {order.Status}
Ожидаемое время доставки: {order.DeliveryTime} минут

С уважением,
Команда FoodDesk";

            await _emailSender.SendAsync(model.Email, $"Заказ #{order.Id} подтвержден", emailMessage);

            TempData["SuccessMessage"] = $"Заказ #{order.Id} успешно оформлен! Подтверждение отправлено на ваш email.";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "Произошла ошибка при оформлении заказа. Пожалуйста, попробуйте снова.");
            model.CartItems = cart;
            return View(model);
        }
    }

    private List<CartItem> GetCartFromSession()
    {
        var cartJson = HttpContext.Session.GetString("Cart");
        return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
    }
}
