# 3. Use Case Diagram (Biểu đồ ca sử dụng)

## 3.1. Mô tả

Biểu đồ Use Case thể hiện các chức năng mà hệ thống **E360 Clone** (Quản lý Lịch thi & Coi thi) cung cấp cho từng nhóm tác nhân. Hệ thống được phân chia thành các nhóm chức năng chính: Xác thực, Quản lý dữ liệu danh mục, Quản lý lịch thi, Phân công giám thị, Quản lý điểm danh, Báo cáo & Thống kê.

---

## 3.2. Use Case — Admin / SuperAdmin (Quản trị viên)

| Mã UC | Tên Use Case | Mô tả |
|-------|-------------|-------|
| UC-A01 | Đăng nhập hệ thống | Đăng nhập bằng email/username + password, nhận JWT token |
| UC-A02 | Xem Dashboard tổng quan | Thống kê kỳ thi sắp diễn ra, số sinh viên, tình trạng điểm danh trong 14 ngày gần nhất |
| UC-A03 | Quản lý sinh viên | Xem danh sách, tìm kiếm, thêm/sửa/xóa sinh viên; lọc theo lớp học |
| UC-A04 | Quản lý giảng viên | Xem danh sách, tìm kiếm, thêm/sửa/xóa thông tin giảng viên |
| UC-A05 | Xem ngành học | Xem danh sách và chi tiết ngành học (chỉ đọc) |
| UC-A06 | Quản lý môn học | Thêm/sửa/xóa môn học; lọc theo loại môn, bộ môn, trạng thái |
| UC-A07 | Quản lý lớp học | Thêm/sửa/xóa lớp học; lọc theo ngành, khóa, môn học, trạng thái |
| UC-A08 | Xem toàn bộ lịch thi | Xem và lọc tất cả kỳ thi theo khoảng ngày |
| UC-A09 | Quản lý kỳ thi | Thêm/sửa/xóa kỳ thi |
| UC-A10 | Xem phân công giám thị | Xem danh sách phân công giám thị theo kỳ thi |
| UC-A11 | Xem & xuất báo cáo điểm danh | Xem báo cáo tổng hợp điểm danh; xuất Excel theo thời gian, môn học, lớp học |

---

## 3.3. Use Case — Staff (Nhân viên giáo vụ)

| Mã UC | Tên Use Case | Mô tả |
|-------|-------------|-------|
| UC-ST01 | Đăng nhập hệ thống | Đăng nhập bằng tài khoản được cấp |
| UC-ST02 | Xem danh sách kỳ thi | Xem, tìm kiếm, lọc kỳ thi theo khoảng ngày |
| UC-ST03 | Xem lịch thi theo ngày/tuần | Xem toàn bộ lịch thi dạng lịch, lọc theo ngày và môn học |
| UC-ST04 | Tạo kỳ thi | Tạo mới kỳ thi: chọn môn học, lớp, phòng thi, ngày giờ, hình thức thi |
| UC-ST05 | Cập nhật kỳ thi | Chỉnh sửa thông tin kỳ thi khi có thay đổi |
| UC-ST06 | Xóa kỳ thi | Hủy và xóa kỳ thi |
| UC-ST07 | Phân công giám thị | Chỉ định giảng viên làm Giám thị quản lý (ChiefProctor) hoặc Giám thị coi thi (Proctor); hệ thống tự kiểm tra xung đột lịch |
| UC-ST08 | Hủy phân công giám thị | Xóa phân công giám thị khỏi kỳ thi |
| UC-ST09 | Theo dõi điểm danh | Xem danh sách điểm danh của từng kỳ thi |
| UC-ST10 | Xem & xuất báo cáo điểm danh | Xem báo cáo tổng hợp; xuất file Excel theo bộ lọc thời gian, môn, lớp |

---

## 3.4. Use Case — Teacher (Giảng viên)

