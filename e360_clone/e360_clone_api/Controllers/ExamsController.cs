using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using e360_clone.BusinessObjects.DTOs;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace e360_clone.Controllers
{
    public class ExamsController : BaseApiController
    {
        private readonly IExamRepository _repository;
        private readonly AppDbContext _context;
        private readonly ILogger<ExamsController> _logger;

        public ExamsController(IExamRepository repository, AppDbContext context, ILogger<ExamsController> logger)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PagedRequest request,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var term = request.SearchTerm?.Trim();
            Func<IQueryable<Exam>, IOrderedQueryable<Exam>> orderBy = q => q.OrderBy(x => x.ExamDate);

            var hasFilter = !string.IsNullOrEmpty(term) || fromDate.HasValue || toDate.HasValue;
            Expression<Func<Exam, bool>>? filter = null;
            if (hasFilter)
            {
                var from = fromDate?.Date;
                var to = toDate?.Date;
                filter = x =>
                    (string.IsNullOrEmpty(term) || x.ExamName.Contains(term) || x.ExamCode.Contains(term)) &&
                    (!from.HasValue || x.ExamDate.Date >= from.Value) &&
                    (!to.HasValue || x.ExamDate.Date <= to.Value);
            }

            IEnumerable<Exam> data = await _repository.GetPagedFilteredAsync(
                request.PageNumber,
                request.PageSize,
                filter,
                orderBy);
            var totalRecords = filter == null
                ? await _repository.CountAsync()
                : await _repository.CountAsync(filter);

            return Ok(new PagedResponse<Exam>
            {
                Success = true,
                Message = "Láº¥y danh sÃ¡ch ká»³ thi thÃ nh cÃ´ng",
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
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y ká»³ thi cÃ³ ID = {id}");

            return HandleResult(item, "Láº¥y thÃ´ng tin ká»³ thi thÃ nh cÃ´ng");
        }

        [HttpGet("student")]
        public async Task<IActionResult> GetByStudent(
            [FromQuery] int studentId,
            [FromQuery] string? email,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                if (studentId <= 0 && string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new ApiResponse<List<StudentExamScheduleDto>>
                    {
                        Success = false,
                        Message = "StudentId hoặc email không hợp lệ"
                    });
                }

                var from = (fromDate ?? DateTime.UtcNow.Date.AddDays(-7)).Date;
                var to = (toDate ?? DateTime.UtcNow.Date.AddDays(7)).Date;
                var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
                var toUtcExclusive = DateTime.SpecifyKind(to.AddDays(1), DateTimeKind.Utc);

                if (studentId <= 0 && !string.IsNullOrWhiteSpace(email))
                {
                    studentId = await _context.Accounts
                        .Where(a => a.Email == email)
                        .Select(a => a.StudentId)
                        .Where(id => id.HasValue)
                        .Select(id => id!.Value)
                        .FirstOrDefaultAsync();
                }

                if (studentId <= 0)
                {
                    return Ok(new ApiResponse<List<StudentExamScheduleDto>>
                    {
                        Success = true,
                        Message = "Không tìm thấy sinh viên.",
                        Data = new List<StudentExamScheduleDto>()
                    });
                }

                var exams = await _context.ExamRoomAllocations
                    .Where(alloc => alloc.StudentId == studentId)
                    .Join(
                        _context.Exams,
                        alloc => alloc.ExamId,
                        e => e.Id,
                        (alloc, e) => new StudentExamScheduleDto
                        {
                            Id = e.Id,
                            ExamCode = e.ExamCode,
                            ExamName = e.ExamName,
                            ExamType = e.ExamType,
                            SubjectId = e.SubjectId,
                            ClassId = e.ClassId,
                            RoomId = alloc.RoomId,
                            ExamDate = e.ExamDate,
                            StartTime = e.StartTime,
                            EndTime = e.EndTime,
                            Duration = e.Duration,
                            AcademicYear = e.AcademicYear,
                            Semester = e.Semester,
                            Status = e.Status,
                            Notes = e.Notes,
                            SeatNumber = alloc.SeatNumber
                        })
                    .Where(e => e.ExamDate >= fromUtc && e.ExamDate < toUtcExclusive)
                    .Distinct()
                    .OrderBy(e => e.ExamDate)
                    .ThenBy(e => e.StartTime)
                    .ToListAsync();

                if (exams.Count == 0)
                {
                    exams = await _context.StudentExams
                        .Where(se => se.StudentId == studentId)
                        .Join(
                            _context.Exams,
                            se => se.ExamId,
                            e => e.Id,
                            (se, e) => new StudentExamScheduleDto
                            {
                                Id = e.Id,
                                ExamCode = e.ExamCode,
                                ExamName = e.ExamName,
                                ExamType = e.ExamType,
                                SubjectId = e.SubjectId,
                                ClassId = e.ClassId,
                                RoomId = e.RoomId,
                                ExamDate = e.ExamDate,
                                StartTime = e.StartTime,
                                EndTime = e.EndTime,
                                Duration = e.Duration,
                                AcademicYear = e.AcademicYear,
                                Semester = e.Semester,
                                Status = e.Status,
                                Notes = e.Notes,
                                SeatNumber = 0
                            })
                        .Where(e => e.ExamDate >= fromUtc && e.ExamDate < toUtcExclusive)
                        .Distinct()
                        .OrderBy(e => e.ExamDate)
                        .ThenBy(e => e.StartTime)
                        .ToListAsync();
                }

                if (exams.Count == 0)
                {
                    var classId = await _context.Students
                        .Where(s => s.Id == studentId)
                        .Select(s => s.ClassId)
                        .FirstOrDefaultAsync();

                    if (classId > 0)
                    {
                        exams = await _context.Exams
                            .Where(e => e.ClassId == classId)
                            .Where(e => e.ExamDate >= fromUtc && e.ExamDate < toUtcExclusive)
                            .Select(e => new StudentExamScheduleDto
                            {
                                Id = e.Id,
                                ExamCode = e.ExamCode,
                                ExamName = e.ExamName,
                                ExamType = e.ExamType,
                                SubjectId = e.SubjectId,
                                ClassId = e.ClassId,
                                RoomId = e.RoomId,
                                ExamDate = e.ExamDate,
                                StartTime = e.StartTime,
                                EndTime = e.EndTime,
                                Duration = e.Duration,
                                AcademicYear = e.AcademicYear,
                                Semester = e.Semester,
                                Status = e.Status,
                                Notes = e.Notes,
                                SeatNumber = 0
                            })
                            .OrderBy(e => e.ExamDate)
                            .ThenBy(e => e.StartTime)
                            .ToListAsync();
                    }
                }

                return Ok(new ApiResponse<List<StudentExamScheduleDto>>
                {
                    Success = true,
                    Message = "Lấy lịch thi sinh viên thành công",
                    Data = exams
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByStudent failed. studentId={StudentId} email={Email} from={From} to={To}", studentId, email, fromDate, toDate);
                return HandleError($"GetByStudent failed: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Exam exam)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Exam> { Success = false, Message = "Dá»¯ liá»‡u khÃ´ng há»£p lá»‡" });

            exam.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(exam);

            return CreatedAtAction(nameof(GetById), new { id = exam.Id }, new ApiResponse<Exam>
            {
                Success = true,
                Message = "ThÃªm ká»³ thi thÃ nh cÃ´ng",
                Data = exam
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Exam exam)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y ká»³ thi cÃ³ ID = {id}");

            existing.ExamCode = exam.ExamCode;
            existing.ExamName = exam.ExamName;
            existing.ExamType = exam.ExamType;
            existing.SubjectId = exam.SubjectId;
            existing.ClassId = exam.ClassId;
            existing.ExamDate = exam.ExamDate;
            existing.StartTime = exam.StartTime;
            existing.EndTime = exam.EndTime;
            existing.Duration = exam.Duration;
            existing.RoomId = exam.RoomId;
            existing.AcademicYear = exam.AcademicYear;
            existing.Semester = exam.Semester;
            existing.Status = exam.Status;
            existing.Notes = exam.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cáº­p nháº­t ká»³ thi thÃ nh cÃ´ng");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y ká»³ thi cÃ³ ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "XÃ³a ká»³ thi thÃ nh cÃ´ng");
        }
    }
}
