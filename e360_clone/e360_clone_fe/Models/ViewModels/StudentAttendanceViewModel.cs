namespace e360_clone_fe.Models.ViewModels
{
    public class StudentAttendanceItemViewModel
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

    public class StudentAttendancePageViewModel
    {
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public List<StudentAttendanceItemViewModel> Items { get; set; } = new();
    }
}
