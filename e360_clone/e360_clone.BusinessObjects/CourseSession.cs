using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects
{
    /// <summary>
    /// Course session (buổi học)
    /// Represents each class session for a subject
    /// </summary>
    public class CourseSession
    {
        public int Id { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int ClassId { get; set; }

        public int SessionNumber { get; set; } // Buổi 1, 2, 3...

        [Required]
        public DateTime Date { get; set; }

        [StringLength(200)]
        public string Topic { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Completed"; // Completed, Cancelled, Makeup

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Subject? Subject { get; set; }
        public Class? Class { get; set; }
        public ICollection<StudentAttendance>? Attendances { get; set; }
    }
}
