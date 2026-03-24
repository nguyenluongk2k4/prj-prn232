namespace e360_clone_fe.Models.ViewModels
{
    public class GradeImportResultViewModel
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
