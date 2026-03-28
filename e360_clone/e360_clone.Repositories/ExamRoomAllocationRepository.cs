using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;

namespace e360_clone.Repositories
{
    public class ExamRoomAllocationRepository : IExamRoomAllocationRepository
    {
        private readonly AppDbContext _context;
        private readonly ExamRoomAllocationDAO _dao;

        public ExamRoomAllocationRepository(AppDbContext context, ExamRoomAllocationDAO dao)
        {
            _context = context;
            _dao = dao;
        }

        public async Task<ExamAllocationSummaryDto?> GetSummaryAsync(int examId)
        {
            var exam = await _dao.GetExamAsync(examId);
            if (exam == null)
            {
                return null;
            }

            var students = await _dao.GetStudentsForExamAsync(exam);
            var allocations = await _dao.GetAllocationsAsync(examId);
            var rooms = await _dao.GetRoomsAsync();

            return new ExamAllocationSummaryDto
            {
                ExamId = exam.Id,
                SubjectId = exam.SubjectId,
                ClassId = exam.ClassId,
                StudentCount = students.Count,
                AllocatedCount = allocations.Count,
                Students = students,
                Rooms = rooms,
                Allocations = allocations
            };
        }

        public async Task<ExamAllocationSummaryDto?> AutoAllocateAsync(int examId, List<int> roomIds)
        {
            var exam = await _dao.GetExamAsync(examId);
            if (exam == null)
            {
                return null;
            }

            var students = await _dao.GetStudentsForExamAsync(exam);
            var rooms = await _dao.GetRoomsAsync();
            var selectedRooms = rooms.Where(r => roomIds.Contains(r.RoomId)).ToList();

            if (selectedRooms.Count == 0)
            {
                throw new InvalidOperationException("Vui lòng chọn ít nhất một phòng thi.");
            }

            var totalCapacity = selectedRooms.Sum(r => r.Capacity);
            if (totalCapacity < students.Count)
            {
                throw new InvalidOperationException($"Tổng sức chứa ({totalCapacity}) không đủ cho {students.Count} sinh viên.");
            }

            var allocations = new List<ExamRoomAllocation>();
            var seatCounters = selectedRooms.ToDictionary(r => r.RoomId, _ => 1);
            var remaining = selectedRooms.ToDictionary(r => r.RoomId, r => r.Capacity);
            var roomIndex = 0;

            foreach (var student in students)
            {
                var assigned = false;
                for (var attempt = 0; attempt < selectedRooms.Count; attempt++)
                {
                    var idx = (roomIndex + attempt) % selectedRooms.Count;
                    var room = selectedRooms[idx];
                    if (remaining[room.RoomId] <= 0)
                    {
                        continue;
                    }

                    allocations.Add(new ExamRoomAllocation
                    {
                        ExamId = exam.Id,
                        StudentId = student.StudentId,
                        RoomId = room.RoomId,
                        SeatNumber = seatCounters[room.RoomId],
                        CreatedAt = DateTime.UtcNow
                    });

                    seatCounters[room.RoomId] += 1;
                    remaining[room.RoomId] -= 1;
                    roomIndex = (idx + 1) % selectedRooms.Count;
                    assigned = true;
                    break;
                }

                if (!assigned)
                {
                    throw new InvalidOperationException("Không đủ sức chứa để xếp tất cả sinh viên.");
                }
            }

            await _dao.ReplaceAllocationsAsync(examId, allocations);
            await _dao.SaveChangesAsync();

            var allocationDtos = allocations.Select(a => new ExamAllocationItemDto
            {
                StudentId = a.StudentId,
                RoomId = a.RoomId,
                SeatNumber = a.SeatNumber
            }).ToList();

            return new ExamAllocationSummaryDto
            {
                ExamId = exam.Id,
                SubjectId = exam.SubjectId,
                ClassId = exam.ClassId,
                StudentCount = students.Count,
                AllocatedCount = allocationDtos.Count,
                Students = students,
                Rooms = rooms,
                Allocations = allocationDtos
            };
        }

        public async Task<ExamAllocationSummaryDto?> SaveAllocationsAsync(int examId, List<ExamAllocationItemDto> allocations)
        {
            var exam = await _dao.GetExamAsync(examId);
            if (exam == null)
            {
                return null;
            }

            var students = await _dao.GetStudentsForExamAsync(exam);
            var studentIds = new HashSet<int>(students.Select(s => s.StudentId));

            var rooms = await _dao.GetRoomsAsync();
            var roomIds = new HashSet<int>(rooms.Select(r => r.RoomId));

            var normalized = allocations
                .Where(a => studentIds.Contains(a.StudentId))
                .Where(a => roomIds.Contains(a.RoomId))
                .Select(a => new ExamRoomAllocation
                {
                    ExamId = examId,
                    StudentId = a.StudentId,
                    RoomId = a.RoomId,
                    SeatNumber = a.SeatNumber > 0 ? a.SeatNumber : 0,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            await _dao.ReplaceAllocationsAsync(examId, normalized);
            await _dao.SaveChangesAsync();

            var allocationDtos = normalized.Select(a => new ExamAllocationItemDto
            {
                StudentId = a.StudentId,
                RoomId = a.RoomId,
                SeatNumber = a.SeatNumber
            }).ToList();

            return new ExamAllocationSummaryDto
            {
                ExamId = exam.Id,
                SubjectId = exam.SubjectId,
                ClassId = exam.ClassId,
                StudentCount = students.Count,
                AllocatedCount = allocationDtos.Count,
                Students = students,
                Rooms = rooms,
                Allocations = allocationDtos
            };
        }
    }
}
