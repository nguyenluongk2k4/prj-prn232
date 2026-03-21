using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class ExamsController : BaseApiController
    {
        private readonly IRepository<Exam> _repository;

        public ExamsController(IRepository<Exam> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var items = await _repository.GetAllAsync();
            var query = items.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(x => x.ExamName.Contains(request.SearchTerm) || x.ExamCode.Contains(request.SearchTerm));
            }

            var totalRecords = query.Count();
            var data = query
                .OrderBy(x => x.ExamDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Ok(new PagedResponse<Exam>
            {
                Success = true,
                Message = "Lấy danh sách kỳ thi thành công",
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
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {id}");

            return HandleResult(item, "Lấy thông tin kỳ thi thành công");
        }

        [HttpPost]
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
