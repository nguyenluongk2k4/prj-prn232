using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    public class ExamDAO : BaseDAO<Exam>
    {
        public ExamDAO(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<Exam> Items, int TotalRecords)> GetPagedFilteredWithMetaAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var term = searchTerm?.Trim();
            var from = fromDate?.Date;
            var to = toDate?.Date;

            IQueryable<Exam> query = _context.Exams;

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(x => x.ExamName.Contains(term) || x.ExamCode.Contains(term));
            }

            if (from.HasValue)
            {
                query = query.Where(x => x.ExamDate.Date >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(x => x.ExamDate.Date <= to.Value);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.ExamDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalRecords);
        }

        public async Task<List<StudentExamScheduleDto>> GetStudentScheduleAsync(
            int studentId,
            string? email,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (studentId <= 0 && string.IsNullOrWhiteSpace(email))
            {
                return new List<StudentExamScheduleDto>();
            }

            var from = (fromDate ?? DateTime.UtcNow.Date.AddDays(-7)).Date;
            var to = (toDate ?? DateTime.UtcNow.Date.AddDays(7)).Date;
            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtcExclusive = DateTime.SpecifyKind(to.AddDays(1), DateTimeKind.Utc);

            if (studentId <= 0 && !string.IsNullOrWhiteSpace(email))
            {
                studentId = await _context.Accounts
                    .Where(a => a.Email == email)
                    .Select(a => a.StudentId)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .FirstOrDefaultAsync();
            }

            if (studentId <= 0)
            {
                return new List<StudentExamScheduleDto>();
            }

            var exams = await _context.ExamRoomAllocations
                .Where(alloc => alloc.StudentId == studentId)
                .Join(
                    _context.Exams,
                    alloc => alloc.ExamId,
                    e => e.Id,
                    (alloc, e) => new StudentExamScheduleDto
                    {
                        Id = e.Id,
                        ExamCode = e.ExamCode,
                        ExamName = e.ExamName,
                        ExamType = e.ExamType,
                        SubjectId = e.SubjectId,
                        ClassId = e.ClassId,
                        RoomId = alloc.RoomId,
                        ExamDate = e.ExamDate,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                        Duration = e.Duration,
                        AcademicYear = e.AcademicYear,
                        Semester = e.Semester,
                        Status = e.Status,
                        Notes = e.Notes,
                        SeatNumber = alloc.SeatNumber
                    })
                .Where(e => e.ExamDate >= fromUtc && e.ExamDate < toUtcExclusive)
                .Distinct()
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.StartTime)
                .ToListAsync();

            return exams;
        }
    }
}
