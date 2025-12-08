using Microsoft.AspNetCore.Mvc;

namespace ComplectGroup.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}