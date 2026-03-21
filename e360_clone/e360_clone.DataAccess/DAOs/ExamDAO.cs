using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.DAOs
{
    public class ExamDAO : BaseDAO<Exam>
    {
        public ExamDAO(AppDbContext context) : base(context)
        {
        }
    }
}
