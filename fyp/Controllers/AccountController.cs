using fyp.Models;
using fyp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace fyp.Controllers
{
    public class AccountController : Controller
    {
        private readonly FirebaseAuthService _authService;

        public AccountController(FirebaseAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _authService.LoginAsync(model);

                    // Store user information in session
                    HttpContext.Session.SetString("Uid", user.Uid);
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    // Always redirect to Dashboard
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Login failed: {ex.Message}");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}