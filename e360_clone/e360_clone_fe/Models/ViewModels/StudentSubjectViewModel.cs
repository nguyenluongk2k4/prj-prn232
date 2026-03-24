namespace e360_clone_fe.Models.ViewModels
{
    public class StudentSubjectViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int? ClassId { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int Semester { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
