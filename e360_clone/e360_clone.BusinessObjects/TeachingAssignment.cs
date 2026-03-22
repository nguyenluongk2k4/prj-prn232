using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects
{
    /// <summary>
    /// Lecturer teaching assignment per class + subject + term
    /// </summary>
    public class TeachingAssignment
    {
        public int Id { get; set; }

        [Required]
        public int LecturerId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        [StringLength(20)]
        public string Semester { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Ended

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
