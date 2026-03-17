# Enums Tiếng Việt - E360 Clone Frontend

## 📋 Danh sách Enums

### **AccountStatus** - Trạng thái tài khoản
```javascript
AccountStatus.HOAT_DONG         // 'Hoạt động'
AccountStatus.KHONG_HOAT_DONG   // 'Không hoạt động'
AccountStatus.BI_KHOA           // 'Bị khóa'
AccountStatus.DINH_CHI          // 'Đình chỉ'

// Helper
AccountStatusHelper.getText('Hoạt động')      // 'Hoạt động'
AccountStatusHelper.getBadge('Hoạt động')     // 'bg-success'
```

### **Role** - Vai trò người dùng
```javascript
Role.QUAN_TRI_CAO_CAP     // 'Quản trị cao cấp'
Role.QUAN_TRI_VIEN        // 'Quản trị viên'
Role.SINH_VIEN            // 'Sinh viên'
Role.GIANG_VIEN           // 'Giảng viên'
Role.PHU_HUYNH            // 'Phụ huynh'
Role.THUTHU               // 'Thủ thư'
Role.NHAN_VIEN            // 'Nhân viên'

// Helper
RoleHelper.getText('Quản trị viên')           // 'Quản trị viên'
RoleHelper.getIcon('Quản trị viên')           // 'ri-admin-line'
RoleHelper.isAdmin('Quản trị viên')           // true
```

### **StudentStatus** - Trạng thái sinh viên
```javascript
StudentStatus.DANG_HOC        // 'Đang học'
StudentStatus.TAM_NGUNG       // 'Tạm ngưng'
StudentStatus.TOT_NGHIEP      // 'Tốt nghiệp'
StudentStatus.DINH_CHI        // 'Đình chỉ'
StudentStatus.BUOC_THOI_HOC   // 'Buộc thôi học'

// Helper
StudentStatusHelper.getText('Đang học')    // 'Đang học'
StudentStatusHelper.getBadge('Đang học')   // 'bg-success'
StudentStatusHelper.isDangHoc('Đang học')  // true
```

### **Gender** - Giới tính
```javascript
Gender.NAM    // 'Nam'
Gender.NU     // 'Nữ'
Gender.KHAC   // 'Khác'

// Helper
GenderHelper.getText('Nam')    // 'Nam'
GenderHelper.getIcon('Nam')    // 'ri-mars-line'
```

### **ExamStatus** - Trạng thái kỳ thi
```javascript
ExamStatus.LEN_KE_HOACH     // 'Lên kế hoạch'
ExamStatus.DA_XEP_LICH      // 'Đã xếp lịch'
ExamStatus.DANG_DIEN_RA     // 'Đang diễn ra'
ExamStatus.HOAN_THANH       // 'Hoàn thành'
ExamStatus.DA_HUY           // 'Đã hủy'

// Helper
ExamStatusHelper.getText('Đã xếp lịch')    // 'Đã xếp lịch'
ExamStatusHelper.getBadge('Đã xếp lịch')   // 'bg-primary'
```

### **GradeType** - Loại điểm
```javascript
GradeType.KIEM_TRA_15_PHUT    // 'Kiểm tra 15 phút'
GradeType.KIEM_TRA_1_TIET     // 'Kiểm tra 1 tiết'
GradeType.GIUA_KY             // 'Giữa kỳ'
GradeType.CUOI_KY             // 'Cuối kỳ'
GradeType.BAI_TAP             // 'Bài tập'
GradeType.DO_AN               // 'Đồ án'
GradeType.CHUYEN_CAN          // 'Chuyên cần'
GradeType.KHAC                // 'Khác'

// Helper
GradeTypeHelper.getWeight('Cuối kỳ')    // 0.5
GradeTypeHelper.getText('Cuối kỳ')      // 'Cuối kỳ'
```

### **AttendanceStatus** - Trạng thái điểm danh
```javascript
AttendanceStatus.CO_MAT       // 'Có mặt'
AttendanceStatus.VANG_MAT     // 'Vắng mặt'
AttendanceStatus.DI_MUON      // 'Đi muộn'
AttendanceStatus.VANG_CO_PHEP // 'Vắng có phép'
AttendanceStatus.VE_SOM       // 'Về sớm'

// Helper
AttendanceStatusHelper.getText('Có mặt')    // 'Có mặt'
AttendanceStatusHelper.getBadge('Có mặt')   // 'bg-success'
```

## 📝 Cách sử dụng

### **So sánh với constants**
```javascript
// ✅ Đúng - Dùng constants
if (student.status === StudentStatus.DANG_HOC) { }
if (account.role === Role.QUAN_TRI_VIEN) { }

// ❌ Sai - Hard-coded string
if (student.status === 'Đang học') { }
```

### **Lấy text hiển thị**
```javascript
// Tự động map text từ enum value
const statusText = StudentStatusHelper.getText(student.status);
const roleText = RoleHelper.getText(user.role);
const genderText = GenderHelper.getText(student.gender);
```

### **Lấy badge class**
```javascript
// Tự động map badge class
const badgeClass = StudentStatusHelper.getBadge(status);
// 'Đang học' → 'bg-success'
// 'Tạm ngưng' → 'bg-secondary'
// 'Tốt nghiệp' → 'bg-info'
```

### **Dropdown options**
```javascript
// Lấy tất cả options cho dropdown
const statusOptions = StudentStatusHelper.getAllWithText();
// [
//   { value: 'Đang học', text: 'Đang học', badge: 'bg-success' },
//   { value: 'Tạm ngưng', text: 'Tạm ngưng học', badge: 'bg-secondary' },
//   ...
// ]
```

### **Validate**
```javascript
// Kiểm tra giá trị có hợp lệ không
if (!StudentStatusHelper.isValid(status)) {
    errors.push('Trạng thái không hợp lệ');
}

if (!GenderHelper.isValid(gender)) {
    errors.push('Giới tính không hợp lệ');
}
```

### **Permissions**
```javascript
// Kiểm tra quyền
if (RoleHelper.hasPermission(userRole, 'students.manage')) {
    // User có quyền quản lý sinh viên
}

// Kiểm tra admin
if (RoleHelper.isAdmin(userRole)) {
    // User là quản trị
}

// Kiểm tra giảng viên
if (RoleHelper.isTeacher(userRole)) {
    // User là giảng viên
}
```

## 🔄 Mapping với Database

| Enum Value | Database Value | Display Text |
|------------|---------------|--------------|
| `StudentStatus.DANG_HOC` | 'Đang học' | 'Đang học' |
| `StudentStatus.TAM_NGUNG` | 'Tạm ngưng' | 'Tạm ngưng học' |
| `Role.QUAN_TRI_VIEN` | 'Quản trị viên' | 'Quản trị viên' |
| `Role.SINH_VIEN` | 'Sinh viên' | 'Sinh viên' |
| `Gender.NAM` | 'Nam' | 'Nam' |
| `Gender.NU` | 'Nữ' | 'Nữ' |

## ✅ Best Practices

1. **Luôn dùng constants** để so sánh:
   ```javascript
   if (status === StudentStatus.DANG_HOC) // ✅
   if (status === 'Đang học') // ❌
   ```

2. **Dùng helper để lấy text**:
   ```javascript
   StudentStatusHelper.getText(status) // ✅
   ```

3. **Dùng helper để lấy badge**:
   ```javascript
   StudentStatusHelper.getBadge(status) // ✅
   ```

4. **Validate với enum**:
   ```javascript
   StudentStatusHelper.isValid(status) // ✅
   ```
