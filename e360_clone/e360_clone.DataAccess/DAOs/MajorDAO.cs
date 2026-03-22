using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class MajorDAO : BaseDAO<Major>
    {
        public MajorDAO(AppDbContext context) : base(context)
        {
        }
    }
}
