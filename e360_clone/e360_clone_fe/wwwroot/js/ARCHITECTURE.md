# Frontend JavaScript Architecture - E360 Clone

## 📁 Cấu trúc thư mục

```
wwwroot/js/
├── core/                      # Core libraries (dùng chung toàn hệ thống)
│   ├── http.js               # Base HTTP client (GET, POST, PUT, DELETE)
│   └── auth.js               # Auth helper (login, logout, token management)
│
├── helpers/                   # Global helpers (optional)
│   └── ...                   # Các helper dùng chung (nếu có)
│
├── modules/                   # Feature modules
│   ├── auth/
│   │   └── api.js            # Auth API calls
│   ├── students/
│   │   ├── api.js            # Student API calls
│   │   ├── helpers.js        # Student-specific helpers
│   │   └── ui.js             # Student UI logic
│   ├── teachers/
│   │   ├── api.js
│   │   ├── helpers.js
│   │   └── ui.js
│   └── ...
│
├── config.js                  # App configuration
└── utils.js                   # Global utilities (toast, loading, date format)
```

## 🏗️ Kiến trúc phân lớp

### **Layer 1: Core (Base)**
- `Http` - HTTP client wrapper
- `Auth` - Authentication helper

### **Layer 2: Module API**
- `StudentsApi`, `TeachersApi`, etc. - API calls cho từng module

### **Layer 3: Module Helpers**
- `StudentsHelper`, `TeachersHelper`, etc. - Helper functions riêng của module

### **Layer 4: Module UI**
- `StudentsUI`, `TeachersUI`, etc. - UI logic, event handlers, rendering

## 📖 Cách sử dụng

### **1. Core HTTP Client**

```javascript
// GET request
const response = await Http.get('/students', { page: 1, pageSize: 10 });

// POST request
const response = await Http.post('/students', { name: 'John' });

// PUT request
const response = await Http.put('/students/1', { name: 'Jane' });

// DELETE request
const response = await Http.delete('/students/1');

// Upload file
const formData = new FormData();
formData.append('file', fileInput.files[0]);
const response = await Http.upload('/students/import', formData);

// Download file
const blob = await Http.download('/students/export', { format: 'excel' });
```

**Features:**
- ✅ Tự động attach JWT token vào Authorization header
- ✅ Handle 401 Unauthorized → redirect to login
- ✅ Timeout support
- ✅ Error handling

### **2. Core Auth Helper**

```javascript
// Check if authenticated
if (Auth.isAuthenticated()) {
    // User is logged in
}

// Get user info
const user = Auth.getUser();
console.log(user.fullName, user.role);

// Get user role
const role = Auth.getRole();

// Check role
if (Auth.hasRole('Admin')) {
    // User is admin
}

// Check multiple roles
if (Auth.hasRole(['Admin', 'SuperAdmin'])) {
    // User is admin or super admin
}

// Login & save token
Auth.setAuth(token, userInfo);

// Logout
Auth.logout();

// Logout with custom redirect
Auth.logout('/Custom/Redirect');

// Redirect by role after login
Auth.redirectByRole();
```

### **3. Module Pattern (Students Example)**

#### **api.js** - API calls
```javascript
// Get all students
const response = await StudentsApi.getAll(1, 10, { status: 'Active' });

// Get by ID
const student = await StudentsApi.getById(1);

// Create
await StudentsApi.create({ studentCode: 'SV001', fullName: 'John' });

// Update
await StudentsApi.update(1, { fullName: 'Jane' });

// Delete
await StudentsApi.delete(1);

// Search
const results = await StudentsApi.search('John', 1, 10);
```

#### **helpers.js** - Helper functions
```javascript
// Format student data
const formatted = StudentsHelper.formatStudent(student);

// Get status badge class
const badgeClass = StudentsHelper.getStatusClass('Active'); // 'bg-success'

// Get status text
const statusText = StudentsHelper.getStatusText('Active'); // 'Đang học'

// Validate form
const validation = StudentsHelper.validateForm(formData);
if (validation.isValid) {
    // Form is valid
} else {
    console.log(validation.errors);
}

// Create table row HTML
const rowHtml = StudentsHelper.createStudentRow(student, index);
```

#### **ui.js** - UI logic
```javascript
// Initialize (auto-called on DOM ready)
StudentsUI.init();

// Load students
StudentsUI.loadStudents(1);

// Search
StudentsUI.search();

// Edit student
StudentsUI.edit(1);

// Delete student
StudentsUI.delete(1, 'Student Name');

// Prepare add form
StudentsUI.prepareAdd();

// Save (create/update)
StudentsUI.save();
```

## 🔐 Authentication Flow

