using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class ClassesController : BaseApiController
    {
        private readonly IRepository<Class> _repository;

        public ClassesController(IRepository<Class> repository)
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
                query = query.Where(x => x.ClassName.Contains(request.SearchTerm) || x.ClassCode.Contains(request.SearchTerm));
            }

            var totalRecords = query.Count();
            var data = query
                .OrderBy(x => x.ClassCode)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Ok(new PagedResponse<Class>
            {
                Success = true,
                Message = "Lấy danh sách lớp học thành công",
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
