# Database Schema Documentation

## Overview

This document describes the database schema for the E360 Clone system, using PostgreSQL via Supabase.

## Entity Relationship Diagram

```mermaid
erDiagram
    Student ||--o{ Grade : "receives"
    Student ||--o{ Attendance : "has"
    Student }o--|| Class : "belongs to"
    
    Lecturer ||--o{ ProctorAssignment : "assigned to"
    Lecturer ||--o{ Grade : "enters"
    
    Subject ||--o{ Exam : "has"
    Subject ||--o{ Class : "taught in"
    
    Class ||--o{ Exam : "participates in"
    Class ||--o{ Student : "contains"
    
    Exam ||--o{ ExamSchedule : "scheduled as"
    Exam ||--o{ Grade : "has grades"
    
    ExamRoom ||--o{ ExamSchedule : "hosts"
    ExamRoom ||--o{ Exam : "assigned to"
    
    ExamSchedule ||--o{ ProctorAssignment : "has proctors"
    ExamSchedule ||--o{ Attendance : "has attendance"
    
    Student {
        int Id PK
        string StudentCode UK
        string FullName
        date DateOfBirth
        string Gender
        string Email UK
        string PhoneNumber
        string Address
        int ClassId FK
        string Status
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Lecturer {
        int Id PK
        string EmployeeCode UK
        string FullName
        date DateOfBirth
        string Gender
        string Email UK
        string PhoneNumber
        string Department
        string Position
        string Status
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Subject {
        int Id PK
        string SubjectCode UK
        string SubjectName
        int Credits
        int TheoryHours
        int PracticeHours
        string Department
        string SubjectType
        string Status
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Class {
        int Id PK
        string ClassCode UK
        string ClassName
        int MajorId
        int CourseId
        string AcademicYear
        int Semester
        int StudentCount
        string Status
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    ExamRoom {
        int Id PK
        string RoomCode UK
        string RoomName
        string Building
        int Capacity
        string Floor
        bool HasComputer
        bool HasProjector
        string Status
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Exam {
        int Id PK
        string ExamCode UK
        string ExamName
        string ExamType
        int SubjectId FK
        int ClassId FK
        datetime ExamDate
        time StartTime
        time EndTime
        int Duration
        int RoomId FK
        string AcademicYear
        string Semester
        string Status
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    ExamSchedule {
        int Id PK
        int ExamId FK
        datetime Date
        time StartTime
        time EndTime
        int RoomId FK
        string Status
        string Notes
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    ProctorAssignment {
        int Id PK
        int ExamScheduleId FK
        int LecturerId FK
        string Role
        datetime AssignedAt
        string Status
        string Notes
    }
    
    Grade {
        int Id PK
        int StudentId FK
        int ExamId FK
        decimal Score
        string ScoreType
        string LetterGrade
        string Notes
        int EnteredBy FK
        datetime EnteredAt
        int ApprovedBy FK
        datetime ApprovedAt
        string Status
    }
    
    Attendance {
        int Id PK
        int ExamScheduleId FK
        int StudentId FK
        string Status
        datetime CheckInTime
        datetime CheckOutTime
        string Notes
        string Violation
        datetime RecordedAt
    }
```

---

## Table Specifications

### Students

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| StudentCode | VARCHAR(20) | UNIQUE, NOT NULL | Mã sinh viên |
| FullName | NVARCHAR(100) | NOT NULL | Họ và tên đầy đủ |
| DateOfBirth | DATE | NOT NULL | Ngày sinh |
| Gender | NVARCHAR(20) | NOT NULL | Giới tính (Nam/Nữ/Khác) |
| Email | VARCHAR(100) | UNIQUE, NOT NULL | Email |
| PhoneNumber | VARCHAR(20) | NOT NULL | Số điện thoại |
| Address | NVARCHAR(200) | NULL | Địa chỉ |
| ClassId | INT | FK → Class.Id, NOT NULL | Lớp học |
| Status | VARCHAR(20) | DEFAULT 'Active' | Trạng thái |
| CreatedAt | DATETIME | DEFAULT GETDATE() | Ngày tạo |
| UpdatedAt | DATETIME | NULL | Ngày cập nhật |

**Indexes:**
- IX_Students_StudentCode (UNIQUE)
- IX_Students_Email (UNIQUE)
- IX_Students_ClassId
- IX_Students_Status

---

