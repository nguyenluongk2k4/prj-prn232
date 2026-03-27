namespace e360_clone_fe.Models.ViewModels
{
    public class GradeScoreTypeOptionViewModel
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class GradeImportPageViewModel
    {
        public List<SubjectFormViewModel> Subjects { get; set; } = new();
        public List<GradeScoreTypeOptionViewModel> ScoreTypes { get; set; } = new();
        public int? SelectedExamId { get; set; }
        public int? SelectedSubjectId { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int? Semester { get; set; }
        public string ScoreType { get; set; } = "Final";
        public GradeImportResultViewModel? Result { get; set; }
    }
}
