namespace e360_clone_fe.Models.ViewModels
{
    public class ClassDetailsViewModel
    {
        public ClassFormViewModel Class { get; set; } = new ClassFormViewModel();
        public string MajorName { get; set; } = string.Empty;
        public List<StudentViewModel> Students { get; set; } = new();
    }
}
