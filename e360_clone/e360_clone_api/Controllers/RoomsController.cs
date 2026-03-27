using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class RoomsController : BaseApiController
    {
        private readonly IExamRoomRepository _repository;

        public RoomsController(IExamRoomRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var term = request.SearchTerm?.Trim();
            Func<IQueryable<ExamRoom>, IOrderedQueryable<ExamRoom>> orderBy = q => q.OrderBy(x => x.RoomCode);

            IEnumerable<ExamRoom> data;
            int totalRecords;

            if (!string.IsNullOrEmpty(term))
            {
                data = await _repository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    x => x.RoomName.Contains(term) || x.RoomCode.Contains(term),
                    orderBy);
                totalRecords = await _repository.CountAsync(
                    x => x.RoomName.Contains(term) || x.RoomCode.Contains(term));
            }
            else
            {
                data = await _repository.GetPagedFilteredAsync(
                    request.PageNumber,
                    request.PageSize,
                    null,
                    orderBy);
                totalRecords = await _repository.CountAsync();
            }

            return Ok(new PagedResponse<ExamRoom>
            {
                Success = true,
                Message = "Lấy danh sách phòng thi thành công",
                Data = data.ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords
            });
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms(
            [FromQuery] DateTime examDate,
            [FromQuery] TimeSpan startTime,
            [FromQuery] TimeSpan endTime,
            [FromQuery] int? excludeExamId)
        {
            var available = await _repository.GetAvailableRoomsAsync(examDate, startTime, endTime, excludeExamId);
            return HandleResult(available, "Lấy danh sách phòng trống thành công");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy phòng thi có ID = {id}");

            return HandleResult(item, "Lấy thông tin phòng thi thành công");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExamRoom room)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<ExamRoom> { Success = false, Message = "Dữ liệu không hợp lệ" });

            room.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(room);

            return CreatedAtAction(nameof(GetById), new { id = room.Id }, new ApiResponse<ExamRoom>
            {
                Success = true,
                Message = "Thêm phòng thi thành công",
                Data = room
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExamRoom room)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy phòng thi có ID = {id}");

            existing.RoomCode = room.RoomCode;
            existing.RoomName = room.RoomName;
            existing.Building = room.Building;
            existing.Capacity = room.Capacity;
            existing.Floor = room.Floor;
            existing.HasComputer = room.HasComputer;
            existing.HasProjector = room.HasProjector;
            existing.Status = room.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật phòng thi thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy phòng thi có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa phòng thi thành công");
        }
    }
}
