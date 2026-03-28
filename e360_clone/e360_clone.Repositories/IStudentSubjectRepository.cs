using e360_clone.BusinessObjects;

namespace e360_clone.Repositories
{
    public interface IStudentSubjectRepository : IRepository<StudentSubject>
    {
        Task<List<int>> GetClassIdsBySubjectAsync(int subjectId);
        Task<List<StudentSubject>> GetByStudentIdAsync(int studentId);
        Task<int> AddStudentsToSubjectAsync(int subjectId, int classId, List<int> studentIds);
        Task<string> DiagnoseAddStudentsAsync(int subjectId, int classId, List<int> studentIds);
    }
}
