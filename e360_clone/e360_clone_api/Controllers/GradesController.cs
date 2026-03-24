using System.Globalization;
using ClosedXML.Excel;
using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using e360_clone.BusinessObjects.Enums;
using e360_clone.DataAccess;
using e360_clone.Repositories;
using e360_clone_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Controllers
{
    public class GradesController : BaseApiController
    {
        private readonly IGradeRepository _repository;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<GradesController> _logger;

        public GradesController(IGradeRepository repository, AppDbContext context, IEmailService emailService, ILogger<GradesController> logger)
        {
            _repository = repository;
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var data = await _repository.GetPagedFilteredAsync(
                request.PageNumber,
                request.PageSize,
                null,
                q => q.OrderBy(x => x.StudentId));
            var totalRecords = await _repository.CountAsync();

            return Ok(new PagedResponse<Grade>
            {
                Success = true,
                Message = "Lấy danh sách điểm thành công",
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
                return HandleNotFound($"Không tìm thấy điểm có ID = {id}");

            return HandleResult(item, "Lấy thông tin điểm thành công");
        }

        [HttpGet("student")]
        public async Task<IActionResult> GetByStudent([FromQuery] int studentId)
        {
            if (studentId <= 0)
            {
                return BadRequest(new ApiResponse<List<StudentGradeItemDto>>
                {
                    Success = false,
                    Message = "StudentId không hợp lệ",
                    Data = new List<StudentGradeItemDto>()
                });
            }

            var enrollments = await _context.StudentSubjects
                .Where(s => s.StudentId == studentId)
                .ToListAsync();

            if (enrollments.Count == 0)
            {
                return Ok(new ApiResponse<List<StudentGradeItemDto>>
                {
                    Success = true,
                    Message = "Chưa có môn học cho sinh viên này",
                    Data = new List<StudentGradeItemDto>()
                });
            }

            var subjectIds = enrollments.Select(e => e.SubjectId).Distinct().ToList();
            var classIds = enrollments.Where(e => e.ClassId.HasValue).Select(e => e.ClassId!.Value).Distinct().ToList();

            var subjects = await _context.Subjects.Where(s => subjectIds.Contains(s.Id)).ToListAsync();
            var classes = await _context.Classes.Where(c => classIds.Contains(c.Id)).ToListAsync();
            var subjectMap = subjects.ToDictionary(s => s.Id, s => s);
            var classMap = classes.ToDictionary(c => c.Id, c => c);

            var grades = await _context.Grades
                .Where(g => g.StudentId == studentId && g.Status == "Published")
                .ToListAsync();

            var examIds = grades.Select(g => g.ExamId).Distinct().ToList();
            var exams = examIds.Count > 0
                ? await _context.Exams.Where(e => examIds.Contains(e.Id)).ToListAsync()
                : new List<Exam>();
            var examMap = exams.ToDictionary(e => e.Id, e => e);

            var gradeItems = grades
                .GroupBy(g => g.ExamId)
                .Select(group =>
                {
                    if (!examMap.TryGetValue(group.Key, out var exam))
                    {
                        return null;
                    }

                    subjectMap.TryGetValue(exam.SubjectId, out var subject);
                    Class? cls = null;
                    if (exam.ClassId > 0)
                    {
                        classMap.TryGetValue(exam.ClassId, out cls);
                    }

                    var components = group
                        .OrderBy(g => g.ScoreType)
                        .Select(g => new StudentGradeComponentDto
                        {
                            ScoreType = g.ScoreType,
                            ScoreTypeText = ResolveScoreTypeText(g.ScoreType),
                            Score = g.Score,
                            LetterGrade = g.LetterGrade,
                            Notes = g.Notes,
                            Weight = ResolveScoreTypeWeight(g.ScoreType)
                        })
                        .ToList();

                    return new StudentGradeItemDto
                    {
                        ExamId = exam.Id,
                        SubjectId = exam.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassId = exam.ClassId,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        ExamDate = exam.ExamDate,
                        StartTime = exam.StartTime,
                        EndTime = exam.EndTime,
                        Components = components
                    };
                })
                .Where(x => x != null)
                .ToList()!;

            var defaultComponents = BuildDefaultComponents();
            var subjectItems = enrollments
                .Select(e =>
                {
                    subjectMap.TryGetValue(e.SubjectId, out var subject);
                    Class? cls = null;
                    if (e.ClassId.HasValue)
                    {
                        classMap.TryGetValue(e.ClassId.Value, out cls);
                    }

                    return new StudentGradeItemDto
                    {
                        ExamId = 0,
                        SubjectId = e.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassId = e.ClassId,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        ExamDate = default,
                        StartTime = default,
                        EndTime = default,
                        Components = defaultComponents.Select(c => new StudentGradeComponentDto
                        {
                            ScoreType = c.ScoreType,
                            ScoreTypeText = c.ScoreTypeText,
                            Score = null,
                            LetterGrade = string.Empty,
                            Notes = string.Empty,
                            Weight = c.Weight
                        }).ToList()
                    };
                })
                .ToList();

            var result = gradeItems
                .Concat(subjectItems)
                .GroupBy(x => new { x.SubjectId, x.ExamId })
                .Select(g => g.First())
                .OrderByDescending(x => x.ExamDate == default ? DateTime.MinValue : x.ExamDate)
                .ToList();

            return Ok(new ApiResponse<List<StudentGradeItemDto>>
            {
                Success = true,
                Message = "Lấy điểm theo sinh viên thành công",
                Data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Grade grade)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Grade> { Success = false, Message = "Dữ liệu không hợp lệ" });

            await _repository.AddAsync(grade);

            return CreatedAtAction(nameof(GetById), new { id = grade.Id }, new ApiResponse<Grade>
            {
                Success = true,
                Message = "Thêm điểm thành công",
                Data = grade
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Grade grade)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy điểm có ID = {id}");

            existing.StudentId = grade.StudentId;
            existing.ExamId = grade.ExamId;
            existing.Score = grade.Score;
            existing.ScoreType = grade.ScoreType;
            existing.LetterGrade = grade.LetterGrade;
            existing.Notes = grade.Notes;
            existing.EnteredBy = grade.EnteredBy;
            existing.EnteredAt = grade.EnteredAt;
            existing.ApprovedBy = grade.ApprovedBy;
            existing.ApprovedAt = grade.ApprovedAt;
            existing.Status = grade.Status;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật điểm thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy điểm có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa điểm thành công");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportGrades(
            [FromQuery] int examId,
            [FromQuery] int? enteredBy,
            [FromQuery] string? scoreType,
            IFormFile? file)
        {
            if (examId <= 0)
            {
                return BadRequest(new ApiResponse<GradeImportResultDto>
                {
                    Success = false,
                    Message = "ExamId không hợp lệ",
                    Data = new GradeImportResultDto()
                });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<GradeImportResultDto>
                {
                    Success = false,
                    Message = "Chưa chọn file điểm",
                    Data = new GradeImportResultDto()
                });
            }

            var rows = await ParseImportRowsAsync(file);
            var result = new GradeImportResultDto
            {
                TotalRows = rows.Count
            };

            if (rows.Count == 0)
            {
                return Ok(new ApiResponse<GradeImportResultDto>
                {
                    Success = true,
                    Message = "File không có dữ liệu",
                    Data = result
                });
            }

            var studentCodes = rows
                .Select(r => r.StudentCode)
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var students = await _context.Students
                .Where(s => studentCodes.Contains(s.StudentCode))
                .ToListAsync();
            var studentMap = students.ToDictionary(s => s.StudentCode, s => s, StringComparer.OrdinalIgnoreCase);

            var studentIds = students.Select(s => s.Id).ToList();
            var existing = await _context.Grades
                .Where(g => g.ExamId == examId && studentIds.Contains(g.StudentId))
                .ToListAsync();
            var gradeMap = existing.ToDictionary(g => (g.StudentId, g.ScoreType), g => g);

            var now = DateTime.UtcNow;
            var normalizedScoreType = string.IsNullOrWhiteSpace(scoreType) ? "Final" : scoreType.Trim();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.StudentCode))
                {
                    result.Skipped++;
                    result.Errors.Add("Thiếu mã sinh viên.");
                    continue;
                }

                if (!studentMap.TryGetValue(row.StudentCode, out var student))
                {
                    result.Skipped++;
                    result.Errors.Add($"Không tìm thấy sinh viên: {row.StudentCode}");
                    continue;
                }

                if (!TryParseScore(row.ScoreText, out var score))
                {
                    result.Skipped++;
                    result.Errors.Add($"Điểm không hợp lệ cho {row.StudentCode}: {row.ScoreText}");
                    continue;
                }

                var key = (student.Id, normalizedScoreType);
                if (gradeMap.TryGetValue(key, out var grade))
                {
                    grade.Score = score;
                    grade.ScoreType = normalizedScoreType;
                    grade.LetterGrade = row.LetterGrade ?? string.Empty;
                    grade.Notes = row.Notes ?? string.Empty;
                    grade.EnteredBy = enteredBy;
                    grade.EnteredAt = now;
                    grade.Status = "Submitted";
                    result.Updated++;
                }
                else
                {
                    _context.Grades.Add(new Grade
                    {
                        StudentId = student.Id,
                        ExamId = examId,
                        Score = score,
                        ScoreType = normalizedScoreType,
                        LetterGrade = row.LetterGrade ?? string.Empty,
                        Notes = row.Notes ?? string.Empty,
                        EnteredBy = enteredBy,
                        EnteredAt = now,
                        Status = "Submitted"
                    });
                    result.Imported++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<GradeImportResultDto>
            {
                Success = true,
                Message = "Import điểm thành công",
                Data = result
            });
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishGrades([FromQuery] int examId, [FromQuery] int? approvedBy)
        {
            if (examId <= 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "ExamId không hợp lệ"
                });
            }

            var grades = await _context.Grades
                .Where(g => g.ExamId == examId)
                .ToListAsync();

            if (grades.Count == 0)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Không có điểm để công bố"
                });
            }

            var now = DateTime.UtcNow;
            foreach (var grade in grades)
            {
                grade.Status = "Published";
                grade.ApprovedBy = approvedBy;
                grade.ApprovedAt = now;
            }

            await _context.SaveChangesAsync();

            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId);
            var subject = exam != null
                ? await _context.Subjects.FirstOrDefaultAsync(s => s.Id == exam.SubjectId)
                : null;
            var classCode = exam != null && exam.ClassId > 0
                ? await _context.Classes.Where(c => c.Id == exam.ClassId).Select(c => c.ClassCode).FirstOrDefaultAsync()
                : string.Empty;

            var studentIds = grades.Select(g => g.StudentId).Distinct().ToList();
            var students = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();
            var studentMap = students.ToDictionary(s => s.Id, s => s);

            var sent = 0;
            foreach (var grade in grades)
            {
                if (!studentMap.TryGetValue(grade.StudentId, out var student))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(student.Email))
                {
                    continue;
                }

                var subjectName = subject != null
                    ? $"{subject.SubjectCode} - {subject.SubjectName}"
                    : "môn học";

                var body = $@"
