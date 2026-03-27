namespace e360_clone_fe.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public int UpcomingExamCount { get; set; }
        public int SubjectCount { get; set; }
        public List<StudentDashboardExamItemViewModel> UpcomingExams { get; set; } = new();
    }

    public class StudentDashboardExamItemViewModel
    {
        public int ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
    }
}
