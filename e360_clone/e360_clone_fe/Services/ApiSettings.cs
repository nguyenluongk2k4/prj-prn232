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

        /// <summary>
        /// Get the appropriate base URL based on current request scheme
        /// </summary>
        public string GetBaseUrl(bool preferHttps = false)
        {
            if (preferHttps)
                return HttpsUrl;
            
            return BaseUrl;
        }
    }
}
