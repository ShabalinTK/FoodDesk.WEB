using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers
{
    public class MessageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
