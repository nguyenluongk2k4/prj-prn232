namespace e360_clone.BusinessObjects
{
    public class GradeEntry
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int Semester { get; set; }
        public decimal? Score { get; set; }
        public string ScoreType { get; set; } = string.Empty;
        public string LetterGrade { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int? EnteredBy { get; set; }
        public DateTime? EnteredAt { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
