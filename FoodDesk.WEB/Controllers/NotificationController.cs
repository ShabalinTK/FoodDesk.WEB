using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers;

[Authorize]
public class NotificationController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
