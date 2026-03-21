\# chủ đề: API cho Nền tảng quản lý lịch thi \& coi thi

## Database

**PostgreSQL** tự host trên **Coolify** (VPS riêng) tại `postgres.memore.vn`, port **5433** (thay vì 5432 mặc định để tránh bị block bởi firewall/ISP).

- Dashboard Coolify: https://postgres.memore.vn/project/default/editor/25312
- Connection string (xem `e360_clone_api/appsettings.json`):
  ```
  Host=103.72.56.152;Port=5433;Database=postgres;Username=postgres;Password=...
  ```

> Lý do dùng port 5433: port 5432 bị một số mạng/ISP block, port 5433 tránh được vấn đề này.





\## 1. Nghiệp vụ



\### 1.1. Các tác nhân nghiệp vụ

\- \*\*Admin\*\*: Quản trị viên hệ thống

\- \*\*Nhân viên giáo vụ\*\*: Phụ trách tổ chức thi, phân công coi thi

\- \*\*Giảng viên\*\*: Coi thi, chấm thi, nhập điểm

\- \*\*Sinh viên\*\*: Xem lịch thi, xem điểm thi



\### 1.2. Các nghiệp vụ chính



\#### 1.2.1. Quản lý Lịch thi

\- \*\*Lập lịch thi lớp học\*\*: Tạo và quản lý lịch thi cho các lớp học môn thường kỳ

\- \*\*Lập lịch thi tốt nghiệp\*\*: Tạo và quản lý lịch thi tốt nghiệp

\- \*\*Quản lý phòng thi\*\*: Phân bổ phòng học cho các ca thi

\- \*\*Xuất danh sách phòng thi\*\*: In danh sách phòng thi cho giám thị



\#### 1.2.2. Phân công Coi thi

\- \*\*Phân công coi thi lớp học\*\*: Sắp xếp giảng viên coi thi các môn học

\- \*\*Phân công coi thi tốt nghiệp\*\*: Sắp xếp giảng viên coi thi tốt nghiệp

\- \*\*Quản lý giám thị\*\*: Quản lý thông tin giám thị và phân công



\#### 1.2.3. Quản lý Điểm thi

\- \*\*Nhập điểm môn học\*\*: Giảng viên nhập điểm thi cho sinh viên

\- \*\*Nhập điểm môn tốt nghiệp\*\*: Nhập điểm thi tốt nghiệp

\- \*\*Xem điểm sinh viên\*\*: Sinh viên xem điểm thi của mình

\- \*\*In điểm thi lớp học\*\*: In bảng điểm tập trung cho lớp học



\#### 1.2.4. Quản lý Dữ liệu nền

\- \*\*Quản lý sinh viên\*\*: Quản lý thông tin sinh viên

\- \*\*Quản lý giảng viên\*\*: Quản lý thông tin giảng viên

\- \*\*Quản lý môn học\*\*: Quản lý danh mục môn học

\- \*\*Quản lý lớp học\*\*: Quản lý thông tin các lớp học

\- \*\*Quản lý ngành học\*\*: Quản lý thông tin các ngành đào tạo



\#### 1.2.5. Báo cáo và Thống kê

\- \*\*Kết xuất học bạ\*\*: Tạo học bạ cho sinh viên

\- \*\*In danh sách lớp thi\*\*: In danh sách sinh viên tham gia thi

\- \*\*Thống kê kết quả thi\*\*: Báo cáo thống kê kết quả thi theo kỳ

\- \*\*Xuất báo biểu\*\*: Xuất các loại báo cáo quản lý



\### 1.3. Quy trình nghiệp vụ chính



\#### Quy trình tổ chức kỳ thi

1\. \*\*Lập kế hoạch thi\*\*: Xác định danh sách môn học cần thi

2\. \*\*Lập lịch thi\*\*: Phân bổ thời gian, địa điểm thi

3\. \*\*Phân công coi thi\*\*: Sắp xếp giám thị cho các ca thi

4\. \*\*Thông báo lịch thi\*\*: Gửi thông báo cho sinh viên và giảng viên

5\. \*\*Tổ chức thi\*\*: Quản lý quá trình thi diễn ra

6\. \*\*Nhập điểm\*\*: Giảng viên nhập điểm sau thi

7\. \*\*Công bố kết quả\*\*: Sinh viên xem điểm thi



\#### Quy trình quản lý điểm thi

1\. \*\*Tạo cấu trúc điểm\*\*: Thiết lập thang điểm và cấu trúc đánh giá

