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
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int? ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public int Semester { get; set; }
        public string TermLabel { get; set; } = string.Empty;
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

    public class TranscriptComponentViewModel
    {
        public string ScoreType { get; set; } = string.Empty;
        public string ScoreTypeText { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public string LetterGrade { get; set; } = string.Empty;
        public decimal Weight { get; set; }
    }

    public class TranscriptCourseViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public List<TranscriptComponentViewModel> Components { get; set; } = new();
    }

    public class TranscriptTermViewModel
    {
        public string AcademicYear { get; set; } = string.Empty;
        public int Semester { get; set; }
        public string TermLabel { get; set; } = string.Empty;
        public List<TranscriptCourseViewModel> Courses { get; set; } = new();
    }

    public class TranscriptPageViewModel
    {
        public List<TranscriptTermViewModel> Terms { get; set; } = new();
        public int? SelectedSemester { get; set; }
        public string? SelectedAcademicYear { get; set; }
        public TranscriptTermViewModel? SelectedTerm { get; set; }
    }
}
