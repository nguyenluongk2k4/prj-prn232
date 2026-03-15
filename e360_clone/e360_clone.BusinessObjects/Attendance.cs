namespace e360_clone.BusinessObjects
{
    public class Attendance
    {
        public int Id { get; set; }
        public int ExamScheduleId { get; set; }
        public int StudentId { get; set; }
        public string Status { get; set; } = string.Empty; // Present, Absent, Late, Excused
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Violation { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}
