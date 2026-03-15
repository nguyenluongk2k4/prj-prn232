# Repository & Utilities Documentation

## Overview

Tài liệu hướng dẫn sử dụng Repository pattern và các utility classes đã tạo.

---

## Repository Pattern

### Basic Usage

```csharp
// Inject repository vào controller
public class StudentsController : BaseApiController
{
    private readonly IRepository<Student> _studentRepository;
    
    public StudentsController(IRepository<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }
}
```

### CRUD Operations

```csharp
// GET ALL
var students = await _studentRepository.GetAllAsync();

// GET BY ID
var student = await _studentRepository.GetByIdAsync(1);

// CREATE
var newStudent = new Student
{
    StudentCode = "SV001",
    FullName = "Nguyen Van A",
    // ... other properties
};
await _studentRepository.AddAsync(newStudent);

// UPDATE
student.FullName = "Nguyen Van B";
await _studentRepository.UpdateAsync(student);

// DELETE
await _studentRepository.DeleteAsync(student);
```

### Query Operations

```csharp
// FIND BY PREDICATE
var activeStudents = await _studentRepository.FindAsync(s => s.Status == "Active");

// FIRST OR DEFAULT
var student = await _studentRepository.FirstOrDefaultAsync(s => s.StudentCode == "SV001");

// CHECK EXISTS
var exists = await _studentRepository.AnyAsync(s => s.Email == "test@example.com");

// COUNT
var total = await _studentRepository.CountAsync();
var activeCount = await _studentRepository.CountAsync(s => s.Status == "Active");
```

### Paged Queries

```csharp
// SIMPLE PAGED
var page1 = await _studentRepository.GetPagedAsync(1, 10);

// PAGED WITH FILTER AND SORT
var result = await _studentRepository.GetPagedFilteredAsync(
    pageNumber: 1,
    pageSize: 10,
    filter: s => s.Status == "Active",
    orderBy: q => q.OrderBy(s => s.StudentCode)
);

// PAGED RESULT WITH METADATA
var pagedResult = await _studentRepository.GetPagedResultAsync(
    pageNumber: 1,
    pageSize: 10,
    filter: s => s.FullName.Contains("Nguyen"),
    orderBy: q => q.OrderBy(s => s.CreatedAt)
);

// Access metadata
Console.WriteLine($"Total: {pagedResult.TotalRecords}, Pages: {pagedResult.TotalPages}");
Console.WriteLine($"Has Next: {pagedResult.HasNext}");
```

### Search Operations

```csharp
// SEARCH BY MULTIPLE FIELDS
var results = await _studentRepository.SearchAsync(
    searchTerm: "Nguyen",
    searchProperties: nameof(Student.FullName), nameof(Student.Email)
);
```

### Bulk Operations

```csharp
// BULK INSERT
var students = new List<Student> { ... };
await _studentRepository.AddRangeAsync(students);

// BULK UPDATE
foreach (var student in students)
{
    student.Status = "Inactive";
}
await _studentRepository.UpdateRangeAsync(students);

// BULK DELETE
await _studentRepository.DeleteRangeAsync(students);
```

---

## Unit of Work Pattern

### Usage

```csharp
public class StudentsController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    
    public StudentsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IActionResult> CreateStudentWithClass(Student student, Class classEntity)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var studentRepo = _unitOfWork.Repository<Student>();
            var classRepo = _unitOfWork.Repository<Class>();
            
            await classRepo.AddAsync(classEntity);
            await studentRepo.AddAsync(student);
            
            await _unitOfWork.CommitTransactionAsync();
            
            return HandleResult(student, "Success");
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return HandleError("Transaction failed");
        }
    }
}
```

---

## Utilities

### GradeUtils

```csharp
// Calculate letter grade from score
var letterGrade = GradeUtils.CalculateLetterGrade(8.5m); // Returns "A"

// Check if passed
var isPass = GradeUtils.IsPass("C"); // Returns true

// Calculate GPA from letter grade
var gpa = GradeUtils.CalculateGPA("B"); // Returns 3.0

// Validate score
var isValid = GradeUtils.IsValidScore(11.0m); // Returns false
```

### DateTimeUtils

```csharp
// Check working hours
var isWorkingHours = DateTimeUtils.IsWithinWorkingHours(
    new DateTime(2024, 1, 1, 9, 0, 0),
    new DateTime(2024, 1, 1, 17, 0, 0)
); // Returns true

// Check time overlap
var isOverlap = DateTimeUtils.IsTimeOverlap(
    new DateTime(2024, 1, 1, 8, 0, 0),
    new DateTime(2024, 1, 1, 10, 0, 0),
    new DateTime(2024, 1, 1, 9, 0, 0),
    new DateTime(2024, 1, 1, 11, 0, 0)
); // Returns true

// Calculate duration in minutes
var duration = DateTimeUtils.CalculateDuration(
    new TimeSpan(8, 0, 0),
    new TimeSpan(10, 30, 0)
); // Returns 150

// Format Vietnamese date
var formatted = DateTimeUtils.FormatVietnamese(DateTime.Now); // "15/03/2026"
var formattedDateTime = DateTimeUtils.FormatVietnameseDateTime(DateTime.Now); // "15/03/2026 14:30"
```

