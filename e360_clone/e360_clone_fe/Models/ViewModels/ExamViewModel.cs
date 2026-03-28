using System.ComponentModel.DataAnnotations;

namespace e360_clone_fe.Models.ViewModels
{
    public class ExamViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Mã kỳ thi")]
        [StringLength(20)]
        public string ExamCode { get; set; } = string.Empty;

        [Display(Name = "Tên kỳ thi")]
        [StringLength(200)]
        public string ExamName { get; set; } = string.Empty;

        [Display(Name = "Loại kỳ thi")]
        [StringLength(50)]
        public string ExamType { get; set; } = "Final";

        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public int RoomId { get; set; }

        [Display(Name = "Ngày thi")]
        public DateTime ExamDate { get; set; } = DateTime.Today;

        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan StartTime { get; set; } = new TimeSpan(7, 30, 0);

        [Display(Name = "Giờ kết thúc")]
        public TimeSpan EndTime { get; set; } = new TimeSpan(9, 30, 0);

        [Display(Name = "Thời lượng (phút)")]
        public int Duration { get; set; }

        [Display(Name = "Năm học")]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        [Display(Name = "Học kỳ")]
        [StringLength(20)]
        public string Semester { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        [StringLength(20)]
        public string Status { get; set; } = "Planned";

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; } = string.Empty;
    }

    public class SubjectViewModel
    {
        public int Id { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
    }

    public class ExamRoomViewModel
    {
        public int Id { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
    }

    public class ExamFormViewModel
    {
        public ExamViewModel Exam { get; set; } = new ExamViewModel();
        public List<SubjectViewModel> Subjects { get; set; } = new();
        public List<ClassViewModel> Classes { get; set; } = new();
        public List<ExamRoomViewModel> Rooms { get; set; } = new();
        public List<int> SelectedRoomIds { get; set; } = new();
        public bool ApplyAllClasses { get; set; } = true;
    }

    public class ExamCreateRequestViewModel
    {
        public ExamViewModel Exam { get; set; } = new ExamViewModel();
        public List<int> RoomIds { get; set; } = new();
        public bool ApplyAllClasses { get; set; }
    }
}
