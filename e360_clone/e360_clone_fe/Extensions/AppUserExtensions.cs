using Microsoft.AspNetCore.Http;
using System.Text.Json;
using e360_clone.BusinessObjects;

namespace e360_clone_fe.Extensions
{
    /// <summary>
    /// Extension methods for AppUser session management
    /// Stores user data in session instead of claims
    /// </summary>
    public static class AppUserExtensions
    {
        private const string UserSessionKey = "CurrentUser";

        /// <summary>
        /// Save user to session
        /// </summary>
        public static void SetUser(this ISession session, AppUser user)
        {
            session.SetString(UserSessionKey, JsonSerializer.Serialize(user));
        }

        /// <summary>
        /// Get user from session
        /// </summary>
        public static AppUser? GetUser(this ISession session)
        {
            var userJson = session.GetString(UserSessionKey);
            if (string.IsNullOrEmpty(userJson))
                return null;

            return JsonSerializer.Deserialize<AppUser>(userJson);
        }

        /// <summary>
        /// Check if user is logged in
        /// </summary>
        public static bool IsLoggedIn(this ISession session)
        {
            return session.GetUser() != null;
        }

        /// <summary>
        /// Clear user from session (logout)
        /// </summary>
        public static void Logout(this ISession session)
        {
            session.Remove(UserSessionKey);
        }

        /// <summary>
        /// Get user from HttpContext session
        /// </summary>
        public static AppUser? GetCurrentUser(this HttpContext context)
        {
            return context.Session?.GetUser();
        }

        /// <summary>
        /// Check if current user has role
        /// </summary>
        public static bool HasRole(this HttpContext context, params string[] roles)
        {
            var user = context.GetCurrentUser();
            if (user == null) return false;

            return roles.Contains(user.Role);
        }

        /// <summary>
        /// Require user to be logged in
        /// </summary>
        public static bool RequireAuth(this HttpContext context)
        {
            return context.Session?.IsLoggedIn() ?? false;
        }

        /// <summary>
        /// Require user to have specific role
        /// </summary>
        public static bool RequireRole(this HttpContext context, params string[] roles)
        {
            var user = context.GetCurrentUser();
            if (user == null) return false;

            return roles.Contains(user.Role);
        }
    }
}
