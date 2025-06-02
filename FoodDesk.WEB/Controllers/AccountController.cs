using FoodDesk.Application.Features.Auth.Command.Logout;
using FoodDesk.Application.Features.Auth.Command.Register;
using FoodDesk.Application.Features.Auth.Queries.Login;
using FoodDesk.WEB.Models.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers;

public class AccountController : Controller
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
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
            return RedirectToAction("Index", "Home");

        //ModelState.AddModelError(string.Empty, result.Error);
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
            ConfirmPassword = model.ConfirmPassword
        };

        var IsSuccess = await _mediator.Send(command);

        if (IsSuccess)
            return RedirectToAction("Index", "Home");

        //ModelState.AddModelError(string.Empty, result.Error);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _mediator.Send(new LogoutCommand());
        return RedirectToAction("Index", "Home");
    }
}
