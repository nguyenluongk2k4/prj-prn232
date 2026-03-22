using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class TeachingAssignmentDAO : BaseDAO<TeachingAssignment>
    {
        public TeachingAssignmentDAO(AppDbContext context) : base(context)
        {
        }
    }
}
