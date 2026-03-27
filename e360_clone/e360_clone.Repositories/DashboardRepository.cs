using e360_clone.BusinessObjects.DTOs;
using e360_clone.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardSummaryDto> GetAdminSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            var fromDateOnly = fromDate.Date;
            var toDateOnly = toDate.Date;

            var totalStudents = await _context.Students.CountAsync();
            var totalLecturers = await _context.Lecturers.CountAsync();
            var totalClasses = await _context.Classes.CountAsync();
            var totalSubjects = await _context.Subjects.CountAsync();
            var totalExamRooms = await _context.ExamRooms.CountAsync();

            var upcomingExamCount = await _context.Exams
                .Where(e => e.ExamDate.Date >= fromDateOnly && e.ExamDate.Date <= toDateOnly)
                .CountAsync();

            var studentsByMajor = await (from s in _context.Students
                                         join c in _context.Classes on s.ClassId equals c.Id
                                         join m in _context.Majors on c.MajorId equals m.Id
                                         group s by new { m.MajorCode, m.MajorName } into g
                                         orderby g.Count() descending
                                         select new DashboardChartPointDto
                                         {
                                             Label = $"{g.Key.MajorCode} - {g.Key.MajorName}",
                                             Value = g.Count()
                                         })
                .ToListAsync();

            var classesByCohort = await _context.Classes
                .GroupBy(c => c.CohortYear > 0
                    ? c.CohortYear.ToString()
                    : (c.Cohort > 0 ? $"K{c.Cohort}" : "Unknown"))
                .Select(g => new DashboardChartPointDto
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            var examCounts = await _context.Exams
                .Where(e => e.ExamDate.Date >= fromDateOnly && e.ExamDate.Date <= toDateOnly)
                .GroupBy(e => e.ExamDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var examsByDate = Enumerable.Range(0, (toDateOnly - fromDateOnly).Days + 1)
                .Select(offset =>
                {
                    var date = fromDateOnly.AddDays(offset);
                    var count = examCounts.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
                    return new DashboardChartPointDto
                    {
                        Label = date.ToString("dd/MM"),
                        Value = count
                    };
                })
                .ToList();

            var upcomingExams = await (from e in _context.Exams
                                       join s in _context.Subjects on e.SubjectId equals s.Id
                                       join c in _context.Classes on e.ClassId equals c.Id
                                       join r in _context.ExamRooms on e.RoomId equals r.Id into roomJoin
                                       from r in roomJoin.DefaultIfEmpty()
                                       where e.ExamDate.Date >= fromDateOnly && e.ExamDate.Date <= toDateOnly
                                       orderby e.ExamDate, e.StartTime
                                       select new AdminDashboardExamItemDto
                                       {
                                           ExamId = e.Id,
                                           ExamDate = e.ExamDate,
                                           StartTime = e.StartTime,
                                           EndTime = e.EndTime,
                                           SubjectCode = s.SubjectCode,
                                           SubjectName = s.SubjectName,
                                           ClassCode = c.ClassCode,
                                           RoomCode = r != null ? r.RoomCode : string.Empty,
                                           ExamType = e.ExamType
                                       })
                .Take(6)
                .ToListAsync();

            return new AdminDashboardSummaryDto
            {
                TotalStudents = totalStudents,
                TotalLecturers = totalLecturers,
                TotalClasses = totalClasses,
                TotalSubjects = totalSubjects,
                TotalExamRooms = totalExamRooms,
                UpcomingExamCount = upcomingExamCount,
                StudentsByMajor = studentsByMajor,
                ClassesByCohort = classesByCohort,
                ExamsByDate = examsByDate,
                UpcomingExams = upcomingExams
            };
        }
    }
}
