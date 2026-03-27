# Use Case Flow (E360 Clone)

Tài liệu mô tả luồng thao tác chính theo vai trò trong hệ thống quản lý lịch thi & coi thi.

## Roles

| Role | Mô tả |
|------|-------|
| **SuperAdmin** | Quản trị cao cấp — toàn quyền hệ thống |
| **Admin** | Quản trị viên — quản lý dữ liệu và cấu hình |
| **Staff** | Nhân viên giáo vụ — lập lịch thi, phân công giám thị |
| **Teacher** | Giảng viên — làm giám thị, điểm danh, nhập điểm |
| **Student** | Sinh viên — xem lịch thi, xem điểm, xác nhận điểm danh |
| **Parent** | Phụ huynh — xem thông tin học tập của con |
| **Librarian** | Thủ thư — quản lý thư viện (ngoài scope lịch thi) |

> **Lưu ý về giám thị:** Không có role riêng. Giảng viên được phân công vào ca thi với `ProctorRole`:
> - `ChiefProctor` — Giám thị quản lý: chịu trách nhiệm toàn bộ ca thi, xử lý vi phạm.
> - `Proctor` — Giám thị coi thi: ngồi trong phòng thi, điểm danh sinh viên.

---

## Admin / SuperAdmin

### Đăng nhập & tổng quan
1. Đăng nhập qua `POST /api/auth/login`.
2. Xem dashboard tổng quan: số kỳ thi sắp diễn ra, số sinh viên, thống kê điểm danh (`GET /api/dashboard/summary`).

### Quản lý dữ liệu danh mục
3. Quản lý sinh viên: xem danh sách, tìm kiếm, thêm/sửa/xóa (`/api/students`).
4. Quản lý giảng viên: xem danh sách, thêm/sửa/xóa (`/api/lecturers`).
5. Quản lý ngành học: xem danh sách, xem chi tiết (`/api/majors`).
6. Quản lý môn học: thêm/sửa/xóa, lọc theo loại/bộ môn (`/api/subjects`).
7. Quản lý lớp học: thêm/sửa/xóa, lọc theo ngành/khóa/môn (`/api/classes`).
8. Quản lý phòng thi: thêm/sửa/xóa (`/api/rooms`).

### Quản lý lịch thi & phân công
9. Xem toàn bộ lịch thi, lọc theo ngày (`GET /api/exams`).
10. Xem danh sách phân công giảng dạy (`GET /api/teaching-assignments`).
11. Xem phân công giám thị theo kỳ thi (`GET /api/proctors`).

### Báo cáo
12. Xem báo cáo điểm danh theo kỳ/môn/lớp (`GET /api/attendances/report`).

---

## Staff — Nhân viên giáo vụ

### Quản lý lịch thi
1. Xem danh sách kỳ thi hiện có, lọc theo ngày (`GET /api/exams`).
2. Kiểm tra phòng thi còn trống trong khung giờ (`GET /api/rooms/available`).
3. Tạo kỳ thi mới: chọn môn học, lớp, phòng thi, ngày giờ, hình thức thi (`POST /api/exams`).
4. Cập nhật thông tin kỳ thi khi có thay đổi (`PUT /api/exams/{id}`).
5. Xóa kỳ thi khi cần hủy (`DELETE /api/exams/{id}`).

### Phân công giám thị
6. Xem danh sách giảng viên để chọn phân công (`GET /api/lecturers`).
7. Phân công giảng viên làm giám thị: chỉ định `ChiefProctor` hoặc `Proctor` cho kỳ thi — hệ thống tự kiểm tra xung đột lịch (`POST /api/proctors`).
8. Cập nhật hoặc hủy phân công giám thị (`PUT /api/proctors/{id}`, `DELETE /api/proctors/{id}`).
9. Xem danh sách phân công theo kỳ thi (`GET /api/proctors`).

### Quản lý phân công giảng dạy
10. Tạo/cập nhật/xóa phân công giảng dạy (`/api/teaching-assignments`).

