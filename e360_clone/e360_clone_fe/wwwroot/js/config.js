// Environment Configuration
const isHttps = window.location.protocol === 'https:';
const APP_CONFIG = {
    API_BASE_URL: isHttps ? 'https://localhost:7052/api' : 'http://localhost:5104/api',
    API_TIMEOUT: 10000,
    APP_NAME: 'E360 Clone',
    VERSION: '1.0.0'
};

// Expose for non-module scripts
window.APP_CONFIG = APP_CONFIG;