### StringUtils

```csharp
// Normalize email
var normalized = StringUtils.NormalizeEmail("  TEST@Example.com "); // "test@example.com"

// Normalize code
var code = StringUtils.NormalizeCode("  sv001 "); // "SV001"

// Validate email
var isValid = StringUtils.IsValidEmail("test@example.com"); // true

// Validate Vietnamese phone
var isValidPhone = StringUtils.IsValidVietnamesePhone("0901234567"); // true

// Generate random code
var randomCode = StringUtils.GenerateRandomCode("SV", 6); // "SV1A2B3C"
```

### PaginationUtils

```csharp
// Calculate total pages
var totalPages = PaginationUtils.CalculateTotalPages(100, 10); // Returns 10

// Calculate skip count
var skip = PaginationUtils.CalculateSkip(3, 10); // Returns 20

// Validate page number
var validPage = PaginationUtils.ValidatePageNumber(15, 10); // Returns 10
```

---

## Helpers

### ApiResponse Helper

```csharp
// Success response
var response = ApiResponse<Student>.SuccessResponse(student, "Lấy thông tin thành công");

// Error response
var errorResponse = ApiResponse<Student>.ErrorResponse(
    "Có lỗi xảy ra",
    new List<string> { "Chi tiết lỗi 1", "Chi tiết lỗi 2" }
);
```

### PagedApiResponse Helper

```csharp
var pagedResponse = PagedApiResponse<Student>.Create(
    items: students,
    pageNumber: 1,
    pageSize: 10,
    totalRecords: 100,
    message: "Lấy danh sách thành công"
);
```

### QueryHelpers

```csharp
// Date range filter
var dateFilter = QueryHelpers.DateRangeFilter<Student>(
    s => s.CreatedAt,
    startDate: DateTime.Now.AddMonths(-1),
    endDate: DateTime.Now
);

// Active filter
var activeFilter = QueryHelpers.ActiveFilter<Student>(s => s.Status);

// Multi-field search filter
var searchFilter = QueryHelpers.MultiFieldSearchFilter<Student>(
    searchTerm: "Nguyen",
    propertySelectors: 
        s => s.FullName,
        s => s.Email,
        s => s.StudentCode
);
```

---

## Example: Complete Controller

```csharp
using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.Helpers;
using e360_clone.BusinessObjects.Utilities;
using e360_clone.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class StudentsController : BaseApiController
    {
        private readonly IRepository<Student> _studentRepository;
        
        public StudentsController(IRepository<Student> studentRepository)
        {
            _studentRepository = studentRepository;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            // Build filter
            Expression<Func<Student, bool>>? filter = null;
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                filter = s => s.FullName.Contains(request.SearchTerm) 
                           || s.StudentCode.Contains(request.SearchTerm);
            }
            
            // Get paged result
            var pagedResult = await _studentRepository.GetPagedResultAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                filter: filter,
                orderBy: q => q.OrderBy(s => s.StudentCode)
            );
            
            // Return formatted response
            return Ok(PagedApiResponse<Student>.Create(
                items: pagedResult.Items,
                pageNumber: pagedResult.PageNumber,
                pageSize: pagedResult.PageSize,
                totalRecords: pagedResult.TotalRecords,
                message: "Lấy danh sách sinh viên thành công"
            ));
        }
        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Student student)
        {
            // Validate
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Student>.ErrorResponse("Dữ liệu không hợp lệ"));
            }
            
            // Check duplicate
            var exists = await _studentRepository.AnyAsync(s => s.StudentCode == student.StudentCode);
            if (exists)
            {
                return BadRequest(ApiResponse<Student>.ErrorResponse("Mã sinh viên đã tồn tại"));
            }
            
            // Set defaults
            student.CreatedAt = DateTime.Now;
            student.Status = "Active";
            
            // Save
            await _studentRepository.AddAsync(student);
            
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, 
                ApiResponse<Student>.SuccessResponse(student, "Thêm sinh viên thành công"));
        }
    }
}
```

---

## Best Practices

1. **Luôn dùng async/await** cho database operations
2. **Validate input** trước khi gọi repository
3. **Use PagedResult** cho danh sách lớn
4. **Transaction** khi có nhiều database operations
5. **Check exists** trước khi create để tránh duplicate
6. **Use Expression** cho dynamic queries
7. **Return ApiResponse** wrapper cho consistency
