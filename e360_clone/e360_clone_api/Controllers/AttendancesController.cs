using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using e360_clone.DataAccess;
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
                Message = "Lấy danh sách điểm danh thành công",
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
                return HandleNotFound($"Không tìm thấy điểm danh có ID = {id}");

            return HandleResult(item, "Lấy thông tin điểm danh thành công");
        }

        [HttpGet("roster")]
        public async Task<IActionResult> GetRoster([FromQuery] int examId)
        {
            if (examId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "ExamId không hợp lệ" });
            }

            var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null)
            {
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {examId}");
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

            var roster = students
                .OrderBy(s => s.StudentCode)
                .Select(s =>
                {
                    var attendance = attendanceByStudent[s.Id];
                    classMap.TryGetValue(s.ClassId, out var classCode);

                    return new AttendanceRosterItem
                    {
                        AttendanceId = attendance.Id,
                        ExamId = examId,
                        StudentId = s.Id,
                        StudentCode = s.StudentCode,
                        FullName = s.FullName,
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
                Message = "Lấy danh sách điểm danh theo ca thi thành công",
                Data = roster
            });
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? subjectId,
            [FromQuery] int? classId)
        {
            var startDate = (fromDate ?? DateTime.UtcNow.Date.AddDays(-7)).Date;
            var endDate = (toDate ?? DateTime.UtcNow.Date).Date;

            var query = from a in _context.Attendances
                        join e in _context.Exams on a.ExamId equals e.Id
                        join s in _context.Subjects on e.SubjectId equals s.Id
                        join c in _context.Classes on e.ClassId equals c.Id
                        where e.ExamDate.Date >= startDate && e.ExamDate.Date <= endDate
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
                    x.Exam.SubjectId,
                    x.Subject.SubjectCode,
                    x.Subject.SubjectName,
                    x.Exam.ClassId,
                    x.Class.ClassCode
                })
                .Select(g => new AttendanceReportItem
                {
                    ExamDate = g.Key.Date,
                    StartTime = g.Key.StartTime,
                    EndTime = g.Key.EndTime,
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

            return Ok(new ApiResponse<List<AttendanceReportItem>>
            {
                Success = true,
                Message = "Lấy báo cáo điểm danh thành công",
                Data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Attendance attendance)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Attendance> { Success = false, Message = "Dữ liệu không hợp lệ" });

            attendance.RecordedAt = DateTime.UtcNow;
            await _repository.AddAsync(attendance);

            return CreatedAtAction(nameof(GetById), new { id = attendance.Id }, new ApiResponse<Attendance>
            {
                Success = true,
                Message = "Thêm điểm danh thành công",
                Data = attendance
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Attendance attendance)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy điểm danh có ID = {id}");

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
            return HandleResult(existing, "Cập nhật điểm danh thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy điểm danh có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa điểm danh thành công");
        }

        private sealed class AttendanceRosterItem
        {
            public int AttendanceId { get; set; }
            public int ExamId { get; set; }
            public int StudentId { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string ClassCode { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime? CheckInTime { get; set; }
            public DateTime? CheckOutTime { get; set; }
            public string Notes { get; set; } = string.Empty;
            public string Violation { get; set; } = string.Empty;
            public bool StudentConfirmed { get; set; }
            public DateTime? StudentConfirmedAt { get; set; }
        }

        private sealed class AttendanceReportItem
        {
            public DateTime ExamDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int SubjectId { get; set; }
            public string SubjectCode { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public int ClassId { get; set; }
            public string ClassCode { get; set; } = string.Empty;
            public int Total { get; set; }
            public int Present { get; set; }
            public int Absent { get; set; }
            public int Late { get; set; }
            public int Excused { get; set; }
            public int Confirmed { get; set; }
        }
    }
}
