using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class ReviewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