### Lecturers

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| EmployeeCode | VARCHAR(20) | UNIQUE, NOT NULL | Mã giảng viên |
| FullName | NVARCHAR(100) | NOT NULL | Họ và tên đầy đủ |
| DateOfBirth | DATE | NOT NULL | Ngày sinh |
| Gender | NVARCHAR(20) | NOT NULL | Giới tính |
| Email | VARCHAR(100) | UNIQUE, NOT NULL | Email |
| PhoneNumber | VARCHAR(20) | NOT NULL | Số điện thoại |
| Department | NVARCHAR(100) | NOT NULL | Bộ môn |
| Position | NVARCHAR(50) | NULL | Chức vụ |
| Status | VARCHAR(20) | DEFAULT 'Active' | Trạng thái |
| CreatedAt | DATETIME | DEFAULT GETDATE() | Ngày tạo |
| UpdatedAt | DATETIME | NULL | Ngày cập nhật |

**Indexes:**
- IX_Lecturers_EmployeeCode (UNIQUE)
- IX_Lecturers_Email (UNIQUE)
- IX_Lecturers_Department

---

### Subjects

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| SubjectCode | VARCHAR(20) | UNIQUE, NOT NULL | Mã môn học |
| SubjectName | NVARCHAR(200) | NOT NULL | Tên môn học |
| Credits | INT | NOT NULL | Số tín chỉ |
| TheoryHours | INT | NOT NULL | Số tiết lý thuyết |
| PracticeHours | INT | NOT NULL | Số tiết thực hành |
| Department | NVARCHAR(100) | NULL | Bộ môn phụ trách |
| SubjectType | VARCHAR(50) | DEFAULT 'Core' | Loại môn |
| Status | VARCHAR(20) | DEFAULT 'Active' | Trạng thái |
| CreatedAt | DATETIME | DEFAULT GETDATE() | Ngày tạo |
| UpdatedAt | DATETIME | NULL | Ngày cập nhật |

**Indexes:**
- IX_Subjects_SubjectCode (UNIQUE)
- IX_Subjects_SubjectType

---

### Classes

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| ClassCode | VARCHAR(20) | UNIQUE, NOT NULL | Mã lớp |
| ClassName | NVARCHAR(100) | NOT NULL | Tên lớp |
| MajorId | INT | FK → Subject.Id, NULL | Ngành học |
| CourseId | INT | NULL | Khóa học |
| AcademicYear | VARCHAR(20) | NOT NULL | Năm học |
| Semester | INT | NOT NULL | Học kỳ |
| StudentCount | INT | DEFAULT 0 | Số sinh viên |
| Status | VARCHAR(20) | DEFAULT 'Active' | Trạng thái |
| CreatedAt | DATETIME | DEFAULT GETDATE() | Ngày tạo |
| UpdatedAt | DATETIME | NULL | Ngày cập nhật |

**Indexes:**
- IX_Classes_ClassCode (UNIQUE)
- IX_Classes_AcademicYear
- IX_Classes_Status

---

### ExamRooms

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| RoomCode | VARCHAR(20) | UNIQUE, NOT NULL | Mã phòng |
| RoomName | NVARCHAR(100) | NOT NULL | Tên phòng |
| Building | NVARCHAR(100) | NULL | Tòa nhà |
| Capacity | INT | NOT NULL | Sức chứa |
| Floor | VARCHAR(20) | NULL | Tầng |
| HasComputer | BIT | DEFAULT 0 | Có máy tính |
| HasProjector | BIT | DEFAULT 0 | Có máy chiếu |
| Status | VARCHAR(20) | DEFAULT 'Available' | Trạng thái |
| CreatedAt | DATETIME | DEFAULT GETDATE() | Ngày tạo |
| UpdatedAt | DATETIME | NULL | Ngày cập nhật |

**Indexes:**
- IX_ExamRooms_RoomCode (UNIQUE)
- IX_ExamRooms_Building
- IX_ExamRooms_Status

---

