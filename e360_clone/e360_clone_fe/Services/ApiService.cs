using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace e360_clone_fe.Services
{
    /// <summary>
    /// API Settings configuration
    /// </summary>
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:5104/api";
        public string HttpsUrl { get; set; } = "https://localhost:7052/api";
        public int Timeout { get; set; } = 30;
    }

    /// <summary>
    /// Service for calling backend API with JWT authentication
    /// JWT token stored in HttpOnly cookie
    /// </summary>
    public interface IApiService
    {
        Task<ApiResponse<T>> GetAsync<T>(string endpoint);
        Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string> queryParams);
        Task<ApiResponse<T>> GetWithTokenAsync<T>(string endpoint, string token, Dictionary<string, string>? queryParams = null);
        Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
        Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
        Task<ApiResponse<T>> DeleteAsync<T>(string endpoint);
        Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data);
        Task<byte[]> DownloadAsync(string endpoint, Dictionary<string, string>? queryParams = null);
        Task<ApiResponse<T>> UploadAsync<T>(string endpoint, IFormFile file, Dictionary<string, string>? additionalData = null);
        void SetAuthToken(string token);
        string? GetAuthToken();
        void ClearAuthToken();
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<ApiService> _logger;

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
        /// Set JWT token in HttpOnly cookie
        /// </summary>
        public void SetAuthToken(string token)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var isHttps = context.Request.IsHttps;
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                };
                context.Response.Cookies.Append("jwt_token", token, cookieOptions);
            }
        }

        /// <summary>
        /// Get JWT token from cookie
        /// </summary>
        public string? GetAuthToken()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Cookies["jwt_token"];
        }

        /// <summary>
        /// Clear JWT token (logout)
        /// </summary>
        public void ClearAuthToken()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var isHttps = context.Request.IsHttps;
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(-1)
                };
                context.Response.Cookies.Append("jwt_token", "", cookieOptions);
            }
        }

        /// <summary>
        /// Configure HttpClient with JWT token
        /// </summary>
        private async Task ConfigureClientAsync()
        {
            var token = GetAuthToken();

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Handle API response
        /// </summary>
                private async Task<ApiResponse<T>> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var requestUri = response.RequestMessage?.RequestUri?.ToString() ?? "(unknown)";

            if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
            {
                var location = response.Headers.Location?.ToString() ?? "(no location)";
                _logger.LogWarning("API redirect {Status} from {RequestUri} to {Location}", response.StatusCode, requestUri, location);
                return ApiResponse<T>.ErrorResult("Loi API: " + response.StatusCode);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var isAuthEndpoint = requestUri.Contains("/api/Auth/login", StringComparison.OrdinalIgnoreCase)
                    || requestUri.Contains("/api/Auth/quick-login", StringComparison.OrdinalIgnoreCase)
                    || requestUri.Contains("/api/Auth/register", StringComparison.OrdinalIgnoreCase)
                    || requestUri.Contains("/api/Auth/me", StringComparison.OrdinalIgnoreCase);

                var context = _httpContextAccessor.HttpContext;
                if (!isAuthEndpoint)
                {
                    ClearAuthToken();
                }

                if (context != null)
                {
                    var onLoginPage = context.Request.Path.StartsWithSegments("/Auth/Login", StringComparison.OrdinalIgnoreCase);
                    if (!isAuthEndpoint && !onLoginPage)
                    {
                        context.Response.Redirect("/Auth/Login?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                    }
                }
                return ApiResponse<T>.ErrorResult("Phien dang nhap da het han. Vui long dang nhap lai.");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Empty API response from {RequestUri} with status {StatusCode}", requestUri, response.StatusCode);
                return response.IsSuccessStatusCode
                    ? ApiResponse<T>.SuccessResult(default!, "Thanh cong")
                    : ApiResponse<T>.ErrorResult("Loi API: " + response.StatusCode);
            }

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse != null)
                {
                    if (string.IsNullOrWhiteSpace(apiResponse.Message))
                    {
                        apiResponse.Message = response.IsSuccessStatusCode
                            ? "Thanh cong"
                            : $"Loi API: {(int)response.StatusCode}";
                    }
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
                var preview = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
                if (!response.IsSuccessStatusCode)
                {
                    return ApiResponse<T>.ErrorResult(string.Format("Loi API: {0} - {1}", (int)response.StatusCode, preview));
                }
            }

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var data = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return ApiResponse<T>.SuccessResult(data, "Thanh cong");
                }
                catch
                {
                    return ApiResponse<T>.SuccessResult(default!, "Thanh cong");
                }
            }

            var fallbackPreview = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
            return ApiResponse<T>.ErrorResult(string.Format("Loi API: {0} - {1}", (int)response.StatusCode, fallbackPreview));
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
                return ApiResponse<T>.ErrorResult($"Lá»—i káº¿t ná»‘i: {ex.Message}");
            }
        }

        public async Task<ApiResponse<T>> GetWithTokenAsync<T>(string endpoint, string token, Dictionary<string, string>? queryParams = null)
        {
            try
            {
                await ConfigureClientAsync();

                var url = queryParams != null && queryParams.Count > 0
                    ? $"{endpoint}?{new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result}"
                    : endpoint;

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET with token failed for {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Lá»—i káº¿t ná»‘i: {ex.Message}");
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
                return ApiResponse<T>.ErrorResult($"Lá»—i káº¿t ná»‘i: {ex.Message}");
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
                return ApiResponse<T>.ErrorResult($"Lá»—i káº¿t ná»‘i: {ex.Message}");
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
                return ApiResponse<T>.ErrorResult($"Lá»—i káº¿t ná»‘i: {ex.Message}");
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
                return ApiResponse<T>.ErrorResult($"Lá»—i káº¿t ná»‘i: {ex.Message}");
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
                
                using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                formData.Add(streamContent, "file", file.FileName);

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
                return ApiResponse<T>.ErrorResult($"Lá»—i káº¿t ná»‘i: {ex.Message}");
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

        public static ApiResponse<T> SuccessResult(T? data, string message = "ThÃ nh cÃ´ng")
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
            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
            
            services.AddHttpContextAccessor();
            
            services.AddHttpClient<IApiService, ApiService>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
                var baseUrl = (settings.BaseUrl ?? string.Empty).TrimEnd('/') + "/";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.Timeout);
            });

            return services;
        }
    }
}

