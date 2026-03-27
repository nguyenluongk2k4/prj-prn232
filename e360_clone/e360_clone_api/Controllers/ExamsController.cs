using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class ExamsController : BaseApiController
    {
        private readonly IExamRepository _repository;
        private readonly ILogger<ExamsController> _logger;

        public ExamsController(IExamRepository repository, ILogger<ExamsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher,Student,Parent")]
        public async Task<IActionResult> GetAll(
            [FromQuery] PagedRequest request,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await _repository.GetPagedFilteredWithMetaAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                fromDate,
                toDate);

            return Ok(new PagedResponse<Exam>
            {
                Success = true,
                Message = "Lấy danh sách kỳ thi thành công",
                Data = result.Items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = result.TotalRecords
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher,Student,Parent")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {id}");

            return HandleResult(item, "Lấy thông tin kỳ thi thành công");
        }

        [HttpGet("student")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher,Student,Parent")]
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

                var exams = await _repository.GetStudentScheduleAsync(studentId, email, fromDate, toDate);

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
        [Authorize(Roles = "SuperAdmin,Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] Exam exam)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Exam> { Success = false, Message = "Dữ liệu không hợp lệ" });

            exam.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(exam);

            return CreatedAtAction(nameof(GetById), new { id = exam.Id }, new ApiResponse<Exam>
            {
                Success = true,
                Message = "Thêm kỳ thi thành công",
                Data = exam
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff")]
        public async Task<IActionResult> Update(int id, [FromBody] Exam exam)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {id}");

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
            return HandleResult(existing, "Cập nhật kỳ thi thành công");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa kỳ thi thành công");
        }
    }
}
