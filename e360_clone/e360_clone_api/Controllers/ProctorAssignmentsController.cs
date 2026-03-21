using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    [Route("api/proctors")]
    public class ProctorAssignmentsController : BaseApiController
    {
        private readonly IRepository<ProctorAssignment> _repository;

        public ProctorAssignmentsController(IRepository<ProctorAssignment> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var items = await _repository.GetAllAsync();
            var query = items.AsQueryable();

            var totalRecords = query.Count();
            var data = query
                .OrderBy(x => x.ExamId)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Ok(new PagedResponse<ProctorAssignment>
            {
                Success = true,
                Message = "Lấy danh sách phân công coi thi thành công",
                Data = data,
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
