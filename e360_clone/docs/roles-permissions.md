# Roles & Permissions Summary

Tài liệu này tổng hợp vai trò (roles) và quyền (permissions) được mô tả trong các tài liệu hiện có của dự án. Nội dung được suy luận từ mô tả trách nhiệm, workflow, và các yêu cầu "User có quyền ..." trong module docs.

## Nguồn tài liệu đã dùng
- `docs/README.md`
- `docs/modules/01-student-management.md`
- `docs/modules/02-lecturer-management.md`
- `docs/modules/03-exam-management.md`
- `docs/modules/04-exam-schedule.md`
- `docs/modules/05-proctor-assignment.md`
- `docs/modules/06-grade-management.md`
- `docs/modules/07-attendance.md`
- `docs/workflows/exam-organization-workflow.md`
- `docs/workflows/grade-management-workflow.md`
- `docs/workflows/attendance-workflow.md`
- `QWEN.md`
- `CLAUDE.md`
- `DATABASE_AUTH_SETUP.md`

## Chuẩn hóa vai trò (mapping VN/EN)
- `Admin` = Quản trị hệ thống
- `Nhân viên giáo vụ` = `Staff` (Academic Staff)
- `Giảng viên` = `Teacher` / `Lecturer`
- `Sinh viên` = `Student`

## Danh sách vai trò và quyền tổng hợp

### Admin
- Quản trị hệ thống, quản lý người dùng, phân quyền (theo `docs/README.md`, `QWEN.md`, `CLAUDE.md`).
- Toàn quyền với module Quản lý sinh viên (CRUD) (`docs/modules/01-student-management.md`).
- Toàn quyền với module Quản lý giảng viên (CRUD) (`docs/modules/02-lecturer-management.md`).
- Quản lý môn thi và đề thi (CRUD môn thi; xóa đề thi) (`docs/modules/03-exam-management.md`).
- Quản lý phòng thi (CRUD phòng thi) (`docs/modules/04-exam-schedule.md`).
- Duyệt điểm, công bố điểm (`docs/modules/06-grade-management.md`, `docs/workflows/grade-management-workflow.md`).
- Tham gia giám sát toàn bộ quy trình tổ chức thi (`docs/workflows/exam-organization-workflow.md`).
- Nhận và xử lý hồ sơ/biên bản điểm danh cuối buổi (`docs/workflows/attendance-workflow.md`).

### Nhân viên giáo vụ (Staff)
- Lập lịch thi, phân công coi thi, quản lý điểm danh, báo cáo (theo `docs/README.md`, `QWEN.md`, `CLAUDE.md`).
- Quản lý sinh viên (CRUD) cùng Admin (`docs/modules/01-student-management.md`).
- Xem danh sách giảng viên để phân công (`docs/modules/02-lecturer-management.md`).
- Tạo môn thi, xem danh sách môn thi (`docs/modules/03-exam-management.md`).
- Tạo/cập nhật lịch thi, hủy lịch thi (`docs/modules/04-exam-schedule.md`).
- Tạo/cập nhật/xóa phân công giám thị, auto-assign giám thị (`docs/modules/05-proctor-assignment.md`).
- Duyệt điểm, công bố điểm (`docs/modules/06-grade-management.md`).
- Xem báo cáo điểm danh, xử lý vắng mặt (`docs/modules/07-attendance.md`).
- Thực hiện các bước tổ chức thi (kiểm tra điều kiện dự thi, lập kế hoạch, tạo lịch) (`docs/workflows/exam-organization-workflow.md`).

### Giảng viên (Teacher/Lecturer)
- Coi thi, điểm danh, nhập điểm (theo `docs/README.md`, `QWEN.md`, `CLAUDE.md`).
- Xem thông tin cá nhân (read-only) trong quản lý giảng viên (`docs/modules/02-lecturer-management.md`).
- Xem môn thi mình phụ trách (`docs/modules/03-exam-management.md`).
- Xem lịch thi môn mình phụ trách (`docs/modules/04-exam-schedule.md`).
- Xem phân công coi thi, xác nhận hoặc từ chối (`docs/modules/05-proctor-assignment.md`).
- Nhập/cập nhật điểm cho môn mình dạy, submit điểm (`docs/modules/06-grade-management.md`, `docs/workflows/grade-management-workflow.md`).
- Tham gia coi thi và chấm thi trong quy trình tổ chức thi (`docs/workflows/exam-organization-workflow.md`).

### Sinh viên (Student)
- Xem lịch thi, xem điểm, điểm danh/check-in (theo `docs/README.md`, `QWEN.md`, `CLAUDE.md`).
- Xem thông tin cá nhân (read-only) trong quản lý sinh viên (`docs/modules/01-student-management.md`).
- Xem môn thi cần tham dự (`docs/modules/03-exam-management.md`).
- Xem lịch thi cá nhân (`docs/modules/04-exam-schedule.md`).
- Xem điểm thi đã công bố, xem học bạ (`docs/modules/06-grade-management.md`).
- Xem lịch sử điểm danh, check-in bằng QR (`docs/modules/07-attendance.md`).
- Tham dự kỳ thi và xem kết quả (`docs/workflows/exam-organization-workflow.md`).
- Khiếu nại điểm (nếu có) theo workflow (`docs/workflows/grade-management-workflow.md`).

## Ghi chú về vai trò nghiệp vụ
- Module điểm danh và phân công coi thi có nhắc các vai trò nghiệp vụ như `Giám thị`, `Chief Proctor`, `Proctor`. Đây là vai trò trong nghiệp vụ (assignment), không phải vai trò đăng nhập hệ thống. Cần làm rõ nếu muốn map sang role hệ thống.

## Tóm tắt nhanh theo module

### Module 01: Student Management
- Admin/Staff: CRUD sinh viên
- Student: xem thông tin cá nhân

### Module 02: Lecturer Management
- Admin: CRUD giảng viên
- Staff: xem danh sách giảng viên để phân công
- Lecturer: xem thông tin cá nhân (read-only)

### Module 03: Exam Management
- Admin: CRUD môn thi; xóa đề thi
- Staff: tạo môn thi, tạo/cập nhật đề thi; xem danh sách
- Lecturer: xem môn thi phụ trách
- Student: xem môn thi cần tham dự

### Module 04: Exam Schedule
- Staff: tạo/cập nhật/hủy lịch thi
- Admin: CRUD phòng thi
- Lecturer: xem lịch thi môn phụ trách
- Student: xem lịch thi cá nhân

### Module 05: Proctor Assignment
- Staff: tạo/cập nhật/xóa phân công; auto-assign
- Lecturer: xem phân công; xác nhận/từ chối

### Module 06: Grade Management
- Lecturer: nhập/cập nhật/submit điểm
- Admin/Staff: duyệt và công bố điểm
- Student: xem điểm đã công bố; xem học bạ

### Module 07: Attendance
- Staff: xem báo cáo điểm danh, xử lý vắng mặt
- Student: xem lịch sử điểm danh, check-in
- Lecturer/Proctor: thực hiện điểm danh và cập nhật trạng thái điểm danh
