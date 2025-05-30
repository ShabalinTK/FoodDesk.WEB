using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class MenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
