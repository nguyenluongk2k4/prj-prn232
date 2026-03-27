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
        public async Task<IActionResult> GetAll(
            [FromQuery] PagedRequest request,
            [FromQuery] string? majorCode,
            [FromQuery] int? cohort,
            [FromQuery] int? subjectId,
            [FromQuery] string? status)
        {
            var result = await _repository.GetPagedFilteredWithMetaAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                majorCode,
                cohort,
                subjectId,
                status);

            return Ok(new PagedResponse<Class>
            {
                Success = true,
                Message = "Láº¥y danh sÃ¡ch lá»›p há»c thÃ nh cÃ´ng",
                Data = result.Items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = result.TotalRecords
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

