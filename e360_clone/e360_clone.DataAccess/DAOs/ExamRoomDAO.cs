using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    public class ExamRoomDAO : BaseDAO<ExamRoom>
    {
        public ExamRoomDAO(AppDbContext context) : base(context)
        {
        }

        public async Task<List<ExamRoom>> GetAvailableRoomsAsync(DateTime examDate, TimeSpan startTime, TimeSpan endTime, int? excludeExamId)
        {
            var dateOnly = examDate.Date;
            var dayStart = DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
            var dayEnd = dayStart.AddDays(1);

            var conflicts = _context.Exams
                .Where(x => x.ExamDate >= dayStart && x.ExamDate < dayEnd)
                .Where(x => x.StartTime < endTime && x.EndTime > startTime);

            if (excludeExamId.HasValue)
            {
                conflicts = conflicts.Where(x => x.Id != excludeExamId.Value);
            }

            var conflictRoomIds = await conflicts
                .Select(x => x.RoomId)
                .Distinct()
                .ToListAsync();

            return await _context.ExamRooms
                .Where(r => r.Status == "Available" || string.IsNullOrWhiteSpace(r.Status))
                .Where(r => !conflictRoomIds.Contains(r.Id))
                .ToListAsync();
        }
    }
}
