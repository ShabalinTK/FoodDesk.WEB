using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers;

[Authorize(Roles = "courier")]
public class DeliverOrderController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
