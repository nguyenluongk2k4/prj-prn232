using e360_clone.BusinessObjects;

namespace e360_clone.Repositories
{
    public interface IClassRepository : IRepository<Class>
    {
        Task<(List<Class> Items, int TotalRecords)> GetPagedFilteredWithMetaAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            string? majorCode,
            int? cohort,
            int? subjectId,
            string? status);
    }
}
