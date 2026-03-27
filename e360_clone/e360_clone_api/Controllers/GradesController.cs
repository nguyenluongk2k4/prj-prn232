using System.Globalization;
using ClosedXML.Excel;
using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using e360_clone.BusinessObjects.Enums;
using e360_clone.DataAccess;
using e360_clone_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Controllers
{
    public class GradesController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<GradesController> _logger;

        public GradesController(AppDbContext context, IEmailService emailService, ILogger<GradesController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var query = _context.GradeEntries.AsQueryable();
            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderBy(x => x.StudentId)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return Ok(new PagedResponse<GradeEntry>
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
            var item = await _context.GradeEntries.FirstOrDefaultAsync(g => g.Id == id);
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

            var grades = await _context.GradeEntries
                .Where(g => g.StudentId == studentId && g.Status == "Published")
                .ToListAsync();

            var gradeItems = grades
                .GroupBy(g => new { g.SubjectId, g.AcademicYear, g.Semester })
                .Select(group =>
                {
                    subjectMap.TryGetValue(group.Key.SubjectId, out var subject);
                    var enrollment = enrollments.FirstOrDefault(e =>
                        e.SubjectId == group.Key.SubjectId &&
                        e.AcademicYear == group.Key.AcademicYear &&
                        e.Semester == group.Key.Semester);

                    Class? cls = null;
                    if (enrollment?.ClassId.HasValue == true)
                    {
                        classMap.TryGetValue(enrollment.ClassId.Value, out cls);
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
                        SubjectId = group.Key.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassId = enrollment?.ClassId,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        AcademicYear = group.Key.AcademicYear,
                        Semester = group.Key.Semester,
                        TermLabel = BusinessObjects.Utilities.TermMapper.GetTermLabel(group.Key.AcademicYear, group.Key.Semester),
                        Components = components
                    };
                })
                .ToList();

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
                        SubjectId = e.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassId = e.ClassId,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        AcademicYear = e.AcademicYear,
                        Semester = e.Semester,
                        TermLabel = BusinessObjects.Utilities.TermMapper.GetTermLabel(e.AcademicYear, e.Semester),
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
                .GroupBy(x => new { x.SubjectId, x.AcademicYear, x.Semester })
                .Select(g => g.First())
                .OrderByDescending(x => x.AcademicYear)
                .ThenByDescending(x => x.Semester)
                .ToList();

            return Ok(new ApiResponse<List<StudentGradeItemDto>>
            {
                Success = true,
                Message = "Lấy điểm theo sinh viên thành công",
                Data = result
            });
        }

        [HttpGet("transcript")]
        public async Task<IActionResult> GetTranscript([FromQuery] int studentId)
        {
            if (studentId <= 0)
            {
                return BadRequest(new ApiResponse<List<TranscriptTermDto>>
                {
                    Success = false,
                    Message = "StudentId không hợp lệ",
                    Data = new List<TranscriptTermDto>()
                });
            }

            var enrollments = await _context.StudentSubjects
                .Where(s => s.StudentId == studentId)
                .ToListAsync();

            if (enrollments.Count == 0)
            {
                return Ok(new ApiResponse<List<TranscriptTermDto>>
                {
                    Success = true,
                    Message = "Chưa có môn học cho sinh viên này",
                    Data = new List<TranscriptTermDto>()
                });
            }

            var subjectIds = enrollments.Select(e => e.SubjectId).Distinct().ToList();
            var classIds = enrollments.Where(e => e.ClassId.HasValue).Select(e => e.ClassId!.Value).Distinct().ToList();

            var subjects = await _context.Subjects.Where(s => subjectIds.Contains(s.Id)).ToListAsync();
            var classes = await _context.Classes.Where(c => classIds.Contains(c.Id)).ToListAsync();
            var subjectMap = subjects.ToDictionary(s => s.Id, s => s);
            var classMap = classes.ToDictionary(c => c.Id, c => c);

            var grades = await _context.GradeEntries
                .Where(g => g.StudentId == studentId && g.Status == "Published")
                .ToListAsync();

            var gradeComponentsBySubject = grades
                .GroupBy(g => (g.SubjectId, g.AcademicYear, g.Semester))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new TranscriptComponentDto
                    {
                        ScoreType = x.ScoreType,
                        ScoreTypeText = ResolveScoreTypeText(x.ScoreType),
                        Score = x.Score,
                        LetterGrade = x.LetterGrade,
                        Weight = ResolveScoreTypeWeight(x.ScoreType)
                    }).OrderBy(x => x.ScoreType).ToList());

            var defaultComponents = BuildDefaultTranscriptComponents();

            var terms = enrollments
                .GroupBy(e => new { e.AcademicYear, e.Semester })
                .Select(term =>
                {
                    var courses = term.Select(e =>
                    {
                        subjectMap.TryGetValue(e.SubjectId, out var subject);
                        Class? cls = null;
                        if (e.ClassId.HasValue)
                        {
                            classMap.TryGetValue(e.ClassId.Value, out cls);
                        }

                        var key = (e.SubjectId, e.AcademicYear, e.Semester);
                        var components = gradeComponentsBySubject.TryGetValue(key, out var comps)
                            ? comps
                            : defaultComponents.Select(c => new TranscriptComponentDto
                            {
                                ScoreType = c.ScoreType,
                                ScoreTypeText = c.ScoreTypeText,
                                Score = null,
                                LetterGrade = string.Empty,
                                Weight = c.Weight
                            }).ToList();

                        return new TranscriptCourseDto
                        {
                            SubjectId = e.SubjectId,
                            SubjectCode = subject?.SubjectCode ?? string.Empty,
                            SubjectName = subject?.SubjectName ?? string.Empty,
                            ClassCode = cls?.ClassCode ?? string.Empty,
                            Components = components
                        };
                    })
                    .OrderBy(c => c.SubjectCode)
                    .ToList();

                    return new TranscriptTermDto
                    {
                        AcademicYear = term.Key.AcademicYear,
                        Semester = term.Key.Semester,
                        TermLabel = BusinessObjects.Utilities.TermMapper.GetTermLabel(term.Key.AcademicYear, term.Key.Semester),
                        Courses = courses
                    };
                })
                .OrderByDescending(t => t.AcademicYear)
                .ThenByDescending(t => t.Semester)
                .ToList();

            return Ok(new ApiResponse<List<TranscriptTermDto>>
            {
                Success = true,
                Message = "Lấy transcript thành công",
                Data = terms
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GradeEntry grade)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<GradeEntry> { Success = false, Message = "Dữ liệu không hợp lệ" });

            _context.GradeEntries.Add(grade);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = grade.Id }, new ApiResponse<GradeEntry>
            {
                Success = true,
                Message = "Thêm điểm thành công",
                Data = grade
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GradeEntry grade)
        {
            var existing = await _context.GradeEntries.FirstOrDefaultAsync(g => g.Id == id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy điểm có ID = {id}");

            existing.StudentId = grade.StudentId;
            existing.SubjectId = grade.SubjectId;
            existing.AcademicYear = grade.AcademicYear;
            existing.Semester = grade.Semester;
            existing.Score = grade.Score;
            existing.ScoreType = grade.ScoreType;
            existing.LetterGrade = grade.LetterGrade;
            existing.Notes = grade.Notes;
            existing.EnteredBy = grade.EnteredBy;
            existing.EnteredAt = grade.EnteredAt;
            existing.ApprovedBy = grade.ApprovedBy;
            existing.ApprovedAt = grade.ApprovedAt;
            existing.Status = grade.Status;

            await _context.SaveChangesAsync();
            return HandleResult(existing, "Cập nhật điểm thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.GradeEntries.FirstOrDefaultAsync(g => g.Id == id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy điểm có ID = {id}");

            _context.GradeEntries.Remove(item);
            await _context.SaveChangesAsync();
            return HandleResult(true, "Xóa điểm thành công");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportGrades(
            [FromForm] int subjectId,
            [FromForm] string? academicYear,
            [FromForm] int? semester,
            [FromForm] int? enteredBy,
            [FromForm] string? scoreType,
            [FromForm] string? role,
            IFormFile? file)
        {
            if (subjectId <= 0 || string.IsNullOrWhiteSpace(academicYear) || !semester.HasValue)
            {
                return BadRequest(new ApiResponse<GradeImportResultDto>
                {
                    Success = false,
                    Message = "Thiếu môn hoặc kỳ học",
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
            var enrollments = await _context.StudentSubjects
                .Where(s => s.SubjectId == subjectId &&
                            s.AcademicYear == academicYear &&
                            s.Semester == semester.Value &&
                            studentIds.Contains(s.StudentId))
                .ToListAsync();
            var enrolledStudentIds = enrollments.Select(s => s.StudentId).ToHashSet();

            var existing = await _context.GradeEntries
                .Where(g => g.SubjectId == subjectId &&
                            g.AcademicYear == academicYear &&
                            g.Semester == semester.Value &&
                            studentIds.Contains(g.StudentId))
                .ToListAsync();
            var gradeMap = existing.ToDictionary(g => (g.StudentId, g.ScoreType), g => g);

            var now = DateTime.UtcNow;
            var normalizedScoreType = string.IsNullOrWhiteSpace(scoreType) ? string.Empty : scoreType.Trim();
            var normalizedRole = role?.Trim() ?? string.Empty;

            if (!IsRoleAllowed(normalizedRole, normalizedScoreType))
            {
                return BadRequest(new ApiResponse<GradeImportResultDto>
                {
                    Success = false,
                    Message = "Loại điểm không hợp lệ cho quyền hiện tại.",
                    Data = result
                });
            }

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

                if (!enrolledStudentIds.Contains(student.Id))
                {
                    result.Skipped++;
                    result.Errors.Add($"Sinh viên {row.StudentCode} không học môn này trong kỳ đã chọn.");
                    continue;
                }

                if (!TryParseScore(row.ScoreText, out var score))
                {
                    result.Skipped++;
                    result.Errors.Add($"Điểm không hợp lệ cho {row.StudentCode}: {row.ScoreText}");
                    continue;
                }

                var key = (student.Id, normalizedScoreType);
                var autoPublish = IsAutoPublishScoreType(normalizedScoreType);
                if (gradeMap.TryGetValue(key, out var grade))
                {
                    grade.Score = score;
                    grade.ScoreType = normalizedScoreType;
                    grade.LetterGrade = row.LetterGrade ?? string.Empty;
                    grade.Notes = row.Notes ?? string.Empty;
                    grade.EnteredBy = enteredBy;
                    grade.EnteredAt = now;
                    grade.Status = autoPublish ? "Published" : "Submitted";
                    if (autoPublish)
                    {
                        grade.ApprovedBy = enteredBy;
                        grade.ApprovedAt = now;
                    }
                    result.Updated++;
                }
                else
                {
                    _context.GradeEntries.Add(new GradeEntry
                    {
                        StudentId = student.Id,
                        SubjectId = subjectId,
                        AcademicYear = academicYear!.Trim(),
                        Semester = semester.Value,
                        Score = score,
                        ScoreType = normalizedScoreType,
                        LetterGrade = row.LetterGrade ?? string.Empty,
                        Notes = row.Notes ?? string.Empty,
                        EnteredBy = enteredBy,
                        EnteredAt = now,
                        Status = autoPublish ? "Published" : "Submitted",
                        ApprovedBy = autoPublish ? enteredBy : null,
                        ApprovedAt = autoPublish ? now : null
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
        public async Task<IActionResult> PublishGrades([FromQuery] int subjectId, [FromQuery] string? academicYear, [FromQuery] int? semester, [FromQuery] int? approvedBy)
        {
            if (subjectId <= 0 || string.IsNullOrWhiteSpace(academicYear) || !semester.HasValue)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Thiếu môn hoặc kỳ học"
                });
            }

            var grades = await _context.GradeEntries
                .Where(g => g.SubjectId == subjectId &&
                            g.AcademicYear == academicYear &&
                            g.Semester == semester.Value)
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

            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);

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
<p>Điểm đã được công bố cho kỳ {academicYear} - kỳ {semester}.</p>
<p><strong>Môn:</strong> {subjectName}</p>
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

        [HttpPost("manual")]
        public async Task<IActionResult> EnterManual([FromBody] ManualGradeRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            if (request.SubjectId <= 0 || string.IsNullOrWhiteSpace(request.AcademicYear) || request.Semester <= 0 || string.IsNullOrWhiteSpace(request.ScoreType))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Thiếu môn/kỳ hoặc loại điểm"
                });
            }

            if (!IsRoleAllowed(request.Role ?? string.Empty, request.ScoreType))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Loại điểm không hợp lệ cho quyền hiện tại"
                });
            }

            if (string.IsNullOrWhiteSpace(request.StudentCode))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Vui lòng nhập mã sinh viên"
                });
            }

            if (!request.Score.HasValue)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Vui lòng nhập điểm"
                });
            }

            var student = await _context.Students.FirstOrDefaultAsync(s =>
                s.StudentCode == request.StudentCode.Trim());
            if (student == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Không tìm thấy sinh viên"
                });
            }

            var enrollment = await _context.StudentSubjects.FirstOrDefaultAsync(s =>
                s.StudentId == student.Id &&
                s.SubjectId == request.SubjectId &&
                s.AcademicYear == request.AcademicYear &&
                s.Semester == request.Semester);
            if (enrollment == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Sinh viên không học môn này trong kỳ đã chọn"
                });
            }

            var normalizedScoreType = request.ScoreType.Trim();
            var existing = await _context.GradeEntries.FirstOrDefaultAsync(g =>
                g.SubjectId == request.SubjectId &&
                g.AcademicYear == request.AcademicYear &&
                g.Semester == request.Semester &&
                g.StudentId == student.Id &&
                g.ScoreType == normalizedScoreType);

            var now = DateTime.UtcNow;
            if (existing != null)
            {
                existing.Score = request.Score;
                existing.LetterGrade = request.LetterGrade ?? string.Empty;
                existing.Notes = request.Notes ?? string.Empty;
                existing.EnteredBy = request.EnteredBy;
                existing.EnteredAt = now;
                var autoPublish = IsAutoPublishScoreType(normalizedScoreType);
                existing.Status = autoPublish ? "Published" : "Submitted";
                if (autoPublish)
                {
                    existing.ApprovedBy = request.EnteredBy;
                    existing.ApprovedAt = now;
                }
            }
            else
            {
                var autoPublish = IsAutoPublishScoreType(normalizedScoreType);
                _context.GradeEntries.Add(new GradeEntry
                {
                    StudentId = student.Id,
                    SubjectId = request.SubjectId,
                    AcademicYear = request.AcademicYear,
                    Semester = request.Semester,
                    ScoreType = normalizedScoreType,
                    Score = request.Score,
                    LetterGrade = request.LetterGrade ?? string.Empty,
                    Notes = request.Notes ?? string.Empty,
                    EnteredBy = request.EnteredBy,
                    EnteredAt = now,
                    Status = autoPublish ? "Published" : "Submitted",
                    ApprovedBy = autoPublish ? request.EnteredBy : null,
                    ApprovedAt = autoPublish ? now : null
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Đã lưu điểm"
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
                "ProgressTest" => "Kiểm tra quá trình",
                "Practical" => "Thực hành",
                "Final" => "Cuối kỳ",
                "PracticalRetake" => "Thi lại thực hành",
                "FinalRetake" => "Thi lại cuối kỳ",
                "Lab" => "Lab",
                "Assignment" => "Bài tập",
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
                "PracticalRetake" => 0.2m,
                "FinalRetake" => 0.4m,
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

        private static bool IsRoleAllowed(string role, string scoreType)
        {
            if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(scoreType))
            {
                return false;
            }

            if (string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            {
                return scoreType.Equals("ProgressTest", StringComparison.OrdinalIgnoreCase)
                       || scoreType.Equals("Assignment", StringComparison.OrdinalIgnoreCase)
                       || scoreType.Equals("Lab", StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                return scoreType.Equals("Final", StringComparison.OrdinalIgnoreCase)
                       || scoreType.Equals("Practical", StringComparison.OrdinalIgnoreCase)
                       || scoreType.Equals("FinalRetake", StringComparison.OrdinalIgnoreCase)
                       || scoreType.Equals("PracticalRetake", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool IsAutoPublishScoreType(string scoreType)
        {
            if (string.IsNullOrWhiteSpace(scoreType))
            {
                return false;
            }

            return scoreType.Equals("ProgressTest", StringComparison.OrdinalIgnoreCase)
                   || scoreType.Equals("Assignment", StringComparison.OrdinalIgnoreCase)
                   || scoreType.Equals("Lab", StringComparison.OrdinalIgnoreCase);
        }


        public sealed class ManualGradeRequest
        {
            public int SubjectId { get; set; }
            public string AcademicYear { get; set; } = string.Empty;
            public int Semester { get; set; }
            public string ScoreType { get; set; } = string.Empty;
            public string StudentCode { get; set; } = string.Empty;
            public decimal? Score { get; set; }
            public string? LetterGrade { get; set; }
            public string? Notes { get; set; }
            public int? EnteredBy { get; set; }
            public string? Role { get; set; }
        }

        private static List<TranscriptComponentDto> BuildDefaultTranscriptComponents()
        {
            var defaultTypes = new[]
            {
                "ProgressTest",
                "Practical",
                "Final",
                "Lab",
                "Assignment"
            };

            return defaultTypes.Select(type => new TranscriptComponentDto
            {
                ScoreType = type,
                ScoreTypeText = ResolveScoreTypeText(type),
                Score = null,
                LetterGrade = string.Empty,
                Weight = ResolveScoreTypeWeight(type)
            }).ToList();
        }
    }
}
