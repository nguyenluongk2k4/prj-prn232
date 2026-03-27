using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult HandleResult<T>(T? data, string? message = null, bool success = true)
        {
            if (success)
            {
                return Ok(new ApiResponse<T>
                {
                    Success = true,
                    Message = message ?? "Thành công",
                    Data = data
                });
            }

            return BadRequest(new ApiResponse<T>
            {
                Success = false,
                Message = message ?? "Có lỗi xảy ra",
                Data = default
            });
        }

        protected IActionResult HandleNotFound(string? message = null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = message ?? "Không tìm thấy",
                Data = null
            });
        }

        protected IActionResult HandleError(string? message = null)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = message ?? "Lỗi máy chủ nội bộ",
                Data = null
            });
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class PagedResponse<T> : ApiResponse<List<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
    }

    public class PagedRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