### **Login Page**
```html
<!-- Login.cshtml -->
@section Scripts {
    <script src="~/js/core/http.js"></script>
    <script src="~/js/core/auth.js"></script>
    <script src="~/js/modules/auth/api.js"></script>
    <script>
        // Handle login form
        document.getElementById('loginForm').addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const response = await AuthApi.login(email, password);
            
            if (response.success) {
                // Save token & user info
                Auth.setAuth(response.data.token, {
                    username: response.data.username,
                    role: response.data.role,
                    fullName: response.data.fullName
                });
                
                // Redirect based on role
                Auth.redirectByRole();
            }
        });
    </script>
}
```

### **Protected Page**
```html
<!-- Students/Index.cshtml -->
@section Scripts {
    <script src="~/js/core/http.js"></script>
    <script src="~/js/core/auth.js"></script>
    <script src="~/js/modules/students/api.js"></script>
    <script src="~/js/modules/students/helpers.js"></script>
    <script src="~/js/modules/students/ui.js"></script>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            // Check authentication
            if (!Auth.isAuthenticated()) {
                Auth.setRedirectUrl(window.location.pathname);
                window.location.href = '/Auth/Login';
                return;
            }
            
            // Initialize module
            StudentsUI.init();
        });
    </script>
}
```

### **Global Layout**
```html
<!-- _ScriptsPartial.cshtml -->
<!-- Core libraries (load first) -->
<script src="~/js/core/http.js"></script>
<script src="~/js/core/auth.js"></script>

<!-- App utilities -->
<script src="~/js/config.js"></script>
<script src="~/js/utils.js"></script>

<!-- Module scripts (loaded per page via @section Scripts) -->
@await RenderSectionAsync("Scripts", required: false)

<script>
  // Load user info into sidebar
  const userInfo = Auth.getUser();
  if (userInfo) {
      document.getElementById('sidebarUserName').textContent = userInfo.fullName;
      document.getElementById('sidebarUserRole').textContent = userInfo.role;
  }
  
  // Handle logout
  document.getElementById('logoutBtn')?.addEventListener('click', function() {
      Auth.logout();
  });
</script>
```

## 📝 Tạo Module Mới

### **1. Tạo folder module**
```
wwwroot/js/modules/teachers/
```

### **2. Tạo api.js**
```javascript
const TeachersApi = (function() {
    const ENDPOINTS = {
        BASE: '/teachers',
        BY_ID: (id) => `/teachers/${id}`
    };

    async function getAll(page = 1, pageSize = 10) {
        return Http.get(ENDPOINTS.BASE, { pageNumber: page, pageSize });
    }

    async function getById(id) {
        return Http.get(ENDPOINTS.BY_ID(id));
    }

    async function create(data) {
        return Http.post(ENDPOINTS.BASE, data);
    }

    async function update(id, data) {
        return Http.put(ENDPOINTS.BY_ID(id), data);
    }

    async function deleteTeacher(id) {
        return Http.delete(ENDPOINTS.BY_ID(id));
    }

    return {
        getAll,
        getById,
        create,
        update,
        delete: deleteTeacher
    };
})();
```

### **3. Tạo helpers.js**
```javascript
const TeachersHelper = (function() {
    function formatTeacher(teacher) {
        return {
            ...teacher,
            formattedDate: formatDate(teacher.dateOfBirth),
            statusText: getStatusText(teacher.status)
        };
    }

    function getStatusText(status) {
        return { 'Active': 'Đang làm việc' }[status] || status;
    }

    return {
        formatTeacher,
        getStatusText
    };
})();
```

### **4. Tạo ui.js**
```javascript
const TeachersUI = (function() {
    function init() {
        loadTeachers(1);
        bindEvents();
    }

    async function loadTeachers(page) {
        const response = await TeachersApi.getAll(page, 10);
        renderTable(response.data);
    }

    function renderTable(teachers) {
        // Render logic
    }

    function bindEvents() {
        // Event handlers
    }

    return {
        init,
        loadTeachers
    };
})();
```

### **5. Tạo View**
```html
@{
    ViewData["Title"] = "Teachers";
}

<!-- Your HTML here -->

@section Scripts {
    <script src="~/js/core/http.js"></script>
    <script src="~/js/core/auth.js"></script>
    <script src="~/js/modules/teachers/api.js"></script>
    <script src="~/js/modules/teachers/helpers.js"></script>
    <script src="~/js/modules/teachers/ui.js"></script>
}
```

## ✅ Best Practices

1. **Không gọi Http trực tiếp trong UI** - Luôn qua Api layer
2. **Helpers chỉ xử lý data** - Không gọi API
3. **UI chỉ xử lý rendering & events** - Không gọi Http trực tiếp
4. **Luôn check Auth** - Trước khi load data
5. **Error handling** - Try/catch trong UI layer
6. **Loading states** - Show loading khi call API
7. **Token handling** - Tự động qua Http core

## 🚀 Benefits

✅ **Separation of Concerns** - Mỗi layer có trách nhiệm riêng  
✅ **Reusability** - Core dùng chung, modules độc lập  
✅ **Maintainability** - Dễ tìm, dễ sửa  
✅ **Scalability** - Thêm module mới dễ dàng  
✅ **Testability** - Dễ unit test từng layer
