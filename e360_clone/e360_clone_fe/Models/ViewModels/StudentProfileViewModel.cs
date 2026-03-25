namespace e360_clone_fe.Models.ViewModels
{
    public class StudentProfileViewModel
    {
        public StudentViewModel Student { get; set; } = new StudentViewModel();
        public ClassFormViewModel? Class { get; set; }
    }
}
