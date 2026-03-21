using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class ClassDAO : BaseDAO<Class>
    {
        public ClassDAO(AppDbContext context) : base(context)
        {
        }
    }
}
