using e360_clone.BusinessObjects;

namespace e360_clone.Repositories
{
    public interface IMajorRepository : IRepository<Major>
    {
        Task<Major?> GetByCodeAsync(string code);
    }
}
