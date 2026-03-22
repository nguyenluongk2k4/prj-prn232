using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace e360_clone.Controllers
{
    public class ClassesController : BaseApiController
    {
        private readonly IClassRepository _repository;
        private readonly IMajorRepository _majorRepository;

        public ClassesController(IClassRepository repository, IMajorRepository majorRepository)
        {
            _repository = repository;
            _majorRepository = majorRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PagedRequest request,
            [FromQuery] string? majorCode,
            [FromQuery] int? cohort,
            [FromQuery] int? subjectId,
            [FromQuery] string? status)
        {
            var term = request.SearchTerm?.Trim();
            var major = majorCode?.Trim().ToUpperInvariant();
            var statusFilter = status?.Trim();
            Func<IQueryable<Class>, IOrderedQueryable<Class>> orderBy = q => q.OrderBy(x => x.ClassCode);

            IEnumerable<Class> data;
            int totalRecords;

            var hasFilter = !string.IsNullOrEmpty(term)
                            || !string.IsNullOrEmpty(major)
                            || cohort.HasValue
                            || subjectId.HasValue
                            || !string.IsNullOrEmpty(statusFilter);
            if (hasFilter)
            {
                int? majorId = null;
                if (!string.IsNullOrEmpty(major))
                {
                    var majorEntity = await _majorRepository.GetByCodeAsync(major);
                    if (majorEntity == null)
                    {
                        return Ok(new PagedResponse<Class>
                        {
                            Success = true,
                            Message = "KhÃ´ng cÃ³ dá»¯ liá»‡u",
                            Data = new List<Class>(),
                            PageNumber = request.PageNumber,
                            PageSize = request.PageSize,
                            TotalRecords = 0
                        });
                    }

                    majorId = majorEntity.Id;
                }

                List<int>? classIds = null;
                if (subjectId.HasValue)
                {
                    var ids = await HttpContext.RequestServices
                        .GetRequiredService<IStudentSubjectRepository>()
                        .GetClassIdsBySubjectAsync(subjectId.Value);
                    classIds = ids;
                    if (classIds.Count == 0)
                    {
                        return Ok(new PagedResponse<Class>
                        {
                            Success = true,
                            Message = "KhÃ´ng cÃ³ dá»¯ liá»‡u",
                            Data = new List<Class>(),
                            PageNumber = request.PageNumber,
                            PageSize = request.PageSize,
                            TotalRecords = 0
                        });
                    }
                }

                System.Linq.Expressions.Expression<Func<Class, bool>> filter = x =>
                    (string.IsNullOrEmpty(term) || x.ClassName.Contains(term) || x.ClassCode.Contains(term)) &&
                    (!majorId.HasValue || x.MajorId == majorId.Value) &&
                    (!cohort.HasValue || x.Cohort == cohort.Value) &&
                    (classIds == null || classIds.Contains(x.Id)) &&
                    (string.IsNullOrEmpty(statusFilter) || x.Status == statusFilter);

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

            return Ok(new PagedResponse<Class>
            {
                Success = true,
                Message = "Láº¥y danh sÃ¡ch lá»›p há»c thÃ nh cÃ´ng",
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
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y lá»›p há»c cÃ³ ID = {id}");

            return HandleResult(item, "Láº¥y thÃ´ng tin lá»›p há»c thÃ nh cÃ´ng");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Class cls)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Class> { Success = false, Message = "Dá»¯ liá»‡u khÃ´ng há»£p lá»‡" });

            cls.CreatedAt = DateTime.UtcNow;
            NormalizeCohort(cls);
            await _repository.AddAsync(cls);

            return CreatedAtAction(nameof(GetById), new { id = cls.Id }, new ApiResponse<Class>
            {
                Success = true,
                Message = "ThÃªm lá»›p há»c thÃ nh cÃ´ng",
                Data = cls
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Class cls)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y lá»›p há»c cÃ³ ID = {id}");

            existing.ClassCode = cls.ClassCode;
            existing.ClassName = cls.ClassName;
            existing.MajorId = cls.MajorId;
            existing.CourseId = cls.CourseId;
            existing.AcademicYear = cls.AcademicYear;
            existing.Cohort = cls.Cohort;
            existing.CohortYear = cls.CohortYear;
            existing.Semester = cls.Semester;
            existing.StudentCount = cls.StudentCount;
            existing.Status = cls.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            NormalizeCohort(existing);
            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cáº­p nháº­t lá»›p há»c thÃ nh cÃ´ng");
        }

        private static void NormalizeCohort(Class cls)
        {
            if (cls.Cohort > 0 && cls.CohortYear > 0)
                return;

            if (string.IsNullOrWhiteSpace(cls.ClassCode))
                return;

            var match = System.Text.RegularExpressions.Regex.Match(cls.ClassCode, @"\d{2}");
            if (!match.Success)
                return;

            if (!int.TryParse(match.Value, out var cohort))
                return;

            cls.Cohort = cohort;
            cls.CohortYear = 2000 + cohort;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"KhÃ´ng tÃ¬m tháº¥y lá»›p há»c cÃ³ ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "XÃ³a lá»›p há»c thÃ nh cÃ´ng");
        }
    }
}

