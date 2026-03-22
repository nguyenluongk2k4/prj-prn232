using e360_clone.BusinessObjects;
using e360_clone.Repositories;
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
                Message = "Láº¥y danh sÃ¡ch mÃ´n há»c thÃ nh cÃ´ng",
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
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y mÃ´n há»c cÃ³ ID = {id}");

            return HandleResult(item, "Láº¥y thÃ´ng tin mÃ´n há»c thÃ nh cÃ´ng");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Subject subject)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Subject> { Success = false, Message = "Dá»¯ liá»‡u khÃ´ng há»£p lá»‡" });

            subject.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(subject);

            return CreatedAtAction(nameof(GetById), new { id = subject.Id }, new ApiResponse<Subject>
            {
                Success = true,
                Message = "ThÃªm mÃ´n há»c thÃ nh cÃ´ng",
                Data = subject
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Subject subject)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y mÃ´n há»c cÃ³ ID = {id}");

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
            return HandleResult(existing, "Cáº­p nháº­t mÃ´n há»c thÃ nh cÃ´ng");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y mÃ´n há»c cÃ³ ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "XÃ³a mÃ´n há»c thÃ nh cÃ´ng");
        }
    }
}

