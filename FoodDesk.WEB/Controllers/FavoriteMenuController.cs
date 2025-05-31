using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers
{
    public class FavoriteMenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
