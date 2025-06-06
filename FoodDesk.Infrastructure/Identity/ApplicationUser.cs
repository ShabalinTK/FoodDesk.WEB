using Microsoft.AspNetCore.Identity;

namespace FoodDesk.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsCourier { get; set; }
    public string Address { get; set; } = "";
}
        