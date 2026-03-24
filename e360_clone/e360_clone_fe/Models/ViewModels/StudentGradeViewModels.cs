namespace e360_clone_fe.Models.ViewModels
{
    public class StudentGradeComponentViewModel
    {
        public string ScoreType { get; set; } = string.Empty;
        public string ScoreTypeText { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public string LetterGrade { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal Weight { get; set; }
    }

    public class StudentGradeItemViewModel
    {
        public int ExamId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int? ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<StudentGradeComponentViewModel> Components { get; set; } = new();
    }

    public class StudentGradeSubjectViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public List<StudentGradeItemViewModel> Items { get; set; } = new();
    }

    public class MyGradesPageViewModel
    {
        public List<StudentGradeSubjectViewModel> Subjects { get; set; } = new();
        public int? SelectedSubjectId { get; set; }
        public StudentGradeSubjectViewModel? SelectedSubject { get; set; }
    }
}
