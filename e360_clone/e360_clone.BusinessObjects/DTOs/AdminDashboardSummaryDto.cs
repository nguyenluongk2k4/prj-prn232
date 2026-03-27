namespace e360_clone.BusinessObjects.DTOs
{
    public class DashboardChartPointDto
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class AdminDashboardExamItemDto
    {
        public int ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty;
    }

    public class AdminDashboardSummaryDto
    {
        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalExamRooms { get; set; }
        public int UpcomingExamCount { get; set; }
        public List<DashboardChartPointDto> StudentsByMajor { get; set; } = new();
        public List<DashboardChartPointDto> ClassesByCohort { get; set; } = new();
        public List<DashboardChartPointDto> ExamsByDate { get; set; } = new();
        public List<AdminDashboardExamItemDto> UpcomingExams { get; set; } = new();
    }
}
