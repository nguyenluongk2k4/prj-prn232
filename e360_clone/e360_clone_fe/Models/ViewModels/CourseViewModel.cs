namespace e360_clone_fe.Models.ViewModels
{
    public class MyCourseItemViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
    }

    public class MyCoursesPageViewModel
    {
        public string LecturerName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentClassCode { get; set; } = string.Empty;
        public int StudentClassId { get; set; }
        public List<MyCourseItemViewModel> Courses { get; set; } = new();
    }
}
