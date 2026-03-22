using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects
{
    /// <summary>
    /// Exam form (hình thức thi)
    /// Supports multiple exam forms per exam (MCQ, Essay, Practical, etc.)
    /// </summary>
    public class ExamForm
    {
        public int Id { get; set; }

        [Required]
        public int ExamId { get; set; }

        [StringLength(50)]
        public string FormType { get; set; } = string.Empty; // MCQ, Essay, Practical, Oral, Project

        public int Duration { get; set; } // minutes

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Exam? Exam { get; set; }
    }
}