2\. \*\*Nhập điểm\*\*: Giảng viên nhập điểm theo môn học

3\. \*\*Kiểm tra điểm\*\*: Kiểm tra tính hợp lệ của điểm

4\. \*\*Duyệt điểm\*\*: Phê duyệt điểm thi trước khi công bố

5\. \*\*Công bố điểm\*\*: Mở cho sinh viên xem điểm

6\. \*\*Xử lý khiếu nại\*\*: Giải quyết các vấn đề về điểm thi



\### 1.4. Quy trình theo trình tự thời gian



\#### 1.4.1. Quy trình toàn kỳ học (Theo tuần)



\*\*Tuần 1-2: Chuẩn bị kỳ thi\*\*

\- Admin xác định danh sách môn học cần thi trong kỳ

\- Nhân viên giáo vụ kiểm tra và cập nhật danh sách sinh viên đủ điều kiện thi

\- Giảng viên hoàn thành chương trình giảng dạy, xác nhận đủ điều kiện thi



\*\*Tuần 3: Lập kế hoạch thi\*\*

\- Nhân viên giáo vụ lập kế hoạch chi tiết kỳ thi

\- Xác định số lượng ca thi, phòng thi cần thiết

\- Lập danh sách giảng viên khả dụng để coi thi



\*\*Tuần 4: Lập lịch thi\*\*

\- Nhân viên giáo vụ tạo lịch thi chi tiết

\- Phân bổ phòng học cho từng ca thi

\- Kiểm tra xung đột lịch thi giữa các môn học



\*\*Tuần 5: Phân công coi thi\*\*

\- Nhân viên giáo vụ phân công giám thị cho các ca thi

\- Gửi thông báo lịch thi cho giảng viên và sinh viên

\- Chuẩn bị phòng thi và thiết bị



\*\*Tuần 6-8: Tổ chức thi\*\*

\- Tổ chức các ca thi theo lịch đã lập

\- Giám thị quản lý quá trình thi

\- Nhận và bảo quản bài thi



\*\*Tuần 9-10: Chấm thi và nhập điểm\*\*

\- Giảng viên chấm bài thi

\- Nhập điểm vào hệ thống

\- Kiểm tra và xác nhận điểm



\*\*Tuần 11: Duyệt và công bố điểm\*\*

\- Admin/Trưởng khoa duyệt điểm

\- Công bố điểm cho sinh viên

\- Xử lý các khiếu nại (nếu có)



\*\*Tuần 12: Báo cáo và tổng kết\*\*

\- Nhân viên giáo vụ tạo báo cáo tổng kết kỳ thi

\- Thống kê kết quả thi theo môn học, lớp học

\- Lưu trữ hồ sơ thi



\#### 1.4.2. Quy trình theo ngày (Trong tuần thi)



\*\*Ngày thi trước 2 ngày:\*\*

\- Nhân viên giáo vụ kiểm tra cuối cùng phòng thi

\- Chuẩn bị giấy thi, văn phòng phẩm

\- Gửi nhắc nhở lịch thi cho giám thị và sinh viên



\*\*Ngày thi trước 1 ngày:\*\*

\- Bố trí phòng thi, dán biển chỉ dẫn

\- Kiểm tra thiết bị (điều hòa, đèn, bảng)

\- Phân phát đề thi cho giám thị (đã niêm phong)



\*\*Ngày thi:\*\*

\- \*\*7:00-7:30\*\*: Giám thị có mặt, nhận đề thi

\- \*\*7:30-8:00\*\*: Sinh viên vào phòng, điểm danh và kiểm tra danh tính

\- \*\*8:00-8:15\*\*: Phổ biến quy chế thi

\- \*\*8:15-10:15\*\*: Thời gian làm bài (60-120 phút tùy môn)

\- \*\*10:15-10:30\*\*: Thu bài thi, điểm danh ra phòng

\- \*\*10:30-11:00\*\*: Bàn giao bài thi và biên bản điểm danh cho phòng giáo vụ



\*\*Ngày thi sau 1 ngày:\*\*

\- Phòng giáo vụ kiểm tra biên bản điểm danh

\- Phân phát bài thi cho giảng viên

\- Giảng viên bắt đầu quá trình chấm thi



\#### 1.4.3. Quy trình điểm danh chi tiết



\*\*1. Điểm danh khi vào phòng thi (7:30-8:00)\*\*

\- Giám thị kiểm tra thẻ sinh viên/CMTND

\- Đối chiếu với danh sách sinh viên dự thi

\- Ghi nhận thời gian vào phòng của từng sinh viên