### Theo dõi điểm danh
11. Xem danh sách điểm danh theo kỳ thi (`GET /api/attendances/roster?examId=...`).
12. Xem báo cáo tổng hợp điểm danh theo khoảng thời gian, môn, lớp (`GET /api/attendances/report`).

---

## Teacher — Giảng viên

### Xem lịch được phân công
1. Đăng nhập hệ thống.
2. Xem danh sách kỳ thi mình được phân công làm giám thị (`GET /api/proctors?lecturerId=...` hoặc qua dashboard).
3. Xem chi tiết kỳ thi: phòng thi, giờ thi, danh sách sinh viên (`GET /api/exams/{id}`).

### Vai trò Giám thị quản lý (ChiefProctor)
4. Kiểm tra danh sách giám thị coi thi trong ca (`GET /api/proctors?examId=...`).
5. Quản lý toàn bộ ca thi, xử lý vi phạm sinh viên nếu có.
6. Xem danh sách điểm danh toàn ca (`GET /api/attendances/roster?examId=...`).

### Vai trò Giám thị coi thi (Proctor)
7. Lấy danh sách điểm danh (roster) của ca thi (`GET /api/attendances/roster?examId=...`).
8. Ghi nhận điểm danh từng sinh viên: `Present`, `Late`, `Absent`, `Excused` (`POST /api/attendances`).
9. Cập nhật bản ghi điểm danh nếu cần (thêm ghi chú vi phạm, chỉnh trạng thái) (`PUT /api/attendances/{id}`).

### Giảng dạy
10. Xem danh sách môn/lớp đang phụ trách (`GET /api/teaching-assignments?lecturerId=...`).
11. Xem danh sách sinh viên theo môn học mình dạy (`GET /api/students/by-subject?subjectId=...`).

---

## Student — Sinh viên

### Thông tin cá nhân
1. Đăng nhập hệ thống.
2. Xem thông tin cá nhân (`GET /api/auth/me`).
3. Xem danh sách môn học đang đăng ký (`GET /api/student-subjects?studentId=...`).

### Lịch thi
4. Xem lịch thi của bản thân, có thể lọc theo khoảng ngày (`GET /api/exams/student?studentId=...`).
5. Xem chi tiết ca thi: phòng, giờ, giám thị (`GET /api/exams/{id}`).

### Điểm danh
6. Xem lịch sử điểm danh của bản thân (`GET /api/attendances/student?studentId=...`).
7. Xác nhận điểm danh sau buổi thi kết thúc — cập nhật `StudentConfirmed = true` (`PUT /api/attendances/{id}`).

---

## Parent — Phụ huynh

1. Đăng nhập hệ thống.
2. Xem thông tin sinh viên (con em) (`GET /api/students/{id}`).
3. Xem lịch thi của sinh viên (`GET /api/exams/student?studentId=...`).
4. Xem lịch sử điểm danh (`GET /api/attendances/student?studentId=...`).

---

## Luồng nghiệp vụ chính — Tổ chức 1 kỳ thi

```
[Staff] Tạo kỳ thi
    → Kiểm tra phòng trống (GET /api/rooms/available)
    → Tạo exam (POST /api/exams)
    → Phân công giám thị (POST /api/proctors) — hệ thống kiểm tra xung đột

[Teacher/ChiefProctor] Ca thi diễn ra
    → Xem danh sách sinh viên (GET /api/attendances/roster)

[Teacher/Proctor] Trong ca thi
    → Điểm danh từng sinh viên (POST /api/attendances)
    → Ghi vi phạm nếu có (PUT /api/attendances/{id})

[Student] Sau ca thi
    → Xác nhận điểm danh của bản thân (PUT /api/attendances/{id} với StudentConfirmed=true)
    → Xem điểm thi khi được công bố

[Staff] Báo cáo
    → Xem báo cáo điểm danh tổng hợp (GET /api/attendances/report)
```

---

## Lưu ý điểm danh

Giám thị (`Proctor`) ghi nhận trạng thái chính thức: `Status`, `CheckInTime`, `Violation`. Sinh viên xác nhận thêm qua `StudentConfirmed = true` sau buổi thi — dùng để đối chiếu khi có tranh chấp.
