using System.ComponentModel.DataAnnotations;
using e360_clone.BusinessObjects.Enums;

namespace e360_clone_fe.Models.ViewModels
{
    /// <summary>
    /// ViewModel for Student display and editing
    /// </summary>
    public class StudentViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Mã sinh viên")]
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã sinh viên không quá 20 ký tự")]
        public string StudentCode { get; set; } = string.Empty;

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ không quá 200 ký tự")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Lớp")]
        public int ClassId { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Active";

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ClassViewModel? Class { get; set; }

        // Display helpers
        public string GenderText => GenderHelper.GetText(Gender);
        public string StatusText => GetStatusText();
        public string StatusBadgeClass => GetStatusBadgeClass();

        private string GetStatusText()
        {
            return Status switch
            {
                "Active" => "Đang học",
                "Inactive" => "Tạm nghỉ",
                "Graduated" => "Đã tốt nghiệp",
                _ => Status
            };
        }

        private string GetStatusBadgeClass()
        {
            return Status switch
            {
                "Active" => "bg-success",
                "Inactive" => "bg-warning",
                "Graduated" => "bg-info",
                _ => "bg-secondary"
            };
        }
    }

    /// <summary>
    /// Create Student ViewModel
    /// </summary>
    public class CreateStudentViewModel
    {
        [Display(Name = "Mã sinh viên")]
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã sinh viên không quá 20 ký tự")]
        public string StudentCode { get; set; } = string.Empty;

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-18);

        [Display(Name = "Giới tính")]
        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string Gender { get; set; } = "Nam";

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ không quá 200 ký tự")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Lớp")]
        [Required(ErrorMessage = "Lớp là bắt buộc")]
        public int ClassId { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Active";
    }

    /// <summary>
    /// Update Student ViewModel
    /// </summary>
    public class UpdateStudentViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Mã sinh viên")]
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã sinh viên không quá 20 ký tự")]
        public string StudentCode { get; set; } = string.Empty;

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ không quá 200 ký tự")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Lớp")]
        [Required(ErrorMessage = "Lớp là bắt buộc")]
        public int ClassId { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Active";
    }
}