\- Đánh dấu sinh viên có mặt/vắng mặt

\- Xử lý trường hợp sinh viên đi muộn (sau 8:00)



\*\*2. Xử lý sinh viên đi muộn\*\*

\- \*\*Đi muộn 5-15 phút\*\*: Được vào thi nhưng không được gia hạn thời gian

\- \*\*Đi muộn 15-30 phút\*\*: Cần sự chấp thuận của giám thị chính

\- \*\*Đi muộn trên 30 phút\*\*: Không được vào thi, coi như vắng mặt



\*\*3. Điểm danh khi ra phòng thi (10:15-10:30)\*\*

\- Ghi nhận thời gian nộp bài của từng sinh viên

\- Kiểm tra số lượng bài thi thu được

\- Đối chiếu với số lượng sinh viên có mặt

\- Ghi nhận sinh viên nộp bài sớm (nếu có)



\*\*4. Xử lý trường hợp đặc biệt\*\*

\- \*\*Sinh viên ra sớm\*\*: Phải có lý do hợp lệ, giám thị ghi chú

\- \*\*Sinh viên không nộp bài\*\*: Ghi rõ lý do vào biên bản

\- \*\*Sinh viên vi phạm quy chế\*\*: Ghi nhận vào biên bản điểm danh



\*\*5. Hoàn thành biên bản điểm danh\*\*

\- Giám thị ký xác nhận biên bản

\- Ghi rõ các sự kiện bất thường (nếu có)

\- Nộp biên bản cho phòng giáo vụ cùng bài thi



\#### 1.4.4. Quy trình xử lý sự cố



\*\*Sự cố trong quá trình thi:\*\*

\- \*\*15 phút đầu\*\*: Giám thị xử lý tại chỗ, báo cáo nhanh

\- \*\*Sau 15 phút\*\*: Báo cáo cấp cao hơn, quyết định phương án

\- \*\*Sau 30 phút\*\*: Quyết định dừng thi hoặc tiếp tục với điều kiện đặc biệt



\*\*Sự cố hệ thống:\*\*

\- \*\*5 phút đầu\*\*: Kỹ thuật xử lý nhanh

\- \*\*Sau 5 phút\*\*: Chuyển sang phương án dự phòng (thi giấy)

\- \*\*Sau 15 phút\*\*: Thông báo dời lịch thi



\#### 1.4.5. Quy trình bảo mật



\*\*Trước thi:\*\*

\- Đề thi được lưu trữ trong két sắt

\- Chỉ người có thẩm quyền được tiếp cận

\- Mã hóa đề thi khi truyền tải



\*\*Trong thi:\*\*

\- Giám thị tuân thủ quy chế nghiêm ngặt

\- Không sử dụng thiết bị điện tử cá nhân

\- Theo dõi và ngăn chặn gian lận



\*\*Sau thi:\*\*

\- Bài thi được lưu trữ an toàn

\- Quá trình chấm thi có giám sát

\- Điểm chỉ được công bố sau khi đã kiểm tra kỹ lưỡng



\## 2. Chức năng Web Application



\### 2.1. Chức năng cho Admin



\#### 2.1.1. Quản lý hệ thống

\- \*\*Dashboard\*\*: Tổng quan hệ thống, thống kê hoạt động

\- \*\*Quản lý người dùng\*\*: Tạo, sửa, xóa tài khoản người dùng

\- \*\*Phân quyền\*\*: Gán quyền cho các vai trò (Admin, Nhân viên giáo vụ, Giảng viên, Sinh viên)

\- \*\*Cấu hình hệ thống\*\*: Thiết lập tham số hệ thống

\- \*\*Nhật ký hoạt động\*\*: Xem log hoạt động của người dùng

\- \*\*Sao lưu dữ liệu\*\*: Backup và restore dữ liệu hệ thống



\#### 2.1.2. Quản lý học vụ (cơ bản)

\- \*\*Quản lý ngành học\*\*: Thêm, sửa, xóa thông tin ngành đào tạo

\- \*\*Quản lý khóa học\*\*: Quản lý thông tin các khóa học

\- \*\*Quản lý lớp học\*\*: Thêm, sửa, xóa thông tin lớp học

\- \*\*Quản lý môn học\*\*: Quản lý danh mục môn học (chỉ thông tin cơ bản cho việc lập lịch thi)



\### 2.2. Chức năng cho Nhân viên giáo vụ



\#### 2.2.1. Quản lý sinh viên

