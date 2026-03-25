namespace e360_clone_fe.Models.ViewModels
{
    public class ExamScheduleItemViewModel
    {
        public int ExamId { get; set; }
        public int SubjectId { get; set; }
        public int RoomId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool HasAssignment { get; set; }
        public string AssignedLecturers { get; set; } = string.Empty;
    }

    public class ExamScheduleSlotViewModel
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<ExamScheduleItemViewModel> Items { get; set; } = new();
    }

    public class ExamSchedulePageViewModel
    {
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public List<ExamScheduleSlotViewModel> Slots { get; set; } = new();
        public List<ExamScheduleCalendarItemViewModel> CalendarItems { get; set; } = new();
        public List<SubjectFormViewModel> Subjects { get; set; } = new();
        public List<LecturerViewModel> Lecturers { get; set; } = new();
        public Dictionary<int, List<int>> BlockedLecturerIds { get; set; } = new();
        public int? SelectedSubjectId { get; set; }
        public TimeSpan? SelectedStartTime { get; set; }
        public TimeSpan? SelectedEndTime { get; set; }
    }

    public class ExamScheduleCalendarItemViewModel
    {
        public int ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
    }
}
