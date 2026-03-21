using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class LecturersController : BaseApiController
    {
        private readonly ILecturerRepository _repository;

        public LecturersController(ILecturerRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var term = request.SearchTerm?.Trim();
            Func<IQueryable<Lecturer>, IOrderedQueryable<Lecturer>> orderBy = q => q.OrderBy(x => x.EmployeeCode);

            IEnumerable<Lecturer> data;
            int totalRecords;

            if (!string.IsNullOrEmpty(term))
            {
                data = await _repository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    x => x.FullName.Contains(term) || x.EmployeeCode.Contains(term),
                    orderBy);
                totalRecords = await _repository.CountAsync(
                    x => x.FullName.Contains(term) || x.EmployeeCode.Contains(term));
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

            return Ok(new PagedResponse<Lecturer>
            {
                Success = true,
                Message = "Lấy danh sách giảng viên thành công",
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
