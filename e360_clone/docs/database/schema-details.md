# Database Schema Details

## Account and Identity
Login accounts and links to student/lecturer.

### Accounts
**Description:** Login accounts and identity data.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `Username` | `character varying(50)` | NOT NULL; UNIQUE | Username |
| `Email` | `character varying(255)` | NOT NULL; UNIQUE | Email |
| `PasswordHash` | `character varying(255)` | NOT NULL | Hashed password |
| `Role` | `character varying(50)` | NOT NULL | Role |
| `FullName` | `character varying(100)` |  | Full name |
| `PhoneNumber` | `character varying(20)` |  | Phone number |
| `AvatarUrl` | `text` |  | Avatar URL |
| `Status` | `character varying(20)` | NOT NULL; DEFAULT 'Active'::character varying | Status |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `LastLoginAt` | `timestamp with time zone` |  | Last login time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |
| `StudentId` | `integer` | FK -> Students.Id (ON DELETE SET NULL) | Student reference |
| `LecturerId` | `integer` | FK -> Lecturers.Id (ON DELETE SET NULL) | Lecturer reference |

## Master Data
Core reference data for the system.

### Students
**Description:** Student profiles and personal info.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `StudentCode` | `character varying(20)` | NOT NULL; UNIQUE | Student code |
| `FullName` | `character varying(100)` | NOT NULL | Full name |
| `DateOfBirth` | `timestamp with time zone` | NOT NULL | Date of birth |
| `Gender` | `text` | NOT NULL | Gender |
| `Email` | `character varying(100)` | NOT NULL | Email |
| `PhoneNumber` | `character varying(20)` | NOT NULL | Phone number |
| `Address` | `text` | NOT NULL | Address |
| `ClassId` | `integer` | NOT NULL | Class reference |
| `Status` | `character varying(20)` | NOT NULL; DEFAULT 'Active'::character varying | Status |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |

### Lecturers
**Description:** Lecturer profiles and work info.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `EmployeeCode` | `character varying(20)` | NOT NULL; UNIQUE | Lecturer code |
| `FullName` | `character varying(100)` | NOT NULL | Full name |
| `DateOfBirth` | `timestamp with time zone` | NOT NULL | Date of birth |
| `Gender` | `text` | NOT NULL | Gender |
| `Email` | `character varying(100)` | NOT NULL | Email |
| `PhoneNumber` | `character varying(20)` | NOT NULL | Phone number |
| `Department` | `character varying(100)` | NOT NULL | Department |
| `Position` | `text` | NOT NULL |  |
| `Status` | `character varying(20)` | NOT NULL; DEFAULT 'Active'::character varying | Status |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |

### Majors
**Description:** Major catalog.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `MajorCode` | `character varying(10)` | NOT NULL; UNIQUE | Major code |
| `MajorName` | `character varying(100)` | NOT NULL | Major name |
| `MajorGroup` | `character varying(100)` | NOT NULL | Major group |
| `Status` | `character varying(20)` | NOT NULL; DEFAULT 'Active'::character varying | Status |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |

### Classes
**Description:** Class info and cohort mapping.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `ClassCode` | `character varying(20)` | NOT NULL; UNIQUE | Class code |
| `ClassName` | `character varying(100)` | NOT NULL | Class name |
| `MajorId` | `integer` | NOT NULL; FK -> Majors.Id (ON DELETE CASCADE) | Major reference |
| `CourseId` | `integer` | NOT NULL | Course id |
| `AcademicYear` | `character varying(20)` | NOT NULL | Academic year |
| `Semester` | `integer` | NOT NULL | Semester |
| `StudentCount` | `integer` | NOT NULL | Student count |
| `Status` | `character varying(20)` | NOT NULL; DEFAULT 'Active'::character varying | Status |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |
| `Cohort` | `integer` | NOT NULL; DEFAULT 0 | Cohort |
| `CohortYear` | `integer` | NOT NULL; DEFAULT 0 | Cohort year |

