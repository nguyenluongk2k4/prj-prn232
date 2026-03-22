namespace e360_clone.BusinessObjects
{
    public class Class
    {
        public int Id { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public int CourseId { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int Cohort { get; set; }
        public int CohortYear { get; set; }
        public int Semester { get; set; }
        public int StudentCount { get; set; }
        public string Status { get; set; } = string.Empty; // Active, Graduated, Disbanded
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Major? Major { get; set; }
    }
}
