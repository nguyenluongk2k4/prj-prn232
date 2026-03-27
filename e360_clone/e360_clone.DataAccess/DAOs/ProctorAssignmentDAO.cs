using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess.DAOs
{
    public class ProctorAssignmentDAO : BaseDAO<ProctorAssignment>
    {
        public ProctorAssignmentDAO(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> HasScheduleConflictAsync(int lecturerId, int examId, int? excludeId)
        {
            var targetExam = await _context.Exams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == examId);

            if (targetExam == null)
            {
                return false;
            }

            var query = from pa in _context.ProctorAssignments
                        join e in _context.Exams on pa.ExamId equals e.Id
                        where pa.LecturerId == lecturerId
                        select new { Assignment = pa, Exam = e };

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Assignment.Id != excludeId.Value);
            }

            query = query.Where(x => x.Exam.ExamDate.Date == targetExam.ExamDate.Date);

            return await query.AnyAsync(x =>
                x.Exam.StartTime < targetExam.EndTime &&
                targetExam.StartTime < x.Exam.EndTime);
        }
    }
}
