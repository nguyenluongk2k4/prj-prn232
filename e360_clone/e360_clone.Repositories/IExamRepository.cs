using e360_clone.BusinessObjects;

namespace e360_clone.Repositories
{
    public interface IExamRepository : IRepository<Exam>
    {
        Task<List<int>> GetConflictingRoomIdsAsync(DateTime examDate, TimeSpan startTime, TimeSpan endTime, int? excludeExamId = null);
    }
}
