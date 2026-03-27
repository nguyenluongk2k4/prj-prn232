using Microsoft.AspNetCore.Mvc;
using e360_clone_fe.Models;
using e360_clone_fe.Services;
using System.Text.Json;

namespace e360_clone_fe.Controllers
{
    /// <summary>
    /// Authentication Controller
    /// Uses JWT token from Backend API (stored in HttpOnly cookie)
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IApiService apiService,
            ILogger<AuthController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        /// <summary>
        /// Display login page
        /// </summary>
        public IActionResult Login(string? returnUrl = null)
        {
            // Check if already has JWT token
            var token = _apiService.GetAuthToken();
            var role = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        /// <summary>
        /// Handle login - Call Backend API to get JWT token
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
                var email = model.Email?.Trim() ?? string.Empty;
                var password = model.Password?.Trim() ?? string.Empty;

                _logger.LogInformation("Login attempt for email: {Email}", email);

                // Call Backend API to authenticate and get JWT token
                // Backend route: /api/Auth/login
                var loginData = new { Email = email, Password = password };
                var response = await _apiService.PostAsync<LoginResponse>("Auth/login", loginData);

                if (!response.Success || response.Data == null)
                {
                    _logger.LogWarning("Login failed: {Message}", response.Message);
                    TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng";
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                // Save JWT token to HttpOnly cookie
                _apiService.SetAuthToken(response.Data.Token);

                // Fetch user profile after login
                var profileResponse = await _apiService.GetWithTokenAsync<UserProfileResponse>(
                    "Auth/me",
                    response.Data.Token);

                if (profileResponse.Success && profileResponse.Data != null)
                {
                    HttpContext.Session.SetString("Role", profileResponse.Data.Role);
                    HttpContext.Session.SetString("FullName", profileResponse.Data.FullName);
                    HttpContext.Session.SetString("Username", profileResponse.Data.Username);
                    HttpContext.Session.SetString("Email", profileResponse.Data.Email);
                    if (!string.IsNullOrWhiteSpace(profileResponse.Data.AvatarUrl))
                    {
                        HttpContext.Session.SetString("AvatarUrl", profileResponse.Data.AvatarUrl);
                    }
                    if (profileResponse.Data.LecturerId.HasValue)
                    {
                        HttpContext.Session.SetInt32("LecturerId", profileResponse.Data.LecturerId.Value);
                    }
                    if (profileResponse.Data.StudentId.HasValue)
                    {
                        HttpContext.Session.SetInt32("StudentId", profileResponse.Data.StudentId.Value);
                    }
                    ViewData["Role"] = profileResponse.Data.Role;
                }
                else
                {
                    // Fallback to data from login response
                    HttpContext.Session.SetString("Role", response.Data.Role);
                    HttpContext.Session.SetString("FullName", response.Data.FullName);
                    HttpContext.Session.SetString("Username", response.Data.Username);
                    HttpContext.Session.SetString("Email", response.Data.Email);
                    if (response.Data.LecturerId.HasValue)
                    {
                        HttpContext.Session.SetInt32("LecturerId", response.Data.LecturerId.Value);
                    }
                    if (response.Data.StudentId.HasValue)
                    {
                        HttpContext.Session.SetInt32("StudentId", response.Data.StudentId.Value);
                    }
                    ViewData["Role"] = response.Data.Role;
                }

                _logger.LogInformation("User {Username} logged in successfully", response.Data.Username);

                // Redirect
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if (response.Data.Role is "Admin" or "SuperAdmin" or "Staff")
                {
                    return RedirectToAction("School", "Dashboard");
                }

                if (response.Data.Role == "Student")
                {
                    return RedirectToAction("MyStudent", "ExamSchedules");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
        }

        /// <summary>
        /// Quick login for demo (bypass password)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QuickLogin(string role, string? returnUrl = null)
        {
            try
            {
                var validRoles = new[] { "SuperAdmin", "Admin", "Student", "Teacher", "Parent", "Librarian", "Staff" };
                if (!validRoles.Contains(role))
                {
                    TempData["ErrorMessage"] = "Role không hợp lệ";
                    return RedirectToAction("Login");
                }

                // Call backend API for quick login
                var loginData = new { Email = $"{role}@demo.com", Password = "demo123", QuickLogin = true };
                var response = await _apiService.PostAsync<LoginResponse>("Auth/quick-login", loginData);

                if (response.Success && response.Data != null)
                {
                    _apiService.SetAuthToken(response.Data.Token);

                    var profileResponse = await _apiService.GetWithTokenAsync<UserProfileResponse>(
                        "Auth/me",
                        response.Data.Token);

                    if (profileResponse.Success && profileResponse.Data != null)
                    {
                        HttpContext.Session.SetString("Role", profileResponse.Data.Role);
                        HttpContext.Session.SetString("FullName", profileResponse.Data.FullName);
                        HttpContext.Session.SetString("Username", profileResponse.Data.Username);
                        HttpContext.Session.SetString("Email", profileResponse.Data.Email);
                        if (!string.IsNullOrWhiteSpace(profileResponse.Data.AvatarUrl))
                        {
                            HttpContext.Session.SetString("AvatarUrl", profileResponse.Data.AvatarUrl);
                        }
                        if (profileResponse.Data.LecturerId.HasValue)
                        {
                            HttpContext.Session.SetInt32("LecturerId", profileResponse.Data.LecturerId.Value);
                        }
                        if (profileResponse.Data.StudentId.HasValue)
                        {
                            HttpContext.Session.SetInt32("StudentId", profileResponse.Data.StudentId.Value);
                        }
                    }
                    else
                    {
                        HttpContext.Session.SetString("Role", response.Data.Role);
                        HttpContext.Session.SetString("FullName", response.Data.FullName);
                        HttpContext.Session.SetString("Username", response.Data.Username);
                        HttpContext.Session.SetString("Email", response.Data.Email);
                        if (response.Data.LecturerId.HasValue)
                        {
                            HttpContext.Session.SetInt32("LecturerId", response.Data.LecturerId.Value);
                        }
                        if (response.Data.StudentId.HasValue)
                        {
                            HttpContext.Session.SetInt32("StudentId", response.Data.StudentId.Value);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during QuickLogin for role {Role}", role);
                TempData["ErrorMessage"] = "Có lỗi xảy ra";
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
                var userName = HttpContext.Session.GetString("Username");

                // Clear JWT token
                _apiService.ClearAuthToken();

                // Clear session
                HttpContext.Session.Clear();

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

    /// <summary>
    /// Login response from Backend API
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int? LecturerId { get; set; }
        public int? StudentId { get; set; }
    }

    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
        public int? LecturerId { get; set; }
        public int? StudentId { get; set; }
    }
}
