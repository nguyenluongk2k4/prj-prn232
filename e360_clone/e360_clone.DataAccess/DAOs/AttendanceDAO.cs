using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class AttendanceDAO : BaseDAO<Attendance>
    {
        public AttendanceDAO(AppDbContext context) : base(context)
        {
        }
    }
}
