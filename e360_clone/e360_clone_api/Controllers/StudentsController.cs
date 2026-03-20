using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class StudentsController : BaseApiController
    {
        private readonly IRepository<Student> _studentRepository;

        public StudentsController(IRepository<Student> studentRepository)
        {
            _studentRepository = studentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var students = await _studentRepository.GetAllAsync();
            var query = students.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(s => s.FullName.Contains(request.SearchTerm) || s.StudentCode.Contains(request.SearchTerm));
            }

            var totalRecords = query.Count();
            var data = query
                .OrderBy(s => s.StudentCode)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Ok(new PagedResponse<Student>
            {
                Success = true,
                Message = "Lấy danh sách sinh viên thành công",
                Data = data,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return HandleNotFound($"Không tìm thấy sinh viên có ID = {id}");
            }

            return HandleResult(student, "Lấy thông tin sinh viên thành công");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<Student>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    Data = null
                });
            }

            student.CreatedAt = DateTime.UtcNow;
            await _studentRepository.AddAsync(student);

            return CreatedAtAction(nameof(GetById), new { id = student.Id }, new ApiResponse<Student>
            {
                Success = true,
                Message = "Thêm sinh viên thành công",
                Data = student
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Student student)
        {
            var existing = await _studentRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return HandleNotFound($"Không tìm thấy sinh viên có ID = {id}");
            }

            existing.FullName = student.FullName;
            existing.DateOfBirth = student.DateOfBirth;
            existing.Gender = student.Gender;
            existing.Email = student.Email;
            existing.PhoneNumber = student.PhoneNumber;
            existing.Address = student.Address;
            existing.ClassId = student.ClassId;
            existing.Status = student.Status;
            existing.UpdatedAt = DateTime.Now;

            await _studentRepository.UpdateAsync(existing);

            return HandleResult(existing, "Cập nhật sinh viên thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return HandleNotFound($"Không tìm thấy sinh viên có ID = {id}");
            }

            await _studentRepository.DeleteAsync(student);

            return HandleResult(true, "Xóa sinh viên thành công");
        }
    }
}
