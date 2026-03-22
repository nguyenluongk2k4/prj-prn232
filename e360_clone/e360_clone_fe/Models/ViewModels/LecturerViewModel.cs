using System.ComponentModel.DataAnnotations;

namespace e360_clone_fe.Models.ViewModels
{
    public class LecturerViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Mã giảng viên")]
        [StringLength(20)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Display(Name = "Họ và tên")]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Bộ môn")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Chức danh")]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }

    public class TeachingAssignmentViewModel
    {
        public int Id { get; set; }
        public int LecturerId { get; set; }
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class TeachingAssignmentItemViewModel
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class LecturerDetailsViewModel
    {
        public LecturerViewModel Lecturer { get; set; } = new LecturerViewModel();
        public List<TeachingAssignmentItemViewModel> TeachingAssignments { get; set; } = new();
    }
}
