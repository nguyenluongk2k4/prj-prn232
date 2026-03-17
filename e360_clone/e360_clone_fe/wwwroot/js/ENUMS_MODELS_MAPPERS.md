# Enums, Models & Mappers - E360 Clone Frontend

## 📁 Cấu trúc

```
wwwroot/js/
├── enums/                    # Enum definitions
│   ├── AccountStatus.js      # Account status enum
│   ├── Role.js               # User roles enum
│   ├── StudentStatus.js      # Student status enum
│   ├── Gender.js             # Gender enum
│   ├── ExamStatus.js         # Exam status enum
│   ├── GradeType.js          # Grade type enum
│   ├── AttendanceStatus.js   # Attendance status enum
│   └── index.js              # Export all enums
│
├── models/                   # Data models
│   ├── ApiResponse.js        # API response wrappers
│   ├── Student.js            # Student model
│   ├── Account.js            # Account model
│   ├── UserInfo.js           # User info model
│   ├── Exam.js               # Exam model
│   └── index.js              # Export all models
│
├── mappers/                  # Data mappers
│   ├── StudentMapper.js      # Student mapping logic
│   ├── AccountMapper.js      # Account mapping logic
│   ├── ExamMapper.js         # Exam mapping logic
│   └── index.js              # Export all mappers
│
└── modules/                  # Feature modules
    └── students/
        ├── api.js            # Uses StudentMapper
        ├── helpers.js        # Uses enums
        └── ui.js             # Uses mapped data
```

## 🔢 Enums

### **AccountStatus**
```javascript
AccountStatus.Active        // 'Active'
AccountStatus.Inactive      // 'Inactive'
AccountStatus.Locked        // 'Locked'
AccountStatus.Suspended     // 'Suspended'

// Helper
AccountStatusHelper.getText('Active')      // 'Hoạt động'
AccountStatusHelper.getBadge('Active')     // 'bg-success'
AccountStatusHelper.isValid('Active')      // true
AccountStatusHelper.getAllWithText()       // [{value, text}, ...]
```

### **Role**
```javascript
Role.SuperAdmin     // 'SuperAdmin'
Role.Admin          // 'Admin'
Role.Student        // 'Student'
Role.Teacher        // 'Teacher'
Role.Parent         // 'Parent'
Role.Librarian      // 'Librarian'

// Helper
RoleHelper.getText('Admin')           // 'Quản trị viên'
RoleHelper.getIcon('Admin')           // 'ri-admin-line'
RoleHelper.hasPermission('Admin', 'users.manage')  // true
RoleHelper.isAdmin('Admin')           // true
```

### **StudentStatus**
```javascript
StudentStatus.Active          // 'Active'
StudentStatus.Inactive        // 'Inactive'
StudentStatus.Graduated       // 'Graduated'
StudentStatus.Suspended       // 'Suspended'
StudentStatus.Expelled        // 'Expelled'

// Helper
StudentStatusHelper.getText('Active')    // 'Đang học'
StudentStatusHelper.getBadge('Active')   // 'bg-success'
StudentStatusHelper.isActive('Active')   // true
```

### **Gender**
```javascript
Gender.Male       // 'Male'
Gender.Female     // 'Female'
Gender.Other      // 'Other'

// Helper
GenderHelper.getText('Male')    // 'Nam'
GenderHelper.getIcon('Male')    // 'ri-mars-line'
```

### **ExamStatus**
```javascript
ExamStatus.Planned       // 'Planned'
ExamStatus.Scheduled     // 'Scheduled'
ExamStatus.InProgress    // 'InProgress'
ExamStatus.Completed     // 'Completed'
ExamStatus.Cancelled     // 'Cancelled'
```

### **GradeType**
```javascript
GradeType.Quiz          // 'Quiz'
GradeType.Midterm       // 'Midterm'
GradeType.Final         // 'Final'
GradeType.Assignment    // 'Assignment'
GradeType.Project       // 'Project'

// Helper
GradeTypeHelper.getWeight('Final')    // 0.5
GradeTypeHelper.getText('Final')      // 'Cuối kỳ'
```

### **AttendanceStatus**
```javascript
AttendanceStatus.Present       // 'Present'
AttendanceStatus.Absent        // 'Absent'
AttendanceStatus.Late          // 'Late'
AttendanceStatus.Excused       // 'Excused'
AttendanceStatus.EarlyLeave    // 'EarlyLeave'
```

## 📦 Models

### **ApiResponse**
```javascript
const response = new ApiResponse({
    success: true,
    message: 'Success',
    data: [...]
});

response.isSuccess    // true
response.isError      // false
```

### **PagedResponse**
```javascript
const paged = new PagedResponse({
    success: true,
    data: [...],
    pageNumber: 1,
    pageSize: 10,
    totalRecords: 100,
    totalPages: 10
});

paged.isEmpty      // false
paged.hasData      // true
paged.hasNext      // true
```

### **PagedRequest**
```javascript
const request = PagedRequest.create(1, 10, { status: 'Active' });
const params = request.toParams();
// { pageNumber: 1, pageSize: 10, status: 'Active' }
```

### **Student Model**
```javascript
const student = new Student({
    id: 1,
    studentCode: 'SV001',
    fullName: 'Nguyen Van A',
    dateOfBirth: '2000-01-01',
    gender: 'Male',
    email: 'a@example.com',
    status: 'Active'
});

// Computed properties (after mapping)
student.formattedDateOfBirth    // '01/01/2000'
student.statusText              // 'Đang học'
student.statusBadge             // 'bg-success'
student.genderText              // 'Nam'
student.age                     // 26

// Methods
student.toCreateDto()    // For API create
student.toUpdateDto()    // For API update
```

