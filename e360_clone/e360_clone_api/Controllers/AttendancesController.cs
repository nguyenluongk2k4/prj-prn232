using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using e360_clone.DataAccess;
using e360_clone.BusinessObjects.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class AttendancesController : BaseApiController
    {
        private readonly IAttendanceRepository _repository;
        private readonly AppDbContext _context;

        public AttendancesController(IAttendanceRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var data = await _repository.GetPagedFilteredAsync(
                request.PageNumber,
                request.PageSize,
                null,
                q => q.OrderBy(x => x.ExamId));
            var totalRecords = await _repository.CountAsync();

            return Ok(new PagedResponse<Attendance>
            {
                Success = true,
                Message = "Láº¥y danh sÃ¡ch Ä‘iá»ƒm danh thÃ nh cÃ´ng",
                Data = data.ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y Ä‘iá»ƒm danh cÃ³ ID = {id}");

            return HandleResult(item, "Láº¥y thÃ´ng tin Ä‘iá»ƒm danh thÃ nh cÃ´ng");
        }

        [HttpGet("roster")]
        public async Task<IActionResult> GetRoster([FromQuery] int examId)
        {
            if (examId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "ExamId khÃ´ng há»£p lá»‡" });
            }

            var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null)
            {
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y ká»³ thi cÃ³ ID = {examId}");
            }

            var studentExams = await _context.StudentExams
                .Where(se => se.ExamId == examId)
                .ToListAsync();

            var studentIds = studentExams.Select(se => se.StudentId).Distinct().ToList();
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

            var roster = students
                .OrderBy(s => s.StudentCode)
                .Select(s =>
                {
                    var attendance = attendanceByStudent[s.Id];
                    classMap.TryGetValue(s.ClassId, out var classCode);

                    accountMap.TryGetValue(s.Id, out var avatarUrl);

                    return new AttendanceRosterItem
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

            return Ok(new ApiResponse<List<AttendanceRosterItem>>
            {
                Success = true,
                Message = "Láº¥y danh sÃ¡ch Ä‘iá»ƒm danh theo ca thi thÃ nh cÃ´ng",
                Data = roster
            });
        }

        [HttpGet("student")]
        public async Task<IActionResult> GetStudentAttendance([FromQuery] int studentId, [FromQuery] DateTime? date)
        {
            if (studentId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "StudentId khÃƒÂ´ng hÃ¡Â»Â£p lÃ¡Â»â€¡" });
            }

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
                return Ok(new ApiResponse<List<StudentAttendanceItemDto>>
                {
                    Success = true,
                    Message = "KhÃƒÂ´ng cÃƒÂ³ ca thi.",
                    Data = new List<StudentAttendanceItemDto>()
                });
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

            var items = await query
                .OrderBy(x => x.ExamDate)
                .ThenBy(x => x.StartTime)
                .ThenBy(x => x.SubjectCode)
                .ToListAsync();

            return Ok(new ApiResponse<List<StudentAttendanceItemDto>>
            {
                Success = true,
                Message = "LÃ¡ÂºÂ¥y danh sÃƒÂ¡ch Ã„â€˜iÃ¡Â»Æ’m danh sinh viÃƒÂªn thÃƒÂ nh cÃƒÂ´ng",
                Data = items
            });
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? subjectId,
            [FromQuery] int? classId)
        {
            try
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

                var data = await query
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

                return Ok(new ApiResponse<List<AttendanceReportItemDto>>
                {
                    Success = true,
                    Message = "L?y b?o c?o ?i?m danh th?nh c?ng",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return HandleError("GetReport failed: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Attendance attendance)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Attendance> { Success = false, Message = "Dá»¯ liá»‡u khÃ´ng há»£p lá»‡" });

            attendance.RecordedAt = DateTime.UtcNow;
            await _repository.AddAsync(attendance);

            return CreatedAtAction(nameof(GetById), new { id = attendance.Id }, new ApiResponse<Attendance>
            {
                Success = true,
                Message = "ThÃªm Ä‘iá»ƒm danh thÃ nh cÃ´ng",
                Data = attendance
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Attendance attendance)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y Ä‘iá»ƒm danh cÃ³ ID = {id}");

            existing.ExamId = attendance.ExamId;
            existing.StudentId = attendance.StudentId;
            existing.Status = attendance.Status;
            existing.CheckInTime = attendance.CheckInTime;
            existing.CheckOutTime = attendance.CheckOutTime;
            existing.Notes = attendance.Notes;
            existing.Violation = attendance.Violation;
            existing.StudentConfirmed = attendance.StudentConfirmed;
            existing.StudentConfirmedAt = attendance.StudentConfirmedAt;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cáº­p nháº­t Ä‘iá»ƒm danh thÃ nh cÃ´ng");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y Ä‘iá»ƒm danh cÃ³ ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "XÃ³a Ä‘iá»ƒm danh thÃ nh cÃ´ng");
        }

        private sealed class AttendanceRosterItem
        {
            public int AttendanceId { get; set; }
            public int ExamId { get; set; }
            public int StudentId { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string? AvatarUrl { get; set; }
            public string ClassCode { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime? CheckInTime { get; set; }
            public DateTime? CheckOutTime { get; set; }
            public string Notes { get; set; } = string.Empty;
            public string Violation { get; set; } = string.Empty;
            public bool StudentConfirmed { get; set; }
            public DateTime? StudentConfirmedAt { get; set; }
        }

    }
}


