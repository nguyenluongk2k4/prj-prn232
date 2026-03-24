namespace e360_clone_fe.Models.ViewModels
{
    public class StudentCourseItemDto
    {
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public int Semester { get; set; }
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
