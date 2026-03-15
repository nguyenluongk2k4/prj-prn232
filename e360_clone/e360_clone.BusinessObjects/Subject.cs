namespace e360_clone.BusinessObjects
{
    public class Subject
    {
        public int Id { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int TheoryHours { get; set; }
        public int PracticeHours { get; set; }
        public string Department { get; set; } = string.Empty;
        public string SubjectType { get; set; } = string.Empty; // Core, Elective, General
        public string Status { get; set; } = string.Empty; // Active, Inactive
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
