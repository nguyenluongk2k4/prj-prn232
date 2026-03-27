# API Specification (E360 Clone)

## Overview

- **Base URL**: `/api`
- **Response wrapper**: `ApiResponse<T>` — fields: `success`, `message`, `data`
- **Paged response**: `PagedResponse<T>` — thêm `pageNumber`, `pageSize`, `totalRecords`, `totalPages`
- **Common query params cho list endpoints**: `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`
- **Auth**: JWT Bearer. Token lấy từ `POST /api/auth/login`. Chỉ `GET /api/auth/me` có `[Authorize]` — các endpoint còn lại chưa enforce authorization ở controller level.

---

## Auth (`/api/auth`)

### `POST /api/auth/login`
Đăng nhập bằng email/username + password. Trả JWT token và thông tin tài khoản.
- **Body**: `email` (string), `password` (string)

### `POST /api/auth/register`
Đăng ký tài khoản sinh viên mới.
- **Body**: `email`, `password`, `fullName`, `studentId` (int)

### `POST /api/auth/quick-login`
Login demo theo role, không kiểm tra DB (chỉ dùng khi demo/seed).
- **Body**: `role` (string — "Admin", "Student", "Teacher", ...)

### `GET /api/auth/me` *(Authorize)*
Lấy thông tin tài khoản đang đăng nhập từ JWT claims.

---

## Students (`/api/students`)

### `GET /api/students`
Danh sách sinh viên có phân trang + tìm kiếm.
- **Query**: `pageNumber`, `pageSize`, `searchTerm` (theo `fullName` hoặc `studentCode`), `classId?` (int)

### `GET /api/students/{id}`
Chi tiết 1 sinh viên theo ID.

### `GET /api/students/by-subject`
Danh sách sinh viên theo môn học và lớp.
- **Query**: `pageNumber`, `pageSize`, `subjectId` (int, bắt buộc), `classId?` (int)

### `POST /api/students`
Tạo sinh viên mới.
- **Body**: `Student` object

### `PUT /api/students/{id}`
Cập nhật thông tin sinh viên.
- **Body**: `Student` object

### `DELETE /api/students/{id}`
Xóa sinh viên.

---

## Lecturers (`/api/lecturers`)

### `GET /api/lecturers`
Danh sách giảng viên có phân trang + tìm kiếm.
- **Query**: `pageNumber`, `pageSize`, `searchTerm`

### `GET /api/lecturers/{id}`
Chi tiết giảng viên theo ID.

### `POST /api/lecturers`
Thêm giảng viên mới.
- **Body**: `Lecturer` object

### `PUT /api/lecturers/{id}`
Cập nhật thông tin giảng viên.
- **Body**: `Lecturer` object

### `DELETE /api/lecturers/{id}`
Xóa giảng viên.

---

## Subjects (`/api/subjects`)

### `GET /api/subjects`
Danh sách môn học có phân trang + filter.
- **Query**: `pageNumber`, `pageSize`, `searchTerm`, `status?`, `subjectType?`, `department?`

### `GET /api/subjects/{id}`
Chi tiết môn học theo ID.

### `POST /api/subjects`
Thêm môn học mới.
- **Body**: `Subject` object

### `PUT /api/subjects/{id}`
Cập nhật môn học.
- **Body**: `Subject` object

### `DELETE /api/subjects/{id}`
Xóa môn học.

---

## Majors (`/api/majors`)

### `GET /api/majors`
Danh sách ngành học có phân trang.
- **Query**: `pageNumber`, `pageSize`

### `GET /api/majors/{id}`
Chi tiết ngành học theo ID.

---

## Classes (`/api/classes`)

### `GET /api/classes`
Danh sách lớp học có phân trang + filter.
- **Query**: `pageNumber`, `pageSize`, `searchTerm`, `majorCode?`, `cohort?`, `subjectId?`, `status?`

### `GET /api/classes/{id}`
Chi tiết lớp học theo ID.

### `POST /api/classes`
Tạo lớp học mới.
- **Body**: `Class` object

### `PUT /api/classes/{id}`
Cập nhật thông tin lớp học.
- **Body**: `Class` object

### `DELETE /api/classes/{id}`
Xóa lớp học.

---

## Rooms (`/api/rooms`)

### `GET /api/rooms`
Danh sách phòng thi có phân trang + tìm kiếm.
- **Query**: `pageNumber`, `pageSize`, `searchTerm`

### `GET /api/rooms/available`
Lấy danh sách phòng thi còn trống trong khung giờ chỉ định.
- **Query**: `examDate` (DateTime), `startTime` (TimeSpan), `endTime` (TimeSpan), `excludeExamId?` (int)

### `GET /api/rooms/{id}`
Chi tiết phòng thi theo ID.

### `POST /api/rooms`
Thêm phòng thi mới.
- **Body**: `ExamRoom` object

### `PUT /api/rooms/{id}`
Cập nhật thông tin phòng thi.
- **Body**: `ExamRoom` object

### `DELETE /api/rooms/{id}`
Xóa phòng thi.

---

## Exams (`/api/exams`)

### `GET /api/exams`
Danh sách kỳ thi có phân trang + lọc theo ngày.
- **Query**: `pageNumber`, `pageSize`, `searchTerm`, `fromDate?` (DateTime), `toDate?` (DateTime)

