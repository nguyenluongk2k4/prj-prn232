using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    public class AttendanceDAO : BaseDAO<Attendance>
    {
        public AttendanceDAO(AppDbContext context) : base(context)
        {
        }

        public async Task<List<AttendanceRosterItemDto>?> GetRosterAsync(int examId)
        {
            var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null)
            {
                return null;
            }

            var studentExams = await _context.StudentExams
                .Where(se => se.ExamId == examId)
                .ToListAsync();

            var studentIds = studentExams.Select(se => se.StudentId).Distinct().ToList();
            if (studentIds.Count == 0)
            {
                return new List<AttendanceRosterItemDto>();
            }

            var students = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();
            var classIds = students.Select(s => s.ClassId).Distinct().ToList();
            var classes = await _context.Classes
                .Where(c => classIds.Contains(c.Id))
                .ToListAsync();

            var attendances = await _context.Attendances
                .Where(a => a.ExamId == examId)
                .ToListAsync();

            var attendanceByStudent = attendances.ToDictionary(a => a.StudentId, a => a);
            var toAdd = new List<Attendance>();
            foreach (var studentId in studentIds)
            {
                if (!attendanceByStudent.ContainsKey(studentId))
                {
                    var attendance = new Attendance
                    {
                        ExamId = examId,
                        StudentId = studentId,
                        Status = "Pending",
                        RecordedAt = DateTime.UtcNow
                    };
                    toAdd.Add(attendance);
                    attendanceByStudent[studentId] = attendance;
                }
            }

            if (toAdd.Count > 0)
            {
                await _context.Attendances.AddRangeAsync(toAdd);
                await _context.SaveChangesAsync();
            }

            var classMap = classes.ToDictionary(c => c.Id, c => c.ClassCode);
            var studentIdsForAccounts = students.Select(s => s.Id).ToList();
            var accountMap = await _context.Accounts
                .Where(a => a.StudentId.HasValue && studentIdsForAccounts.Contains(a.StudentId.Value))
                .ToDictionaryAsync(a => a.StudentId!.Value, a => a.AvatarUrl);

            return students
                .OrderBy(s => s.StudentCode)
                .Select(s =>
                {
                    var attendance = attendanceByStudent[s.Id];
                    classMap.TryGetValue(s.ClassId, out var classCode);
                    accountMap.TryGetValue(s.Id, out var avatarUrl);

                    return new AttendanceRosterItemDto
                    {
                        AttendanceId = attendance.Id,
                        ExamId = examId,
                        StudentId = s.Id,
                        StudentCode = s.StudentCode,
                        FullName = s.FullName,
                        AvatarUrl = avatarUrl,
                        ClassCode = classCode ?? string.Empty,
                        Status = attendance.Status,
                        CheckInTime = attendance.CheckInTime,
                        CheckOutTime = attendance.CheckOutTime,
                        Notes = attendance.Notes,
                        Violation = attendance.Violation,
                        StudentConfirmed = attendance.StudentConfirmed,
                        StudentConfirmedAt = attendance.StudentConfirmedAt
                    };
                })
                .ToList();
        }

        public async Task<List<StudentAttendanceItemDto>> GetStudentAttendanceAsync(int studentId, DateTime? date)
        {
            var targetDate = (date ?? DateTime.UtcNow.Date).Date;
            var fromUtc = DateTime.SpecifyKind(targetDate, DateTimeKind.Utc);
            var toUtcExclusive = DateTime.SpecifyKind(targetDate.AddDays(1), DateTimeKind.Utc);

            var allocationExamIds = await _context.ExamRoomAllocations
                .Where(a => a.StudentId == studentId)
                .Select(a => a.ExamId)
                .Distinct()
                .ToListAsync();

            if (allocationExamIds.Count == 0)
            {
                allocationExamIds = await _context.StudentExams
                    .Where(se => se.StudentId == studentId)
                    .Select(se => se.ExamId)
                    .Distinct()
                    .ToListAsync();
            }

            if (allocationExamIds.Count == 0)
            {
                return new List<StudentAttendanceItemDto>();
            }

            var existingAttendances = await _context.Attendances
                .Where(a => a.StudentId == studentId && allocationExamIds.Contains(a.ExamId))
                .ToListAsync();

            var existingByExam = existingAttendances.ToDictionary(a => a.ExamId, a => a);
            var toAdd = new List<Attendance>();
            foreach (var examId in allocationExamIds)
            {
                if (!existingByExam.ContainsKey(examId))
                {
                    var attendance = new Attendance
                    {
                        ExamId = examId,
                        StudentId = studentId,
                        Status = "Pending",
                        RecordedAt = DateTime.UtcNow
                    };
                    toAdd.Add(attendance);
                    existingByExam[examId] = attendance;
                }
            }

            if (toAdd.Count > 0)
            {
                await _context.Attendances.AddRangeAsync(toAdd);
                await _context.SaveChangesAsync();
            }

            var query =
                from e in _context.Exams
                join s in _context.Subjects on e.SubjectId equals s.Id
                join c in _context.Classes on e.ClassId equals c.Id
                join a in _context.Attendances on new { ExamId = e.Id, StudentId = studentId }
                    equals new { a.ExamId, a.StudentId }
                join alloc in _context.ExamRoomAllocations
                    on new { ExamId = e.Id, StudentId = studentId } equals new { alloc.ExamId, alloc.StudentId }
                    into allocs
                from alloc in allocs.DefaultIfEmpty()
                join r in _context.ExamRooms on alloc.RoomId equals r.Id into rooms
                from r in rooms.DefaultIfEmpty()
                where allocationExamIds.Contains(e.Id)
                      && e.ExamDate >= fromUtc
                      && e.ExamDate < toUtcExclusive
                select new StudentAttendanceItemDto
                {
                    AttendanceId = a.Id,
                    ExamId = e.Id,
                    ExamDate = e.ExamDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    SubjectCode = s.SubjectCode,
                    SubjectName = s.SubjectName,
                    ClassCode = c.ClassCode,
                    RoomCode = r != null ? r.RoomCode : string.Empty,
                    Status = a.Status,
                    StudentConfirmed = a.StudentConfirmed,
                    StudentConfirmedAt = a.StudentConfirmedAt,
                    CheckOutTime = a.CheckOutTime
                };

            return await query
                .OrderBy(x => x.ExamDate)
                .ThenBy(x => x.StartTime)
                .ThenBy(x => x.SubjectCode)
                .ToListAsync();
        }

        public async Task<List<AttendanceReportItemDto>> GetReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            int? subjectId,
            int? classId)
        {
            var startDate = (fromDate ?? DateTime.UtcNow.Date.AddDays(-7)).Date;
            var endDate = (toDate ?? DateTime.UtcNow.Date).Date;
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endUtcExclusive = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);

            var query = from a in _context.Attendances
                        join e in _context.Exams on a.ExamId equals e.Id
                        join s in _context.Subjects on e.SubjectId equals s.Id
                        join c in _context.Classes on e.ClassId equals c.Id
                        where e.ExamDate >= startUtc && e.ExamDate < endUtcExclusive
                        select new { Attendance = a, Exam = e, Subject = s, Class = c };

            if (subjectId.HasValue)
            {
                query = query.Where(x => x.Exam.SubjectId == subjectId.Value);
            }

            if (classId.HasValue)
            {
                query = query.Where(x => x.Exam.ClassId == classId.Value);
            }

            return await query
                .GroupBy(x => new
                {
                    Date = x.Exam.ExamDate.Date,
                    x.Exam.StartTime,
                    x.Exam.EndTime,
                    x.Exam.Id,
                    x.Exam.SubjectId,
                    x.Subject.SubjectCode,
                    x.Subject.SubjectName,
                    x.Exam.ClassId,
                    x.Class.ClassCode
                })
                .Select(g => new AttendanceReportItemDto
                {
                    ExamDate = g.Key.Date,
                    StartTime = g.Key.StartTime,
                    EndTime = g.Key.EndTime,
                    ExamId = g.Key.Id,
                    SubjectId = g.Key.SubjectId,
                    SubjectCode = g.Key.SubjectCode,
                    SubjectName = g.Key.SubjectName,
                    ClassId = g.Key.ClassId,
                    ClassCode = g.Key.ClassCode,
                    Total = g.Count(),
                    Present = g.Count(x => x.Attendance.Status == "Present"),
                    Absent = g.Count(x => x.Attendance.Status == "Absent"),
                    Late = g.Count(x => x.Attendance.Status == "Late"),
                    Excused = g.Count(x => x.Attendance.Status == "Excused"),
                    Confirmed = g.Count(x => x.Attendance.StudentConfirmed)
                })
                .OrderBy(x => x.ExamDate)
                .ThenBy(x => x.StartTime)
                .ThenBy(x => x.SubjectCode)
                .ThenBy(x => x.ClassCode)
                .ToListAsync();
        }
    }
}
