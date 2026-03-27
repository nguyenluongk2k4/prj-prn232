using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    [Route("api/proctors")]
    public class ProctorAssignmentsController : BaseApiController
    {
        private readonly IProctorAssignmentRepository _repository;
        private readonly IExamRepository _examRepository;
        public ProctorAssignmentsController(
            IProctorAssignmentRepository repository,
            IExamRepository examRepository)
        {
            _repository = repository;
            _examRepository = examRepository;
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

            return Ok(new PagedResponse<ProctorAssignment>
            {
                Success = true,
                Message = "Lấy danh sách phân công coi thi thành công",
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
                return HandleNotFound($"Không tìm thấy phân công coi thi có ID = {id}");

            return HandleResult(item, "Lấy thông tin phân công coi thi thành công");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProctorAssignment assignment)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<ProctorAssignment> { Success = false, Message = "Dữ liệu không hợp lệ" });

            var exam = await _examRepository.GetByIdAsync(assignment.ExamId);
            if (exam == null)
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {assignment.ExamId}");

            var hasConflict = await _repository.HasScheduleConflictAsync(assignment.LecturerId, assignment.ExamId, null);
            if (hasConflict)
            {
                return BadRequest(new ApiResponse<ProctorAssignment>
                {
                    Success = false,
                    Message = "Giảng viên đã có lịch coi thi trùng thời gian"
                });
            }

            assignment.AssignedAt = DateTime.UtcNow;
            await _repository.AddAsync(assignment);

            return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, new ApiResponse<ProctorAssignment>
            {
                Success = true,
                Message = "Thêm phân công coi thi thành công",
                Data = assignment
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProctorAssignment assignment)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy phân công coi thi có ID = {id}");

            var exam = await _examRepository.GetByIdAsync(assignment.ExamId);
            if (exam == null)
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {assignment.ExamId}");

            var hasConflict = await _repository.HasScheduleConflictAsync(assignment.LecturerId, assignment.ExamId, id);
            if (hasConflict)
            {
                return BadRequest(new ApiResponse<ProctorAssignment>
                {
                    Success = false,
                    Message = "Giảng viên đã có lịch coi thi trùng thời gian"
                });
            }

            existing.ExamId = assignment.ExamId;
            existing.LecturerId = assignment.LecturerId;
            existing.Role = assignment.Role;
            existing.Status = assignment.Status;
            existing.Notes = assignment.Notes;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật phân công coi thi thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy phân công coi thi có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa phân công coi thi thành công");
        }

    }
}
