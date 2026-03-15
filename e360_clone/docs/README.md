# E360 Clone - Documentation

## Overview

This folder contains comprehensive documentation for the E360 Clone project - an exam scheduling and proctoring management system.

## Folder Structure

```
docs/
├── README.md                 # This file
├── modules/                  # Module-specific documentation
│   ├── 01-student-management.md
│   ├── 02-lecturer-management.md
│   ├── 03-exam-management.md
│   ├── 04-exam-schedule.md
│   ├── 05-proctor-assignment.md
│   ├── 06-grade-management.md
│   └── 07-attendance.md
├── workflows/                # Business process workflows
│   ├── exam-organization-workflow.md
│   ├── grade-management-workflow.md
│   └── attendance-workflow.md
├── api/                      # API documentation
│   ├── overview.md
│   └── endpoints.md
└── database/                 # Database documentation
    └── erd.md
```

## Modules Overview

| # | Module | Description | Status |
|---|--------|-------------|--------|
| 01 | Student Management | Quản lý thông tin sinh viên | 🟡 In Progress |
| 02 | Lecturer Management | Quản lý thông tin giảng viên | ⚪ Not Started |
| 03 | Exam Management | Quản lý đề thi và môn thi | ⚪ Not Started |
| 04 | Exam Schedule | Lập lịch thi chi tiết | ⚪ Not Started |
| 05 | Proctor Assignment | Phân công giám thị coi thi | ⚪ Not Started |
| 06 | Grade Management | Quản lý điểm thi | ⚪ Not Started |
| 07 | Attendance | Điểm danh sinh viên dự thi | ⚪ Not Started |

## User Roles

| Role | Description | Permissions |
|------|-------------|-------------|
| **Admin** | Quản trị hệ thống | Full access |
| **Nhân viên giáo vụ** | Academic staff | Exam scheduling, proctor assignment, reports |
| **Giảng viên** | Lecturer | Grade entry, attendance taking, proctoring |
| **Sinh viên** | Student | View schedules, view grades, check-in |

## Technology Stack

- **Backend**: ASP.NET Core 8.0 Web API
- **Frontend**: ASP.NET Core 8.0 MVC + AJAX
- **Database**: PostgreSQL (Supabase)
- **ORM**: Entity Framework Core 8
- **Architecture**: Layered (BusinessObjects, DataAccess, Repositories)

## Quick Links

- [Module Documentation](./modules/)
- [Workflows](./workflows/)
- [API Documentation](./api/)
- [Database Schema](./database/)
