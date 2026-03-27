namespace e360_clone.BusinessObjects.DTOs.Auth
{
    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
        public int? LecturerId { get; set; }
        public int? StudentId { get; set; }
    }
}
