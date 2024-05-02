using Microsoft.AspNetCore.Mvc;

namespace PustokHomework.Areas.Manage.Controllers
{
    public class DashboardController : Controller
    {
        [Area("manage")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
