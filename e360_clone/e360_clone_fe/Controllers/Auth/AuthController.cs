using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.BusinessObjects.Helpers;
using e360_clone_fe.Models;
using Microsoft.EntityFrameworkCore;

namespace e360_clone_fe.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? role = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find account by email or username
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == model.Email || a.Username == model.Email);

            if (account == null)
            {
                // Demo mode: If no account found, allow quick login with role
                if (!string.IsNullOrEmpty(role))
                {
                    return await QuickLogin(role);
                }

                TempData["Error"] = "Invalid email or password";
                return View(model);
            }

            // Check status
            if (account.Status != "Active")
            {
                TempData["Error"] = "Your account has been locked or deactivated";
                return View(model);
            }

            // Verify password
            if (!PasswordHelper.VerifyPassword(model.Password, account.PasswordHash))
            {
                TempData["Error"] = "Invalid email or password";
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role),
                new Claim("FullName", account.FullName ?? account.Username),
                new Claim("UserId", account.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Update last login
            account.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Redirect based on role
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> QuickLogin(string role)
        {
            // For demo/development: allow quick login without password
            var username = $"{role}@demo.com";
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", $"Demo {role}"),
                new Claim("UserId", "0")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
