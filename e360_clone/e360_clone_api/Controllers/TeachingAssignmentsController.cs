using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace e360_clone.Controllers
{
    [Route("api/teaching-assignments")]
    public class TeachingAssignmentsController : BaseApiController
    {
        private readonly ITeachingAssignmentRepository _repository;

        public TeachingAssignmentsController(ITeachingAssignmentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, [FromQuery] int? lecturerId, [FromQuery] int? subjectId, [FromQuery] int? classId)
        {
            Expression<Func<TeachingAssignment, bool>> filter = x =>
                (!lecturerId.HasValue || x.LecturerId == lecturerId.Value) &&
                (!subjectId.HasValue || x.SubjectId == subjectId.Value) &&
                (!classId.HasValue || x.ClassId == classId.Value);

            var data = await _repository.GetPagedFilteredAsync(
                request.PageNumber,
                request.PageSize,
                filter,
                q => q.OrderBy(x => x.Id));
            var totalRecords = await _repository.CountAsync(filter);

            return Ok(new PagedResponse<TeachingAssignment>
            {
                Success = true,
                Message = "Lấy danh sách phân công giảng dạy thành công",
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
                return HandleNotFound($"Không tìm thấy phân công giảng dạy có ID = {id}");

            return HandleResult(item, "Lấy thông tin phân công giảng dạy thành công");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TeachingAssignment assignment)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<TeachingAssignment> { Success = false, Message = "Dữ liệu không hợp lệ" });

            var exists = await _repository.AnyAsync(x =>
                x.SubjectId == assignment.SubjectId &&
                x.ClassId == assignment.ClassId &&
                x.Status == "Active");
            if (exists)
            {
                return BadRequest(new ApiResponse<TeachingAssignment>
                {
                    Success = false,
                    Message = "Lớp này đã được gán cho môn học.",
                    Data = null
                });
            }

            assignment.Status = string.IsNullOrWhiteSpace(assignment.Status) ? "Active" : assignment.Status;
            assignment.AcademicYear ??= string.Empty;
            assignment.Semester ??= string.Empty;
            assignment.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(assignment);

            return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, new ApiResponse<TeachingAssignment>
            {
                Success = true,
                Message = "Thêm phân công giảng dạy thành công",
                Data = assignment
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TeachingAssignment assignment)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy phân công giảng dạy có ID = {id}");

            existing.LecturerId = assignment.LecturerId;
            existing.SubjectId = assignment.SubjectId;
            existing.ClassId = assignment.ClassId;
            existing.AcademicYear = assignment.AcademicYear;
            existing.Semester = assignment.Semester;
            existing.Status = assignment.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật phân công giảng dạy thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy phân công giảng dạy có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa phân công giảng dạy thành công");
        }
    }
}
