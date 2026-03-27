using e360_clone.BusinessObjects.DTOs;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardRepository _repository;

        public DashboardController(IDashboardRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("summary")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetSummary([FromQuery] int days = 14)
        {
            if (days <= 0)
            {
                days = 14;
            }

            var fromDate = DateTime.Today;
            var toDate = fromDate.AddDays(days);

            AdminDashboardSummaryDto summary = await _repository.GetAdminSummaryAsync(fromDate, toDate);
            return HandleResult(summary, "Lấy thống kê dashboard thành công");
        }
    }
}
