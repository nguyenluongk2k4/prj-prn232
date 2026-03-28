# Context Data Flows

## Admin

**Actions**
- Manage users and roles
- Manage students, lecturers, classes, subjects
- Manage exams and schedules
- Approve/publish grades
- View reports and dashboards

**Data In**
- User accounts, roles, permissions
- Master data: students, lecturers, classes, subjects
- Exam/schedule data
- Grade submissions from lecturers
- Attendance summaries

**Data Out**
- Created/updated master data
- Exam schedules and room assignments
- Approved/published grades
- Reports (summary, statistics)

---

## Lecturer (Teacher)

**Actions**
- View assigned teaching/exam schedules
- Take attendance for exam sessions
- Enter/update grades for assigned exams
- Submit grades for approval

**Data In**
- Assigned exam schedules
- Student rosters for classes/exams
- Grade policies (score types, thresholds)

**Data Out**
- Attendance records
- Grade entries (draft/submitted)

---

## Student

**Actions**
- View personal exam schedule
- Check-in attendance (QR/manual)
- View published grades/transcript

**Data In**
- Personal exam schedule
- Notifications/reminders
- Published grades

**Data Out**
- Attendance check-in records

---

## CronJob (Scheduled Jobs)

**Actions**
- Update exam/subject status by time rules
- Send reminders when check-in is missing after exam session

**Data In**
- Exam schedules
- Attendance status
- Subject/exam status rules

**Data Out**
- Updated status records (exam/subject)
- Notification events to Email/SMS gateway
