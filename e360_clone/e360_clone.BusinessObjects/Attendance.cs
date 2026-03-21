namespace e360_clone.BusinessObjects
{
    public class Attendance
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }

        // Giám thị ghi nhận
        public string Status { get; set; } = string.Empty;    // Present, Absent, Late, Excused
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Violation { get; set; } = string.Empty; // None, Cheating, Disruptive, ...
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Sinh viên tự xác nhận sau buổi thi
        public bool StudentConfirmed { get; set; } = false;
        public DateTime? StudentConfirmedAt { get; set; }
    }
}
