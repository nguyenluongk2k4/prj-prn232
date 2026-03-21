using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class GradeDAO : BaseDAO<Grade>
    {
        public GradeDAO(AppDbContext context) : base(context)
        {
        }
    }
}
