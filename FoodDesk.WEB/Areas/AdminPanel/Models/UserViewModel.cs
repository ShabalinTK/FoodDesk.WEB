using System.ComponentModel.DataAnnotations;

namespace FoodDesk.WEB.Areas.AdminPanel.Models;

public class UserViewModel
{
    public string Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [Display(Name = "Username")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Display(Name = "Address")]
    public string Address { get; set; }

    public List<string> Roles { get; set; }
} 