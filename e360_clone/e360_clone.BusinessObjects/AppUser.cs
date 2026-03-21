namespace e360_clone.BusinessObjects
{
    /// <summary>
    /// User model for authentication and session management
    /// Used instead of Claims for type-safe user data access
    /// </summary>
    public class AppUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime? LastLoginAt { get; set; }

        // Helper properties
        public bool IsAdmin => Role == "Admin" || Role == "SuperAdmin";
        public bool IsSuperAdmin => Role == "SuperAdmin";
        public bool IsStudent => Role == "Student";
        public bool IsTeacher => Role == "Teacher";
        public bool IsParent => Role == "Parent";
        public bool IsActive => Status == "Active";

        // Display helpers
        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : Username;
        public string RoleText => GetRoleText();

        private string GetRoleText()
        {
            return Role switch
            {
                "SuperAdmin" => "Quản trị cao cấp",
                "Admin" => "Quản trị viên",
                "Student" => "Sinh viên",
                "Teacher" => "Giảng viên",
                "Parent" => "Phụ huynh",
                "Librarian" => "Thủ thư",
                "Staff" => "Nhân viên",
                _ => Role
            };
        }
    }
}
