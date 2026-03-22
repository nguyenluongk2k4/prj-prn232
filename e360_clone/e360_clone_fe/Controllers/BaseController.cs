using Microsoft.AspNetCore.Mvc;
using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;

namespace e360_clone_fe.Controllers
{
    /// <summary>
    /// Base controller with common patterns for MVC controllers
    /// Uses JWT authentication (token stored in HttpOnly cookie)
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected readonly IApiService _apiService;
        protected readonly ILogger<BaseController> _logger;

        protected BaseController(IApiService apiService, ILogger<BaseController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        /// <summary>
        /// Check if user is authenticated (has valid JWT token)
        /// </summary>
        protected bool IsAuthenticated()
        {
            var token = _apiService.GetAuthToken();
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        /// Get current user role from TempData
        /// </summary>
        protected string? GetCurrentUserRole()
        {
            var role = HttpContext.Session.GetString("Role");
            return !string.IsNullOrEmpty(role) ? role : TempData["Role"]?.ToString();
        }

        /// <summary>
        /// Redirect to login if not authenticated
        /// </summary>
        protected IActionResult RequireAuth()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
            }
            return null!;
        }

        /// <summary>
        /// Check if user has required role
        /// </summary>
        protected bool HasRole(params string[] roles)
        {
            var userRole = GetCurrentUserRole();
            if (string.IsNullOrEmpty(userRole)) return false;
            return roles.Contains(userRole);
        }

        /// <summary>
        /// Require specific roles
        /// </summary>
        protected IActionResult RequireRole(params string[] roles)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (!HasRole(roles))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return null!;
        }

        /// <summary>
        /// Handle API response and show toast notification
        /// </summary>
        protected IActionResult HandleApiResponse<T>(ApiResponse<T> response, string successMessage = "", string? viewName = null, object? model = null)
        {
            if (response.Success)
            {
                if (!string.IsNullOrEmpty(successMessage))
                {
                    TempData["SuccessMessage"] = successMessage;
                }

                if (model != null && !string.IsNullOrEmpty(viewName))
                {
                    return View(viewName, model);
                }

                if (model != null)
                {
                    return View(model);
                }

                return View();
            }

            TempData["ErrorMessage"] = response.Message;
            return model != null ? View(model) : View();
        }

        /// <summary>
        /// Handle error response
        /// </summary>
        protected IActionResult HandleError(string message, string? viewName = null, object? model = null)
        {
            TempData["ErrorMessage"] = message;
            _logger.LogError(message);

            if (model != null && !string.IsNullOrEmpty(viewName))
            {
                return View(viewName, model);
            }

            return model != null ? View(model) : View();
        }

        /// <summary>
        /// Get current page number from query string
        /// </summary>
        protected int GetPageNumber(int defaultPage = 1)
        {
            var page = Request.Query["pageNumber"].FirstOrDefault();
            return int.TryParse(page, out var result) ? result : defaultPage;
        }

        /// <summary>
        /// Get page size from query string or settings
        /// </summary>
        protected int GetPageSize(int defaultSize = 10)
        {
            var pageSize = Request.Query["pageSize"].FirstOrDefault();
            return int.TryParse(pageSize, out var result) ? result : defaultSize;
        }

        /// <summary>
        /// Get search term from query string
        /// </summary>
        protected string? GetSearchTerm()
        {
            return Request.Query["searchTerm"].FirstOrDefault();
        }
    }
}