### **Account Model**
```javascript
const account = new Account({
    id: 1,
    username: 'admin',
    email: 'admin@e360.com',
    role: 'Admin',
    fullName: 'System Admin',
    status: 'Active'
});

// Computed properties (after mapping)
account.roleText        // 'Quản trị viên'
account.roleIcon        // 'ri-admin-line'
account.statusText      // 'Hoạt động'
account.displayName     // 'System Admin'
account.isAdmin         // true
```

### **UserInfo Model**
```javascript
const userInfo = new UserInfo({
    username: 'admin',
    email: 'admin@e360.com',
    fullName: 'System Admin',
    role: 'Admin'
});

// Static methods
UserInfo.fromJwtResponse(apiData);
UserInfo.fromStorage(localStorage.getItem('userInfo'));

// Methods
userInfo.toStorage();    // JSON.stringify
userInfo.displayName     // 'System Admin'
userInfo.initials        // 'SA'
```

## 🔄 Mappers

### **StudentMapper**
```javascript
// Map from API response
const student = StudentMapper.fromApi(apiData);
// Returns Student model with computed properties

// Map list
const students = StudentMapper.fromApiList(apiDataList);

// Map paged response
const paged = StudentMapper.fromPagedResponse(apiResponse);

// To DTO
const createDto = StudentMapper.toCreateDto(student);
const updateDto = StudentMapper.toUpdateDto(student);

// Format helpers
StudentMapper.formatDate('2000-01-01');     // '01/01/2000'
StudentMapper.formatDateForInput(date);     // '2000-01-01'
```

### **AccountMapper**
```javascript
// Map from API
const account = AccountMapper.fromApi(apiData);

// Map login response to UserInfo
const userInfo = AccountMapper.fromLoginResponse(loginResponse);

// To DTO
const createDto = AccountMapper.toCreateDto(account);
const updateDto = AccountMapper.toUpdateDto(account);
```

### **ExamMapper**
```javascript
// Map from API
const exam = ExamMapper.fromApi(apiData);

// Format helpers
ExamMapper.formatDate('2024-01-15');        // '15/01/2024'
ExamMapper.formatTimeRange('08:00', '10:00'); // '08:00 - 10:00'
```

## 📝 Usage Examples

### **1. Load Students with Mapping**
```javascript
async function loadStudents() {
    // API returns mapped data automatically
    const response = await StudentsApi.getAll(1, 10, { status: 'Active' });
    
    // Data is already mapped with enums applied
    response.data.forEach(student => {
        console.log(student.statusText);    // 'Đang học'
        console.log(student.statusBadge);   // 'bg-success'
        console.log(student.formattedDateOfBirth);
    });
    
    renderTable(response.data);
}
```

### **2. Create Student**
```javascript
async function createStudent(formData) {
    // Validate using enum
    if (!StudentStatusHelper.isValid(formData.status)) {
        showToast('Invalid status', 'danger');
        return;
    }
    
    // API automatically maps to DTO
    const response = await StudentsApi.create(formData);
    
    if (response.success) {
        showToast('Student created', 'success');
    }
}
```

### **3. Render with Enum Values**
```javascript
function renderStudentRow(student) {
    // Use enum helpers for consistent display
    const statusText = StudentStatusHelper.getText(student.status);
    const statusBadge = StudentStatusHelper.getBadge(student.status);
    const genderText = GenderHelper.getText(student.gender);
    const genderIcon = GenderHelper.getIcon(student.gender);
    
    return `
        <tr>
            <td>${student.studentCode}</td>
            <td>${student.fullName}</td>
            <td><span class="badge ${statusBadge}">${statusText}</span></td>
            <td><i class="${genderIcon}"></i> ${genderText}</td>
        </tr>
    `;
}
```

### **4. Role-based UI**
```javascript
function renderMenu() {
    const userRole = Auth.getRole();
    
    // Check permissions using enum
    if (RoleHelper.hasPermission(userRole, 'students.manage')) {
        showMenuItem('students');
    }
    
    // Check if admin
    if (RoleHelper.isAdmin(userRole)) {
        showAdminMenu();
    }
    
    // Get role display text
    const roleText = RoleHelper.getText(userRole);
    document.getElementById('role').textContent = roleText;
}
```

### **5. Dropdown Options from Enum**
```javascript
function populateStatusDropdown() {
    const select = document.getElementById('statusSelect');
    
    // Get all options with text from enum
    const options = StudentStatusHelper.getAllWithText();
    
    select.innerHTML = options.map(opt => `
        <option value="${opt.value}">${opt.text}</option>
    `).join('');
}
```

## ✅ Benefits

### **Type Safety**
```javascript
// ❌ Hard-coded strings (error-prone)
if (student.status === 'Active') { }

// ✅ Enum (type-safe)
if (student.status === StudentStatus.Active) { }
```

### **Consistency**
```javascript
// All modules use same enum values
StudentStatusHelper.getText('Active')    // 'Đang học'
AccountStatusHelper.getText('Active')    // 'Hoạt động'
```

### **Centralized Mapping**
```javascript
// API response automatically mapped
const response = await StudentsApi.getAll();
// response.data is array of Student models with computed properties
```

### **Easy to Maintain**
- Change enum text in one place
- Update mapping logic in mapper files
- Models define data structure clearly

## 📖 Best Practices

1. **Always use enums** for status, role, type values
2. **Map API responses** before using in UI
3. **Use helper methods** from enums for display
4. **Validate with enums** before submitting forms
5. **Keep models clean** - computed properties from mappers
6. **DTO conversion** in mappers, not in UI
