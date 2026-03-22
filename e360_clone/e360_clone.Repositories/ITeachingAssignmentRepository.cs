using e360_clone.BusinessObjects;

namespace e360_clone.Repositories
{
    public interface ITeachingAssignmentRepository : IRepository<TeachingAssignment>
    {
        Task<bool> ExistsForLecturerAndExamAsync(int lecturerId, Exam exam);
    }
}
