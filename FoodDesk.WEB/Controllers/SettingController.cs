using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers
{
    [Authorize(Roles = "client")]
    public class SettingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
