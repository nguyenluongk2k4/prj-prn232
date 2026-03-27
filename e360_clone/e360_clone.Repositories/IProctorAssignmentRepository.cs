using e360_clone.BusinessObjects;

namespace e360_clone.Repositories
{
    public interface IProctorAssignmentRepository : IRepository<ProctorAssignment>
    {
        Task<bool> HasScheduleConflictAsync(int lecturerId, int examId, int? excludeId);
    }
}