| Mã UC | Tên Use Case | Mô tả |
|-------|-------------|-------|
| UC-T01 | Đăng nhập hệ thống | Đăng nhập bằng tài khoản giảng viên |
| UC-T02 | Xem lịch coi thi được phân công | Xem danh sách kỳ thi mình được phân công làm giám thị |
| UC-T03 | Xem chi tiết kỳ thi | Xem thông tin phòng thi, giờ thi, danh sách sinh viên |
| UC-T04 | Xem danh sách giám thị trong ca | Xem các giám thị cùng ca thi (chức năng của Giám thị quản lý) |
| UC-T05 | Lấy roster điểm danh ca thi | Tải danh sách sinh viên cần điểm danh của kỳ thi được phân công |
| UC-T06 | Ghi nhận điểm danh sinh viên | Đánh dấu trạng thái Present / Late / Absent / Excused cho từng sinh viên |
| UC-T07 | Điểm danh hàng loạt | Đánh dấu tất cả sinh viên chưa điểm danh là Present cùng lúc |
| UC-T08 | Cập nhật bản ghi điểm danh | Chỉnh trạng thái, thêm ghi chú hoặc ghi nhận vi phạm |
| UC-T09 | Xuất danh sách điểm danh | Xuất file Excel danh sách điểm danh của ca thi |
| UC-T10 | Xem môn học đang phụ trách | Xem danh sách môn học và lớp được phân công giảng dạy |
| UC-T11 | Xem sinh viên theo môn học | Xem danh sách sinh viên đang học môn mình phụ trách |
| UC-T12 | Xem hồ sơ cá nhân | Xem thông tin tài khoản và danh sách môn đang phụ trách |

---

## 3.5. Use Case — Student (Sinh viên)

| Mã UC | Tên Use Case | Mô tả |
|-------|-------------|-------|
| UC-S01 | Đăng ký tài khoản | Tạo tài khoản mới với thông tin cá nhân và mã sinh viên |
| UC-S02 | Đăng nhập hệ thống | Đăng nhập bằng email/username + password |
| UC-S03 | Xem hồ sơ cá nhân | Xem thông tin tài khoản, lớp học, thông tin sinh viên |
| UC-S04 | Xem môn học đang đăng ký | Xem danh sách môn học đang theo học trong kỳ |
| UC-S05 | Xem lịch thi cá nhân | Xem lịch thi của bản thân; lọc theo khoảng ngày |
| UC-S06 | Xem chi tiết ca thi | Xem thông tin phòng thi, giờ thi, giám thị phụ trách |
| UC-S07 | Xem lịch sử điểm danh | Xem toàn bộ lịch sử điểm danh của bản thân |
| UC-S08 | Xác nhận điểm danh | Xác nhận có mặt sau khi ca thi kết thúc (dùng đối chiếu khi tranh chấp) |

---

## 3.6. Use Case — Parent (Phụ huynh)

| Mã UC | Tên Use Case | Mô tả |
|-------|-------------|-------|
| UC-P01 | Đăng nhập hệ thống | Đăng nhập bằng tài khoản phụ huynh được cấp |
| UC-P02 | Xem thông tin sinh viên | Xem hồ sơ của con em đang theo học |
| UC-P03 | Xem lịch thi của sinh viên | Theo dõi lịch thi sắp tới của con |
| UC-P04 | Xem lịch sử điểm danh | Theo dõi tình trạng có mặt tại các ca thi của con |

---

## 3.7. Luồng nghiệp vụ chính — Tổ chức 1 kỳ thi

```
[Staff] Chuẩn bị kỳ thi
    → Xem lịch thi theo ngày/tuần để xác định khung giờ (UC-ST03)
    → Tạo kỳ thi: chọn môn, lớp, phòng, giờ (UC-ST04)
    → Phân công giám thị, hệ thống kiểm tra xung đột lịch (UC-ST07)

[Teacher — ChiefProctor] Quản lý ca thi
    → Xem danh sách giám thị trong ca (UC-T04)
    → Xem roster điểm danh toàn ca (UC-T05)

[Teacher — Proctor] Trong ca thi
    → Lấy danh sách điểm danh (UC-T05)
    → Ghi nhận điểm danh từng sinh viên (UC-T06) hoặc hàng loạt (UC-T07)
    → Ghi vi phạm nếu có (UC-T08)
    → Xuất file Excel danh sách điểm danh (UC-T09)

[Student] Sau ca thi
    → Xác nhận điểm danh của bản thân (UC-S08)
    → Xem lịch sử điểm danh (UC-S07)

[Staff / Admin] Báo cáo
    → Xem và xuất báo cáo điểm danh tổng hợp (UC-ST10 / UC-A11)
```
