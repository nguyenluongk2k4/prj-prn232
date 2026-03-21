/**
 * Core Authentication Helper
 * Global auth functions for JWT token management
 * Usage: Auth.login(), Auth.logout(), Auth.getUser(), etc.
 */

const Auth = (function() {
    const STORAGE_KEYS = {
        TOKEN: 'authToken',
        USER_INFO: 'userInfo',
        REDIRECT_URL: 'redirectAfterLogin'
    };

    // Save auth data
    function setAuth(token, userInfo) {
        localStorage.setItem(STORAGE_KEYS.TOKEN, token);
        localStorage.setItem(STORAGE_KEYS.USER_INFO, JSON.stringify(userInfo));
    }

    // Get token
    function getToken() {
        return localStorage.getItem(STORAGE_KEYS.TOKEN);
    }

    // Get user info
    function getUser() {
        const info = localStorage.getItem(STORAGE_KEYS.USER_INFO);
        return info ? JSON.parse(info) : null;
    }

    // Check if authenticated
    function isAuthenticated() {
        return !!getToken();
    }

    // Get user role
    function getRole() {
        const user = getUser();
        return user?.role || null;
    }

    // Check if user has role
    function hasRole(role) {
        const userRole = getRole();
        if (!role || !userRole) return false;
        
        // Handle array of roles
        if (Array.isArray(role)) {
            return role.includes(userRole);
        }
        return userRole === role;
    }

    // Check if user is admin
    function isAdmin() {
        return hasRole(['Admin', 'SuperAdmin']);
    }

    // Logout
    function logout(redirectUrl = '/Auth/Login') {
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        localStorage.removeItem(STORAGE_KEYS.USER_INFO);
        localStorage.removeItem(STORAGE_KEYS.REDIRECT_URL);
        window.location.href = redirectUrl;
    }

    // Clear auth data (for session expiry)
    function clear() {
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        localStorage.removeItem(STORAGE_KEYS.USER_INFO);
    }

    // Set redirect URL after login
    function setRedirectUrl(url) {
        if (url) {
            sessionStorage.setItem(STORAGE_KEYS.REDIRECT_URL, url);
        }
    }

    // Get and clear redirect URL
    function getRedirectUrl() {
        const url = sessionStorage.getItem(STORAGE_KEYS.REDIRECT_URL);
        sessionStorage.removeItem(STORAGE_KEYS.REDIRECT_URL);
        return url || '/';
    }

    // Redirect based on role
    function redirectByRole() {
        const role = getRole();
        const redirectUrl = getRedirectUrl();
        
        // If specific redirect URL set, use it
        if (redirectUrl && redirectUrl !== '/') {
            window.location.href = redirectUrl;
            return;
        }

        // Default redirects by role
        const roleRedirects = {
            'SuperAdmin': '/',
            'Admin': '/',
            'Student': '/Dashboard/Student',
            'Teacher': '/Dashboard/Teacher',
            'Parent': '/Dashboard/Parent',
            'Librarian': '/'
        };

        window.location.href = roleRedirects[role] || '/';
    }

    // Public API
    return {
        setAuth,
        getToken,
        getUser,
        isAuthenticated,
        getRole,
        hasRole,
        isAdmin,
        logout,
        clear,
        setRedirectUrl,
        getRedirectUrl,
        redirectByRole
    };
})();

// Auto-redirect if not authenticated (for protected pages)
function requireAuth() {
    if (!Auth.isAuthenticated()) {
        Auth.setRedirectUrl(window.location.pathname);
        window.location.href = '/Auth/Login';
        return false;
    }
    return true;
}
