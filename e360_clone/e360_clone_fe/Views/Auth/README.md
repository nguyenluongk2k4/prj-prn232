# Authentication & Dashboard Flow - E360 Clone

## 📋 Tổng quan

Hệ thống login và redirect đến dashboard tương ứng theo role của user.

## 🔐 Luồng Authentication

```
┌─────────────────┐
│   Login Page    │
│  /Auth/Login    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Quick Login    │───► Chọn role: SuperAdmin, Admin, Student,
│  (Role Buttons) │       Teacher, Parent, Librarian
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  AuthController │
│  Login POST     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Create Claims   │
│ - Name          │
│ - Email         │
│ - Role          │
│ - FullName      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Sign In         │
│ (Cookie Auth)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Redirect theo   │
│ Role            │
└─────────────────┘
```

## 🎯 Role-based Dashboard Redirect

| Role | Dashboard Route | View |
|------|----------------|------|
| SuperAdmin | `/Dashboard/School` | `Views/Dashboard/School.cshtml` |
| Admin | `/Dashboard/School` | `Views/Dashboard/School.cshtml` |
| Student | `/Dashboard/Student` | `Views/Dashboard/Student.cshtml` |
| Teacher | `/Dashboard/Teacher` | `Views/Dashboard/Teacher.cshtml` |
| Parent | `/Dashboard/Parent` | `Views/Dashboard/Parent.cshtml` |
| Librarian | `/Dashboard/School` | `Views/Dashboard/School.cshtml` |

## 📁 Cấu trúc files

### Controllers
```
Controllers/
├── Auth/
│   └── AuthController.cs      # Login, Logout, QuickLogin
├── DashboardController.cs      # Dashboard views (School, Student, Teacher, Parent, LMS)
└── HomeController.cs           # Main page (redirects based on role)
```

### Views
```
Views/
├── Auth/
│   ├── Login.cshtml           # Login page
│   └── AccessDenied.cshtml    # Access denied page
├── Dashboard/
│   ├── School.cshtml          # Admin/School dashboard
│   ├── Student.cshtml         # Student dashboard
│   ├── Teacher.cshtml         # Teacher dashboard
│   ├── Parent.cshtml          # Parent dashboard
│   └── Lms.cshtml             # LMS dashboard
└── Shared/
    ├── _Layout.cshtml         # Main layout (có sidebar/header)
    ├── _LayoutAuth.cshtml     # Auth layout (không sidebar/header)
    ├── _Sidebar.cshtml        # Sidebar navigation
    ├── _Header.cshtml         # Top header
    └── ...
```

## 🔧 Cấu hình Authentication

### Program.cs
```csharp
// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();
```

### AuthController.cs
```csharp
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model, string? role = null)
{
    var userRole = role ?? "Admin";
    
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, model.Email),
        new Claim(ClaimTypes.Email, model.Email),
        new Claim(ClaimTypes.Role, userRole),
        new Claim("FullName", "Demo User")
    };

    var claimsIdentity = new ClaimsIdentity(
        claims, CookieAuthenticationDefaults.AuthenticationScheme);

    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        authProperties);

    // Redirect based on role
    return RedirectToAction("Index", "Dashboard", new { area = userRole });
}
```

## 🚀 Test Login

### 1. Truy cập Login Page
```
http://localhost:5000/Auth/Login
```

### 2. Quick Login (Recommended)
Click vào một trong các nút quick login:
- **Super Admin** → School Dashboard
- **Admin** → School Dashboard
- **Student** → Student Dashboard
- **Teacher** → Teacher Dashboard
- **Guardians** → Parent Dashboard
- **Librarian** → School Dashboard

### 3. Form Login
Nhập bất kỳ email/password nào (demo mode):
- Email: `admin@example.com`
- Password: `password123`

## 📊 Dashboard Features

### School Dashboard (Admin)
- Statistics: Students, Teachers, Classes, Exams
- Quick Actions: Students, Teachers, Exams, Classes
- Recent Activity Table

### Student Dashboard
- Welcome card with student info
- Statistics: Courses, Attendance, Exams, GPA
- Quick Links: Exam Schedule, Results, Study Materials
- Upcoming Exams Table

### Teacher Dashboard
- Welcome card with teacher info
- Statistics: Classes, Students, Proctor Duties, Pending Grades
- Quick Actions: Enter Grades, Take Attendance, Proctor Schedule
- Today's Schedule Table

### Parent Dashboard
- Children profile cards
- Statistics: Attendance, GPA, Exams, Fees
- Quick Links: View Results, Exam Schedule, Contact Teacher
- Recent Exam Results Table

### LMS Dashboard
- Statistics: Courses, Completed, In Progress, Certificates
- My Courses with progress bars
- Quick Resources: Video Lectures, Study Materials, Assignments
- Recent Activity

## 🔒 Authorization

### Attribute-based Authorization
```csharp
[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return role switch
        {
            "Student" => RedirectToAction("Student", "Dashboard"),
            "Teacher" => RedirectToAction("Teacher", "Dashboard"),
            "Parent" => RedirectToAction("Parent", "Dashboard"),
            _ => RedirectToAction("School", "Dashboard")
        };
    }
}
```

### Logout
```csharp
[HttpPost]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Login");
}
```

## 📝 Next Steps

1. **Database Integration**: Connect to actual user database
2. **Password Hashing**: Implement BCrypt/Argon2
3. **Role-based Permissions**: Fine-grained access control
4. **Remember Me**: Implement persistent login
5. **2FA**: Two-factor authentication
6. **Password Reset**: Forgot password flow
7. **User Registration**: Sign up functionality
