using e360_clone.BusinessObjects;
using e360_clone.Repositories;
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
    }
}
