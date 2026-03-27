using e360_clone.BusinessObjects;

namespace e360_clone.Repositories
{
    public interface IExamRoomRepository : IRepository<ExamRoom>
    {
        Task<List<ExamRoom>> GetAvailableRoomsAsync(DateTime examDate, TimeSpan startTime, TimeSpan endTime, int? excludeExamId);
    }
}
