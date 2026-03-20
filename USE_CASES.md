# Use Case Flow (E360 Clone)

Tài liệu mô tả luồng thao tác chính theo vai trò trong hệ thống quản lý lịch thi.

## Roles

| Role | Mô tả |
|------|-------|
| **Admin / SuperAdmin** | Quản trị hệ thống toàn quyền |
| **Nhân viên giáo vụ (AcademicStaff)** | Lập lịch thi, phân công giám thị, duyệt điểm |
| **Giảng viên (Teacher)** | Được phân công làm giám thị — 2 loại: Giám thị quản lý (ChiefProctor) hoặc Giám thị coi thi (Proctor) |
| **Sinh viên (Student)** | Xem lịch thi, xem điểm của bản thân |

> **Lưu ý về giám thị:** Không có role riêng. Giảng viên được phân công vào ca thi với `ProctorRole`:
> - `ChiefProctor` — Giám thị quản lý: chịu trách nhiệm toàn bộ ca thi, xử lý vi phạm, ký biên bản.
> - `Proctor` — Giám thị coi thi: ngồi trong phòng thi, điểm danh sinh viên, giám sát.

---

## Admin / SuperAdmin

1. Đăng nhập hệ thống.
2. Quản lý tài khoản người dùng: tạo, cập nhật, khóa/mở khóa, reset mật khẩu.
3. Quản lý dữ liệu danh mục: sinh viên, giảng viên, môn học, lớp, phòng thi.
4. Xem toàn bộ lịch thi, phân công giám thị, kết quả điểm.
5. Xem báo cáo và thống kê toàn hệ thống.
6. Cấu hình và phân quyền hệ thống.

---

## Nhân viên giáo vụ (AcademicStaff)

### Quản lý lịch thi
1. Tạo kỳ thi: chọn môn, lớp, ngày thi, phòng thi, thời gian, hình thức thi.
2. Kiểm tra xung đột lịch (phòng thi, giờ thi) trước khi xác nhận.
3. Cập nhật hoặc hủy lịch thi khi có thay đổi.
4. Xem danh sách toàn bộ lịch thi theo ngày/phòng/môn/lớp.

### Phân công giám thị
1. Xem danh sách giảng viên có thể phân công (không trùng lịch).
2. Phân công giảng viên vào ca thi với vai trò `ChiefProctor` hoặc `Proctor`.
3. Cập nhật hoặc hủy phân công khi cần.
4. Xem báo cáo phân công giám thị theo ca thi / giảng viên.

### Quản lý điểm
1. Xem danh sách điểm đã được giảng viên nhập (trạng thái `Submitted`).
2. Duyệt điểm (`Approved`) hoặc trả lại để nhập lại.
3. Công bố điểm (`Published`) để sinh viên xem.
4. Xuất báo cáo kết quả thi theo môn/lớp/kỳ.

---

## Giảng viên (Teacher)

### Với vai trò Giám thị quản lý (ChiefProctor)
1. Xem danh sách ca thi được phân công với vai trò ChiefProctor.
2. Xác nhận hoặc từ chối phân công.
3. Quản lý toàn bộ ca thi: kiểm tra danh sách sinh viên, phòng thi, giám thị coi thi.
4. Xử lý vi phạm trong ca thi: ghi nhận loại vi phạm (`Cheating`, `Disruptive`, ...).
5. Ký xác nhận kết thúc ca thi.

### Với vai trò Giám thị coi thi (Proctor)
1. Xem danh sách ca thi được phân công với vai trò Proctor.
2. Xác nhận hoặc từ chối phân công.
3. Điểm danh sinh viên tại ca thi: `Present`, `Late`, `Absent`, `Excused`.
4. Ghi nhận vi phạm của sinh viên nếu có.
5. Nhập điểm cho sinh viên sau ca thi (trạng thái `Draft` → `Submitted`).
6. Cập nhật điểm trước khi nhân viên giáo vụ duyệt.

---

## Sinh viên (Student)

1. Đăng nhập hệ thống.
2. Xem thông tin cá nhân và lớp đang học.
3. Xem danh sách lịch thi của bản thân theo môn/ngày.
4. Xem chi tiết ca thi: phòng thi, giờ thi, giám thị.
5. Xem trạng thái điểm danh của bản thân tại từng ca thi.
6. **Tự xác nhận điểm danh** sau khi buổi thi kết thúc (`StudentConfirmed = true`).
7. Xem điểm thi sau khi đã được công bố (`Published`).

> **Lưu ý điểm danh:** Giám thị ghi nhận trạng thái chính thức (`Status`, `CheckInTime`, `Violation`). Sinh viên xác nhận thêm bằng `StudentConfirmed` sau buổi thi — dùng để đối chiếu nếu có tranh chấp.
