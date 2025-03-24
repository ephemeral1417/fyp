using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using fyp.Models;

namespace fyp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
