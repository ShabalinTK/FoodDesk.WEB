using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDesk.Infrastructure.Identity;
using FoodDesk.WEB.Areas.AdminPanel.Models;
using Microsoft.Extensions.Logging;

namespace FoodDesk.WEB.Areas.AdminPanel.Controllers;

[Area("AdminPanel")]
[Authorize(Roles = "admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Roles = roles.ToList()
            });
        }

        return View(userViewModels);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _roleManager.Roles.ToListAsync();
        var model = new UserViewModel
        {
            Roles = new List<string>()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserViewModel model, string password, List<string> selectedRoles)
    {
        ModelState.Remove("Id");
        ModelState.Remove("Roles");
        model.Roles = selectedRoles ?? new List<string>();
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("User creation validation failed: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(model);
        }
        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Address = model.Address
        };
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            if (selectedRoles != null && selectedRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, selectedRoles);
            }
            _logger.LogInformation("User created: {Email}", user.Email);
            return RedirectToAction(nameof(Index));
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
            _logger.LogError("User creation failed: {Error}", error.Description);
        }
        ViewBag.Roles = await _roleManager.Roles.ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var model = new UserViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            Roles = roles?.ToList() ?? new List<string>()
        };

        ViewBag.Roles = await _roleManager.Roles.ToListAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserViewModel model, string password, List<string> selectedRoles)
    {
        ModelState.Remove("Id");
        ModelState.Remove("Roles");
        model.Roles = selectedRoles ?? new List<string>();
        if (id != model.Id)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    result = await _userManager.ResetPasswordAsync(user, token, password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                        return View(model);
                    }
                }
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (selectedRoles != null && selectedRoles.Any())
                {
                    await _userManager.AddToRolesAsync(user, selectedRoles);
                }
                _logger.LogInformation("User edited: {Email}", user.Email);
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        ViewBag.Roles = await _roleManager.Roles.ToListAsync();
        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Delete(string id)
    {
        ApplicationUser user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            IdentityResult result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User deleted: {Email}", user.Email);
                return RedirectToAction("Index");
            }
            else
            {
                return View("Error", result.Errors);
            }
        }
        else
        {
            return View("Error", new string[] { "Пользователь не найден" });
        }
    }
} 