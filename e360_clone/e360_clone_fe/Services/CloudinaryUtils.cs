namespace e360_clone_fe.Services
{
    public class CloudinaryOptions
    {
        public string CloudName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://res.cloudinary.com";
        public string DefaultFolder { get; set; } = string.Empty;
    }

    public interface ICloudinaryUrlBuilder
    {
        string BuildImageUrl(string publicId, string? transformation = null, string? format = null);
    }

    public class CloudinaryUrlBuilder : ICloudinaryUrlBuilder
    {
        private readonly CloudinaryOptions _options;

        public CloudinaryUrlBuilder(Microsoft.Extensions.Options.IOptions<CloudinaryOptions> options)
        {
            _options = options.Value;
        }

        public string BuildImageUrl(string publicId, string? transformation = null, string? format = null)
        {
            if (string.IsNullOrWhiteSpace(_options.CloudName))
            {
                return publicId;
            }

            var folder = string.IsNullOrWhiteSpace(_options.DefaultFolder)
                ? string.Empty
                : _options.DefaultFolder.TrimEnd('/') + "/";
            var safePublicId = publicId.TrimStart('/');
            var transPart = string.IsNullOrWhiteSpace(transformation) ? string.Empty : $"{transformation.Trim('/')}/";
            var formatPart = string.IsNullOrWhiteSpace(format) ? string.Empty : $".{format.TrimStart('.')}";

            return $"{_options.BaseUrl.TrimEnd('/')}/{_options.CloudName}/image/upload/{transPart}{folder}{safePublicId}{formatPart}";
        }
    }
}
