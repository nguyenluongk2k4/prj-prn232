using System.ComponentModel.DataAnnotations;

namespace e360_clone_fe.Models.ViewModels
{
    public class AttendanceRosterItemViewModel
    {
        public int AttendanceId { get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Violation { get; set; } = string.Empty;
        public bool StudentConfirmed { get; set; }
        public DateTime? StudentConfirmedAt { get; set; }
    }

    public class AttendanceFilterViewModel
    {
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public int? SubjectId { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? ExamId { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class AttendancePageViewModel
    {
        public AttendanceFilterViewModel Filter { get; set; } = new();
        public List<SubjectFormViewModel> Subjects { get; set; } = new();
        public List<ExamScheduleSlotViewModel> Slots { get; set; } = new();
        public List<ExamOptionViewModel> Exams { get; set; } = new();
        public List<AttendanceRosterItemViewModel> Roster { get; set; } = new();
    }

    public class AttendanceReportItemViewModel
    {
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int Excused { get; set; }
        public int Confirmed { get; set; }
    }

    public class AttendanceReportFilterViewModel
    {
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-7);
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int? SubjectId { get; set; }
        public int? ClassId { get; set; }
    }

    public class AttendanceReportPageViewModel
    {
        public AttendanceReportFilterViewModel Filter { get; set; } = new();
        public List<SubjectFormViewModel> Subjects { get; set; } = new();
        public List<ClassFormViewModel> Classes { get; set; } = new();
        public List<AttendanceReportItemViewModel> Items { get; set; } = new();
        public int Total { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int Excused { get; set; }
        public int Confirmed { get; set; }
    }
}
