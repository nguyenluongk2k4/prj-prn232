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
| **DataAccess** | `e360_clone.DataAccess/` | Class Library - EF Core DbContext |
| **Repositories** | `e360_clone.Repositories/` | Class Library - Repository pattern |

### Solution Structure

```
C:\Git\prj-prn232\e360_clone\
├── e360_clone.slnx              # Solution file
├── e360_clone_api/              # Web API project
│   ├── Controllers/
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
├── e360_clone_fe/               # MVC Frontend project
│   ├── Controllers/
│   ├── Views/
│   └── wwwroot/
├── e360_clone.BusinessObjects/  # Domain models (entities)
│   ├── Student.cs
│   ├── Lecturer.cs
│   ├── Exam.cs
│   └── ...
├── e360_clone.DataAccess/       # EF Core DbContext
│   └── AppDbContext.cs
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
cd e360_clone/e360_clone_api
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

### Frontend (`e360_clone_fe/`)

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
- Configure API base URL in `wwwroot/js/site.js` or individual page scripts
- Example: `fetch('http://localhost:5104/api/weatherforecast')`

### Running Both Projects

Use the solution file in Visual Studio or run both projects simultaneously:

```bash
# Terminal 1 - API
cd e360_clone/e360_clone_api && dotnet run

# Terminal 2 - Frontend
cd e360_clone/e360_clone_fe && dotnet run
```

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
- **api-client.js**: Base API client with fetch wrapper
- **modules/*.api.js**: API service per module
- **modules/*.ui.js**: UI logic per module
- Dynamic rendering via JavaScript DOM manipulation
- Bootstrap 5 for UI components and styling

### File Organization

**Backend API:**
```
e360_clone_api/
├── Controllers/
│   ├── BaseApiController.cs    # Base controller with common patterns
│   ├── StudentsController.cs   # Student CRUD operations (sample)
│   └── WeatherForecastController.cs
├── Migrations/                 # Database migrations
├── Program.cs                  # CORS, Swagger, DbContext, DI configuration
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
└── Subject.cs
```

**DataAccess (Class Library):**
```
e360_clone.DataAccess/
└── AppDbContext.cs             # EF Core DbContext with entity configurations
```

**Repositories (Class Library):**
```
e360_clone.Repositories/
├── IRepository.cs              # Generic repository interface
└── Repository.cs               # Generic repository implementation
```

**Frontend:**
```
e360_clone_fe/
├── Controllers/
│   ├── HomeController.cs
│   └── StudentsController.cs    # MVC controller for Students view
├── Views/
│   ├── Home/
│   │   └── Index.cshtml         # Dashboard
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   └── Students/
│       └── Index.cshtml         # Students management view
├── wwwroot/
│   ├── css/
│   │   └── site.css
│   ├── js/
│   │   ├── config.js            # Environment config
│   │   ├── utils.js             # Common utilities
│   │   ├── api-client.js        # Base API client
│   │   └── modules/
│   │       ├── students.api.js  # Student API calls
│   │       └── students.ui.js   # Student UI logic
│   └── lib/                     # Third-party libraries
└── Program.cs                   # MVC configuration
```

### Module Pattern

Each feature module follows this structure:

1. **Backend API Controller** (`e360_clone_api/Controllers/[Feature]Controller.cs`)
2. **Frontend MVC Controller** (`e360_clone_fe/Controllers/[Feature]Controller.cs`)
3. **View** (`e360_clone_fe/Views/[Feature]/Index.cshtml`)
4. **JS Module** (`wwwroot/js/modules/[feature].api.js` + `[feature].ui.js`)

Example for Students module:
```csharp
// Frontend Controller
public class StudentsController : Controller
{
    public IActionResult Index() => View();
}
```

```html
<!-- View (Index.cshtml) -->
@section Scripts {
    <script src="/js/config.js"></script>
    <script src="/js/utils.js"></script>
    <script src="/js/api-client.js"></script>
    <script src="/js/modules/students.api.js"></script>
    <script src="/js/modules/students.ui.js"></script>
}
```

## Key Files Reference

| File | Purpose |
|------|---------|
| `e360_clone.slnx` | Solution file linking API and Frontend projects |
| `e360_clone_api/Program.cs` | API entry point with Swagger and CORS configuration |
| `e360_clone_api/Controllers/BaseApiController.cs` | Base controller with common response patterns |
| `e360_clone_api/Models/` | Domain models (Student, Lecturer, Exam, etc.) |
| `e360_clone_fe/Program.cs` | MVC configuration |
| `e360_clone_fe/Controllers/` | MVC controllers for each view |
| `e360_clone_fe/Views/` | Razor views organized by feature |
| `e360_clone_fe/wwwroot/js/config.js` | Environment configuration (API URL, timeout) |
| `e360_clone_fe/wwwroot/js/utils.js` | Common utilities (toast, loading, formatDate) |
| `e360_clone_fe/wwwroot/js/api-client.js` | Base API client with fetch wrapper |
| `e360_clone_fe/wwwroot/js/modules/` | Feature modules (API + UI) |
| `README.md` | Detailed business requirements and functional specifications |

## Notes

- **Architecture**: Layered architecture with separation of concerns
  - **BusinessObjects**: Domain models/entities (no dependencies)
  - **DataAccess**: EF Core DbContext, entity configurations
  - **Repositories**: Generic repository pattern for data access
  - **API**: Controllers, DI, CORS, Swagger
  - **Frontend**: MVC + AJAX calling API
- **Database**: PostgreSQL via Supabase
- **ORM**: Entity Framework Core 8 with Code-First migrations
- **CORS**: Configured in API project to allow frontend calls from `http://localhost:5000`
- **API Response Format**: All responses wrapped in `ApiResponse<T>` or `PagedResponse<T>`
- **Module Pattern**: Each feature has separate API and UI files for easy maintenance
- **Environment Config**: Change API URL in `config.js`, DB connection in `appsettings.json`
- **Running the Application**:
  1. Configure database connection in `e360_clone_api/appsettings.json`
  2. Run migrations: `cd e360_clone/e360_clone_api && dotnet ef database update`
  3. Start API: `cd e360_clone/e360_clone_api && dotnet run` (http://localhost:5104)
  4. Start Frontend: `cd e360_clone/e360_clone_fe && dotnet run` (http://localhost:5000)
  5. Open browser: http://localhost:5000
  6. Navigate to Students: http://localhost:5000/Students
- **Adding New Module**: 
  1. Create Model in `e360_clone.BusinessObjects/`
  2. Add DbSet to `AppDbContext.cs` in `e360_clone.DataAccess/`
  3. (Optional) Create specific repository in `e360_clone.Repositories/`
  4. Create API Controller in `e360_clone_api/Controllers/`
  5. Run `dotnet ef migrations add [MigrationName]`
  6. Run `dotnet ef database update`
  7. Create Frontend MVC Controller in `e360_clone_fe/Controllers/`
  8. Create View in `e360_clone_fe/Views/[Feature]/`
  9. Create JS modules in `e360_clone_fe/wwwroot/js/modules/`
- The project is currently in early scaffolded state with sample Student CRUD
- Extensive business requirements documented in `README.md` for future implementation
