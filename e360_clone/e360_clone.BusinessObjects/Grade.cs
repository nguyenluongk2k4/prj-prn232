namespace e360_clone.BusinessObjects
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public decimal? Score { get; set; }
        public string ScoreType { get; set; } = string.Empty; // Midterm, Final, Other
        public string LetterGrade { get; set; } = string.Empty; // A, B, C, D, F
        public string Notes { get; set; } = string.Empty;
        public int? EnteredBy { get; set; } // LecturerId
        public DateTime? EnteredAt { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Status { get; set; } = string.Empty; // Draft, Submitted, Approved, Published
    }
}
