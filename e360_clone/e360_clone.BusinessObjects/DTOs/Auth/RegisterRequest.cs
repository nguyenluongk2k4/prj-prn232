using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public int? StudentId { get; set; }
    }
}
