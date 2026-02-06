using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ComplectGroup.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    [AllowAnonymous] // ← Разрешить доступ без авторизации
    public IActionResult Index()
    {
        return View();
    }
}