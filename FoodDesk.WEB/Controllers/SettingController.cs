using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FoodDesk.Infrastructure.Identity;
using FoodDesk.WEB.Models;
using Microsoft.Extensions.Logging;

namespace FoodDesk.WEB.Controllers;

[Authorize]
public class SettingController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<SettingController> _logger;

    public SettingController(
        UserManager<ApplicationUser> userManager, 
        IWebHostEnvironment webHostEnvironment,
        ILogger<SettingController> logger)
    {
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
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
            //ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageUrl) ? "/FoodDesk/images/no-img-avatar.png" : user.ProfileImageUrl
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile([FromForm] SettingsViewModel model)
    {
        _logger.LogInformation("Начало обновления профиля для пользователя");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Модель невалидна: {Errors}", 
                string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)));
            return View("Index", model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("Пользователь не найден");
            return NotFound();
        }

        _logger.LogInformation("Текущие данные пользователя: UserName={UserName}, Email={Email}, Phone={Phone}, Address={Address}",
            user.UserName, user.Email, user.PhoneNumber, user.Address);

        bool hasChanges = false;

        // Обработка загрузки изображения
        //if (model.ImageFile != null)
        //{
        //    try 
        //    {
        //        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FoodDesk", "images", "avatars");
        //        if (!Directory.Exists(uploadsFolder))
        //        {
        //            Directory.CreateDirectory(uploadsFolder);
        //        }

        //        string uniqueFileName = $"{Guid.NewGuid()}_{model.ImageFile.FileName}";
        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await model.ImageFile.CopyToAsync(fileStream);
        //        }

        //        user.ProfileImageUrl = $"/FoodDesk/images/avatars/{uniqueFileName}";
        //        hasChanges = true;
        //        _logger.LogInformation("Изображение профиля обновлено: {ImageUrl}", user.ProfileImageUrl);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка при сохранении изображения профиля");
        //    }
        //}

        // Проверяем изменения в основных данных
        if (user.UserName != model.UserName)
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.UserName);
            if (setUserNameResult.Succeeded)
            {
                hasChanges = true;
                _logger.LogInformation("Имя пользователя обновлено: {UserName}", model.UserName);
            }
            else
            {
                _logger.LogError("Ошибка при обновлении имени пользователя: {Errors}",
                    string.Join(", ", setUserNameResult.Errors.Select(e => e.Description)));
                foreach (var error in setUserNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        if (user.Email != model.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (setEmailResult.Succeeded)
            {
                hasChanges = true;
                _logger.LogInformation("Email обновлен: {Email}", model.Email);
            }
            else
            {
                _logger.LogError("Ошибка при обновлении email: {Errors}",
                    string.Join(", ", setEmailResult.Errors.Select(e => e.Description)));
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        if (user.PhoneNumber != model.PhoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
            if (setPhoneResult.Succeeded)
            {
                hasChanges = true;
                _logger.LogInformation("Телефон обновлен: {Phone}", model.PhoneNumber);
            }
            else
            {
                _logger.LogError("Ошибка при обновлении телефона: {Errors}",
                    string.Join(", ", setPhoneResult.Errors.Select(e => e.Description)));
                foreach (var error in setPhoneResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        if (user.Address != model.Address)
        {
            user.Address = model.Address;
            hasChanges = true;
            _logger.LogInformation("Адрес обновлен: {Address}", model.Address);
        }

        // Сохраняем изменения в базе данных
        if (hasChanges)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Ошибка при сохранении изменений: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Index", model);
            }
        }

        // Обновление пароля, если он был указан
        if (!string.IsNullOrEmpty(model.Password))
        {
            _logger.LogInformation("Попытка обновления пароля");
            
            // Сначала проверяем, существует ли у пользователя пароль
            var hasPassword = await _userManager.HasPasswordAsync(user);
            IdentityResult passwordResult;
            
            if (hasPassword)
            {
                // Если пароль существует, используем метод изменения пароля
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
            }
            else
            {
                // Если пароля нет, добавляем новый
                passwordResult = await _userManager.AddPasswordAsync(user, model.Password);
            }

            if (!passwordResult.Succeeded)
            {
                _logger.LogError("Ошибка при обновлении пароля: {Errors}",
                    string.Join(", ", passwordResult.Errors.Select(e => e.Description)));
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Index", model);
            }
            _logger.LogInformation("Пароль успешно обновлен");
        }

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        _logger.LogInformation("Профиль успешно обновлен");
        TempData["SuccessMessage"] = "Профиль успешно обновлен";
        return RedirectToAction(nameof(Index));
    }
}