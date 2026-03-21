/**
 * Auth Module - API Service
 * Authentication API calls
 * Usage: AuthApi.login(), AuthApi.quickLogin()
 */

const AuthApi = (function() {
    const ENDPOINTS = {
        LOGIN: '/auth/login',
        QUICK_LOGIN: '/auth/quick-login',
        LOGOUT: '/auth/logout',
        REFRESH: '/auth/refresh',
        ME: '/auth/me'
    };

    // Login with email and password
    async function login(email, password) {
        return Http.post(ENDPOINTS.LOGIN, { email, password });
    }

    // Quick login by role (for demo)
    async function quickLogin(role) {
        return Http.post(ENDPOINTS.QUICK_LOGIN, { role });
    }

    // Logout
    async function logout() {
        return Http.post(ENDPOINTS.LOGOUT, {});
    }

    // Refresh token
    async function refreshToken() {
        return Http.post(ENDPOINTS.REFRESH, {});
    }

    // Get current user info
    async function getCurrentUser() {
        return Http.get(ENDPOINTS.ME);
    }

    // Public API
    return {
        login,
        quickLogin,
        logout,
        refreshToken,
        getCurrentUser
    };
})();
