using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FoodDesk.Infrastructure.Identity;
using FoodDesk.WEB.Models;

namespace FoodDesk.WEB.Controllers;

[Authorize]
public class SettingController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SettingController(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new SettingsViewModel
        {
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            Address = user.Address ?? "",
            ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageUrl) ? "/FoodDesk/images/no-img-avatar.png" : user.ProfileImageUrl
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(SettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Обработка загрузки изображения
        if (model.ImageFile != null)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FoodDesk", "images", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = $"{Guid.NewGuid()}_{model.ImageFile.FileName}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(fileStream);
            }

            user.ProfileImageUrl = $"/FoodDesk/images/avatars/{uniqueFileName}";
        }

        // Обновление данных пользователя
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;

        var result = await _userManager.UpdateAsync(user);

        // Обновление пароля, если он был указан
        if (!string.IsNullOrEmpty(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
            
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Index", model);
            }
        }

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Профиль успешно обновлен";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("Index", model);
    }
}
