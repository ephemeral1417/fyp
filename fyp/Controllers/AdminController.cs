using fyp.Models;
using fyp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace fyp.Controllers
{
    public class AdminController : Controller
    {
        private readonly FirebaseAuthService _authService;
        private readonly FirestoreService _firestoreService;

        public AdminController(FirebaseAuthService authService, FirestoreService firestoreService)
        {
            _authService = authService;
            _firestoreService = firestoreService;
        }

        // Only allow admin access
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "admin";
        }

        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // Your existing method
        public IActionResult RoleManagement()
        {
            return View();
        }

        // Your existing method (GET)
        public IActionResult Register()
        {
            return View();
        }

        // Your existing method
        public IActionResult RackSetup()
        {
            return View();
        }

        // Adding the POST method for Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var (uid, password) = await _authService.RegisterUserAsync(model);

                    // Pass the generated password to the view to display to the admin
                    ViewBag.GeneratedPassword = password;
                    ViewBag.Success = true;
                    ViewBag.Message = "User created successfully!";

                    return View();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
                }
            }

            return View(model);
        }

        // Adding the async version of role management
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var users = await _authService.GetAllUsersAsync();
            return Json(users);
        }
    }
}