using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects
{
    /// <summary>
    /// Student attendance record for each session
    /// </summary>
    public class StudentAttendance
    {
        public int Id { get; set; }

        [Required]
        public int CourseSessionId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Present"; // Present, Absent, Late, Excused

        public DateTime? CheckInTime { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public CourseSession? Session { get; set; }
        public StudentSubject? StudentSubject { get; set; }
        public int? StudentSubjectId { get; set; }
    }
}
