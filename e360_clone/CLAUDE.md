# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**e360_clone** — Nền tảng quản lý lịch thi & coi thi (clone hệ thống e360 của FPT). Monorepo gồm 2 phần tách biệt:

- `e360_clone_api/` — Backend ASP.NET Core 8.0 Web API (C#)
- `e360_clone_fe/` — Frontend Next.js 16 + React 19 + TypeScript + Tailwind CSS 4

## Build & Run Commands

### Backend (`e360_clone_api/`)
```bash
cd e360_clone_api
dotnet restore          # Restore NuGet packages
dotnet build            # Build project
dotnet run              # Run API server (http://localhost:5104, https://localhost:7052)
dotnet watch run        # Run with hot-reload
```
Swagger UI available at `/swagger` in Development mode.

### Frontend (`e360_clone_fe/`)
```bash
cd e360_clone_fe
npm install             # Install dependencies
npm run dev             # Dev server (http://localhost:3000)
npm run build           # Production build
npm run lint            # ESLint check
```

## Architecture

### Backend
- **Framework**: ASP.NET Core 8.0 Web API with controller-based routing
- **Solution file**: `e360_clone_api/e360_clone.sln`
- **Entry point**: `e360_clone_api/Program.cs`
- **Controllers**: `e360_clone_api/Controllers/` — route pattern `[controller]`
- **API docs**: Swashbuckle/Swagger enabled in dev

### Frontend
- **Framework**: Next.js 16 App Router (`app/` directory)
- **Styling**: Tailwind CSS 4 via PostCSS
- **Path alias**: `@/*` maps to project root (configured in `tsconfig.json`)
- **ESLint**: next/core-web-vitals + next/typescript rules
- **Fonts**: Geist Sans + Geist Mono (from `next/font/google`)

## Business Domain

Hệ thống quản lý lịch thi dành cho môi trường đại học với 4 vai trò (roles):

| Role | Description |
|------|-------------|
| **Admin** | Quản trị hệ thống, quản lý người dùng, phân quyền |
| **Nhân viên giáo vụ** | Lập lịch thi, phân công coi thi, quản lý điểm danh |
| **Giảng viên** | Coi thi, điểm danh, nhập điểm thi |
| **Sinh viên** | Xem lịch thi, xem điểm, điểm danh |

Các module nghiệp vụ chính: Quản lý lịch thi, Phân công coi thi, Quản lý điểm thi, Quản lý dữ liệu nền (sinh viên, giảng viên, môn học, lớp học, ngành học), Báo cáo & Thống kê.

## Conventions

- Backend namespace: `e360_clone`
- Frontend uses strict TypeScript (`strict: true`)
- Vietnamese is the primary language for business logic, UI text, and documentation