### Exams

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| ExamCode | VARCHAR(20) | UNIQUE, NOT NULL | Mã kỳ thi |
| ExamName | NVARCHAR(200) | NOT NULL | Tên kỳ thi |
| ExamType | VARCHAR(50) | NOT NULL | Loại thi |
| SubjectId | INT | FK → Subject.Id, NOT NULL | Môn thi |
| ClassId | INT | FK → Class.Id, NOT NULL | Lớp thi |
| ExamDate | DATETIME | NULL | Ngày thi |
| StartTime | TIME | NULL | Giờ bắt đầu |
| EndTime | TIME | NULL | Giờ kết thúc |
| Duration | INT | NOT NULL | Thời lượng (phút) |
| RoomId | INT | FK → ExamRoom.Id, NULL | Phòng thi |
| AcademicYear | VARCHAR(20) | NOT NULL | Năm học |
| Semester | VARCHAR(20) | NOT NULL | Học kỳ |
| Status | VARCHAR(20) | DEFAULT 'Planned' | Trạng thái |
| CreatedAt | DATETIME | DEFAULT GETDATE() | Ngày tạo |
| UpdatedAt | DATETIME | NULL | Ngày cập nhật |

**Indexes:**
- IX_Exams_ExamCode (UNIQUE)
- IX_Exams_SubjectId
- IX_Exams_ClassId
- IX_Exams_Status
- IX_Exams_AcademicYear

**Foreign Keys:**
- FK_Exams_Subjects (SubjectId → Subjects.Id)
- FK_Exams_Classes (ClassId → Classes.Id)
- FK_Exams_ExamRooms (RoomId → ExamRooms.Id)

---

### ExamSchedules

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| ExamId | INT | FK → Exam.Id, NOT NULL | Kỳ thi |
| Date | DATETIME | NOT NULL | Ngày thi |
| StartTime | TIME | NOT NULL | Giờ bắt đầu |
| EndTime | TIME | NOT NULL | Giờ kết thúc |
| RoomId | INT | FK → ExamRoom.Id, NOT NULL | Phòng thi |
| Status | VARCHAR(20) | DEFAULT 'Scheduled' | Trạng thái |
| Notes | NVARCHAR(500) | NULL | Ghi chú |
| CreatedAt | DATETIME | DEFAULT GETDATE() | Ngày tạo |
| UpdatedAt | DATETIME | NULL | Ngày cập nhật |

**Indexes:**
- IX_ExamSchedules_ExamId
- IX_ExamSchedules_Date
- IX_ExamSchedules_RoomId
- IX_ExamSchedules_Status

**Foreign Keys:**
- FK_ExamSchedules_Exams (ExamId → Exams.Id)
- FK_ExamSchedules_ExamRooms (RoomId → ExamRooms.Id)

---

### ProctorAssignments

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| ExamScheduleId | INT | FK → ExamSchedule.Id, NOT NULL | Lịch thi |
| LecturerId | INT | FK → Lecturer.Id, NOT NULL | Giảng viên |
| Role | VARCHAR(50) | NOT NULL | Vai trò |
| AssignedAt | DATETIME | DEFAULT GETDATE() | Ngày phân công |
| Status | VARCHAR(20) | DEFAULT 'Assigned' | Trạng thái |
| Notes | NVARCHAR(500) | NULL | Ghi chú |
| DeclineReason | NVARCHAR(500) | NULL | Lý do từ chối |

**Indexes:**
- IX_ProctorAssignments_ExamScheduleId
- IX_ProctorAssignments_LecturerId
- IX_ProctorAssignments_Status

**Foreign Keys:**
- FK_ProctorAssignments_ExamSchedules (ExamScheduleId → ExamSchedules.Id)
- FK_ProctorAssignments_Lecturers (LecturerId → Lecturers.Id)

---

### Grades

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| StudentId | INT | FK → Student.Id, NOT NULL | Sinh viên |
| ExamId | INT | FK → Exam.Id, NOT NULL | Kỳ thi |
| Score | DECIMAL(5,2) | NULL | Điểm số |
| ScoreType | VARCHAR(50) | NOT NULL | Loại điểm |
| LetterGrade | VARCHAR(5) | NOT NULL | Điểm chữ |
| Notes | NVARCHAR(500) | NULL | Ghi chú |
| EnteredBy | INT | FK → Lecturer.Id, NULL | Người nhập |
| EnteredAt | DATETIME | NULL | Ngày nhập |
| ApprovedBy | INT | FK → Lecturer.Id, NULL | Người duyệt |
| ApprovedAt | DATETIME | NULL | Ngày duyệt |
| Status | VARCHAR(20) | DEFAULT 'Draft' | Trạng thái |

**Indexes:**
- IX_Grades_StudentId
- IX_Grades_ExamId
- IX_Grades_Status
- IX_Grades_EnteredBy

**Foreign Keys:**
- FK_Grades_Students (StudentId → Students.Id)
- FK_Grades_Exams (ExamId → Exams.Id)
- FK_Grades_Lecturers_EnteredBy (EnteredBy → Lecturers.Id)
- FK_Grades_Lecturers_ApprovedBy (ApprovedBy → Lecturers.Id)

