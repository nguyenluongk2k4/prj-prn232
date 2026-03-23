using System.ComponentModel.DataAnnotations;

namespace e360_clone_fe.Models.ViewModels
{
    public class ProctorAssignmentViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Ca thi")]
        public int ExamId { get; set; }

        [Display(Name = "Giảng viên")]
        public int LecturerId { get; set; }

        [Display(Name = "Vai trò")]
        [StringLength(20)]
        public string Role { get; set; } = "Proctor";

        [Display(Name = "Ngày phân công")]
        public DateTime AssignedAt { get; set; }

        [Display(Name = "Trạng thái")]
        [StringLength(20)]
        public string Status { get; set; } = "Assigned";

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; } = string.Empty;
    }

    public class ProctorAssignmentListItemViewModel
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
    }

    public class ExamOptionViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class ProctorAssignmentFormViewModel
    {
        public ProctorAssignmentViewModel Assignment { get; set; } = new();
        public List<ExamOptionViewModel> Exams { get; set; } = new();
        public List<LecturerViewModel> Lecturers { get; set; } = new();
    }
}
