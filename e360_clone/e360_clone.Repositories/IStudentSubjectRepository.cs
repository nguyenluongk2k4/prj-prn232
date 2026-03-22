using e360_clone.BusinessObjects;

namespace e360_clone.Repositories
{
    public interface IStudentSubjectRepository : IRepository<StudentSubject>
    {
        Task<List<int>> GetClassIdsBySubjectAsync(int subjectId);
    }
}
