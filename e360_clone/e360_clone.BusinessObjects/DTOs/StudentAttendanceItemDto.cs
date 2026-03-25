namespace e360_clone.BusinessObjects.DTOs
{
    public class StudentAttendanceItemDto
    {
        public int AttendanceId { get; set; }
        public int ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool StudentConfirmed { get; set; }
        public DateTime? StudentConfirmedAt { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }
}