### Subjects
**Description:** Subject catalog and grading rules.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `SubjectCode` | `character varying(20)` | NOT NULL; UNIQUE | Subject code |
| `SubjectName` | `character varying(200)` | NOT NULL | Subject name |
| `Credits` | `integer` | NOT NULL | Credits |
| `TheoryHours` | `integer` | NOT NULL | Theory hours |
| `PracticeHours` | `integer` | NOT NULL | Practice hours |
| `Department` | `text` | NOT NULL | Department |
| `SubjectType` | `character varying(50)` | NOT NULL | Subject type |
| `Status` | `character varying(20)` | NOT NULL; DEFAULT 'Active'::character varying | Status |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |
| `MinFinalScore` | `numeric` |  | Min final score |
| `MinPracticalScore` | `numeric` |  | Min practical score |

### Terms
**Description:** Academic terms and date ranges.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `Code` | `character varying(20)` | NOT NULL | Code |
| `Name` | `character varying(100)` | NOT NULL | Name |
| `StartDate` | `timestamp with time zone` | NOT NULL | Start date |
| `EndDate` | `timestamp with time zone` | NOT NULL | End date |
| `IsCurrent` | `boolean` | NOT NULL; DEFAULT false | Is current term |
| `CreatedAt` | `timestamp with time zone` | NOT NULL; DEFAULT now() | Created time |

## Exam and Facilities
Exam sessions, rooms, and allocations.

### Exams
**Description:** Exam sessions by subject/class/time.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `ExamCode` | `character varying(20)` | NOT NULL; UNIQUE | Exam code |
| `ExamName` | `character varying(200)` | NOT NULL | Exam name |
| `ExamType` | `character varying(50)` | NOT NULL | Exam type |
| `SubjectId` | `integer` | NOT NULL | Subject reference |
| `ClassId` | `integer` | NOT NULL | Class reference |
| `ExamDate` | `timestamp with time zone` | NOT NULL | Exam date |
| `StartTime` | `interval` | NOT NULL | Start time |
| `EndTime` | `interval` | NOT NULL | End time |
| `Duration` | `integer` | NOT NULL | Duration (minutes) |
| `RoomId` | `integer` | NOT NULL | Room reference |
| `AcademicYear` | `character varying(20)` | NOT NULL | Academic year |
| `Semester` | `character varying(20)` | NOT NULL | Semester |
| `Status` | `character varying(20)` | NOT NULL; DEFAULT 'Planned'::character varying | Status |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |
| `CreatedBy` | `integer` | NOT NULL; DEFAULT 0 | Created by |
| `Notes` | `text` | NOT NULL; DEFAULT ''::character varying | Notes |

### ExamRooms
**Description:** Exam rooms and capacity.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `RoomCode` | `text` | NOT NULL | Room code |
| `RoomName` | `text` | NOT NULL | Room name |
| `Building` | `text` | NOT NULL | Building |
| `Capacity` | `integer` | NOT NULL | Capacity |
| `Floor` | `text` | NOT NULL | Floor |
| `HasComputer` | `boolean` | NOT NULL | Has computer |
| `HasProjector` | `boolean` | NOT NULL | Has projector |
| `Status` | `text` | NOT NULL | Status |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |

### ExamRoomAllocations
**Description:** Seat allocation per exam.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `ExamId` | `integer` | NOT NULL; FK -> Exams.Id (ON DELETE CASCADE) | Exam reference |
| `StudentId` | `integer` | NOT NULL; FK -> Students.Id (ON DELETE CASCADE) | Student reference |
| `RoomId` | `integer` | NOT NULL; FK -> ExamRooms.Id (ON DELETE CASCADE) | Room reference |
| `SeatNumber` | `integer` | NOT NULL | Seat number |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |

## Assignments
Teaching and proctor assignments.

### TeachingAssignments
**Description:** Teaching assignments by term.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `LecturerId` | `integer` | NOT NULL | Lecturer reference |
| `SubjectId` | `integer` | NOT NULL | Subject reference |
| `ClassId` | `integer` | NOT NULL | Class reference |
| `AcademicYear` | `character varying(20)` | NOT NULL | Academic year |
| `Semester` | `character varying(20)` | NOT NULL | Semester |
| `Status` | `character varying(20)` | NOT NULL; DEFAULT 'Active'::character varying | Status |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |

### ProctorAssignments
**Description:** Proctor assignments per exam.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `ExamId` | `integer` | NOT NULL | Exam reference |
| `LecturerId` | `integer` | NOT NULL | Lecturer reference |
| `Role` | `text` | NOT NULL | Role |
| `AssignedAt` | `timestamp with time zone` | NOT NULL | Assigned time |
| `Status` | `text` | NOT NULL | Status |
| `Notes` | `text` | NOT NULL | Notes |

