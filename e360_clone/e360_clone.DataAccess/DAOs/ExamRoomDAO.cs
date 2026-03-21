using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class ExamRoomDAO : BaseDAO<ExamRoom>
    {
        public ExamRoomDAO(AppDbContext context) : base(context)
        {
        }
    }
}
