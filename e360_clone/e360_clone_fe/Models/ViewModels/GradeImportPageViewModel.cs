namespace e360_clone_fe.Models.ViewModels
{
    public class GradeExamOptionViewModel
    {
        public int Id { get; set; }
        public string Display { get; set; } = string.Empty;
    }

    public class GradeImportPageViewModel
    {
        public List<GradeExamOptionViewModel> Exams { get; set; } = new();
        public int? SelectedExamId { get; set; }
        public string ScoreType { get; set; } = "Final";
        public GradeImportResultViewModel? Result { get; set; }
    }
}
