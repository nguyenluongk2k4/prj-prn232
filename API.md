# API Specification (E360 Clone)

## Overview
- Base URL: `/api`
- Response wrapper: `ApiResponse<T>` (fields: `success`, `message`, `data`).
- Paged response: `PagedResponse<T>` adds `pageNumber`, `pageSize`, `totalRecords`, `totalPages`.
- Common query for list endpoints: `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`.
- Auth: JWT Bearer for protected endpoints. `POST /api/auth/login` returns token.

## Existing APIs (Implemented)
1. `POST /api/auth/login`
- Đăng nhập bằng email hoặc username + password, trả JWT và thông tin tài khoản.
- Body: `email`, `password`, `rememberMe`.

2. `POST /api/auth/quick-login`
- Login demo theo role, không kiểm DB (dùng cho demo/seed).
- Body: `role`.

3. `GET /api/students`
- Lấy danh sách sinh viên có phân trang + tìm kiếm theo `fullName` hoặc `studentCode`.

4. `GET /api/students/{id}`
- Lấy chi tiết 1 sinh viên theo ID.

5. `POST /api/students`
- Tạo sinh viên mới.

6. `PUT /api/students/{id}`
- Cập nhật thông tin sinh viên.

7. `DELETE /api/students/{id}`
- Xóa sinh viên.

8. `GET /WeatherForecast`
- Endpoint mẫu của template .NET, không dùng trong nghiệp vụ.

## Required APIs (To Build)

### Accounts & Auth
1. `POST /api/accounts`
- Tạo tài khoản hệ thống (Admin/SuperAdmin/AcademicStaff/Teacher/Student).

2. `GET /api/accounts`
- Danh sách tài khoản, lọc theo role/status, hỗ trợ paging.

3. `GET /api/accounts/{id}`
- Lấy thông tin tài khoản.

4. `PUT /api/accounts/{id}`
- Cập nhật thông tin account (profile, role, trạng thái).

5. `POST /api/accounts/{id}/lock`
- Khóa tài khoản (status = Locked).

6. `POST /api/accounts/{id}/unlock`
- Mở khóa tài khoản.

7. `POST /api/accounts/{id}/reset-password`
- Reset mật khẩu về mặc định hoặc random.

### Lecturers
1. `GET /api/lecturers`
- Danh sách giảng viên, paging + search.

2. `GET /api/lecturers/{id}`
- Chi tiết giảng viên.

3. `POST /api/lecturers`
- Thêm giảng viên.

4. `PUT /api/lecturers/{id}`
- Cập nhật giảng viên.

5. `DELETE /api/lecturers/{id}`
- Xóa giảng viên.

### Subjects
1. `GET /api/subjects`
- Danh sách môn học, paging + search.

2. `GET /api/subjects/{id}`
- Chi tiết môn học.

3. `POST /api/subjects`
- Thêm môn học.

4. `PUT /api/subjects/{id}`
- Cập nhật môn học.

5. `DELETE /api/subjects/{id}`
- Xóa môn học.

### Classes
1. `GET /api/classes`
- Danh sách lớp, paging + search.

2. `GET /api/classes/{id}`
- Chi tiết lớp.

3. `POST /api/classes`
- Tạo lớp mới.

4. `PUT /api/classes/{id}`
- Cập nhật lớp.

5. `DELETE /api/classes/{id}`
- Xóa lớp.

### Exam Rooms
1. `GET /api/exam-rooms`
- Danh sách phòng thi, filter theo `building`, `hasComputer`, `status`.

2. `GET /api/exam-rooms/{id}`
- Chi tiết phòng thi.

3. `POST /api/exam-rooms`
- Thêm phòng thi.

4. `PUT /api/exam-rooms/{id}`
- Cập nhật phòng thi.

5. `DELETE /api/exam-rooms/{id}`
- Xóa phòng thi.

### Exams
1. `GET /api/exams`
- Danh sách kỳ thi theo lớp/môn/năm học/kỳ.

2. `GET /api/exams/{id}`
- Chi tiết kỳ thi.

3. `POST /api/exams`
- Tạo kỳ thi (môn, lớp, ngày, phòng, thời gian, trạng thái).

4. `PUT /api/exams/{id}`
- Cập nhật thông tin kỳ thi.

5. `DELETE /api/exams/{id}`
- Xóa kỳ thi.

### Exam Schedules
1. `GET /api/exam-schedules`
- Danh sách lịch thi, filter theo ngày/phòng/kỳ thi.

2. `GET /api/exam-schedules/{id}`
- Chi tiết lịch thi.

3. `POST /api/exam-schedules`
- Tạo lịch thi cụ thể (ca thi, phòng, thời gian).

4. `PUT /api/exam-schedules/{id}`
- Cập nhật lịch thi.

5. `POST /api/exam-schedules/{id}/check-conflicts`
- Kiểm tra xung đột lịch/phòng/giờ.

6. `DELETE /api/exam-schedules/{id}`
- Hủy lịch thi.

### Proctor Assignments
1. `GET /api/proctor-assignments`
- Danh sách phân công coi thi theo ca thi/giảng viên.

2. `POST /api/proctor-assignments`
- Phân công giám thị cho ca thi.

3. `PUT /api/proctor-assignments/{id}`
- Cập nhật phân công.

4. `DELETE /api/proctor-assignments/{id}`
- Hủy phân công.

### Attendances
1. `GET /api/attendances`
- Danh sách điểm danh theo ca thi/lớp.

2. `POST /api/attendances`
- Ghi nhận điểm danh cho sinh viên.

3. `PUT /api/attendances/{id}`
- Cập nhật trạng thái điểm danh/ghi chú/vi phạm.

### Grades
1. `GET /api/grades`
- Danh sách điểm theo sinh viên/kỳ thi/môn.

2. `POST /api/grades`
- Nhập điểm cho sinh viên.

3. `PUT /api/grades/{id}`
- Cập nhật điểm (trước khi duyệt).

4. `POST /api/grades/{id}/approve`
- Duyệt điểm.

### Reports & Statistics
1. `GET /api/reports/exam-results`
- Thống kê kết quả thi theo môn/lớp/kỳ.

2. `GET /api/reports/attendance`
- Báo cáo điểm danh theo ca thi/môn.

3. `GET /api/reports/proctoring`
- Báo cáo phân công coi thi.

4. `GET /api/reports/export`
- Xuất báo cáo (Excel/PDF) theo tham số.

## Notes
- Tất cả endpoints quản trị dữ liệu yêu cầu JWT và phân quyền theo role.
- Các thao tác thay đổi dữ liệu cần audit fields (`createdAt`, `updatedAt`, `status`).
