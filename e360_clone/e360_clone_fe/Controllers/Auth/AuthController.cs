using Microsoft.AspNetCore.Mvc;
using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.Helpers;
using e360_clone.Repositories;
using e360_clone_fe.Extensions;
using e360_clone_fe.Models;

namespace e360_clone_fe.Controllers
{
    /// <summary>
    /// Authentication Controller
    /// Uses session-based authentication with AppUser model (NO Claims)
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAccountRepository accountRepository,
            ILogger<AuthController> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        /// <summary>
        /// Display login page
        /// </summary>
        public IActionResult Login(string? returnUrl = null)
        {
            // If already logged in, redirect to home
            if (HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        /// <summary>
        /// Handle login form submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            try
            {
                // Trim email and password to remove accidental spaces
                var email = model.Email?.Trim() ?? string.Empty;
                var password = model.Password?.Trim() ?? string.Empty;

                _logger.LogInformation("Login attempt for email: {Email}", email);

                // Find account by email or username
                var account = await _accountRepository.FindByEmailOrUsernameAsync(email);

                if (account == null)
                {
                    _logger.LogWarning("Login failed: Account not found for {Email}", email);
                    TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng";
                    return View(model);
                }

                // Check account status
                if (account.Status != "Active")
                {
                    _logger.LogWarning("Login failed: Account {Username} has status {Status}", account.Username, account.Status);
                    TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa hoặc không hoạt động";
                    return View(model);
                }

                // Verify password
                var passwordMatch = PasswordHelper.VerifyPassword(password, account.PasswordHash);

                if (!passwordMatch)
                {
                    _logger.LogWarning("Login failed: Invalid password for {Email}", email);
                    TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng";
                    return View(model);
                }

                // Create AppUser model and save to session
                var user = new AppUser
                {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    FullName = account.FullName ?? account.Username,
                    Role = account.Role,
                    AvatarUrl = account.AvatarUrl,
                    Status = account.Status,
                    LastLoginAt = account.LastLoginAt
                };

                // Save user to session
                HttpContext.Session.SetUser(user);

                // Update last login
                await _accountRepository.UpdateLastLoginAsync(account.Id);

                _logger.LogInformation("User {Username} logged in successfully", account.Username);

                // Redirect to return URL or home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        /// <summary>
        /// Quick login for demo/development (no password required)
        /// </summary>
        [HttpGet]
        public IActionResult QuickLogin(string role, string? returnUrl = null)
        {
            try
            {
                // Validate role
                var validRoles = new[] { "SuperAdmin", "Admin", "Student", "Teacher", "Parent", "Librarian", "Staff" };
                if (!validRoles.Contains(role))
                {
                    TempData["ErrorMessage"] = "Role không hợp lệ";
                    return RedirectToAction("Login");
                }

                var username = $"{role}@demo.com";

                // Create demo user
                var user = new AppUser
                {
                    Id = 0,
                    Username = username,
                    Email = username,
                    FullName = $"Demo {role}",
                    Role = role,
                    Status = "Active"
                };

                // Save to session
                HttpContext.Session.SetUser(user);

                _logger.LogInformation("Demo user {Username} logged in via QuickLogin", username);

                // Redirect to return URL or home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during QuickLogin for role {Role}", role);
                TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return RedirectToAction("Login");
            }
        }

        /// <summary>
        /// Handle logout
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            try
            {
                var userName = HttpContext.Session.GetUser()?.Username;

                // Clear session
                HttpContext.Session.Logout();

                _logger.LogInformation("User {UserName} logged out", userName);

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng xuất";
                return RedirectToAction("Login");
            }
        }

        /// <summary>
        /// Access denied page
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
