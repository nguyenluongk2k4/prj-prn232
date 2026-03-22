using System.ComponentModel.DataAnnotations;

namespace e360_clone_fe.Models.ViewModels
{
    public class SubjectFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Mã môn h?c")]
        [StringLength(20)]
        public string SubjectCode { get; set; } = string.Empty;

        [Display(Name = "Tên môn h?c")]
        [StringLength(200)]
        public string SubjectName { get; set; } = string.Empty;

        [Display(Name = "S? tín ch?")]
        public int Credits { get; set; }

        [Display(Name = "S? gi? lý thuy?t")]
        public int TheoryHours { get; set; }

        [Display(Name = "S? gi? th?c hành")]
        public int PracticeHours { get; set; }

        [Display(Name = "B? môn")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Lo?i môn")]
        [StringLength(50)]
        public string SubjectType { get; set; } = string.Empty;

        [Display(Name = "Tr?ng thái")]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }
}


