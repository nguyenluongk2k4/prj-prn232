using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using e360_clone.BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class AttendancesController : BaseApiController
    {
        private readonly IAttendanceRepository _repository;
        public AttendancesController(IAttendanceRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher")]
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
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher,Student,Parent")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy điểm danh có ID = {id}");

            return HandleResult(item, "Lấy thông tin điểm danh thành công");
        }

        [HttpGet("roster")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher")]
        public async Task<IActionResult> GetRoster([FromQuery] int examId)
        {
            if (examId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "ExamId không hợp lệ" });
            }

            var roster = await _repository.GetRosterAsync(examId);

            if (roster == null)
            {
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {examId}");
            }

            return Ok(new ApiResponse<List<AttendanceRosterItemDto>>
            {
                Success = true,
                Message = "Lấy danh sách điểm danh theo ca thi thành công",
                Data = roster
            });
        }

        [HttpGet("student")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher,Student,Parent")]
        public async Task<IActionResult> GetStudentAttendance([FromQuery] int studentId, [FromQuery] DateTime? date)
        {
            if (studentId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "StudentId không hợp lệ" });
            }

            var items = await _repository.GetStudentAttendanceAsync(studentId, date);

            if (items.Count == 0)
            {
                return Ok(new ApiResponse<List<StudentAttendanceItemDto>>
                {
                    Success = true,
                    Message = "Không có ca thi.",
                    Data = new List<StudentAttendanceItemDto>()
                });
            }

            return Ok(new ApiResponse<List<StudentAttendanceItemDto>>
            {
                Success = true,
                Message = "Lấy danh sách điểm danh sinh viên thành công",
                Data = items
            });
        }

        [HttpGet("report")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff")]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? subjectId,
            [FromQuery] int? classId)
        {
            try
            {
                var data = await _repository.GetReportAsync(fromDate, toDate, subjectId, classId);
                return Ok(new ApiResponse<List<AttendanceReportItemDto>>
                {
                    Success = true,
                    Message = "Lấy báo cáo điểm danh thành công",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return HandleError("GetReport failed: " + ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher")]
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
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher,Student")]
        public async Task<IActionResult> Update(int id, [FromBody] Attendance attendance)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy điểm danh có ID = {id}");

            var statusValue = string.IsNullOrWhiteSpace(attendance.Status)
                ? existing.Status?.Trim() ?? string.Empty
                : attendance.Status.Trim();
            var canStudentConfirm = statusValue.Equals("Present", StringComparison.OrdinalIgnoreCase)
                                    || statusValue.Equals("Late", StringComparison.OrdinalIgnoreCase);

            if (attendance.StudentConfirmed && !canStudentConfirm)
            {
                return BadRequest(new ApiResponse<Attendance>
                {
                    Success = false,
                    Message = "Chỉ được ký khi trạng thái là Present hoặc Late.",
                    Data = existing
                });
            }

            existing.ExamId = attendance.ExamId;
            existing.StudentId = attendance.StudentId;
            if (!string.IsNullOrWhiteSpace(attendance.Status))
            {
                existing.Status = attendance.Status;
            }
            existing.CheckInTime = attendance.CheckInTime;
            existing.CheckOutTime = attendance.CheckOutTime;
            existing.Notes = attendance.Notes;
            existing.Violation = attendance.Violation;
            existing.StudentConfirmed = attendance.StudentConfirmed;
            existing.StudentConfirmedAt = attendance.StudentConfirmed
                ? (attendance.StudentConfirmedAt ?? DateTime.UtcNow)
                : null;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật điểm danh thành công");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy điểm danh có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa điểm danh thành công");
        }
    }
}
