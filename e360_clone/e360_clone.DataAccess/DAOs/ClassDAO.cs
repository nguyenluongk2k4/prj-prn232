using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    public class ClassDAO : BaseDAO<Class>
    {
        public ClassDAO(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<Class> Items, int TotalRecords)> GetPagedFilteredWithMetaAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            string? majorCode,
            int? cohort,
            int? subjectId,
            string? status)
        {
            var term = searchTerm?.Trim();
            var major = majorCode?.Trim().ToUpperInvariant();
            var statusFilter = status?.Trim();

            IQueryable<Class> query = _context.Classes;

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(x => x.ClassName.Contains(term) || x.ClassCode.Contains(term));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(x => x.Status == statusFilter);
            }

            if (cohort.HasValue)
            {
                query = query.Where(x => x.Cohort == cohort.Value);
            }

            if (!string.IsNullOrEmpty(major))
            {
                var majorEntity = await _context.Majors.FirstOrDefaultAsync(m => m.MajorCode == major);
                if (majorEntity == null)
                {
                    return (new List<Class>(), 0);
                }
                query = query.Where(x => x.MajorId == majorEntity.Id);
            }

            if (subjectId.HasValue)
            {
                var classIds = await _context.StudentSubjects
                    .Where(x => x.SubjectId == subjectId.Value && x.ClassId.HasValue)
                    .Select(x => x.ClassId!.Value)
                    .Distinct()
                    .ToListAsync();

                if (classIds.Count == 0)
                {
                    return (new List<Class>(), 0);
                }

                query = query.Where(x => classIds.Contains(x.Id));
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.ClassCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalRecords);
        }
    }
}
