using e360_clone.BusinessObjects;
using e360_clone.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Repositories
{
    public class MajorRepository : Repository<Major>, IMajorRepository
    {
        public MajorRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Major?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.MajorCode == code);
        }
    }
}
