# Kiến Trúc Hệ Thống - E360 Clone

## 📋 Mục Lục

1. [Tổng Quan Kiến Trúc](#tổng-quan-kiến-trúc)
2. [Sơ Đồ Kiến Trúc](#sơ-đồ-kiến-trúc)
3. [Chi Tiết Các Lớp](#chi-tiết-các-lớp)
4. [Luồng Dữ Liệu](#luồng-dữ-liệu)
5. [Quy Ước Phát Triển](#quy-ước-phát-triển)

---

## 🏗️ Tổng Quan Kiến Trúc

Hệ thống sử dụng **kiến trúc phân tầng (Layered Architecture)** với các thành phần sau:

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                              │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Frontend (e360_clone_fe) - ASP.NET Core MVC + Razor    │    │
│  │  - Controllers (nhận form submit từ browser)            │    │
│  │  - Views (Razor rendering HTML)                         │    │
│  │  - Models/ViewModels                                    │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP Form Submit / Traditional POST
                              │ (Server-side rendering)
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      BACKEND API LAYER                           │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Backend API (e360_clone_api) - ASP.NET Core Web API    │    │
│  │  - Controllers (RESTful endpoints)                      │    │
│  │  - DTOs/Response Models                                 │    │
│  │  - Authentication/Authorization (JWT)                   │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Repository Pattern
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     REPOSITORY LAYER                             │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Repositories (e360_clone.Repositories)                 │    │
│  │  - IRepository<T> interface                             │    │
│  │  - Repository<T> implementation                         │    │
│  │                                   │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Entity Framework Core
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     DATA ACCESS LAYER                            │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  DataAccess (e360_clone.DataAccess)                     │    │
│  │  - AppDbContext (EF Core)                               │    │
│  │  - Entity Configurations                                │    │
│  │  - Migrations                                           │    │
│  │  - DAO (Data Access Objects)                            │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Maps to
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    BUSINESS OBJECTS LAYER                        │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  BusinessObjects (e360_clone.BusinessObjects)           │    │
│  │  - Domain Models/Entities (POCOs)                       │    │
│  │  - Enums                                                │    │
│  │  - Helper Classes                                       │    │
│  │  - Utilities                                            │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📐 Sơ Đồ Kiến Trúc

### Sơ Đồ Tổng Thể

```
┌──────────────┐
│   Browser    │
│   (Client)   │
└──────┬───────┘
       │
       │ 1. Form Submit (POST/GET)
       │    Traditional HTTP Request
       │    (Server-side rendering)
       ▼
┌─────────────────────────────────────────────────────────────┐
│  FRONTEND MVC (e360_clone_fe)                               │
│  Port: 5000 (HTTP) / 5001 (HTTPS)                           │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │ Controllers │  │ Views (Razor)│  │ ViewModels       │   │
│  │ - Nhận form │  │ - Render HTML│  │ - Data transfer  │   │
│  │ - Validate  │  │ - Display    │  │ - Validation     │   │
│  │ - Call API  │  │   data       │  │                  │   │
│  └──────┬──────┘  └──────────────┘  └──────────────────┘   │
└─────────┼───────────────────────────────────────────────────┘
          │
          │ 2. HTTP API Call (Internal)
          │    Only when necessary:
          │    - Dynamic data loading
          │    - AJAX operations
          │    - File uploads/downloads
          ▼
┌─────────────────────────────────────────────────────────────┐
│  BACKEND API (e360_clone_api)                               │
│  Port: 5104 (HTTP) / 7052 (HTTPS)                           │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │ Controllers │  │   DTOs       │  │ Auth (JWT)       │   │
│  │ - RESTful   │  │ - Request    │  │ - Authorization  │   │
│  │   endpoints │  │ - Response   │  │                  │   │
│  └──────┬──────┘  └──────────────┘  └──────────────────┘   │
└─────────┼───────────────────────────────────────────────────┘
          │
          │ 3. Repository Pattern
          │    Dependency Injection
          ▼
┌─────────────────────────────────────────────────────────────┐
│  REPOSITORIES (e360_clone.Repositories)                     │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  IRepository<T>  - Generic repository interface      │   │
│  │  Repository<T>   - Generic repository implementation │   │
│  │  UnitOfWork      - Transaction management            │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────┼────────────────────────────────────────────────────┘
          │
          │ 4. Entity Framework Core
          │    LINQ → SQL
          ▼
┌─────────────────────────────────────────────────────────────┐
│  DATA ACCESS (e360_clone.DataAccess)                        │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  AppDbContext  - EF Core DbContext                   │   │
│  │  DAO           - Data Access Objects                 │   │
│  │  Configurations - Entity configurations              │   │
│  │  Migrations    - Database schema management          │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────┼────────────────────────────────────────────────────┘
          │
          │ 5. Maps to Database Tables
          ▼
┌─────────────────────────────────────────────────────────────┐
│  BUSINESS OBJECTS (e360_clone.BusinessObjects)              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Entities/Models (POCOs):                            │   │
│  │  - Student, Lecturer, Exam, Grade, etc.              │   │
│  │                                                      │   │
│  │  Enums:                                              │   │
│  │  - Gender, StudentStatus, AccountStatus, etc.        │   │
│  │                                                      │   │
│  │  Helpers:                                            │   │
│  │  - PasswordHelper, EnumHelpers, etc.                 │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
          │
          │ 6. Database Connection
          ▼
┌─────────────────────────────────────────────────────────────┐
│  DATABASE (PostgreSQL - Supabase)                           │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Tables: Students, Lecturers, Exams, Grades, etc.    │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔍 Chi Tiết Các Lớp

### 1. Business Objects Layer (`e360_clone.BusinessObjects/`)

**Mục đích:** Chứa các domain models/entities và các lớp hỗ trợ nghiệp vụ

**Cấu trúc:**
```
e360_clone.BusinessObjects/
├── Student.cs              # Entity model
├── Lecturer.cs
├── Exam.cs
├── ExamRoom.cs
├── ExamSchedule.cs
├── ProctorAssignment.cs
├── Grade.cs
├── Attendance.cs
├── Class.cs
├── Subject.cs
├── Account.cs
├── Enums/
│   └── Enums.cs           # All enum definitions + helpers
├── Common/
│   ├── BaseEntity.cs      # Base class for entities
│   └── AuditableEntity.cs # Base class with audit fields
├── Helpers/
│   └── PasswordHelper.cs  # Password hashing utilities
└── Utilities/
    └── ...                # Utility functions
```

**Ví dụ Entity:**
```csharp
namespace e360_clone.BusinessObjects
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
```

**Dependencies:** NONE (không phụ thuộc project nào khác)

---

### 2. Data Access Layer (`e360_clone.DataAccess/`)

**Mục đích:** Chứa EF Core DbContext, DAO, và cấu hình entity

**Cấu trúc:**
```
e360_clone.DataAccess/
├── AppDbContext.cs         # Main DbContext
├── Configurations/
│   ├── StudentConfiguration.cs
│   ├── LecturerConfiguration.cs
│   └── ...
└── Migrations/
    ├── 20240101000000_InitialCreate.cs
    └── ...
```

**Ví dụ DbContext:**
```csharp
namespace e360_clone.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Exam> Exams { get; set; }
        // ...

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply configurations
            modelBuilder.ApplyConfiguration(new StudentConfiguration());
            // ...
        }
    }
}
```

**Dependencies:**
- `e360_clone.BusinessObjects` (để sử dụng entities)
- `Microsoft.EntityFrameworkCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

---

### 3. Repository Layer (`e360_clone.Repositories/`)

**Mục đích:** abstraction layer cho data access, implement repository pattern

**Cấu trúc:**
```
e360_clone.Repositories/
├── IRepository.cs          # Generic repository interface
├── Repository.cs           # Generic repository implementation
├── IUnitOfWork.cs          # Unit of Work interface
├── UnitOfWork.cs           # Unit of Work implementation
└── SpecificRepositories/
    ├── IStudentRepository.cs
    └── StudentRepository.cs
```

**Ví dụ Repository:**
```csharp
namespace e360_clone.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        // ...
    }

    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // ... implement other methods
    }
}
```

**Dependencies:**
- `e360_clone.BusinessObjects`
- `e360_clone.DataAccess`

---

### 4. Backend API Layer (`e360_clone_api/`)

**Mục đích:** RESTful API endpoints, authentication, authorization

**Cấu trúc:**
```
e360_clone_api/
├── Controllers/
│   ├── BaseApiController.cs           # Base controller
│   ├── AuthController.cs
│   ├── AttendancesController.cs
│   ├── ClassesController.cs
│   ├── ExamsController.cs
│   ├── GradesController.cs
│   ├── LecturersController.cs
│   ├── ProctorAssignmentsController.cs
│   ├── RoomsController.cs
│   ├── StudentsController.cs
│   ├── SubjectsController.cs
│   └── WeatherForecastController.cs   # Sample/demo
├── Models/
│   ├── ApiResponse.cs          # Standard response wrapper
│   └── DTOs/
├── Seeders/
│   └── AccountSeeder.cs
├── Migrations/
├── Program.cs                  # DI, CORS, Swagger config
└── appsettings.json            # Connection strings, settings
```

**Ví dụ API Controller:**
```csharp
namespace e360_clone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : BaseApiController
    {
        private readonly IRepository<Student> _studentRepository;

        public StudentsController(IRepository<Student> studentRepository)
        {
            _studentRepository = studentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var students = await _studentRepository.GetAllAsync();
            // ...
            return Ok(new PagedResponse<Student> { ... });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Student student)
        {
            // ...
            return CreatedAtAction(...);
        }
    }
}
```

**Dependencies:**
- `e360_clone.BusinessObjects`
- `e360_clone.DataAccess`
- `e360_clone.Repositories`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Swashbuckle.AspNetCore` (Swagger)

---

### 5. Frontend MVC Layer (`e360_clone_fe/`)

**Mục đích:** MVC frontend, server-side rendering, form handling

**Cấu trúc:**
```
e360_clone_fe/
├── Controllers/
│   ├── BaseController.cs       # Base controller với auth helpers
│   ├── DashboardController.cs
│   ├── HomeController.cs
│   ├── Auth/
│   │   └── AuthController.cs   # Login, Logout
│   ├── StudentsController.cs   # Student management
│   └── ...
├── Models/
│   ├── ViewModels/
│   │   ├── StudentViewModel.cs
│   │   ├── CreateStudentViewModel.cs
│   │   └── ...
│   └── AuthViewModels.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _Header.cshtml
│   │   ├── _Sidebar.cshtml
│   │   └── _ScriptsPartial.cshtml
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Auth/
│   │   └── Login.cshtml
│   ├── Students/
│   │   ├── Index.cshtml        # List (server-side rendered)
│   │   ├── Create.cshtml       # Form
│   │   ├── Edit.cshtml         # Form
│   │   └── Details.cshtml
│   └── ...
├── Services/
│   ├── ApiService.cs           # HTTP client cho API calls
│   └── ApiSettings.cs
├── wwwroot/
│   ├── css/
│   ├── js/
│   │   ├── config.js
│   │   ├── utils.js
│   │   └── _archived/          # Old JS modules (kept for reference)
│   └── lib/
├── Program.cs                  # MVC, Auth, DI config
└── appsettings.json            # ApiSettings, ConnectionStrings
```

**Ví dụ MVC Controller:**
```csharp
namespace e360_clone_fe.Controllers
{
    public class StudentsController : BaseController
    {
        private const string ApiEndpoint = "/students";

        public StudentsController(IApiService apiService, ILogger<StudentsController> logger)
            : base(apiService, logger) { }

        // GET: Students/Index?pageNumber=1&pageSize=10&searchTerm=...
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var queryParams = new Dictionary<string, string>
            {
                { "pageNumber", pageNumber.ToString() },
                { "pageSize", pageSize.ToString() }
            };

            if (!string.IsNullOrEmpty(searchTerm))
                queryParams["searchTerm"] = searchTerm;

            var response = await _apiService.GetAsync<PagedResponse<StudentViewModel>>(ApiEndpoint, queryParams);

            var pagedModel = new PagedViewModel<StudentViewModel>
            {
                Items = response.Data?.Items ?? new List<StudentViewModel>(),
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalRecords = response.TotalRecords,
                SearchTerm = searchTerm
            };

            return View(pagedModel);
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _apiService.PostAsync<StudentViewModel>(ApiEndpoint, model);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Thêm sinh viên thành công";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            return View(model);
        }
    }
}
```

**Ví dụ Razor View (Index.cshtml):**
```razor
@model PagedViewModel<StudentViewModel>

<div class="main-content__wrapper">
    <!-- Breadcrumb -->
    <div class="breadcrumb-area">
        <h1>Danh sách sinh viên</h1>
    </div>

    <!-- Flash Messages -->
    @if (TempData["SuccessMessage"] != null)
    {
        <div class="alert alert-success">@TempData["SuccessMessage"]</div>
    }

    <!-- Search Form -->
    <form method="get">
        <input type="text" name="searchTerm" value="@Model.SearchTerm" />
        <button type="submit">Tìm kiếm</button>
    </form>

    <!-- Data Table (Server-side rendered) -->
    <table class="table">
        <thead>
            <tr>
                <th>STT</th>
                <th>Mã SV</th>
                <th>Họ tên</th>
                <th>Ngày sinh</th>
                <th>Giới tính</th>
                <th>Email</th>
                <th>Trạng thái</th>
                <th>Thao tác</th>
            </tr>
        </thead>
        <tbody>
            @if (Model.Items.Any())
            {
                var index = Model.StartRecord;
                @foreach (var student in Model.Items)
                {
                    <tr>
                        <td>@index</td>
                        <td>@student.StudentCode</td>
                        <td>@student.FullName</td>
                        <td>@student.DateOfBirth.ToString("dd/MM/yyyy")</td>
                        <td>@student.GenderText</td>
                        <td>@student.Email</td>
                        <td>
                            <span class="badge @student.StatusBadgeClass">
                                @student.StatusText
                            </span>
                        </td>
                        <td>
                            <a asp-action="Edit" asp-route-id="@student.Id">Sửa</a>
                            <a asp-action="Details" asp-route-id="@student.Id">Chi tiết</a>
                            <form asp-action="Delete" asp-route-id="@student.Id" method="post" 
                                  style="display:inline"
                                  onsubmit="return confirm('Bạn có chắc?');">
                                @Html.AntiForgeryToken()
                                <button type="submit">Xóa</button>
                            </form>
                        </td>
                    </tr>
                    index++;
                }
            }
            else
            {
                <tr>
                    <td colspan="8" class="text-center">Không có dữ liệu</td>
                </tr>
            }
        </tbody>
    </table>

    <!-- Pagination -->
    @if (Model.TotalPages > 1)
    {
        <nav>
            <ul class="pagination">
                @if (Model.HasPrevious)
                {
                    <li>
                        <a asp-action="Index" asp-route-pageNumber="1">Đầu</a>
                    </li>
                    <li>
                        <a asp-action="Index" asp-route-pageNumber="@(Model.PageNumber - 1)">Trước</a>
                    </li>
                }
                @for (var i = Model.PageNumber - 2; i <= Model.PageNumber + 2; i++)
                {
                    if (i >= 1 && i <= Model.TotalPages)
                    {
                        <li class="@(i == Model.PageNumber ? "active" : "")">
                            <a asp-action="Index" asp-route-pageNumber="@i">@i</a>
                        </li>
                    }
                }
                @if (Model.HasNext)
                {
                    <li>
                        <a asp-action="Index" asp-route-pageNumber="@(Model.PageNumber + 1)">Sau</a>
                    </li>
                    <li>
                        <a asp-action="Index" asp-route-pageNumber="@Model.TotalPages">Cuối</a>
                    </li>
                }
            </ul>
        </nav>
    }
</div>
```

**Dependencies:**
- `e360_clone.BusinessObjects` (để sử dụng enums, helpers)
- `e360_clone.DataAccess` (cho connection string)
- `e360_clone.Repositories` (cho DI)
- `Microsoft.AspNetCore.Authentication.Cookies`

---

## 🔄 Luồng Dữ Liệu

### Luồng Thông Thường (Form Submit - Server-Side Rendering)

```
┌──────────┐      Form Submit       ┌──────────────┐
│ Browser  │ ──────────────────────▶│ FE Controller│
│          │                        │              │
│          │                        │ 1. Validate  │
│          │                        │ 2. Call API  │
│          │                        └──────┬───────┘
│          │                               │
│          │                               │ HTTP POST
│          │                               ▼
│          │                        ┌──────────────┐
│          │                        │ Backend API  │
│          │                        │              │
│          │                        │ 1. Validate  │
│          │                        │ 2. Repository│
│          │                        └──────┬───────┘
│          │                               │
│          │                               │ Repository
│          │                               ▼
│          │                        ┌──────────────┐
│          │                        │ Repository   │
│          │                        │              │
│          │                        │ EF Core      │
│          │                        └──────┬───────┘
│          │                               │
│          │                               │ SQL
│          │                               ▼
│          │                        ┌──────────────┐
│          │                        │ Database     │
│          │                        └──────────────┘
│          │                               │
│          │                        Response │
│          │◀────────────────────────────────┘
│          │
│  HTML    │
│◀─────────┘
│ Render  │
```

**Khi nào dùng Form Submit:**
- ✅ CRUD operations (Create, Update, Delete)
- ✅ Navigation giữa các trang
- ✅ Search với pagination
- ✅ Các thao tác không cần real-time

**Lợi ích:**
- SEO-friendly
- Không cần JavaScript
- Progressive enhancement
- Đơn giản, dễ maintain

---

### Luồng AJAX (Chỉ Khi Cần Thiết)

```
┌──────────┐      AJAX Call         ┌──────────────┐
│ Browser  │ ──────────────────────▶│ FE Controller│
│          │   (Fetch/XMLHttpRequest)│              │
│          │                        │ Call API     │
│          │                        └──────┬───────┘
│          │                               │
│          │                               │ HTTP
│          │                               ▼
│          │                        ┌──────────────┐
│          │                        │ Backend API  │
│          │                        └──────┬───────┘
│          │                               │
│          │                        JSON Response │
│          │◀─────────────────────────────────────┘
│          │
│  JavaScript renders data dynamically
```

**Khi nào dùng AJAX:**
- ✅ Real-time search (typeahead)
- ✅ Dynamic form fields (dependent dropdowns)
- ✅ File upload với progress bar
- ✅ Bulk operations
- ✅ Export/Import
- ✅ Real-time notifications

**Không nên dùng AJAX:**
- ❌ CRUD operations thông thường
- ❌ Navigation
- ❌ Search với pagination (dùng form submit)

---

## 📝 Quy Ước Phát Triển

### 1. Đặt Tên

**Controllers:**
- Backend (hiện có, trong `e360_clone_api/Controllers/`):
  `AuthController`, `AttendancesController`, `ClassesController`, `ExamsController`,
  `GradesController`, `LecturersController`, `ProctorAssignmentsController`,
  `RoomsController`, `StudentsController`, `SubjectsController`,
  `WeatherForecastController` (sample), `BaseApiController` (base).
- Frontend (hiện có, trong `e360_clone_fe/Controllers/`):
  `Auth/AuthController`, `DashboardController`, `HomeController`,
  `StudentsController`, `BaseController` (base).

**Models/ViewModels:**
- Entity: `Student.cs` (trong `BusinessObjects/`)
- ViewModel: `StudentViewModel.cs`, `CreateStudentViewModel.cs`, `UpdateStudentViewModel.cs`
- DTO: `StudentDto.cs`, `CreateStudentDto.cs`

**Views:**
- Folder: `Views/Students/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Details.cshtml`

### 2. Response Format

**API Response:**
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class PagedResponse<T> : ApiResponse<List<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
}
```

**Example:**
```json
{
  "success": true,
  "message": "Thành công",
  "data": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 100
}
```

### 3. Validation

**Backend API:**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] Student student)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new ApiResponse<Student>
        {
            Success = false,
            Message = "Dữ liệu không hợp lệ",
            Data = null
        });
    }
    // ...
}
```

**Frontend MVC:**
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateStudentViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model); // Re-render form with validation messages
    }
    // ...
}
```

