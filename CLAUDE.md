# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Layout

```
real_project/
├── e360_clone/          # Main .NET solution — all active backend/MVC work happens here
│   ├── e360_clone_api/  # ASP.NET Core 8 Web API (primary backend)
│   ├── e360_clone_fe/   # ASP.NET Core MVC frontend (active UI)
│   ├── e360_clone.BusinessObjects/  # Entity models, enums, helpers, utilities
│   ├── e360_clone.DataAccess/       # EF Core DbContext (PostgreSQL)
│   ├── e360_clone.Repositories/     # Generic repository + UnitOfWork
│   └── SeedDatabase/    # DB seeding utilities
├── API.md               # Full API specification (implemented + required)
├── USE_CASES.md         # Use case flows per role
└── AGENTS.md            # Repository guidelines
```

> Note: `e360_clone/CLAUDE.md` is outdated (references a Next.js frontend that was abandoned). The active frontend is `e360_clone_fe/` (ASP.NET MVC).

## Commands

### Backend API (`e360_clone/e360_clone_api/`)
```bash
cd e360_clone/e360_clone_api
dotnet restore
dotnet build
dotnet run              # http://localhost:5104 | https://localhost:7052 | /swagger
dotnet watch run        # hot-reload
```

### Database migrations
```bash
cd e360_clone/e360_clone_api
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### ASP.NET MVC Frontend (`e360_clone/e360_clone_fe/`)
```bash
cd e360_clone/e360_clone_fe
dotnet run              # http://localhost:5000
```

## Architecture

### Backend layer dependencies
```
e360_clone_api
└── e360_clone.Repositories
    └── e360_clone.DataAccess
        └── e360_clone.BusinessObjects
