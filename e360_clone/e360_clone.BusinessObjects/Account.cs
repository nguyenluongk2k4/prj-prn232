using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty; // SuperAdmin, Admin, Student, Teacher, Parent, Librarian

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive, Locked

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLoginAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Foreign keys - link to specific role tables (nullable)
        public int? StudentId { get; set; }
        public int? LecturerId { get; set; }

        // Navigation properties
        public Student? Student { get; set; }
        public Lecturer? Lecturer { get; set; }
    }
}
