using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects
{
    /// <summary>
    /// Student-Subject enrollment tracking
    /// Tracks which subjects a student is taking in each semester
    /// </summary>
    public class StudentSubject
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        public int? ClassId { get; set; }

        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        public int Semester { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Enrolled"; // Enrolled, Completed, Failed, Retake, Withdrawn

        // Attendance tracking
        public int TotalSessions { get; set; }
        public int PresentSessions { get; set; }
        public int AbsentSessions { get; set; }

        // Calculated property
        public decimal AttendanceRate => TotalSessions > 0 
            ? Math.Round((decimal)PresentSessions / TotalSessions * 100, 2) 
            : 0;

        // Exam eligibility (80% attendance required)
        public bool IsExamEligible => AttendanceRate >= 80;

        // Grade
        public decimal? Grade { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Student? Student { get; set; }
        public Subject? Subject { get; set; }
        public Class? Class { get; set; }
    }
}
