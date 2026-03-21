using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class StudentDAO : BaseDAO<Student>
    {
        public StudentDAO(AppDbContext context) : base(context)
        {
        }
    }
}
