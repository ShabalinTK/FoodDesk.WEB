using FoodDesk.Application.Features.Auth.Command.Logout;
using FoodDesk.Application.Features.Auth.Command.Register;
using FoodDesk.Application.Features.Auth.Queries.Login;
using FoodDesk.WEB.Models.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodDesk.WEB.Controllers;

public class AccountController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IMediator mediator, ILogger<AccountController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var query = new LoginQuery(model.Email, model.Password);
        var IsSuccess = await _mediator.Send(query);
        if (IsSuccess)
        {
            _logger.LogInformation("User logged in: {Email}", model.Email);
            return RedirectToAction("Index", "Home");
        }
        _logger.LogWarning("Login failed for: {Email}", model.Email);
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var command = new RegisterCommand
        {
            UserName = model.UserName,
            Email = model.Email,
            Password = model.Password,
            ConfirmPassword = model.ConfirmPassword,
            IsCourier = model.IsCourier
        };
        var IsSuccess = await _mediator.Send(command);
        if (IsSuccess)
        {
            _logger.LogInformation("User registered: {Email}", model.Email);
            return RedirectToAction("Index", "Home");
        }
        _logger.LogWarning("Registration failed for: {Email}", model.Email);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _mediator.Send(new LogoutCommand());
        return RedirectToAction("Index", "Home");
    }
}
