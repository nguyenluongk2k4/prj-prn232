using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class MajorsController : BaseApiController
    {
        private readonly IMajorRepository _repository;

        public MajorsController(IMajorRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var data = await _repository.GetPagedFilteredAsync(
                request.PageNumber,
                request.PageSize,
                null,
                q => q.OrderBy(x => x.MajorCode));
            var totalRecords = await _repository.CountAsync();

            return Ok(new PagedResponse<Major>
            {
                Success = true,
                Message = "Lấy danh sách ngành thành công",
                Data = data.ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy ngành có ID = {id}");

            return HandleResult(item, "Lấy thông tin ngành thành công");
        }
    }
}