```

### Entity model base
All entities extend `BaseEntity` (`e360_clone.BusinessObjects/Common/BaseEntity.cs`): `Id` (int PK), `CreatedAt`, `UpdatedAt`. Domain enums live in `e360_clone.BusinessObjects/Enums/Enums.cs`: `Gender`, `StudentStatus`, `AccountStatus`, `Role` (enum with helpers), `GradeType` — each has a companion helper class with `GetText()`, `GetBadgeClass()`, `GetAll()`. `Account.Role` is stored as a plain `string` field (not the enum): values are `"Admin"`, `"SuperAdmin"`, `"Student"`, `"Teacher"`, `"Librarian"`, `"Parent"`, `"Staff"`.

### API contract
- All responses: `ApiResponse<T>` (`success`, `message`, `data`) or `PagedResponse<T>` (adds `pageNumber`, `pageSize`, `totalRecords`, `totalPages`). Both defined in `BaseApiController.cs`.
- `BaseApiController` provides helpers: `HandleResult<T>()`, `HandleNotFound()`, `HandleError()`.
- List endpoints accept `PagedRequest`: `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`.
- Auth: JWT Bearer. Login via `POST /api/auth/login` (email/username + password) or `POST /api/auth/quick-login` (role string, demo only). Register via `POST /api/auth/register`.
- JWT claims include `ClaimTypes.Name` (username), `ClaimTypes.Email`, `ClaimTypes.Role`, `"FullName"`, `"UserId"`.
- API controllers: Auth, Classes, Dashboard, Exams, Lecturers, **Majors**, ProctorAssignments, Rooms, Students, **StudentSubjects**, Subjects, **TeachingAssignments**, Attendances.

### Repository & utilities (backend)
- Inject `IRepository<T>` (defined in `e360_clone.Repositories/IRepository.cs`) for single-entity work or `IUnitOfWork` for multi-entity transactions.
- Key `IRepository<T>` methods: `GetAllAsync()`, `GetByIdAsync()`, `FindAsync()`, `FirstOrDefaultAsync()`, `AnyAsync()`, `CountAsync()`, `GetPagedAsync()`, `GetPagedFilteredAsync()`, `SearchAsync()`.
- `PagedResult<T>` (from `IRepository.cs`) holds `Items`, `TotalRecords`, `TotalPages`, `HasPrevious`, `HasNext`.
- Utility classes in `e360_clone.BusinessObjects/Utilities/Utils.cs`: `GradeUtils`, `DateTimeUtils`, `StringUtils`, `PaginationUtils`.
- Response helpers in `e360_clone.BusinessObjects/Helpers/Helpers.cs` and `PasswordHelper.cs` (SHA256 for demo).

### ASP.NET MVC frontend
`e360_clone/e360_clone_fe/` is an ASP.NET Core MVC shell serving Razor views. Each feature follows this pattern:
1. Backend API controller: `e360_clone_api/Controllers/[Feature]Controller.cs`
2. Frontend MVC controller: `e360_clone_fe/Controllers/[Feature]Controller.cs` — calls `IApiService` to proxy requests to the backend API, then returns `View(model)`
3. Razor view: `e360_clone_fe/Views/[Feature]/Index.cshtml`
4. Page-specific JS in `@section Scripts` within each view (IIFE pattern)

**Frontend auth:** Cookie-based (`CookieAuthenticationDefaults`), login path `/Auth/Login`, 8-hour idle session. The MVC layer stores user context (Role, FullName, LecturerId, StudentId, etc.) in ASP.NET Core session after a successful `POST /api/Auth/login` call.

**SignalR:** `AttendanceHub` is registered at `/hubs/attendance` (placeholder, minimal implementation).

### Frontend JS layer
Active files in `wwwroot/js/`:
1. **Libs** (`wwwroot/assets/js/lib/`): jQuery 3.7.1, Bootstrap bundle, ApexCharts, DataTables, Flatpickr, Iconify, jQuery UI
2. **Config** (`wwwroot/js/config.js`): sets `APP_CONFIG.API_BASE_URL` (default `http://localhost:5104/api`)
3. **Utils** (`wwwroot/js/utils.js`): `Utils.showLoading()`, `Utils.hideLoading()`, `Utils.showToast()`
4. **Enums** (`wwwroot/js/enums/`): `AccountStatus`, `Role`, `StudentStatus`, `Gender`, `ExamStatus`, `GradeType`, `AttendanceStatus`
5. **Feature scripts** (`@section Scripts` in each view)

> `wwwroot/js/_archived/` contains old client-side modules (core/http.js, auth.js, models/, mappers/) that are no longer used — the active frontend delegates API calls to the C# `IApiService`, not to client-side HTTP modules.

## Database
- PostgreSQL via Supabase
- Connection string in `e360_clone/e360_clone_api/appsettings.json` → `ConnectionStrings.DefaultConnection`
- Password hashing: SHA256 via `PasswordHelper` (demo). Production should use BCrypt/Argon2.
- Default seeded accounts (password `123456`): `superadmin`, `admin`, `student`, `teacher`, `parent`, `librarian` — see `e360_clone/DATABASE_AUTH_SETUP.md`.

## Roles & Modules
Roles: **Admin / SuperAdmin**, **Nhân viên giáo vụ** (Academic Staff), **Giảng viên** (Teacher), **Sinh viên** (Student), **Parent**, **Librarian**.

Core modules: Exam Schedule Management, Proctor Assignment, Grade Management, Master Data (Students, Lecturers, Subjects, Classes, Exam Rooms), Reports & Statistics.

Full API spec in `API.md`. Use case flows per role in `USE_CASES.md`.

## Conventions
- C# targets `net8.0`; `Nullable` and `ImplicitUsings` enabled; PascalCase types/methods, camelCase locals.
- Vietnamese is the primary language for business logic, UI text, and documentation.
- Backend namespace: `e360_clone`. Frontend JS uses vanilla ES6 IIFEs (no TypeScript, no bundler).
- Commit format: `type: summary` (e.g. `fix: seed account roles`). If DB changes are included, note migration name.
