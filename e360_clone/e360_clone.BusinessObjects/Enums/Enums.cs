namespace e360_clone.BusinessObjects.Enums
{
    /// <summary>
    /// Giới tính (Gender)
    /// </summary>
    public enum Gender
    {
        Nam = 0,
        Nu = 1,
        Khac = 2
    }

    /// <summary>
    /// Trạng thái sinh viên (Student Status)
    /// </summary>
    public enum StudentStatus
    {
        DangHoc = 0,        // Đang học
        TamNgung = 1,       // Tạm ngưng học
        TotNghiep = 2,      // Tốt nghiệp
        DinhChi = 3,        // Đình chỉ học
        BuocThoiHoc = 4     // Buộc thôi học
    }

    /// <summary>
    /// Trạng thái tài khoản (Account Status)
    /// </summary>
    public enum AccountStatus
    {
        HoatDong = 0,           // Hoạt động
        KhongHoatDong = 1,      // Không hoạt động
        BiKhoa = 2,             // Bị khóa
        DinhChi = 3             // Đình chỉ
    }

    /// <summary>
    /// Vai trò người dùng (User Role)
    /// </summary>
    public enum Role
    {
        SuperAdmin = 0,     // Quản trị cao cấp
        Admin = 1,          // Quản trị viên
        Student = 2,        // Sinh viên
        Teacher = 3,        // Giảng viên
        Parent = 4,         // Phụ huynh
        Librarian = 5,      // Thủ thư
        Staff = 6           // Nhân viên
    }

    /// <summary>
    /// Loại điểm (Grade Type)
    /// </summary>
    public enum GradeType
    {
        KiemTra15Phut = 0,  // Kiểm tra 15 phút
        KiemTra1Tiet = 1,   // Kiểm tra 1 tiết
        GiuaKy = 2,         // Giữa kỳ
        CuoiKy = 3,         // Cuối kỳ
        BaiTap = 4,         // Bài tập
        DoAn = 5,           // Đồ án
        ChuyenCan = 6,      // Chuyên cần
        Khac = 7            // Khác
    }

    /// <summary>
    /// Helper methods for Gender enum
    /// </summary>
    public static class GenderHelper
    {
        public static string GetText(string gender)
        {
            return gender switch
            {
                "Nam" => "Nam",
                "Nữ" => "Nữ",
                "Khác" => "Khác",
                _ => gender
            };
        }

        public static string GetText(Gender gender)
        {
            return gender switch
            {
                Gender.Nam => "Nam",
                Gender.Nu => "Nữ",
                Gender.Khac => "Khác",
                _ => gender.ToString()
            };
        }

        public static string GetIcon(string gender)
        {
            return gender switch
            {
                "Nam" => "ri-mars-line",
                "Nữ" => "ri-venus-line",
                "Khác" => "ri-question-line",
                _ => "ri-question-line"
            };
        }

        public static string GetBadgeClass(string gender)
        {
            return gender switch
            {
                "Nam" => "bg-primary",
                "Nữ" => "bg-pink",
                "Khác" => "bg-secondary",
                _ => "bg-secondary"
            };
        }

        public static bool IsValid(string value)
        {
            return value == "Nam" || value == "Nữ" || value == "Khác";
        }

        public static IEnumerable<EnumItem> GetAll()
        {
            return new List<EnumItem>
            {
                new EnumItem { Value = "Nam", Text = "Nam", Icon = "ri-mars-line" },
                new EnumItem { Value = "Nữ", Text = "Nữ", Icon = "ri-venus-line" },
                new EnumItem { Value = "Khác", Text = "Khác", Icon = "ri-question-line" }
            };
        }
    }

    /// <summary>
    /// Helper methods for StudentStatus enum
    /// </summary>
    public static class StudentStatusHelper
    {
        public static string GetText(StudentStatus status)
        {
            return status switch
            {
                StudentStatus.DangHoc => "Đang học",
                StudentStatus.TamNgung => "Tạm ngưng học",
                StudentStatus.TotNghiep => "Đã tốt nghiệp",
                StudentStatus.DinhChi => "Đình chỉ học",
                StudentStatus.BuocThoiHoc => "Buộc thôi học",
                _ => status.ToString()
            };
        }

        public static string GetBadgeClass(StudentStatus status)
        {
            return status switch
            {
                StudentStatus.DangHoc => "bg-success",
                StudentStatus.TamNgung => "bg-secondary",
                StudentStatus.TotNghiep => "bg-info",
                StudentStatus.DinhChi => "bg-warning",
                StudentStatus.BuocThoiHoc => "bg-danger",
                _ => "bg-primary"
            };
        }

        public static bool IsActive(StudentStatus status)
        {
            return status == StudentStatus.DangHoc;
        }

        public static bool IsStudying(StudentStatus status)
        {
            return status == StudentStatus.DangHoc || status == StudentStatus.TamNgung;
        }

        public static IEnumerable<EnumItem> GetAll()
        {
            return new List<EnumItem>
            {
                new EnumItem { Value = "Active", Text = "Đang học", Badge = "bg-success" },
                new EnumItem { Value = "Inactive", Text = "Tạm ngưng", Badge = "bg-secondary" },
                new EnumItem { Value = "Graduated", Text = "Đã tốt nghiệp", Badge = "bg-info" },
                new EnumItem { Value = "Suspended", Text = "Đình chỉ", Badge = "bg-warning" },
                new EnumItem { Value = "Dismissed", Text = "Buộc thôi học", Badge = "bg-danger" }
            };
        }
    }

    /// <summary>
    /// Helper methods for AccountStatus enum
    /// </summary>
    public static class AccountStatusHelper
    {
        public static string GetText(AccountStatus status)
        {
            return status switch
            {
                AccountStatus.HoatDong => "Hoạt động",
                AccountStatus.KhongHoatDong => "Không hoạt động",
                AccountStatus.BiKhoa => "Bị khóa",
                AccountStatus.DinhChi => "Đình chỉ",
                _ => status.ToString()
            };
        }

        public static string GetBadgeClass(AccountStatus status)
        {
            return status switch
            {
                AccountStatus.HoatDong => "bg-success",
                AccountStatus.KhongHoatDong => "bg-secondary",
                AccountStatus.BiKhoa => "bg-danger",
                AccountStatus.DinhChi => "bg-warning",
                _ => "bg-primary"
            };
        }

        public static bool IsActive(AccountStatus status)
        {
            return status == AccountStatus.HoatDong;
        }

        public static IEnumerable<EnumItem> GetAll()
        {
            return new List<EnumItem>
            {
                new EnumItem { Value = "Active", Text = "Hoạt động", Badge = "bg-success" },
                new EnumItem { Value = "Inactive", Text = "Không hoạt động", Badge = "bg-secondary" },
                new EnumItem { Value = "Locked", Text = "Bị khóa", Badge = "bg-danger" },
                new EnumItem { Value = "Suspended", Text = "Đình chỉ", Badge = "bg-warning" }
            };
        }
    }

    /// <summary>
    /// Helper methods for Role enum
    /// </summary>
    public static class RoleHelper
    {
        public static string GetText(Role role)
        {
            return role switch
            {
                Role.SuperAdmin => "Quản trị cao cấp",
                Role.Admin => "Quản trị viên",
                Role.Student => "Sinh viên",
                Role.Teacher => "Giảng viên",
                Role.Parent => "Phụ huynh",
                Role.Librarian => "Thủ thư",
                Role.Staff => "Nhân viên",
                _ => role.ToString()
            };
        }

        public static string GetIcon(Role role)
        {
            return role switch
            {
                Role.SuperAdmin => "ri-shield-star-line",
                Role.Admin => "ri-admin-line",
                Role.Student => "ri-graduation-cap-line",
                Role.Teacher => "ri-user-star-line",
                Role.Parent => "ri-user-follow-line",
                Role.Librarian => "ri-book-open-line",
                Role.Staff => "ri-user-line",
                _ => "ri-user-line"
            };
        }

        public static bool IsAdmin(Role role)
        {
            return role == Role.SuperAdmin || role == Role.Admin;
        }

        public static bool IsStudent(Role role)
        {
            return role == Role.Student;
        }

        public static bool IsTeacher(Role role)
        {
            return role == Role.Teacher;
        }

        public static IEnumerable<EnumItem> GetAll()
        {
            return new List<EnumItem>
            {
                new EnumItem { Value = "SuperAdmin", Text = "Quản trị cao cấp", Icon = "ri-shield-star-line" },
                new EnumItem { Value = "Admin", Text = "Quản trị viên", Icon = "ri-admin-line" },
                new EnumItem { Value = "Student", Text = "Sinh viên", Icon = "ri-graduation-cap-line" },
                new EnumItem { Value = "Teacher", Text = "Giảng viên", Icon = "ri-user-star-line" },
                new EnumItem { Value = "Parent", Text = "Phụ huynh", Icon = "ri-user-follow-line" },
                new EnumItem { Value = "Librarian", Text = "Thủ thư", Icon = "ri-book-open-line" },
                new EnumItem { Value = "Staff", Text = "Nhân viên", Icon = "ri-user-line" }
            };
        }
    }

    /// <summary>
    /// Helper methods for GradeType enum
    /// </summary>
    public static class GradeTypeHelper
    {
        public static string GetText(GradeType type)
        {
            return type switch
            {
                GradeType.KiemTra15Phut => "Kiểm tra 15 phút",
                GradeType.KiemTra1Tiet => "Kiểm tra 1 tiết",
                GradeType.GiuaKy => "Giữa kỳ",
                GradeType.CuoiKy => "Cuối kỳ",
                GradeType.BaiTap => "Bài tập",
                GradeType.DoAn => "Đồ án",
                GradeType.ChuyenCan => "Chuyên cần",
                GradeType.Khac => "Khác",
                _ => type.ToString()
            };
        }

        public static double GetWeight(GradeType type)
        {
            return type switch
            {
                GradeType.KiemTra15Phut => 0.1,
                GradeType.KiemTra1Tiet => 0.2,
                GradeType.GiuaKy => 0.3,
                GradeType.CuoiKy => 0.5,
                GradeType.BaiTap => 0.2,
                GradeType.DoAn => 0.4,
                GradeType.ChuyenCan => 0.1,
                GradeType.Khac => 0.1,
                _ => 0.1
            };
        }

        public static IEnumerable<EnumItem> GetAll()
        {
            return new List<EnumItem>
            {
                new EnumItem { Value = "KiemTra15Phut", Text = "Kiểm tra 15 phút" },
                new EnumItem { Value = "KiemTra1Tiet", Text = "Kiểm tra 1 tiết" },
                new EnumItem { Value = "GiuaKy", Text = "Giữa kỳ" },
                new EnumItem { Value = "CuoiKy", Text = "Cuối kỳ" },
                new EnumItem { Value = "BaiTap", Text = "Bài tập" },
                new EnumItem { Value = "DoAn", Text = "Đồ án" },
                new EnumItem { Value = "ChuyenCan", Text = "Chuyên cần" },
                new EnumItem { Value = "Khac", Text = "Khác" }
            };
        }
    }

    /// <summary>
    /// Generic enum item for UI binding
    /// </summary>
    public class EnumItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Badge { get; set; }
        public double? Weight { get; set; }
    }
}
