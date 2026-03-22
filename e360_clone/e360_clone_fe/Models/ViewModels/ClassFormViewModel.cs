using System.ComponentModel.DataAnnotations;

namespace e360_clone_fe.Models.ViewModels
{
    public class MajorViewModel
    {
        public int Id { get; set; }
        public string MajorCode { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string MajorGroup { get; set; } = string.Empty;
    }

    public class ClassFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "MÃ£ lá»›p")]
        [StringLength(20)]
        public string ClassCode { get; set; } = string.Empty;

        [Display(Name = "TÃªn lá»›p")]
        [StringLength(100)]
        public string ClassName { get; set; } = string.Empty;

        [Display(Name = "NgÃ nh")]
        public int MajorId { get; set; }

        [Display(Name = "KhoÃ¡")]
        public int Cohort { get; set; }

        [Display(Name = "NÄƒm khoÃ¡")]
        public int CohortYear { get; set; }

        [Display(Name = "NÄƒm há»c")]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        [Display(Name = "KhÃ³a há»c")]
        public int CourseId { get; set; }

        [Display(Name = "Há»c ká»³")]
        public int Semester { get; set; }

        [Display(Name = "SÄ© sá»‘")]
        public int StudentCount { get; set; }

        [Display(Name = "Tráº¡ng thÃ¡i")]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }

    public class ClassFormPageViewModel
    {
        public ClassFormViewModel Class { get; set; } = new ClassFormViewModel();
        public List<MajorViewModel> Majors { get; set; } = new();
    }
}


