namespace e360_clone_fe.Models.ViewModels
{
    public class SubjectDetailsViewModel
    {
        public SubjectFormViewModel Subject { get; set; } = new SubjectFormViewModel();
        public List<ClassViewModel> AssignedClasses { get; set; } = new();
        public List<ClassViewModel> AvailableClasses { get; set; } = new();
        public List<LecturerViewModel> Lecturers { get; set; } = new();
        public List<MajorViewModel> Majors { get; set; } = new();
        public AddStudentToSubjectViewModel AddRequest { get; set; } = new AddStudentToSubjectViewModel();
        public AddTeachingAssignmentViewModel AddAssignment { get; set; } = new AddTeachingAssignmentViewModel();
        public ClassFormViewModel NewClass { get; set; } = new ClassFormViewModel();
    }

    public class AddStudentToSubjectViewModel
    {
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public List<int> StudentIds { get; set; } = new();
    }

    public class AddTeachingAssignmentViewModel
    {
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public int LecturerId { get; set; }
    }
}
