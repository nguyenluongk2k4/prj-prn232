using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class ClassesController : BaseApiController
    {
        private readonly IClassRepository _repository;

        public ClassesController(IClassRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var term = request.SearchTerm?.Trim();
            Func<IQueryable<Class>, IOrderedQueryable<Class>> orderBy = q => q.OrderBy(x => x.ClassCode);

            IEnumerable<Class> data;
            int totalRecords;

            if (!string.IsNullOrEmpty(term))
            {
                data = await _repository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    x => x.ClassName.Contains(term) || x.ClassCode.Contains(term),
                    orderBy);
                totalRecords = await _repository.CountAsync(
                    x => x.ClassName.Contains(term) || x.ClassCode.Contains(term));
            }
            else
            {
                data = await _repository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    null,
                    orderBy);
                totalRecords = await _repository.CountAsync();
            }

            return Ok(new PagedResponse<Class>
            {
                Success = true,
                Message = "Lấy danh sách lớp học thành công",
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
                return HandleNotFound($"Không tìm thấy lớp học có ID = {id}");

            return HandleResult(item, "Lấy thông tin lớp học thành công");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Class cls)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Class> { Success = false, Message = "Dữ liệu không hợp lệ" });

            cls.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(cls);

            return CreatedAtAction(nameof(GetById), new { id = cls.Id }, new ApiResponse<Class>
            {
                Success = true,
                Message = "Thêm lớp học thành công",
                Data = cls
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Class cls)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy lớp học có ID = {id}");

            existing.ClassCode = cls.ClassCode;
            existing.ClassName = cls.ClassName;
            existing.MajorId = cls.MajorId;
            existing.CourseId = cls.CourseId;
            existing.AcademicYear = cls.AcademicYear;
            existing.Semester = cls.Semester;
            existing.StudentCount = cls.StudentCount;
            existing.Status = cls.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật lớp học thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy lớp học có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa lớp học thành công");
        }
    }
}
