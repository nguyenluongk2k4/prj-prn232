/**
 * Core HTTP Client
 * Base HTTP client with JWT token handling
 * Usage: Http.get(), Http.post(), Http.put(), Http.delete()
 */

const Http = (function() {
    const config = {
        baseUrl: APP_CONFIG?.API_BASE_URL || 'http://localhost:5104/api',
        timeout: APP_CONFIG?.API_TIMEOUT || 10000,
        headers: {
            'Content-Type': 'application/json'
        }
    };

    // Get auth token from localStorage
    function getToken() {
        return localStorage.getItem('authToken');
    }

    // Build headers with auth token
    function getHeaders(customHeaders = {}) {
        const token = getToken();
        return {
            ...config.headers,
            ...customHeaders,
            ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        };
    }

    // Handle response
    async function handleResponse(response) {
        // Handle 401 Unauthorized
        if (response.status === 401) {
            localStorage.removeItem('authToken');
            localStorage.removeItem('userInfo');
            if (window.location.pathname !== '/Auth/Login') {
                window.location.href = '/Auth/Login';
            }
            throw new Error('Unauthorized');
        }

        const result = await response.json();

        if (!response.ok) {
            throw new Error(result.message || `HTTP ${response.status}: ${response.statusText}`);
        }

        return result;
    }

    // Handle error
    function handleError(error) {
        console.error('HTTP Error:', error);
        if (error.name === 'AbortError') {
            throw new Error('Request timeout');
        }
        throw error;
    }

    // Main request method
    async function request(endpoint, options = {}) {
        const url = endpoint.startsWith('http') ? endpoint : `${config.baseUrl}${endpoint}`;
        
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), config.timeout);

        try {
            const response = await fetch(url, {
                ...options,
                headers: getHeaders(options.headers),
                signal: controller.signal
            });

            clearTimeout(timeoutId);
            return await handleResponse(response);
        } catch (error) {
            clearTimeout(timeoutId);
            handleError(error);
        }
    }

    // HTTP methods
    async function get(endpoint, params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = queryString ? `${endpoint}?${queryString}` : endpoint;
        return request(url, { method: 'GET' });
    }

    async function post(endpoint, data) {
        return request(endpoint, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    }

    async function put(endpoint, data) {
        return request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    }

    async function patch(endpoint, data) {
        return request(endpoint, {
            method: 'PATCH',
            body: JSON.stringify(data)
        });
    }

    async function deleteRequest(endpoint) {
        return request(endpoint, { method: 'DELETE' });
    }

    // Download file
    async function download(endpoint, params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = queryString ? `${endpoint}?${queryString}` : endpoint;
        
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), config.timeout);

        try {
            const response = await fetch(url, {
                headers: getHeaders(),
                signal: controller.signal
            });

            clearTimeout(timeoutId);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            return await response.blob();
        } catch (error) {
            clearTimeout(timeoutId);
            handleError(error);
        }
    }

    // Upload file
    async function upload(endpoint, formData) {
        const token = getToken();
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), config.timeout * 2); // Longer timeout for uploads

        try {
            const response = await fetch(`${config.baseUrl}${endpoint}`, {
                method: 'POST',
                headers: {
                    ...(token ? { 'Authorization': `Bearer ${token}` } : {})
                    // Don't set Content-Type - browser sets it with boundary
                },
                body: formData,
                signal: controller.signal
            });

            clearTimeout(timeoutId);
            return await handleResponse(response);
        } catch (error) {
            clearTimeout(timeoutId);
            handleError(error);
        }
    }

    // Public API
    return {
        get,
        post,
        put,
        patch,
        delete: deleteRequest,
        download,
        upload,
        request
    };
})();
