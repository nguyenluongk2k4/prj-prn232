# Repository Guidelines

## Project Structure & Module Organization
- `e360_clone/` contains the source solution and most development work.
- `e360_clone/e360_clone_api/` is the ASP.NET Core Web API (controllers, EF Core migrations, seeders).
- `e360_clone/e360_clone_fe/` is the ASP.NET Core MVC frontend (Controllers, Views, wwwroot).
- `e360_clone/e360_clone.BusinessObjects/` defines entity models and shared helpers.
- `e360_clone/e360_clone.DataAccess/` contains EF Core DbContext and data access logic.
- `e360_clone/e360_clone.Repositories/` holds repository abstractions and implementations.
- `e360_clone/SeedDatabase/` includes database seeding utilities.
- Top-level `e360_clone_api/` and `e360_clone_fe/` appear to be build outputs (bin/obj/.next). Avoid editing them.

## Build, Test, and Development Commands
- `dotnet build e360_clone/e360_clone_api/e360_clone.csproj` builds the API project.
- `dotnet build e360_clone/e360_clone_fe/e360_clone_fe.csproj` builds the frontend project.
- `dotnet run --project e360_clone/e360_clone_api/e360_clone.csproj` runs the API.
- `dotnet run --project e360_clone/e360_clone_fe/e360_clone_fe.csproj` runs the MVC frontend.
- From `e360_clone/e360_clone_api/`, `dotnet ef migrations add <Name>` and `dotnet ef database update` manage EF Core migrations.

## Coding Style & Naming Conventions
- C# projects target `net8.0` with `Nullable` and `ImplicitUsings` enabled.
- Use 4-space indentation and standard .NET naming: PascalCase for types/methods, camelCase for locals/parameters.
- Keep controllers in `Controllers/`, views in `Views/`, and static assets in `wwwroot/`.

## Testing Guidelines
- No test projects are present yet. When tests are added, prefer `dotnet test` from `e360_clone/`.
- Name test projects `*.Tests` and test classes `*Tests` to align with .NET conventions.

## Commit & Pull Request Guidelines
- Recent commits are short and informal (e.g., `init prj`, `FE`, `fix: ...`). Use clear, descriptive messages.
- Recommended format: `type: summary` (for example `fix: seed account roles`).
- PRs should include a brief summary, testing notes, and screenshots for UI changes.
- If a change affects the database, include the migration name and any seed updates.

## Configuration & Data
- Configuration lives in `appsettings.json` and `appsettings.Development.json` under each project.
- Connection strings and seeded accounts are documented in `e360_clone/DATABASE_AUTH_SETUP.md`.
- Do not commit real credentials or production secrets.