**ViewModel:**
```csharp
public class CreateStudentViewModel
{
    [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã sinh viên không quá 20 ký tự")]
    public string StudentCode { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
    public string FullName { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; }
}
```

### 4. Authentication

**Backend API:** JWT Bearer Token
**Frontend MVC:** Cookie Authentication

### 5. Dependency Injection

**Backend API:**
```csharp
// Program.cs
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

**Frontend MVC:**
```csharp
// Program.cs
builder.Services.AddApiService(builder.Configuration);
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

---

## 📊 Database Schema

### Connection String

**Backend API (`appsettings.json`):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=5432;Database=postgres;Username=...;Password=..."
  }
}
```

**Frontend MVC (`appsettings.json`):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=5432;Database=postgres;Username=...;Password=..."
  },
  "ApiSettings": {
    "BaseUrl": "http://localhost:5104/api",
    "HttpsUrl": "https://localhost:7052/api",
    "Timeout": 30
  }
}
```

---

## ✅ Checklist Khi Tạo Module Mới

1. **BusinessObjects:**
   - [ ] Tạo entity class (nếu cần)
   - [ ] Thêm enum (nếu cần)

2. **DataAccess:**
   - [ ] Thêm DbSet vào AppDbContext
   - [ ] Tạo entity configuration (nếu cần)
   - [ ] Tạo migration: `dotnet ef migrations add Add[EntityName]`

