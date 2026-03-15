using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e360_clone.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert sample Students
            migrationBuilder.Sql(@"
                INSERT INTO ""Students"" (""StudentCode"", ""FullName"", ""DateOfBirth"", ""Gender"", ""Email"", ""PhoneNumber"", ""Address"", ""ClassId"", ""Status"", ""CreatedAt"")
                VALUES 
                ('SV001', N'Nguyễn Văn An', '2000-01-15', N'Nam', 'sv001@e360.edu.vn', '0901234567', N'Hà Nội', 1, 'Active', NOW()),
                ('SV002', N'Trần Thị Bình', '2000-03-20', N'Nữ', 'sv002@e360.edu.vn', '0901234568', N'TP.HCM', 1, 'Active', NOW()),
                ('SV003', N'Lê Văn Cường', '2000-05-10', N'Nam', 'sv003@e360.edu.vn', '0901234569', N'Đà Nẵng', 1, 'Active', NOW()),
                ('SV004', N'Phạm Thị Dung', '2000-07-22', N'Nữ', 'sv004@e360.edu.vn', '0901234570', N'Hải Phòng', 1, 'Active', NOW()),
                ('SV005', N'Hoàng Văn Em', '2000-09-05', N'Nam', 'sv005@e360.edu.vn', '0901234571', N'Cần Thơ', 1, 'Active', NOW()),
                ('SV006', N'Vũ Thị Giang', '2000-11-18', N'Nữ', 'sv006@e360.edu.vn', '0901234572', N'Hà Nội', 2, 'Active', NOW()),
                ('SV007', N'Đỗ Văn Hùng', '2000-02-28', N'Nam', 'sv007@e360.edu.vn', '0901234573', N'TP.HCM', 2, 'Active', NOW()),
                ('SV008', N'Bùi Thị Hương', '2000-04-12', N'Nữ', 'sv008@e360.edu.vn', '0901234574', N'Đà Nẵng', 2, 'Active', NOW()),
                ('SV009', N'Ngô Văn Khánh', '2000-06-30', N'Nam', 'sv009@e360.edu.vn', '0901234575', N'Hà Nội', 2, 'Active', NOW()),
                ('SV010', N'Đặng Thị Lan', '2000-08-25', N'Nữ', 'sv010@e360.edu.vn', '0901234576', N'TP.HCM', 2, 'Active', NOW()),
                ('SV011', N'Phan Văn Minh', '2000-10-08', N'Nam', 'sv011@e360.edu.vn', '0901234577', N'Hải Phòng', 3, 'Active', NOW()),
                ('SV012', N'Trương Thị Nga', '2000-12-15', N'Nữ', 'sv012@e360.edu.vn', '0901234578', N'Cần Thơ', 3, 'Active', NOW()),
                ('SV013', N'Nguyễn Văn Ocean', '2001-01-20', N'Nam', 'sv013@e360.edu.vn', '0901234579', N'Hà Nội', 3, 'Active', NOW()),
                ('SV014', N'Lê Thị Phương', '2001-03-05', N'Nữ', 'sv014@e360.edu.vn', '0901234580', N'TP.HCM', 3, 'Active', NOW()),
                ('SV015', N'Trần Văn Quân', '2001-05-18', N'Nam', 'sv015@e360.edu.vn', '0901234581', N'Đà Nẵng', 3, 'Active', NOW());
            ");

            // Insert sample Lecturers
            migrationBuilder.Sql(@"
                INSERT INTO ""Lecturers"" (""EmployeeCode"", ""FullName"", ""DateOfBirth"", ""Gender"", ""Email"", ""PhoneNumber"", ""Department"", ""Position"", ""Status"", ""CreatedAt"")
                VALUES 
                ('GV001', N'TS. Nguyễn Văn A', '1980-05-10', N'Nam', 'gv001@e360.edu.vn', '0901111222', N'Công nghệ thông tin', N'Giảng viên', 'Active', NOW()),
                ('GV002', N'TS. Trần Thị B', '1982-08-15', N'Nữ', 'gv002@e360.edu.vn', '0901111223', N'Công nghệ thông tin', N'Phó trưởng khoa', 'Active', NOW()),
                ('GV003', N'TS. Lê Văn C', '1978-03-20', N'Nam', 'gv003@e360.edu.vn', '0901111224', N'Toán - Lý', N'Giảng viên', 'Active', NOW()),
                ('GV004', N'TS. Phạm Thị D', '1985-12-01', N'Nữ', 'gv004@e360.edu.vn', '0901111225', N'Ngữ văn', N'Trưởng khoa', 'Active', NOW()),
                ('GV005', N'TS. Hoàng Văn E', '1975-07-25', N'Nam', 'gv005@e360.edu.vn', '0901111226', N'Ngoại ngữ', N'Giảng viên', 'Active', NOW());
            ");

            // Insert sample Subjects
            migrationBuilder.Sql(@"
                INSERT INTO ""Subjects"" (""SubjectCode"", ""SubjectName"", ""Credits"", ""TheoryHours"", ""PracticeHours"", ""Department"", ""SubjectType"", ""Status"", ""CreatedAt"")
                VALUES 
                ('CNTT001', N'Cơ sở dữ liệu', 3, 30, 30, N'Công nghệ thông tin', 'Core', 'Active', NOW()),
                ('CNTT002', N'Lập trình Web', 3, 30, 30, N'Công nghệ thông tin', 'Core', 'Active', NOW()),
                ('CNTT003', N'Nhập môn lập trình', 4, 45, 15, N'Công nghệ thông tin', 'Core', 'Active', NOW()),
                ('TOAN001', N'Toán cao cấp A1', 4, 60, 0, N'Toán - Lý', 'Core', 'Active', NOW()),
                ('VAN001', N'Văn học Việt Nam', 3, 45, 0, N'Ngữ văn', 'Elective', 'Active', NOW()),
                ('ANH001', N'Tiếng Anh cơ bản', 3, 45, 0, N'Ngoại ngữ', 'General', 'Active', NOW());
            ");

            // Insert sample Classes
            migrationBuilder.Sql(@"
                INSERT INTO ""Classes"" (""ClassCode"", ""ClassName"", ""MajorId"", ""CourseId"", ""AcademicYear"", ""Semester"", ""StudentCount"", ""Status"", ""CreatedAt"")
                VALUES 
                ('LopCNTT2020A', N'Lớp CNTT 2020A', 1, 1, '2020-2024', 1, 30, 'Active', NOW()),
                ('LopCNTT2020B', N'Lớp CNTT 2020B', 1, 1, '2020-2024', 1, 28, 'Active', NOW()),
                ('LopCNTT2020C', N'Lớp CNTT 2020C', 1, 1, '2020-2024', 1, 25, 'Active', NOW());
            ");

            // Insert sample ExamRooms
            migrationBuilder.Sql(@"
                INSERT INTO ""ExamRooms"" (""RoomCode"", ""RoomName"", ""Building"", ""Capacity"", ""Floor"", ""HasComputer"", ""HasProjector"", ""Status"", ""CreatedAt"")
                VALUES 
                ('P101', N'Phòng 101', N'Nhà A', 40, N'Tầng 1', false, false, 'Available', NOW()),
                ('P102', N'Phòng 102', N'Nhà A', 40, N'Tầng 1', false, false, 'Available', NOW()),
                ('P201', N'Phòng 201', N'Nhà A', 50, N'Tầng 2', false, true, 'Available', NOW()),
                ('P202', N'Phòng 202', N'Nhà A', 50, N'Tầng 2', false, true, 'Available', NOW()),
                ('LAB01', N'Phòng Lab 1', N'Nhà B', 30, N'Tầng 1', true, true, 'Available', NOW()),
                ('LAB02', N'Phòng Lab 2', N'Nhà B', 30, N'Tầng 1', true, true, 'Available', NOW());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"Students\" WHERE \"StudentCode\" LIKE 'SV%';");
            migrationBuilder.Sql("DELETE FROM \"Lecturers\" WHERE \"EmployeeCode\" LIKE 'GV%';");
            migrationBuilder.Sql("DELETE FROM \"Subjects\" WHERE \"SubjectCode\" LIKE '%%00%';");
            migrationBuilder.Sql("DELETE FROM \"Classes\" WHERE \"ClassCode\" LIKE 'Lop%';");
            migrationBuilder.Sql("DELETE FROM \"ExamRooms\" WHERE \"RoomCode\" LIKE 'P%' OR \"RoomCode\" LIKE 'LAB%';");
        }
    }
}
