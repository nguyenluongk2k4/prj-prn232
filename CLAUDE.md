# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Layout

```
real_project/
├── e360_clone/          # Main .NET solution — all active backend/MVC work happens here
│   ├── e360_clone_api/  # ASP.NET Core 8 Web API (primary backend)
│   ├── e360_clone_fe/   # ASP.NET Core MVC frontend (old MVC approach)
│   ├── e360_clone.BusinessObjects/  # Entity models (no dependencies)
│   ├── e360_clone.DataAccess/       # EF Core DbContext (PostgreSQL)
│   ├── e360_clone.Repositories/     # Generic repository pattern
│   └── SeedDatabase/    # DB seeding utilities
├── e360_clone_fe/       # New Next.js 16 frontend (in early scaffolding)
├── e360_clone_api/      # Build artifacts only — do not edit
├── API.md               # Full API specification (implemented + required)
├── USE_CASES.md         # Use case flows per role
└── AGENTS.md            # Repository guidelines
```

> All active .NET development is inside `e360_clone/`. See `e360_clone/CLAUDE.md` for detailed guidance on that sub-project.

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

### New Next.js Frontend (`e360_clone_fe/`)
```bash
cd e360_clone_fe
npm install
npm run dev             # http://localhost:3000
npm run build
npm run lint
```

## Architecture

### Backend layer dependencies
```
e360_clone_api
└── e360_clone.Repositories
    └── e360_clone.DataAccess
        └── e360_clone.BusinessObjects
```

### API contract
- All responses are wrapped: `ApiResponse<T>` (`success`, `message`, `data`) or `PagedResponse<T>` (adds `pageNumber`, `pageSize`, `totalRecords`, `totalPages`).
- List endpoints accept: `pageNumber`, `pageSize`, `searchTerm`, `sortBy`, `sortDescending`.
- Auth: JWT Bearer. Login via `POST /api/auth/login` (email + password) or `POST /api/auth/quick-login` (role, demo only).
- All data-mutating endpoints require JWT and role-based authorization.
- Audit fields required on all entities: `createdAt`, `updatedAt`, `status`.

### ASP.NET MVC frontend (legacy)
The `e360_clone/e360_clone_fe/` project is an ASP.NET Core MVC shell that serves static pages. Each feature follows a 4-file module pattern:
1. Backend API controller: `e360_clone_api/Controllers/[Feature]Controller.cs`
2. Frontend MVC controller: `e360_clone_fe/Controllers/[Feature]Controller.cs` (returns `View()`)
3. Razor view: `e360_clone_fe/Views/[Feature]/Index.cshtml`
4. JS modules: `wwwroot/js/modules/[feature].api.js` + `[feature].ui.js`

Key JS files: `wwwroot/js/config.js` (API base URL), `utils.js` (toast, loading, formatDate), `api-client.js` (fetch wrapper).

### New Next.js frontend
`e360_clone_fe/` uses Next.js 16 App Router, Tailwind CSS 4, strict TypeScript, path alias `@/*` → project root.

## Database
- PostgreSQL via Supabase
- Connection string in `e360_clone/e360_clone_api/appsettings.json` → `ConnectionStrings.DefaultConnection`
- Password hashing: SHA256 (demo). Production should use BCrypt/Argon2.
- Default seeded accounts (password `123456`): `superadmin`, `admin`, `student`, `teacher`, `parent`, `librarian` — see `e360_clone/DATABASE_AUTH_SETUP.md`.

## Roles & Modules
Roles: **Admin / SuperAdmin**, **Nhân viên giáo vụ** (Academic Staff), **Giảng viên** (Teacher), **Sinh viên** (Student), **Parent**, **Librarian**.

Core modules: Exam Schedule Management, Proctor Assignment, Grade Management, Master Data (Students, Lecturers, Subjects, Classes, Exam Rooms), Reports & Statistics.

Full API spec in `API.md`. Use case flows per role in `USE_CASES.md`.

## Conventions
- C# targets `net8.0`; `Nullable` and `ImplicitUsings` enabled; PascalCase types/methods, camelCase locals.
- Vietnamese is the primary language for business logic, UI text, and documentation.
- Backend namespace: `e360_clone`; frontend uses strict TypeScript.
- Commit format: `type: summary` (e.g. `fix: seed account roles`). If DB changes are included, note migration name.
