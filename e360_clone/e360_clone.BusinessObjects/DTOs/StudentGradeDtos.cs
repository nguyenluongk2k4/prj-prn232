namespace e360_clone.BusinessObjects.DTOs
{
    public class StudentGradeComponentDto
    {
        public string ScoreType { get; set; } = string.Empty;
        public string ScoreTypeText { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public string LetterGrade { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal Weight { get; set; }
    }

    public class StudentGradeItemDto
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
        public List<StudentGradeComponentDto> Components { get; set; } = new();
    }
}
