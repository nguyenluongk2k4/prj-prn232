using e360_clone.BusinessObjects.DTOs;

namespace e360_clone.Repositories
{
    public interface IDashboardRepository
    {
        Task<AdminDashboardSummaryDto> GetAdminSummaryAsync(DateTime fromDate, DateTime toDate);
    }
}