### `GET /api/exams/{id}`
Chi tiết kỳ thi theo ID.

### `GET /api/exams/student`
Lịch thi của sinh viên.
- **Query**: `studentId` (int), `email?` (string), `fromDate?` (DateTime), `toDate?` (DateTime)

### `POST /api/exams`
Tạo kỳ thi mới (môn, lớp, phòng, ngày, giờ, hình thức thi...).
- **Body**: `Exam` object

### `PUT /api/exams/{id}`
Cập nhật thông tin kỳ thi.
- **Body**: `Exam` object

### `DELETE /api/exams/{id}`
Xóa kỳ thi.

---

## Proctor Assignments (`/api/proctors`)

### `GET /api/proctors`
Danh sách phân công giám thị có phân trang.
- **Query**: `pageNumber`, `pageSize`

### `GET /api/proctors/{id}`
Chi tiết phân công giám thị theo ID.

### `POST /api/proctors`
Phân công giảng viên làm giám thị cho kỳ thi. Có kiểm tra xung đột lịch.
- **Body**: `ProctorAssignment` object (gồm `examId`, `lecturerId`, `proctorRole` — "ChiefProctor" hoặc "Proctor")

### `PUT /api/proctors/{id}`
Cập nhật phân công giám thị.
- **Body**: `ProctorAssignment` object

### `DELETE /api/proctors/{id}`
Hủy phân công giám thị.

---

## Teaching Assignments (`/api/teaching-assignments`)

### `GET /api/teaching-assignments`
Danh sách phân công giảng dạy có phân trang + lọc theo giảng viên.
- **Query**: `pageNumber`, `pageSize`, `lecturerId?` (int)

### `GET /api/teaching-assignments/{id}`
Chi tiết phân công giảng dạy theo ID.

### `POST /api/teaching-assignments`
Tạo phân công giảng dạy mới.
- **Body**: `TeachingAssignment` object

### `PUT /api/teaching-assignments/{id}`
Cập nhật phân công giảng dạy.
- **Body**: `TeachingAssignment` object

### `DELETE /api/teaching-assignments/{id}`
Xóa phân công giảng dạy.

---

## Student Subjects (`/api/student-subjects`)

### `GET /api/student-subjects`
Danh sách môn học của sinh viên.
- **Query**: `studentId` (int, bắt buộc)

---

## Attendances (`/api/attendances`)

### `GET /api/attendances`
Danh sách điểm danh có phân trang.
- **Query**: `pageNumber`, `pageSize`

### `GET /api/attendances/{id}`
Chi tiết bản ghi điểm danh theo ID.

### `GET /api/attendances/roster`
Danh sách điểm danh đầy đủ của 1 kỳ thi (attendance roster).
- **Query**: `examId` (int, bắt buộc)

### `GET /api/attendances/student`
Lịch sử điểm danh của sinh viên, có thể lọc theo ngày.
- **Query**: `studentId` (int, bắt buộc), `date?` (DateTime)

### `GET /api/attendances/report`
Báo cáo điểm danh theo khoảng thời gian/môn/lớp.
- **Query**: `fromDate?` (DateTime), `toDate?` (DateTime), `subjectId?` (int), `classId?` (int)

### `POST /api/attendances`
Ghi nhận điểm danh sinh viên tại kỳ thi.
- **Body**: `Attendance` object

### `PUT /api/attendances/{id}`
Cập nhật trạng thái điểm danh (status, ghi chú, vi phạm, xác nhận sinh viên).
- **Body**: `Attendance` object

### `DELETE /api/attendances/{id}`
Xóa bản ghi điểm danh.

---

## Dashboard (`/api/dashboard`)

### `GET /api/dashboard/summary`
Tổng quan thống kê cho dashboard admin.
- **Query**: `days?` (int, mặc định 14) — số ngày nhìn lại

---

## Enums tham chiếu

| Enum | Giá trị |
|------|---------|
| **Gender** | `Nam`, `Nữ`, `Khác` |
| **StudentStatus** | `Active` (Đang học), `Inactive` (Tạm ngưng), `Graduated` (Tốt nghiệp), `Suspended` (Đình chỉ), `Dismissed` (Buộc thôi học) |
| **AccountStatus** | `Active`, `Inactive`, `Locked`, `Suspended` |
| **Role** | `SuperAdmin`, `Admin`, `Student`, `Teacher`, `Parent`, `Librarian`, `Staff` |
| **GradeType** | `KiemTra15Phut`, `KiemTra1Tiet`, `GiuaKy`, `CuoiKy`, `BaiTap`, `DoAn`, `ChuyenCan`, `Khac` |
| **ProctorRole** | `ChiefProctor` (Giám thị quản lý), `Proctor` (Giám thị coi thi) |

---

## Notes

- `Account.Role` lưu dạng **string** (không phải int) — ví dụ `"Admin"`, `"Student"`, `"Teacher"`.
- JWT claims bao gồm: `ClaimTypes.Name` (username), `ClaimTypes.Email`, `ClaimTypes.Role`, `"FullName"`, `"UserId"`.
- Tất cả entity kế thừa `BaseEntity`: `Id` (int PK), `CreatedAt`, `UpdatedAt`.
- Password hash: SHA256 (demo). Tài khoản mặc định seed (password `123456`): `superadmin`, `admin`, `student`, `teacher`, `parent`, `librarian`.
