namespace e360_clone.BusinessObjects.DTOs
{
    public class StudentExamScheduleDto
    {
        public int Id { get; set; }
        public string ExamCode { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public int RoomId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Duration { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
    }
}
