namespace FoodDesk.WEB.Areas.AdminPanel.Models;

public class UserViewModel
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public List<string> Roles { get; set; }
} 