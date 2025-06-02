using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers;

public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HandleError(int statusCode)
    {
        return View(statusCode.ToString());
    }
}
