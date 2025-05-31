using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
