using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class LecturerDAO : BaseDAO<Lecturer>
    {
        public LecturerDAO(AppDbContext context) : base(context)
        {
        }
    }
}