\- \*\*Danh sách sinh viên\*\*: Xem, tìm kiếm, lọc sinh viên

\- \*\*Thêm sinh viên\*\*: Nhập thông tin sinh viên mới

\- \*\*Cập nhật thông tin\*\*: Sửa thông tin sinh viên

\- \*\*Quản lý tình trạng\*\*: Cập nhật tình trạng học tập, đủ điều kiện thi

\- \*\*Xuất báo cáo\*\*: Xuất danh sách sinh viên theo các tiêu chí



\#### 2.2.2. Quản lý giảng viên

\- \*\*Danh sách giảng viên\*\*: Quản lý thông tin giảng viên

\- \*\*Phân công giảng dạy\*\*: Sắp xếp giảng viên dạy các môn học

\- \*\*Quản lý lịch làm việc\*\*: Theo dõi lịch làm việc của giảng viên



\#### 2.2.3. Quản lý lịch thi

\- \*\*Lập lịch thi\*\*: Tạo lịch thi cho các môn học

\- \*\*Quản lý phòng thi\*\*: Phân bổ phòng học cho các ca thi

\- \*\*Kiểm tra xung đột\*\*: Tự động kiểm tra trùng lịch, phòng thi

\- \*\*Xuất lịch thi\*\*: In lịch thi, gửi thông báo

\- \*\*Cập nhật lịch\*\*: Sửa đổi lịch thi khi cần thiết



\#### 2.2.4. Phân công coi thi

\- \*\*Tạo ca coi thi\*\*: Tạo các ca thi cần giám thị

\- \*\*Phân công giám thị\*\*: Tự động hoặc thủ công phân công giảng viên

\- \*\*Quản lý lịch coi thi\*\*: Xem lịch coi thi của giảng viên

\- \*\*Xuất danh sách\*\*: In danh sách giám thị theo ca thi

\- \*\*Gửi thông báo\*\*: Gửi email/thông báo cho giám thị



\#### 2.2.5. Quản lý điểm danh

\- \*\*Nhập biên bản điểm danh\*\*: Nhập dữ liệu từ biên bản điểm danh

\- \*\*Quản lý vắng mặt\*\*: Xử lý lý do vắng mặt, xin phép

\- \*\*Thống kê điểm danh\*\*: Báo cáo tỷ lệ có mặt/vắng mặt

\- \*\*Xuất báo cáo\*\*: In báo cáo điểm danh theo môn/lớp



\#### 2.2.6. Báo cáo và thống kê (thi)

\- \*\*Báo cáo kỳ thi\*\*: Tạo báo cáo tổng kết kỳ thi

\- \*\*Thống kê kết quả thi\*\*: Thống kê điểm thi theo môn, lớp, khóa

\- \*\*Báo cáo điểm danh\*\*: Thống kê tỷ lệ tham gia thi

\- \*\*Báo cáo phân công\*\*: Thống kê phân công coi thi



\### 2.3. Chức năng cho Giảng viên



\#### 2.3.1. Quản lý lớp học (thông tin thi)

\- \*\*Xem danh sách lớp\*\*: Xem sinh viên trong các lớp mình dạy

\- \*\*Xem thông tin thi\*\*: Xem thông tin môn học, ca thi liên quan

\- \*\*Xác nhận điều kiện thi\*\*: Duyệt sinh viên đủ điều kiện dự thi



\#### 2.3.2. Quản lý thi

\- \*\*Xem lịch coi thi\*\*: Xem lịch được phân công coi thi

\- \*\*Xem lịch thi lớp\*\*: Xem lịch thi của các lớp mình dạy

\- \*\*Nhận thông báo\*\*: Nhận thông báo về lịch thi, phân công



\#### 2.3.3. Quản lý điểm thi

\- \*\*Nhập điểm thi\*\*: Nhập điểm thi cho sinh viên

\- \*\*Cập nhật điểm\*\*: Sửa điểm (trước khi duyệt)

\- \*\*Xem thống kê\*\*: Xem thống kê điểm thi của lớp

\- \*\*Xuất bảng điểm\*\*: In bảng điểm tạm thời



\#### 2.3.4. Điểm danh thi

\- \*\*Điểm danh online\*\*: Điểm danh sinh viên qua hệ thống

\- \*\*Nhập biên bản\*\*: Nhập kết quả điểm danh từ biên bản giấy

\- \*\*Ghi chú sự kiện\*\*: Ghi nhận các sự kiện bất thường

\- \*\*Xuất biên bản\*\*: In biên bản điểm danh điện tử



\### 2.4. Chức năng cho Sinh viên



