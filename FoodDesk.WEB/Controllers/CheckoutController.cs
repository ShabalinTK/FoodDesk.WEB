using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
