using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class SubjectDAO : BaseDAO<Subject>
    {
        public SubjectDAO(AppDbContext context) : base(context)
        {
        }
    }
}
