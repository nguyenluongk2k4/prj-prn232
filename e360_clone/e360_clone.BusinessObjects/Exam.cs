namespace e360_clone.BusinessObjects
{
    public class Exam
    {
        public int Id { get; set; }
        public string ExamCode { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty;   // ClassExam, GraduationExam
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Duration { get; set; }                       // phút
        public int RoomId { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;     // Planned, InProgress, Completed, Cancelled
        public string Notes { get; set; } = string.Empty;
        public int CreatedBy { get; set; }                     // AccountId của admin tạo lịch
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
