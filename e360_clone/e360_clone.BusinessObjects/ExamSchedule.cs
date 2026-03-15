namespace e360_clone.BusinessObjects
{
    public class ExamSchedule
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int RoomId { get; set; }
        public List<int> ProctorIds { get; set; } = new();
        public string Status { get; set; } = string.Empty; // Scheduled, Completed, Cancelled
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
