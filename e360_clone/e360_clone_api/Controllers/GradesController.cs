using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class GradesController : BaseApiController
    {
        private readonly IGradeRepository _repository;

        public GradesController(IGradeRepository repository)
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
                q => q.OrderBy(x => x.StudentId));
            var totalRecords = await _repository.CountAsync();

            return Ok(new PagedResponse<Grade>
            {
                Success = true,
                Message = "Lấy danh sách điểm thành công",
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
                return HandleNotFound($"Không tìm thấy điểm có ID = {id}");

            return HandleResult(item, "Lấy thông tin điểm thành công");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Grade grade)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Grade> { Success = false, Message = "Dữ liệu không hợp lệ" });

            await _repository.AddAsync(grade);

            return CreatedAtAction(nameof(GetById), new { id = grade.Id }, new ApiResponse<Grade>
            {
                Success = true,
                Message = "Thêm điểm thành công",
                Data = grade
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Grade grade)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy điểm có ID = {id}");

            existing.StudentId = grade.StudentId;
            existing.ExamId = grade.ExamId;
            existing.Score = grade.Score;
            existing.ScoreType = grade.ScoreType;
            existing.LetterGrade = grade.LetterGrade;
            existing.Notes = grade.Notes;
            existing.EnteredBy = grade.EnteredBy;
            existing.EnteredAt = grade.EnteredAt;
            existing.ApprovedBy = grade.ApprovedBy;
            existing.ApprovedAt = grade.ApprovedAt;
            existing.Status = grade.Status;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật điểm thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy điểm có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa điểm thành công");
        }
    }
}
