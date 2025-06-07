using System.ComponentModel.DataAnnotations;

namespace FoodDesk.WEB.Models;

public class SettingsViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите номер телефона")]
    [Phone(ErrorMessage = "Некорректный номер телефона")]
    [Display(Name = "Phone")]
    public string PhoneNumber { get; set; } = "";

    [Required(ErrorMessage = "Введите адрес")]
    [Display(Name = "Address")]
    public string Address { get; set; } = "";

    [Display(Name = "Password")]
    [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов.", MinimumLength = 6)]
    public string Password { get; set; } = "";

    public string ProfileImageUrl { get; set; } = "";

    public IFormFile ImageFile { get; set; } = null!;
}