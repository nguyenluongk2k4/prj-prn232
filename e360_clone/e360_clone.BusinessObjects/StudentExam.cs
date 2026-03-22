using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects
{
    /// <summary>
    /// Student-Exam registration
    /// Links students to exams they are registered for
    /// </summary>
    public class StudentExam
    {
        public int Id { get; set; }

        [Required]
        public int ExamId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Registered"; // Registered, Attended, Absent, Deferred

        public decimal? Grade { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime? SubmittedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Exam? Exam { get; set; }
        public Student? Student { get; set; }
    }
}
