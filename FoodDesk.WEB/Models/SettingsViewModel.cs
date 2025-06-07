using System.ComponentModel.DataAnnotations;

namespace FoodDesk.WEB.Models;

public class SettingsViewModel
{
    [Required]
    public string UserName { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }

    public string Password { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string Address { get; set; }

    public string ProfileImageUrl { get; set; }

    public IFormFile ProfileImage { get; set; }
}