<p>Chào {student.FullName},</p>
<p>Điểm thi đã được công bố.</p>
<p><strong>Môn:</strong> {subjectName}</p>
<p><strong>Lớp:</strong> {classCode}</p>
<p><strong>Điểm:</strong> {grade.Score?.ToString(CultureInfo.InvariantCulture)}</p>
<p><strong>Điểm chữ:</strong> {grade.LetterGrade}</p>
<p>Trân trọng.</p>";

                var ok = await _emailService.SendAsync(student.Email, "Công bố điểm thi", body);
                if (ok) sent++;
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = $"Đã công bố điểm. Email đã gửi: {sent}/{students.Count}"
            });
        }

        private sealed class ImportRow
        {
            public string StudentCode { get; set; } = string.Empty;
            public string ScoreText { get; set; } = string.Empty;
            public string? LetterGrade { get; set; }
            public string? Notes { get; set; }
        }

        private static bool TryParseScore(string? value, out decimal? score)
        {
            score = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ||
                decimal.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("vi-VN"), out result))
            {
                score = result;
                return true;
            }

            return false;
        }

        private static async Task<List<ImportRow>> ParseImportRowsAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return ext switch
            {
                ".csv" => await ParseCsvAsync(file),
                ".xlsx" => ParseExcel(file),
                _ => new List<ImportRow>()
            };
        }

        private static async Task<List<ImportRow>> ParseCsvAsync(IFormFile file)
        {
            var rows = new List<ImportRow>();
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            string? line;
            var isHeader = true;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var delimiter = line.Contains(';') ? ';' : ',';
                var parts = line.Split(delimiter);
                if (isHeader)
                {
                    isHeader = false;
                    if (parts.Any(p => p.Contains("Student", StringComparison.OrdinalIgnoreCase) ||
                                       p.Contains("Mã", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
                }

                rows.Add(new ImportRow
                {
                    StudentCode = parts.ElementAtOrDefault(0)?.Trim() ?? string.Empty,
                    ScoreText = parts.ElementAtOrDefault(1)?.Trim() ?? string.Empty,
                    LetterGrade = parts.ElementAtOrDefault(2)?.Trim(),
                    Notes = parts.ElementAtOrDefault(3)?.Trim()
                });
            }

            return rows;
        }

        private static List<ImportRow> ParseExcel(IFormFile file)
        {
            var rows = new List<ImportRow>();
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            foreach (var dataRow in worksheet.RowsUsed().Skip(1))
            {
                var studentCode = dataRow.Cell(1).GetString().Trim();
                var scoreText = dataRow.Cell(2).GetString().Trim();
                var letter = dataRow.Cell(3).GetString().Trim();
                var notes = dataRow.Cell(4).GetString().Trim();

                if (string.IsNullOrWhiteSpace(studentCode))
                {
                    continue;
                }

                rows.Add(new ImportRow
                {
                    StudentCode = studentCode,
                    ScoreText = scoreText,
                    LetterGrade = string.IsNullOrWhiteSpace(letter) ? null : letter,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
                });
            }

            return rows;
        }

        private static string ResolveScoreTypeText(string? scoreType)
        {
            if (string.IsNullOrWhiteSpace(scoreType))
            {
                return "Khác";
            }

            return scoreType.Trim() switch
            {
                "ProgressTest" => "Progress test",
                "Practical" => "Practical exam",
                "Final" => "Final exam",
                "Lab" => "Lab",
                "Assignment" => "Assignment",
                "Midterm" => GradeTypeHelper.GetText(GradeType.GiuaKy),
                "Other" => GradeTypeHelper.GetText(GradeType.Khac),
                "KiemTra15Phut" => GradeTypeHelper.GetText(GradeType.KiemTra15Phut),
                "KiemTra1Tiet" => GradeTypeHelper.GetText(GradeType.KiemTra1Tiet),
                "GiuaKy" => GradeTypeHelper.GetText(GradeType.GiuaKy),
                "CuoiKy" => GradeTypeHelper.GetText(GradeType.CuoiKy),
                "BaiTap" => GradeTypeHelper.GetText(GradeType.BaiTap),
                "DoAn" => GradeTypeHelper.GetText(GradeType.DoAn),
                "ChuyenCan" => GradeTypeHelper.GetText(GradeType.ChuyenCan),
                _ => scoreType.Trim()
            };
        }

        private static decimal ResolveScoreTypeWeight(string? scoreType)
        {
            if (string.IsNullOrWhiteSpace(scoreType))
            {
                return (decimal)GradeTypeHelper.GetWeight(GradeType.Khac);
            }

            return scoreType.Trim() switch
            {
                "ProgressTest" => 0.2m,
                "Practical" => 0.2m,
                "Final" => 0.4m,
                "Lab" => 0.1m,
                "Assignment" => 0.1m,
                "Midterm" => (decimal)GradeTypeHelper.GetWeight(GradeType.GiuaKy),
                "Other" => (decimal)GradeTypeHelper.GetWeight(GradeType.Khac),
                "KiemTra15Phut" => (decimal)GradeTypeHelper.GetWeight(GradeType.KiemTra15Phut),
                "KiemTra1Tiet" => (decimal)GradeTypeHelper.GetWeight(GradeType.KiemTra1Tiet),
                "GiuaKy" => (decimal)GradeTypeHelper.GetWeight(GradeType.GiuaKy),
                "CuoiKy" => (decimal)GradeTypeHelper.GetWeight(GradeType.CuoiKy),
                "BaiTap" => (decimal)GradeTypeHelper.GetWeight(GradeType.BaiTap),
                "DoAn" => (decimal)GradeTypeHelper.GetWeight(GradeType.DoAn),
                "ChuyenCan" => (decimal)GradeTypeHelper.GetWeight(GradeType.ChuyenCan),
                _ => (decimal)GradeTypeHelper.GetWeight(GradeType.Khac)
            };
        }

        private static List<StudentGradeComponentDto> BuildDefaultComponents()
        {
            var defaultTypes = new[]
            {
                "ProgressTest",
                "Practical",
                "Final",
                "Lab",
                "Assignment"
            };

            return defaultTypes.Select(type => new StudentGradeComponentDto
            {
                ScoreType = type,
                ScoreTypeText = ResolveScoreTypeText(type),
                Score = null,
                LetterGrade = string.Empty,
                Notes = string.Empty,
                Weight = ResolveScoreTypeWeight(type)
            }).ToList();
        }
    }
}
