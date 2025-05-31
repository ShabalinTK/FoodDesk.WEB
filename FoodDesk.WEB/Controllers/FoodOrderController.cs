using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers
{
    public class FoodOrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