3. **Repositories:**
   - [ ] Tạo specific repository interface (nếu cần)
   - [ ] Tạo specific repository implementation (nếu cần)

4. **Backend API:**
   - [ ] Tạo Controller
   - [ ] Implement CRUD endpoints
   - [ ] Thêm authentication/authorization
   - [ ] Test với Swagger

5. **Frontend MVC:**
   - [ ] Tạo ViewModel classes
   - [ ] Tạo Controller
   - [ ] Tạo Views (Index, Create, Edit, Details)
   - [ ] Thêm partial views (nếu cần)
   - [ ] Test form submit

6. **Documentation:**
   - [ ] Cập nhật README
   - [ ] Thêm API documentation

---

## 🚀 Running The Application

```bash
# Terminal 1 - Backend API
cd e360_clone/e360_clone_api
dotnet run
# URL: http://localhost:5104, https://localhost:7052

# Terminal 2 - Frontend MVC
cd e360_clone/e360_clone_fe
dotnet run
# URL: http://localhost:5000

# Access application
http://localhost:5000/Students

# Login
Email: admin@e360.com
Password: 123456
```

---

## 📚 Tài Liệu Tham Khảo

- [ASP.NET Core MVC](https://docs.microsoft.com/en-us/aspnet/core/mvc/)
- [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Repository Pattern](https://docs.microsoft.com/en-us/aspnet/core/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-implementation)
