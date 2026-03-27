namespace e360_clone_fe.Models.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; } = "Giảng viên";
        public string TeacherCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = "/assets/images/thumbs/teacher-profile.png";

        public int TotalAssignments { get; set; }
        public int UpcomingAssignments { get; set; }
        public int UnconfirmedAssignments { get; set; }
        public int TodayAssignments { get; set; }
        public int WeekAssignments { get; set; }
        public int ConfirmedRate { get; set; }

        public List<TeacherDashboardExamItemViewModel> UpcomingExams { get; set; } = new();
        public List<TeacherDashboardUnconfirmedItemViewModel> UnconfirmedExams { get; set; } = new();
    }

    public class TeacherDashboardExamItemViewModel
    {
        public int ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
    }

    public class TeacherDashboardUnconfirmedItemViewModel
    {
        public int ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public int UnconfirmedCount { get; set; }
        public int TotalCount { get; set; }
    }
}
