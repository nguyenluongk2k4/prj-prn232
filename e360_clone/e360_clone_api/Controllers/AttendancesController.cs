using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using e360_clone.BusinessObjects.DTOs;
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

            var roster = await _repository.GetRosterAsync(examId);

            if (roster == null)
            {
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y ká»³ thi cÃ³ ID = {examId}");
            }

            return Ok(new ApiResponse<List<AttendanceRosterItemDto>>
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

            var items = await _repository.GetStudentAttendanceAsync(studentId, date);

            if (items.Count == 0)
            {
                return Ok(new ApiResponse<List<StudentAttendanceItemDto>>
                {
                    Success = true,
                    Message = "KhÃƒÂ´ng cÃƒÂ³ ca thi.",
                    Data = new List<StudentAttendanceItemDto>()
                });
            }

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
                var data = await _repository.GetReportAsync(fromDate, toDate, subjectId, classId);
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

    }
}


