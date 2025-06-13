using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodDesk.Persistence.Context;
using FoodDesk.WEB.Areas.AdminPanel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FoodDesk.WEB.Areas.AdminPanel.Controllers;

[Area("AdminPanel")]
[Authorize(Roles = "admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: DashboardController
    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders.Include(o => o.OrderItems).ToListAsync();
        var now = DateTime.UtcNow;
        var thisMonth = now.Month;
        var thisYear = now.Year;
        var lastMonth = now.AddMonths(-1).Month;
        var lastMonthYear = now.AddMonths(-1).Year;

        // Доход — 30% от суммы всех заказов
        var totalIncome = orders.Sum(o => o.TotalAmount * 0.3m);
        // Доход за текущий месяц
        var income = orders.Where(o => o.CreatedAt.Month == thisMonth && o.CreatedAt.Year == thisYear).Sum(o => o.TotalAmount * 0.3m);
        // Расход — 10% от суммы всех заказов (пример)
        var expense = orders.Sum(o => o.TotalAmount * 0.1m);

        // Статусы
        var totalOrderConfirmed = orders.Count(o => o.Status == "Confirmed");
        var totalOrderDelivered = orders.Count(o => o.Status == "Delivered");
        var totalOrderCanceled = orders.Count(o => o.Status == "Cancelled");
        var totalOrderPending = orders.Count(o => o.Status == "Pending");

        // Всего заказов
        var orderTotal = orders.Count;
        // Цель (target) — например, 1000 заказов в месяц
        var orderTarget = 1000;
        // Заказы за этот месяц
        var ordersThisMonth = orders.Count(o => o.CreatedAt.Month == thisMonth && o.CreatedAt.Year == thisYear);
        // Заказы за прошлый месяц
        var ordersLastMonth = orders.Count(o => o.CreatedAt.Month == lastMonth && o.CreatedAt.Year == lastMonthYear);

        var model = new DashboardViewModel
        {
            TotalIncome = totalIncome,
            Income = income,
            Expense = expense,
            TotalOrderConfirmed = totalOrderConfirmed,
            TotalOrderDelivered = totalOrderDelivered,
            TotalOrderCanceled = totalOrderCanceled,
            TotalOrderPending = totalOrderPending,
            OrderTotal = orderTotal,
            OrderTarget = orderTarget,
            OrdersThisMonth = ordersThisMonth,
            OrdersLastMonth = ordersLastMonth
        };
        return View(model);
    }

    // GET: DashboardController/Details/5
    public ActionResult Details(int id)
    {
        return View();
    }

    // GET: DashboardController/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: DashboardController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: DashboardController/Edit/5
    public ActionResult Edit(int id)
    {
        return View();
    }

    // POST: DashboardController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: DashboardController/Delete/5
    public ActionResult Delete(int id)
    {
        return View();
    }

    // POST: DashboardController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Withdraw()
    {
        var orders = await _context.Orders.Include(o => o.OrderItems).ToListAsync();
        var now = DateTime.UtcNow;
        var thisMonth = now.Month;
        var thisYear = now.Year;
        var lastMonth = now.AddMonths(-1).Month;
        var lastMonthYear = now.AddMonths(-1).Year;

        var totalIncome = orders.Sum(o => o.TotalAmount * 0.3m);
        var income = 0m; // Обнуляем доход за месяц
        var expense = orders.Sum(o => o.TotalAmount * 0.1m);
        var totalOrderConfirmed = orders.Count(o => o.Status == "Confirmed");
        var totalOrderDelivered = orders.Count(o => o.Status == "Delivered");
        var totalOrderCanceled = orders.Count(o => o.Status == "Cancelled");
        var totalOrderPending = orders.Count(o => o.Status == "Pending");
        var orderTotal = orders.Count;
        var orderTarget = 1000;
        var ordersThisMonth = orders.Count(o => o.CreatedAt.Month == thisMonth && o.CreatedAt.Year == thisYear);
        var ordersLastMonth = orders.Count(o => o.CreatedAt.Month == lastMonth && o.CreatedAt.Year == lastMonthYear);

        var model = new DashboardViewModel
        {
            TotalIncome = totalIncome,
            Income = income,
            Expense = expense,
            TotalOrderConfirmed = totalOrderConfirmed,
            TotalOrderDelivered = totalOrderDelivered,
            TotalOrderCanceled = totalOrderCanceled,
            TotalOrderPending = totalOrderPending,
            OrderTotal = orderTotal,
            OrderTarget = orderTarget,
            OrdersThisMonth = ordersThisMonth,
            OrdersLastMonth = ordersLastMonth
        };
        return View("Index", model);
    }
}
