using e360_clone.BusinessObjects;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    [Route("api/student-subjects")]
    public class StudentSubjectsController : BaseApiController
    {
        private readonly IStudentSubjectRepository _repository;

        public StudentSubjectsController(IStudentSubjectRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Teacher,Student,Parent")]
        public async Task<IActionResult> GetByStudent([FromQuery] int studentId)
        {
            if (studentId <= 0)
            {
                return BadRequest(new ApiResponse<List<StudentSubject>>
                {
                    Success = false,
                    Message = "StudentId không hợp lệ",
                    Data = new List<StudentSubject>()
                });
            }

            var data = await _repository.GetByStudentIdAsync(studentId);

            return Ok(new ApiResponse<List<StudentSubject>>
            {
                Success = true,
                Message = "Lấy danh sách môn học theo sinh viên thành công",
                Data = data
            });
        }
    }
}
