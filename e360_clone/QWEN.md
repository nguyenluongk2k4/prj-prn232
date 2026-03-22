# QWEN.md - Project Context Guide

## Project Overview

**e360_clone** — Nền tảng quản lý lịch thi & coi thi (Exam Scheduling & Proctoring Management System)

A monorepo containing a clone of FPT's e360 system for managing exam schedules, proctor assignments, and exam results in an academic environment.

### Architecture

| Component | Path | Technology |
|-----------|------|------------|
| **Backend API** | `e360_clone_api/` | ASP.NET Core 8.0 Web API (C#) |
| **Frontend** | `e360_clone_fe/` | ASP.NET Core 8.0 MVC + AJAX |
| **BusinessObjects** | `e360_clone.BusinessObjects/` | Class Library - Domain models (entities) |
| **DataAccess** | `e360_clone.DataAccess/` | Class Library - EF Core DbContext, DAOs, Migrations |
| **Repositories** | `e360_clone.Repositories/` | Class Library - Repository pattern |

### Solution Structure

```
C:\Git\prj-prn232\e360_clone\
├── e360_clone.slnx              # Solution file
├── e360_clone_api/              # Web API project
│   ├── Controllers/
│   ├── Migrations/              # [DEPRECATED] Move to DataAccess
│   ├── Seeders/
│   ├── Program.cs
│   └── appsettings.json
├── e360_clone_fe/               # MVC Frontend project
│   ├── Controllers/
│   ├── Views/
│   ├── Extensions/
│   ├── Services/
│   ├── Models/
│   └── wwwroot/
├── e360_clone.BusinessObjects/  # Domain models (entities)
│   ├── Student.cs
│   ├── Lecturer.cs
│   ├── Exam.cs
│   ├── StudentSubject.cs        # NEW: Student-Subject enrollment
│   ├── CourseSession.cs         # NEW: Class sessions
│   ├── StudentAttendance.cs     # NEW: Attendance records
│   ├── ExamForm.cs              # NEW: Exam forms (MCQ, Essay, etc.)
│   ├── StudentExam.cs           # NEW: Student-Exam registration
│   └── ...
├── e360_clone.DataAccess/       # EF Core DbContext, DAOs, Migrations
│   ├── AppDbContext.cs
│   ├── Configurations/          # Entity configurations
│   ├── DAOs/                    # Data Access Objects
│   │   ├── BaseDAO.cs
│   │   └── AccountDAO.cs
│   └── Migrations/              # [REQUIRED] All migrations go here
└── e360_clone.Repositories/     # Repository pattern
    ├── IRepository.cs
    └── Repository.cs
```

### Project Dependencies

```
e360_clone_api
├── e360_clone.BusinessObjects
├── e360_clone.DataAccess
└── e360_clone.Repositories

e360_clone_fe
├── e360_clone.BusinessObjects
├── e360_clone.DataAccess
├── e360_clone.Repositories
└── e360_clone_fe.Services (ApiService)

e360_clone.Repositories
├── e360_clone.BusinessObjects
└── e360_clone.DataAccess

e360_clone.DataAccess
└── e360_clone.BusinessObjects
```

### API Communication Pattern

- Frontend serves static HTML/JS files from `wwwroot/`
- JavaScript makes AJAX/fetch calls to backend API
- API returns JSON responses
- Frontend renders data dynamically in the browser

## Building and Running

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- PostgreSQL database (Supabase recommended)

### Database Configuration

1. Update connection string in `e360_clone_api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-host;Port=5432;Database=postgres;Username=your-user;Password=your-password"
  }
}
```

2. Run migrations to create database schema:
```bash
# Migrations MUST be in DataAccess project
cd e360_clone/e360_clone.DataAccess
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Backend API (`e360_clone_api/`)

```bash
cd e360_clone/e360_clone_api
dotnet restore          # Restore NuGet packages
dotnet build            # Build project
dotnet run              # Run API server
dotnet watch run        # Run with hot-reload
```

**Default URLs:**
- HTTP: `http://localhost:5104`
- HTTPS: `https://localhost:7052`
- Swagger UI: `/swagger` (Development mode only)

### Frontend MVC (`e360_clone_fe/`)

```bash
cd e360_clone/e360_clone_fe
dotnet restore          # Restore NuGet packages
dotnet build            # Build project
dotnet run              # Run static file server
dotnet watch run        # Run with hot-reload
```

**Frontend URL:** `http://localhost:5000` (or as configured in `launchSettings.json`)

**API Integration:**
- Frontend calls API via AJAX/fetch
- Configure API base URL in `wwwroot/js/config.js`
- Example: `fetch('http://localhost:5104/api/students')`

### Running Both Projects

Use the solution file in Visual Studio or run both projects simultaneously:

```bash
# Terminal 1 - API
cd e360_clone/e360_clone_api && dotnet run

# Terminal 2 - Frontend
cd e360_clone/e360_clone_fe && dotnet run
```

**Access the application:** `http://localhost:5000`

## Business Domain

### User Roles

| Role | Responsibilities |
|------|------------------|
| **Admin** | System administration, user management, role assignment |
| **Nhân viên giáo vụ** (Academic Staff) | Exam scheduling, proctor assignment, attendance management |
| **Giảng viên** (Lecturer) | Proctoring, attendance taking, grade entry |
| **Sinh viên** (Student) | View exam schedules, view grades, check-in for exams |

### Core Business Modules

1. **Quản lý Lịch thi** (Exam Schedule Management)
   - Create and manage class exam schedules
   - Create and manage graduation exam schedules
   - Room allocation for exam sessions
   - Export room lists for proctors

2. **Phân công Coi thi** (Proctor Assignment)
   - Assign lecturers to proctor class exams
   - Assign lecturers to proctor graduation exams
   - Manage proctor information and assignments

3. **Quản lý Điểm thi** (Grade Management)
   - Lecturers enter grades for courses
   - Enter graduation exam grades
   - Students view their exam grades
   - Print class grade reports

4. **Quản lý Dữ liệu nền** (Master Data Management)
   - Student information management
   - Lecturer information management
   - Course catalog management
   - Class management
   - Major/Program management

5. **Báo cáo và Thống kê** (Reports & Statistics)
   - Generate student transcripts
   - Print exam class lists
   - Exam result statistics by period
   - Export various management reports

## Development Conventions

### Code Style
- **Namespace**: `e360_clone` for backend, `e360_clone_fe` for frontend
- **C# Version**: Latest (implicit usings enabled)
- **Nullable Reference Types**: Enabled
- **Language**: Vietnamese for business logic, UI text, and documentation

### Project Configuration
- Both projects target `.NET 8.0`
- Swagger/OpenAPI enabled for API documentation (development only)
- Bootstrap 5 and jQuery included in frontend `wwwroot/`

### API Design
- Controller-based routing with `[controller]` attribute
- Base controller: `BaseApiController` with common response patterns
- RESTful conventions for CRUD operations
- JSON responses wrapped in `ApiResponse<T>` or `PagedResponse<T>`
- Swagger UI available at `/swagger` in development
- CORS enabled for frontend API calls

### Frontend (AJAX Pattern)
- ASP.NET Core MVC with Razor Views
- Static files (JS, CSS) in `wwwroot/`
- Views in `Views/` folder, organized by feature
- **config.js**: Centralized environment config (API_BASE_URL, timeout, etc.)
- **utils.js**: Common utilities (toast, loading, date formatting)
- **core/http.js**: Base HTTP client with fetch wrapper
- **core/auth.js**: Authentication helper
- **modules/[feature]/api.js**: API service per module
- **modules/[feature]/ui.js**: UI logic per module
- Dynamic rendering via JavaScript DOM manipulation
- Bootstrap 5 for UI components and styling

### File Organization

**Backend API:**
```
e360_clone_api/
├── Controllers/
│   ├── BaseApiController.cs    # Base controller with common patterns
│   ├── AuthController.cs       # Authentication endpoints
│   ├── StudentsController.cs   # Student CRUD operations (sample)
│   └── WeatherForecastController.cs
├── Migrations/                 # [DEPRECATED] Move to DataAccess
├── Seeders/                    # Database seeders
│   └── AccountSeeder.cs
├── Program.cs                  # DI, CORS, Swagger config
└── appsettings.json            # Connection strings, API settings
```

**BusinessObjects (Class Library):**
```
e360_clone.BusinessObjects/
├── Student.cs
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
├── AppUser.cs                  # NEW: User model for session auth
├── StudentSubject.cs           # NEW: Student-Subject enrollment
├── CourseSession.cs            # NEW: Class sessions
├── StudentAttendance.cs        # NEW: Attendance records
├── ExamForm.cs                 # NEW: Exam forms
├── StudentExam.cs              # NEW: Student-Exam registration
├── Common/                     # Shared base classes
├── Enums/                      # Enum definitions (Gender, Status, etc.)
├── Helpers/                    # Helper utilities (PasswordHelper)
└── Utilities/                  # Utility functions
```

**DataAccess (Class Library):**
```
e360_clone.DataAccess/
├── AppDbContext.cs             # EF Core DbContext
├── Configurations/             # Entity configurations (IEntityTypeConfiguration)
│   ├── StudentConfiguration.cs
│   ├── AccountConfiguration.cs
│   ├── StudentSubjectConfiguration.cs
│   ├── CourseSessionConfiguration.cs
│   ├── StudentAttendanceConfiguration.cs
│   ├── ExamFormConfiguration.cs
│   └── StudentExamConfiguration.cs
├── DAOs/                       # Data Access Objects
│   ├── BaseDAO.cs              # Base class for all DAOs
│   └── AccountDAO.cs           # Account-specific DAO
└── Migrations/                 # [REQUIRED] All EF Core migrations
    ├── 20260315035226_InitialCreate.cs
    ├── 20260315081332_SeedInitialData.cs
    ├── 20260316170242_AddAccountTable.cs
    └── 20260321000000_AddStudentExamManagement.cs
```

**Repositories (Class Library):**
```
e360_clone.Repositories/
├── IRepository.cs              # Generic repository interface
├── Repository.cs               # Generic repository implementation
├── IAccountRepository.cs       # Account-specific repository
├── AccountRepository.cs        # Account repository (uses AccountDAO)
└── UnitOfWork.cs               # Unit of Work pattern
```

**Frontend:**
```
e360_clone_fe/
├── Controllers/
│   ├── BaseController.cs       # Base controller with auth helpers
│   ├── HomeController.cs
│   ├── Auth/
│   │   └── AuthController.cs   # Login, Logout
│   ├── StudentsController.cs   # Student management
│   └── Dashboard/
│       └── DashboardController.cs
├── Models/
│   ├── ViewModels/
│   │   ├── StudentViewModel.cs
│   │   ├── CreateStudentViewModel.cs
│   │   └── PagedViewModel.cs
│   └── AuthViewModels.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml      # Main layout
│   │   ├── _LayoutAuth.cshtml  # Auth layout (login page)
│   │   ├── _Header.cshtml
│   │   ├── _Sidebar.cshtml
│   │   ├── _Footer.cshtml
│   │   ├── _ScriptsPartial.cshtml
│   │   └── _HeadPartial.cshtml
│   ├── Auth/
│   │   └── Login.cshtml
│   ├── Dashboard/
│   │   ├── School.cshtml
│   │   ├── Student.cshtml
│   │   ├── Teacher.cshtml
│   │   ├── Parent.cshtml
│   │   └── Lms.cshtml
│   └── Students/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       └── Details.cshtml
├── Services/
│   ├── ApiService.cs           # HTTP client for calling backend API
│   └── ApiSettings.cs          # API settings class
├── Extensions/
│   └── AppUserExtensions.cs    # Session-based user extensions
├── wwwroot/
│   ├── css/
│   ├── js/
│   │   ├── config.js
│   │   ├── utils.js
│   │   └── _archived/          # Old JS modules
│   └── assets/                 # Template assets (CSS, JS, images)
├── Program.cs                  # MVC, Auth, Session, DI config
└── appsettings.json            # ApiSettings, ConnectionStrings
```

### Module Pattern

Each feature module follows this structure:

1. **BusinessObjects**: Entity model
2. **DataAccess**: Configuration + Migration
3. **Repositories**: Interface + Implementation (uses DAO)
4. **Backend API**: Controller with CRUD endpoints
5. **Frontend MVC**: Controller + Views + ViewModels

Example for Students module:
```csharp
// Frontend Controller
public class StudentsController : BaseController
{
    private readonly IApiService _apiService;
    
    public StudentsController(IApiService apiService, ILogger<StudentsController> logger)
        : base(apiService, logger) { }

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

        var response = await _apiService.GetAsync<PagedResponse<StudentViewModel>>("/students", queryParams);
        
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
}
```

### Authentication Flow

**Session-Based Authentication** (NOT Claims-based):

1. User enters email/password on `/Auth/Login`
2. Frontend validates against `Accounts` table via Repository
3. On success, create `AppUser` model and save to Session
4. Subsequent requests check `HttpContext.Session.GetUser()`
5. Logout clears session

**Default Accounts** (Password: `123456`):

| Username | Email | Role |
|----------|-------|------|
| superadmin | superadmin@e360.com | SuperAdmin |
| admin | admin@e360.com | Admin |
| student | student@e360.com | Student |
| teacher | teacher@e360.com | Teacher |
| parent | parent@e360.com | Parent |
| librarian | librarian@e360.com | Librarian |

**Login Flow:**
```csharp
// AuthController.cs
public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
{
    var account = await _accountRepository.FindByEmailOrUsernameAsync(model.Email);
    
    if (account == null || account.Status != "Active")
    {
        TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng";
        return View(model);
    }

    if (!PasswordHelper.VerifyPassword(model.Password, account.PasswordHash))
    {
        TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng";
        return View(model);
    }

    // Create AppUser and save to session
    var user = new AppUser
    {
        Id = account.Id,
        Username = account.Username,
        Email = account.Email,
        FullName = account.FullName ?? account.Username,
        Role = account.Role,
        Status = account.Status
    };

    HttpContext.Session.SetUser(user);
    await _accountRepository.UpdateLastLoginAsync(account.Id);

    return RedirectToAction("Index", "Home");
}
```

## Key Files Reference

| File | Purpose |
|------|---------|
| `e360_clone.slnx` | Solution file linking API and Frontend projects |
| `e360_clone_api/Program.cs` | API entry point with Swagger, CORS, JWT configuration |
| `e360_clone_api/Controllers/BaseApiController.cs` | Base controller with common response patterns |
| `e360_clone_api/appsettings.json` | Connection strings, JWT settings, CORS origins |
| `e360_clone.DataAccess/AppDbContext.cs` | EF Core DbContext with all entity configurations |
| `e360_clone.DataAccess/DAOs/AccountDAO.cs` | Account-specific data access (DB operations) |
| `e360_clone.Repositories/Repository.cs` | Generic repository with CRUD, paging, search |
| `e360_clone.Repositories/AccountRepository.cs` | Account repository (uses AccountDAO) |
| `e360_clone_fe/Program.cs` | MVC, Session, Auth, DI configuration |
| `e360_clone_fe/Controllers/BaseController.cs` | Base controller with auth helpers |
| `e360_clone_fe/Controllers/Auth/AuthController.cs` | Login, Logout (session-based) |
| `e360_clone_fe/Services/ApiService.cs` | HTTP client for calling backend API |
| `e360_clone_fe/Extensions/AppUserExtensions.cs` | Session-based user extensions |
| `e360_clone_fe/Views/Shared/_Layout.cshtml` | Main layout with CSS/JS includes |
| `e360_clone_fe/Views/_ViewStart.cshtml` | Default layout for all views |
| `e360_clone_fe/Views/_ViewImports.cshtml` | Common using statements for views |
| `README.md` | Detailed business requirements and functional specifications |
| `DATABASE_AUTH_SETUP.md` | Database schema and authentication setup |
| `ARCHITECTURE.md` | Comprehensive architecture documentation |
| `REFACTORING_SUMMARY.md` | Refactoring history and changes |
| `docs/` | Comprehensive documentation (modules, workflows, API) |

## Notes

### Architecture Principles

1. **Layered Architecture** with separation of concerns:
   - **BusinessObjects**: Domain models/entities (no dependencies)
   - **DataAccess**: EF Core DbContext, entity configurations, **Migrations**, DAOs
   - **Repositories**: Generic repository pattern + Unit of Work (uses DAOs)
   - **API**: Controllers, DI, CORS, Swagger
   - **Frontend**: MVC + AJAX calling API with session-based auth

2. **Database**: PostgreSQL via Supabase

3. **ORM**: Entity Framework Core 8 with Code-First migrations

4. **Migrations MUST be in DataAccess project**:
   ```bash
   # CORRECT
   cd e360_clone/e360_clone.DataAccess
   dotnet ef migrations add <MigrationName>
   dotnet ef database update
   
   # WRONG - Don't create migrations in API project
   cd e360_clone/e360_clone_api  ❌
   ```

5. **CORS**: Configured in API project to allow frontend calls from `http://localhost:5000`

6. **API Response Format**: All responses wrapped in `ApiResponse<T>` or `PagedResponse<T>`

7. **Frontend Architecture**: 
   - Session-based authentication (NOT claims-based)
   - `AppUser` model stored in session
   - Extension methods: `SetUser()`, `GetUser()`, `IsLoggedIn()`, `Logout()`

8. **Running the Application**:
   1. Configure database connection in `e360_clone_api/appsettings.json`
   2. Run migrations: `cd e360_clone/e360_clone.DataAccess && dotnet ef database update`
   3. Start API: `cd e360_clone/e360_clone_api && dotnet run` (http://localhost:5104)
   4. Start Frontend: `cd e360_clone/e360_clone_fe && dotnet run` (http://localhost:5000)
   5. Open browser: http://localhost:5000
   6. Login with: `admin@e360.com` / `123456`

9. **Adding New Module**: 
   1. Create Model in `e360_clone.BusinessObjects/`
   2. Create Configuration in `e360_clone.DataAccess/Configurations/`
   3. Add DbSet to `AppDbContext.cs` in `e360_clone.DataAccess/`
   4. (Optional) Create DAO in `e360_clone.DataAccess/DAOs/`
   5. (Optional) Create specific repository in `e360_clone.Repositories/`
   6. **Create Migration**: `cd e360_clone/e360_clone.DataAccess && dotnet ef migrations add <Name>`
   7. **Update Database**: `dotnet ef database update`
   8. Create API Controller in `e360_clone_api/Controllers/`
   9. Create Frontend MVC Controller in `e360_clone_fe/Controllers/`
   10. Create View in `e360_clone_fe/Views/[Feature]/`
   11. Create ViewModels in `e360_clone_fe/Models/ViewModels/`

10. **Database Migration Rules**:
    - ✅ **ADD columns** - Always safe
    - ✅ **ADD tables** - Always safe
    - ✅ **ADD indexes** - Always safe
    - ❌ **DELETE columns** - NEVER (archive instead)
    - ❌ **DELETE tables** - NEVER (archive instead)
    - ❌ **RENAME columns** - NEVER (add new, deprecate old)
    - ⚠️ **MODIFY column types** - Only if backward compatible

11. **Repository Pattern with DAO**:
    ```
    Controller → Repository → DAO → DbContext → Database
    
    Example:
    AuthController 
      → IAccountRepository 
        → AccountDAO 
          → AppDbContext 
            → Accounts table
    ```

12. **Session-Based Authentication**:
    - Store `AppUser` object in session
    - No JWT tokens, no claims
    - Simple and type-safe
    - Extension methods in `AppUserExtensions.cs`

13. **Template Usage**:
    - Frontend uses Bootstrap 5 template
    - Existing HTML templates in `Views/` folder
    - Reuse existing tables, cards, forms from template
    - Convert `.html` to `.cshtml` with Razor syntax

14. **Documentation**: Extensive business requirements in `README.md`, detailed docs in `docs/` folder

15. **Current State**: 
    - ✅ Auth module with session-based auth
    - ✅ Students module (CRUD + server-side rendering)
    - ✅ Dashboard views (Student, Teacher, Parent, LMS, School)
    - ✅ Entity models for Student Exam Management
    - ⏳ Pending: Exam Scheduling, Attendance Tracking