## Attendance and Enrollment
Attendance and subject enrollment data.

### Attendances
**Description:** Attendance records per exam.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `ExamId` | `integer` | NOT NULL | Exam reference |
| `StudentId` | `integer` | NOT NULL | Student reference |
| `Status` | `text` | NOT NULL | Status |
| `CheckInTime` | `timestamp with time zone` |  | Check-in time |
| `CheckOutTime` | `timestamp with time zone` |  | Check-out time |
| `Notes` | `text` | NOT NULL | Notes |
| `Violation` | `text` | NOT NULL | Violation |
| `RecordedAt` | `timestamp with time zone` | NOT NULL | Recorded time |
| `StudentConfirmed` | `boolean` | NOT NULL; DEFAULT false | Student confirmed |
| `StudentConfirmedAt` | `timestamp with time zone` |  | Student confirmed time |

### StudentSubjects
**Description:** Student subject enrollment and stats.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | NOT NULL; PK | Primary key |
| `StudentId` | `integer` | NOT NULL; FK -> Students.Id (ON DELETE CASCADE) | Student reference |
| `SubjectId` | `integer` | NOT NULL; FK -> Subjects.Id (ON DELETE RESTRICT) | Subject reference |
| `ClassId` | `integer` | FK -> Classes.Id (ON DELETE SET NULL) | Class reference |
| `AcademicYear` | `character varying(20)` | NOT NULL | Academic year |
| `Semester` | `integer` | NOT NULL | Semester |
| `Status` | `character varying(20)` | NOT NULL | Status |
| `TotalSessions` | `integer` | NOT NULL | Total sessions |
| `PresentSessions` | `integer` | NOT NULL | Present sessions |
| `AbsentSessions` | `integer` | NOT NULL | Absent sessions |
| `Grade` | `numeric` |  | Final grade |
| `CreatedAt` | `timestamp with time zone` | NOT NULL | Created time |
| `UpdatedAt` | `timestamp with time zone` |  | Updated time |
| `TermId` | `integer` | NOT NULL; FK -> Terms.Id (ON DELETE RESTRICT) | Term reference |

## System
System-level metadata.

### __EFMigrationsHistory
**Description:** EF Core migration history.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `MigrationId` | `character varying(150)` | NOT NULL; PK |  |
| `ProductVersion` | `character varying(32)` | NOT NULL |  |

## Authorization (Proposed)
RBAC tables to replace the single `Role` field in `Accounts`.

### Roles
**Description:** Role catalog.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | PK | Primary key |
| `Code` | `character varying(50)` | NOT NULL; UNIQUE | Role code |
| `Name` | `character varying(100)` | NOT NULL | Role name |
| `Description` | `text` |  | Role description |
| `IsActive` | `boolean` | NOT NULL; DEFAULT true | Active flag |
| `CreatedAt` | `timestamp with time zone` | NOT NULL; DEFAULT now() | Created time |

### Permissions
**Description:** Permission catalog by module/function.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `integer` | PK | Primary key |
| `Code` | `character varying(50)` | NOT NULL; UNIQUE | Permission code |
| `Name` | `character varying(100)` | NOT NULL | Permission name |
| `Module` | `character varying(100)` | NOT NULL | Module name |
| `Description` | `text` |  | Permission description |
| `IsActive` | `boolean` | NOT NULL; DEFAULT true | Active flag |
| `CreatedAt` | `timestamp with time zone` | NOT NULL; DEFAULT now() | Created time |

### RolePermissions
**Description:** Many-to-many between roles and permissions.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `RoleId` | `integer` | PK; FK -> Roles.Id (ON DELETE CASCADE) | Role reference |
| `PermissionId` | `integer` | PK; FK -> Permissions.Id (ON DELETE CASCADE) | Permission reference |

### AccountRoles
**Description:** Many-to-many between accounts and roles.

| Column | Type | Constraints | Description |
|---|---|---|---|
| `AccountId` | `integer` | PK; FK -> Accounts.Id (ON DELETE CASCADE) | Account reference |
| `RoleId` | `integer` | PK; FK -> Roles.Id (ON DELETE CASCADE) | Role reference |
