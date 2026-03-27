namespace e360_clone.BusinessObjects.DTOs
{
    public class AttendanceRosterItemDto
    {
        public int AttendanceId { get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Violation { get; set; } = string.Empty;
        public bool StudentConfirmed { get; set; }
        public DateTime? StudentConfirmedAt { get; set; }
    }
}
