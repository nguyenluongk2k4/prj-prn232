using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class StudentsController : BaseApiController
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IAccountRepository _accountRepository;

        public StudentsController(IStudentRepository studentRepository, IAccountRepository accountRepository)
        {
            _studentRepository = studentRepository;
            _accountRepository = accountRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, [FromQuery] int? classId)
        {
            var term = request.SearchTerm?.Trim();
            Func<IQueryable<Student>, IOrderedQueryable<Student>> orderBy = q => q.OrderBy(s => s.StudentCode);

            IEnumerable<Student> data;
            int totalRecords;

            if (!string.IsNullOrEmpty(term) && classId.HasValue)
            {
                data = await _studentRepository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    s => s.ClassId == classId.Value && (s.FullName.Contains(term) || s.StudentCode.Contains(term)),
                    orderBy);
                totalRecords = await _studentRepository.CountAsync(
                    s => s.ClassId == classId.Value && (s.FullName.Contains(term) || s.StudentCode.Contains(term)));
            }
            else if (classId.HasValue)
            {
                data = await _studentRepository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    s => s.ClassId == classId.Value,
                    orderBy);
                totalRecords = await _studentRepository.CountAsync(
                    s => s.ClassId == classId.Value);
            }
            else if (!string.IsNullOrEmpty(term))
            {
                data = await _studentRepository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    s => s.FullName.Contains(term) || s.StudentCode.Contains(term),
                    orderBy);
                totalRecords = await _studentRepository.CountAsync(
                    s => s.FullName.Contains(term) || s.StudentCode.Contains(term));
            }
            else
            {
                data = await _studentRepository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    null,
                    orderBy);
                totalRecords = await _studentRepository.CountAsync();
            }

            var students = data.ToList();
            var accounts = await _accountRepository.GetByStudentIdsAsync(students.Select(s => s.Id));
            var avatarMap = accounts
                .Where(a => a.StudentId.HasValue)
                .GroupBy(a => a.StudentId!.Value)
                .ToDictionary(g => g.Key, g => g.First().AvatarUrl);

            var dtoList = students.Select(s => new StudentDto
            {
                Id = s.Id,
                StudentCode = s.StudentCode,
                FullName = s.FullName,
                DateOfBirth = s.DateOfBirth,
                Gender = s.Gender,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Address = s.Address,
                ClassId = s.ClassId,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                AvatarUrl = avatarMap.TryGetValue(s.Id, out var avatar) ? avatar : null
            }).ToList();

            return Ok(new PagedResponse<StudentDto>
            {
                Success = true,
                Message = "Lấy danh sách sinh viên thành công",
                Data = dtoList,
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

            var account = (await _accountRepository.GetByStudentIdsAsync(new[] { id })).FirstOrDefault();
            var dto = new StudentDto
            {
                Id = student.Id,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Address = student.Address,
                ClassId = student.ClassId,
                Status = student.Status,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt,
                AvatarUrl = account?.AvatarUrl
            };

            return HandleResult(dto, "Lấy thông tin sinh viên thành công");
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
            existing.UpdatedAt = DateTime.UtcNow;

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

