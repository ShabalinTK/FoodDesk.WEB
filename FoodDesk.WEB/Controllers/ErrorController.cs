using Microsoft.AspNetCore.Mvc;

namespace FoodDesk.WEB.Controllers;

public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HandleError(int statusCode)
    {
        switch (statusCode)
        {
            case 400:
            case 403:
            case 404:
            case 503:
                return View(statusCode.ToString());
            default:
                return View("500");
        }
    }
}
