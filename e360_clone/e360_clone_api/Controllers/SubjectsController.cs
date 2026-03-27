using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class SubjectsController : BaseApiController
    {
        private readonly ISubjectRepository _repository;

        public SubjectsController(ISubjectRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Teacher,Student")]
        public async Task<IActionResult> GetAll(
            [FromQuery] PagedRequest request,
            [FromQuery] string? status,
            [FromQuery] string? subjectType,
            [FromQuery] string? department)
        {
            var term = request.SearchTerm?.Trim();
            var statusFilter = status?.Trim();
            var typeFilter = subjectType?.Trim();
            var departmentFilter = department?.Trim();
            Func<IQueryable<Subject>, IOrderedQueryable<Subject>> orderBy = q => q.OrderBy(x => x.SubjectCode);

            IEnumerable<Subject> data;
            int totalRecords;

            var hasFilter = !string.IsNullOrEmpty(term)
                            || !string.IsNullOrEmpty(statusFilter)
                            || !string.IsNullOrEmpty(typeFilter)
                            || !string.IsNullOrEmpty(departmentFilter);

            if (hasFilter)
            {
                System.Linq.Expressions.Expression<Func<Subject, bool>> filter = x =>
                    (string.IsNullOrEmpty(term) || x.SubjectName.Contains(term) || x.SubjectCode.Contains(term)) &&
                    (string.IsNullOrEmpty(statusFilter) || x.Status == statusFilter) &&
                    (string.IsNullOrEmpty(typeFilter) || x.SubjectType == typeFilter) &&
                    (string.IsNullOrEmpty(departmentFilter) || x.Department.Contains(departmentFilter));

                data = await _repository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    filter,
                    orderBy);
                totalRecords = await _repository.CountAsync(filter);
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

            return Ok(new PagedResponse<Subject>
            {
                Success = true,
                Message = "Lấy danh sách môn học thành công",
                Data = data.ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Teacher,Student")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy môn học có ID = {id}");

            return HandleResult(item, "Lấy thông tin môn học thành công");
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create([FromBody] Subject subject)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Subject> { Success = false, Message = "Dữ liệu không hợp lệ" });

            subject.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(subject);

            return CreatedAtAction(nameof(GetById), new { id = subject.Id }, new ApiResponse<Subject>
            {
                Success = true,
                Message = "Thêm môn học thành công",
                Data = subject
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Subject subject)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy môn học có ID = {id}");

            existing.SubjectCode = subject.SubjectCode;
            existing.SubjectName = subject.SubjectName;
            existing.Credits = subject.Credits;
            existing.TheoryHours = subject.TheoryHours;
            existing.PracticeHours = subject.PracticeHours;
            existing.Department = subject.Department;
            existing.SubjectType = subject.SubjectType;
            existing.Status = subject.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật môn học thành công");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy môn học có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa môn học thành công");
        }
    }
}
