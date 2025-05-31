using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers
{
    [Authorize(Roles = "courier")]
    public class DeliverMainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
