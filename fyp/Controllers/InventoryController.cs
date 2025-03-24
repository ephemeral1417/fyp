using Microsoft.AspNetCore.Mvc;

namespace fyp.Controllers
{
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