**Check Constraints:**
- CK_Grades_Score: Score >= 0 AND Score <= 10

---

### Attendances

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, IDENTITY | Primary key |
| ExamScheduleId | INT | FK → ExamSchedule.Id, NOT NULL | Lịch thi |
| StudentId | INT | FK → Student.Id, NOT NULL | Sinh viên |
| Status | VARCHAR(20) | DEFAULT 'Present' | Trạng thái |
| CheckInTime | DATETIME | NULL | Giờ vào |
| CheckOutTime | DATETIME | NULL | Giờ ra |
| Notes | NVARCHAR(500) | NULL | Ghi chú |
| Violation | VARCHAR(200) | NULL | Vi phạm |
| RecordedAt | DATETIME | DEFAULT GETDATE() | Ngày ghi nhận |

**Indexes:**
- IX_Attendances_ExamScheduleId
- IX_Attendances_StudentId
- IX_Attendances_Status

**Foreign Keys:**
- FK_Attendances_ExamSchedules (ExamScheduleId → ExamSchedules.Id)
- FK_Attendances_Students (StudentId → Students.Id)

---

## Database Views

### vw_StudentGrades

```sql
CREATE VIEW vw_StudentGrades AS
SELECT 
    s.Id AS StudentId,
    s.StudentCode,
    s.FullName,
    e.ExamCode,
    e.ExamName,
    g.Score,
    g.LetterGrade,
    g.Status AS GradeStatus,
    g.EnteredAt
FROM Students s
JOIN Grades g ON s.Id = g.StudentId
JOIN Exams e ON g.ExamId = e.Id;
```

### vw_ExamScheduleDetails

```sql
CREATE VIEW vw_ExamScheduleDetails AS
SELECT 
    es.Id AS ScheduleId,
    e.ExamCode,
    e.ExamName,
    es.Date,
    es.StartTime,
    es.EndTime,
    er.RoomName,
    er.Building,
    COUNT(pa.Id) AS ProctorCount
FROM ExamSchedules es
JOIN Exams e ON es.ExamId = e.Id
JOIN ExamRooms er ON es.RoomId = er.Id
LEFT JOIN ProctorAssignments pa ON es.Id = pa.ExamScheduleId
GROUP BY es.Id, e.ExamCode, e.ExamName, es.Date, es.StartTime, es.EndTime, er.RoomName, er.Building;
```

### vw_AttendanceSummary

```sql
CREATE VIEW vw_AttendanceSummary AS
SELECT 
    es.Id AS ScheduleId,
    e.ExamName,
    COUNT(CASE WHEN a.Status = 'Present' THEN 1 END) AS PresentCount,
    COUNT(CASE WHEN a.Status = 'Late' THEN 1 END) AS LateCount,
    COUNT(CASE WHEN a.Status = 'Absent' THEN 1 END) AS AbsentCount,
    COUNT(CASE WHEN a.Status = 'Excused' THEN 1 END) AS ExcusedCount
FROM ExamSchedules es
JOIN Exams e ON es.ExamId = e.Id
LEFT JOIN Attendances a ON es.Id = a.ExamScheduleId
GROUP BY es.Id, e.ExamName;
```

---

## Migrations

Migrations are stored in `e360_clone_api/Migrations/` folder.

### Initial Migration

The initial migration creates all tables with their relationships:

```bash
dotnet ef migrations add InitialCreate -o Migrations
dotnet ef database update
```

---

## Seed Data

Sample data for testing:

```sql
-- Insert sample students
INSERT INTO Students (StudentCode, FullName, DateOfBirth, Gender, Email, PhoneNumber, ClassId, Status)
VALUES 
('SV001', N'Nguyễn Văn A', '2000-01-15', N'Nam', 'sv001@e360.edu.vn', '0901234567', 1, 'Active'),
('SV002', N'Trần Thị B', '2000-03-20', N'Nữ', 'sv002@e360.edu.vn', '0901234568', 1, 'Active');

-- Insert sample lecturers
INSERT INTO Lecturers (EmployeeCode, FullName, DateOfBirth, Gender, Email, PhoneNumber, Department, Status)
VALUES 
('GV001', N'Phạm Văn C', '1980-05-10', N'Nam', 'gv001@e360.edu.vn', '0901111222', N'Công nghệ thông tin', 'Active');
```
