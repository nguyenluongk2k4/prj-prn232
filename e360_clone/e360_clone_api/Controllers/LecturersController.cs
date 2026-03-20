using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class LecturersController : BaseApiController
    {
        private readonly IRepository<Lecturer> _repository;

        public LecturersController(IRepository<Lecturer> repository)
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
                query = query.Where(x => x.FullName.Contains(request.SearchTerm) || x.EmployeeCode.Contains(request.SearchTerm));
            }

            var totalRecords = query.Count();
            var data = query
                .OrderBy(x => x.EmployeeCode)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Ok(new PagedResponse<Lecturer>
            {
                Success = true,
                Message = "Lấy danh sách giảng viên thành công",
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
                return HandleNotFound($"Không tìm thấy giảng viên có ID = {id}");

            return HandleResult(item, "Lấy thông tin giảng viên thành công");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Lecturer lecturer)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Lecturer> { Success = false, Message = "Dữ liệu không hợp lệ" });

            lecturer.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(lecturer);

            return CreatedAtAction(nameof(GetById), new { id = lecturer.Id }, new ApiResponse<Lecturer>
            {
                Success = true,
                Message = "Thêm giảng viên thành công",
                Data = lecturer
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Lecturer lecturer)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy giảng viên có ID = {id}");

            existing.EmployeeCode = lecturer.EmployeeCode;
            existing.FullName = lecturer.FullName;
            existing.DateOfBirth = lecturer.DateOfBirth;
            existing.Gender = lecturer.Gender;
            existing.Email = lecturer.Email;
            existing.PhoneNumber = lecturer.PhoneNumber;
            existing.Department = lecturer.Department;
            existing.Position = lecturer.Position;
            existing.Status = lecturer.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật giảng viên thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy giảng viên có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa giảng viên thành công");
        }
    }
}
