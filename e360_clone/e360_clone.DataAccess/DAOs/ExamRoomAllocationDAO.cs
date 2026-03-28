using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    public class ExamRoomAllocationDAO : BaseDAO<ExamRoomAllocation>
    {
        public ExamRoomAllocationDAO(AppDbContext context) : base(context)
        {
        }

        public Task<Exam?> GetExamAsync(int examId)
        {
            return _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == examId);
        }

        public async Task<int> GetCurrentTermIdAsync()
        {
            return await _context.Terms
                .Where(t => t.IsCurrent)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ExamAllocationStudentDto>> GetStudentsForExamAsync(Exam exam)
        {
            var currentTermId = await GetCurrentTermIdAsync();

            var query = _context.StudentSubjects
                .Where(s => s.SubjectId == exam.SubjectId)
                .Where(s => s.Status == "Enrolled")
                .Where(s => s.ClassId.HasValue);

            if (exam.ClassId > 0)
            {
                query = query.Where(s => s.ClassId == exam.ClassId);
            }

            if (currentTermId > 0)
            {
                query = query.Where(s => s.TermId == currentTermId);
            }

            var items = await query
                .Join(_context.Students,
                    ss => ss.StudentId,
                    st => st.Id,
                    (ss, st) => new { ss, st })
                .Join(_context.Classes,
                    temp => temp.st.ClassId,
                    c => c.Id,
                    (temp, c) => new ExamAllocationStudentDto
                    {
                        StudentId = temp.st.Id,
                        StudentCode = temp.st.StudentCode,
                        FullName = temp.st.FullName,
                        Email = temp.st.Email,
                        ClassId = c.Id,
                        ClassCode = c.ClassCode
                    })
                .ToListAsync();

            return items
                .GroupBy(x => x.StudentId)
                .Select(g => g.First())
                .OrderBy(x => x.StudentCode)
                .ToList();
        }

        public async Task<List<ExamAllocationItemDto>> GetAllocationsAsync(int examId)
        {
            return await _context.ExamRoomAllocations
                .Where(a => a.ExamId == examId)
                .Select(a => new ExamAllocationItemDto
                {
                    StudentId = a.StudentId,
                    RoomId = a.RoomId,
                    SeatNumber = a.SeatNumber
                })
                .ToListAsync();
        }

        public async Task<List<ExamAllocationRoomDto>> GetRoomsAsync()
        {
            return await _context.ExamRooms
                .OrderBy(r => r.RoomCode)
                .Select(r => new ExamAllocationRoomDto
                {
                    RoomId = r.Id,
                    RoomCode = r.RoomCode,
                    RoomName = r.RoomName,
                    Capacity = r.Capacity
                })
                .ToListAsync();
        }

        public async Task ReplaceAllocationsAsync(int examId, List<ExamRoomAllocation> allocations)
        {
            var existing = await _context.ExamRoomAllocations
                .Where(a => a.ExamId == examId)
                .ToListAsync();

            if (existing.Count > 0)
            {
                _context.ExamRoomAllocations.RemoveRange(existing);
            }

            if (allocations.Count > 0)
            {
                await _context.ExamRoomAllocations.AddRangeAsync(allocations);
            }
        }
    }
}
