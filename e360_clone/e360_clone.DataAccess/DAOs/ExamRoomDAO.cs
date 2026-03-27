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
            var conflicts = _context.Exams
                .Where(x => x.ExamDate.Date == examDate.Date)
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
                .Where(r => !conflictRoomIds.Contains(r.Id))
                .ToListAsync();
        }
    }
}
