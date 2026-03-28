using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;

namespace e360_clone.Repositories
{
    public interface IExamRepository : IRepository<Exam>
    {
        Task<List<int>> GetConflictingRoomIdsAsync(DateTime examDate, TimeSpan startTime, TimeSpan endTime, int? excludeExamId = null);
        Task<(List<Exam> Items, int TotalRecords)> GetPagedFilteredWithMetaAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            DateTime? fromDate,
            DateTime? toDate);
        Task<List<StudentExamScheduleDto>> GetStudentScheduleAsync(int studentId, string? email, DateTime? fromDate, DateTime? toDate);
        Task AddRangeAsync(IEnumerable<Exam> exams);
        Task<List<ClassLookupDto>> GetClassesForSubjectAsync(int subjectId);
        Task<string?> GetSubjectCodeAsync(int subjectId);
        Task<int> GetStudentCountForSubjectAsync(int subjectId, int? classId);
    }
}
