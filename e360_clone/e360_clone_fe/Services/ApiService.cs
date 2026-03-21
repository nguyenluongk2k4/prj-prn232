using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace e360_clone_fe.Services
{
    /// <summary>
    /// Service for calling backend API from MVC controllers
    /// Handles JWT token management, HTTP calls, and error handling
    /// </summary>
    public interface IApiService
    {
        Task<ApiResponse<T>> GetAsync<T>(string endpoint);
        Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string> queryParams);
        Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
        Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
        Task<ApiResponse<T>> DeleteAsync<T>(string endpoint);
        Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data);
        Task<byte[]> DownloadAsync(string endpoint, Dictionary<string, string>? queryParams = null);
        Task<ApiResponse<T>> UploadAsync<T>(string endpoint, IFormFile file, Dictionary<string, string>? additionalData = null);
        void SetAuthToken(string token);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<ApiService> _logger;
        private string? _authToken;

        public ApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ApiSettings> apiSettings,
            ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Set JWT token for API calls (called after login)
        /// </summary>
        public void SetAuthToken(string token)
        {
            _authToken = token;
        }

        /// <summary>
        /// Get JWT token from current user claims
        /// </summary>
        private string? GetTokenFromClaims()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Token might be stored in session or claims
            return _httpContextAccessor.HttpContext?.Session.GetString("authToken");
        }

        /// <summary>
        /// Configure HttpClient with auth token before each request
        /// </summary>
        private async Task ConfigureClientAsync()
        {
            var token = _authToken ?? GetTokenFromClaims();
            
            _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            // Handle 401 Unauthorized
            _httpClient.DefaultRequestHeaders.Add("X-Handle-401", "true");
        }

        /// <summary>
        /// Handle API response and check for errors
        /// </summary>
        private async Task<ApiResponse<T>> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Handle 401 Unauthorized - redirect to login
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Clear session and redirect
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Session.Clear();
                }

                return ApiResponse<T>.ErrorResult("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            }

            // Try to parse as API response
            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse != null)
                {
                    if (!apiResponse.Success)
                    {
                        _logger.LogWarning("API Error: {Message}", apiResponse.Message);
                    }
                    return apiResponse;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse API response");
            }

            // Fallback for non-standard responses
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var data = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return ApiResponse<T>.SuccessResult(data, "Thành công");
                }
                catch
                {
                    return ApiResponse<T>.SuccessResult(default!, "Thành công");
                }
            }

            return ApiResponse<T>.ErrorResult($"Lỗi API: {response.StatusCode}");
        }

        /// <summary>
        /// GET request
        /// </summary>
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            return await GetAsync<T>(endpoint, null);
        }

        /// <summary>
        /// GET request with query parameters
        /// </summary>
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams)
        {
            try
            {
                await ConfigureClientAsync();

                var url = queryParams != null && queryParams.Count > 0
                    ? $"{endpoint}?{new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result}"
                    : endpoint;

                var response = await _httpClient.GetAsync(url);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET request failed for {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Lỗi kết nối: {ex.Message}");
            }
        }

        /// <summary>
        /// POST request
        /// </summary>
        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                await ConfigureClientAsync();

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST request failed for {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Lỗi kết nối: {ex.Message}");
            }
        }

        /// <summary>
        /// PUT request
        /// </summary>
        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                await ConfigureClientAsync();

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PUT request failed for {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Lỗi kết nối: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE request
        /// </summary>
        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            try
            {
                await ConfigureClientAsync();

                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DELETE request failed for {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Lỗi kết nối: {ex.Message}");
            }
        }

        /// <summary>
        /// PATCH request
        /// </summary>
        public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data)
        {
            try
            {
                await ConfigureClientAsync();

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PATCH request failed for {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Lỗi kết nối: {ex.Message}");
            }
        }

        /// <summary>
        /// Download file from API
        /// </summary>
        public async Task<byte[]> DownloadAsync(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            try
            {
                await ConfigureClientAsync();

                var url = queryParams != null && queryParams.Count > 0
                    ? $"{endpoint}?{new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result}"
                    : endpoint;

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Download request failed for {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Upload file to API
        /// </summary>
        public async Task<ApiResponse<T>> UploadAsync<T>(string endpoint, IFormFile file, Dictionary<string, string>? additionalData = null)
        {
            try
            {
                await ConfigureClientAsync();

                using var formData = new MultipartFormDataContent();
                
                // Add file
                using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                formData.Add(streamContent, "file", file.FileName);

                // Add additional data
                if (additionalData != null)
                {
                    foreach (var kvp in additionalData)
                    {
                        formData.Add(new StringContent(kvp.Value), kvp.Key);
                    }
                }

                var response = await _httpClient.PostAsync(endpoint, formData);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload request failed for {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Lỗi kết nối: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => TotalRecords > 0 && PageSize > 0 
            ? (int)Math.Ceiling(TotalRecords / (double)PageSize) 
            : 0;

        public static ApiResponse<T> SuccessResult(T data, string message = "Thành công")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
        }
    }

    /// <summary>
    /// Extension method to register ApiService
    /// </summary>
    public static class ApiServiceExtensions
    {
        public static IServiceCollection AddApiService(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure ApiSettings
            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
            
            services.AddHttpContextAccessor();
            services.AddSession();
            
            services.AddHttpClient<IApiService, ApiService>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.Timeout);
            });

            return services;
        }
    }
}
