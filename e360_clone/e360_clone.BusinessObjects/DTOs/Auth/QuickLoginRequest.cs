using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects.DTOs.Auth
{
    public class QuickLoginRequest
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
