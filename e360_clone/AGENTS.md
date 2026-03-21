# Repository Guidelines

## Project Structure & Module Organization
- `e360_clone/` holds the solution and primary source code.
- `e360_clone/e360_clone_api/` is the ASP.NET Core Web API (controllers, EF Core migrations, seeders).
- `e360_clone/e360_clone_fe/` is the ASP.NET Core MVC frontend (Controllers, Views, `wwwroot` assets).
- `e360_clone.BusinessObjects/` defines entity models and shared helpers.
- `e360_clone.DataAccess/` contains EF Core `DbContext` and data access logic.
- `e360_clone.Repositories/` holds repository abstractions and implementations.
- `SeedDatabase/` includes database seeding utilities.
- Top-level `e360_clone_api/` and `e360_clone_fe/` are build outputs; avoid editing them.

## Build, Test, and Development Commands
- `dotnet build e360_clone/e360_clone_api/e360_clone.csproj` builds the API project.
- `dotnet build e360_clone/e360_clone_fe/e360_clone_fe.csproj` builds the MVC frontend.
- `dotnet run --project e360_clone/e360_clone_api/e360_clone.csproj` runs the API locally.
- `dotnet run --project e360_clone/e360_clone_fe/e360_clone_fe.csproj` runs the MVC frontend.
- From `e360_clone/e360_clone_api/`:
  - `dotnet ef migrations add <Name>` creates a migration.
  - `dotnet ef database update` applies migrations to the database.

## Coding Style & Naming Conventions
- Target framework is `net8.0` with `Nullable` and `ImplicitUsings` enabled.
- Use 4-space indentation.
- .NET naming: PascalCase for types/methods, camelCase for locals/parameters.
- Keep controllers in `Controllers/`, views in `Views/`, and static assets in `wwwroot/`.

## Testing Guidelines
- No test projects are present yet.
- When added, prefer `dotnet test` from `e360_clone/`.
- Name test projects `*.Tests` and test classes `*Tests`.

## Commit & Pull Request Guidelines
- Commit history favors short, descriptive messages (for example `fix: seed account roles`).
- PRs should include:
  - Summary of changes.
  - Testing notes (commands run).
  - Screenshots for UI changes.
- If a change affects the database, include the migration name and any seed updates.

## Configuration & Data
- App settings live in each project’s `appsettings.json` and `appsettings.Development.json`.
- Connection strings and seeded accounts are documented in `DATABASE_AUTH_SETUP.md`.
- Never commit real credentials or production secrets.
