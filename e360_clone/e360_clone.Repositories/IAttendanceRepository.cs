using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;

namespace e360_clone.Repositories
{
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<List<AttendanceRosterItemDto>?> GetRosterAsync(int examId);
        Task<List<StudentAttendanceItemDto>> GetStudentAttendanceAsync(int studentId, DateTime? date);
        Task<List<AttendanceReportItemDto>> GetReportAsync(DateTime? fromDate, DateTime? toDate, int? subjectId, int? classId);
    }
}
