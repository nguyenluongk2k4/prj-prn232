using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class ProctorAssignmentDAO : BaseDAO<ProctorAssignment>
    {
        public ProctorAssignmentDAO(AppDbContext context) : base(context)
        {
        }
    }
}