\#### 2.4.1. Xem thông tin cá nhân

\- \*\*Thông tin sinh viên\*\*: Xem thông tin cá nhân, lớp học

\- \*\*Lịch học\*\*: Xem thời khóa biểu (cơ bản)

\- \*\*Xem thông tin thi\*\*: Xem các môn học cần thi



\#### 2.4.2. Quản lý thi

\- \*\*Lịch thi\*\*: Xem lịch thi các môn học

\- \*\*Đăng ký thi\*\*: Đăng ký các môn thi (nếu có)

\- \*\*Xem phòng thi\*\*: Xem địa điểm, phòng thi

\- \*\*Nhận thông báo\*\*: Nhận thông báo về lịch thi



\#### 2.4.3. Xem điểm thi

\- \*\*Điểm thi mới\*\*: Xem điểm thi vừa được công bố

\- \*\*Lịch sử điểm thi\*\*: Xem điểm các kỳ thi trước

\- \*\*Thống kê điểm thi\*\*: Xem biểu đồ điểm thi

\- \*\*Xuất bảng điểm thi\*\*: In bảng điểm thi cá nhân



\#### 2.4.4. Điểm danh

\- \*\*Điểm danh online\*\*: Điểm danh khi vào phòng thi qua QR code/mã

\- \*\*Xem lịch sử điểm danh\*\*: Xem lịch sử điểm danh các môn thi

\- \*\*Xin phép vắng mặt\*\*: Gửi yêu cầu xin phép vắng thi



\### 2.5. Chức năng chung



\#### 2.5.1. Xác thực và bảo mật

\- \*\*Đăng nhập/Đăng xuất\*\*: Xác thực người dùng

\- \*\*Quên mật khẩu\*\*: Khôi phục mật khẩu

\- \*\*Đổi mật khẩu\*\*: Thay đổi mật khẩu

\- \*\*Hai yếu tố\*\*: Xác thực 2FA (tùy chọn)



\#### 2.5.2. Thông báo

\- \*\*Hộp thư\*\*: Hệ thống thông báo nội bộ

\- \*\*Email thông báo\*\*: Gửi thông báo qua email

\- \*\*Push notification\*\*: Thông báo đẩy (mobile)

\- \*\*SMS thông báo\*\*: Tin nhắn SMS (tùy chọn)



\#### 2.5.3. Tìm kiếm và lọc

\- \*\*Tìm kiếm nhanh\*\*: Tìm kiếm thông tin toàn hệ thống

\- \*\*Bộ lọc nâng cao\*\*: Lọc thông tin theo nhiều tiêu chí

\- \*\*Sắp xếp\*\*: Sắp xếp dữ liệu theo các trường



\#### 2.5.4. Xuất và in ấn

\- \*\*Xuất Excel/PDF\*\*: Xuất báo cáo các định dạng

\- \*\*In ấn\*\*: In các biểu mẫu, báo cáo

\- \*\*Chia sẻ\*\*: Chia sẻ thông tin qua link



\### 2.6. Chức năng Mobile (Responsive)



\#### 2.6.1. Cho sinh viên

\- \*\*Xem lịch thi\*\*: Mobile-friendly

\- \*\*Điểm danh QR code\*\*: Quét mã để điểm danh

\- \*\*Nhận thông báo push\*\*: Thông báo tức thì

\- \*\*Xem điểm thi\*\*: Xem điểm nhanh



\#### 2.6.2. Cho giảng viên

\- \*\*Điểm danh mobile\*\*: Điểm danh qua điện thoại

\- \*\*Xem lịch coi thi\*\*: Xem lịch di động

\- \*\*Nhập điểm nhanh\*\*: Nhập điểm cơ bản

\- \*\*Nhận thông báo\*\*: Thông báo phân công



\### 2.7. Chức năng tích hợp



\#### 2.7.1. Tích hợp hệ thống khác

\- \*\*Student Management System\*\*: Đồng bộ dữ liệu sinh viên

\- \*\*Email Gateway\*\*: Gửi email tự động

\- \*\*SMS Gateway\*\*: Gửi tin nhắn SMS

\- \*\*Payment Gateway\*\*: Thanh toán lệ phí thi (nếu có)



\#### 2.7.2. API cho bên thứ ba

\- \*\*RESTful API\*\*: Cung cấp API cho các hệ thống khác

\- \*\*Webhook\*\*: Thông báo sự kiện cho hệ thống bên ngoài

\- \*\*Documentation\*\*: Tài liệu API đầy đủ



