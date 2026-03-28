using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using e360_clone.Repositories;
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
        public async Task<IActionResult> GetByStudent([FromQuery] int studentId)
        {
            if (studentId <= 0)
            {
                return BadRequest(new ApiResponse<List<StudentSubject>>
                {
                    Success = false,
                    Message = "StudentId khong hop le",
                    Data = new List<StudentSubject>()
                });
            }

            var data = await _repository.GetByStudentIdAsync(studentId);

            return Ok(new ApiResponse<List<StudentSubject>>
            {
                Success = true,
                Message = "Lay danh sach mon hoc theo sinh vien thanh cong",
                Data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentToSubject([FromBody] AddStudentToSubjectRequestDto request)
        {
            if (request == null || request.SubjectId <= 0 || request.ClassId <= 0 || request.StudentIds == null || request.StudentIds.Count == 0)
            {
                return BadRequest(new ApiResponse<StudentSubject>
                {
                    Success = false,
                    Message = "Du lieu khong hop le",
                    Data = null
                });
            }

            var addedCount = await _repository.AddStudentsToSubjectAsync(request.SubjectId, request.ClassId, request.StudentIds);
            if (addedCount == 0)
            {
                var reason = await _repository.DiagnoseAddStudentsAsync(request.SubjectId, request.ClassId, request.StudentIds);
                return BadRequest(new ApiResponse<StudentSubject>
                {
                    Success = false,
                    Message = $"Khong the them sinh vien vao mon. Ly do: {reason}",
                    Data = null
                });
            }

            return Ok(new ApiResponse<StudentSubject>
            {
                Success = true,
                Message = $"Da them {addedCount} sinh vien vao mon",
                Data = null
            });
        }
    }
}
