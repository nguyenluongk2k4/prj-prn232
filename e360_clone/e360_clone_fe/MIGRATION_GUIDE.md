# Migration Guide: JavaScript → Server-Side .NET Core MVC

## Overview

This document describes the migration from client-side JavaScript AJAX calls to server-side .NET Core MVC pattern where controllers call the backend API.

## What Changed

### Before (Client-Side JavaScript)
- JavaScript modules (`core/http.js`, `modules/students/api.js`, etc.) made AJAX calls directly to backend API
- Views were rendered dynamically using JavaScript DOM manipulation
- Required loading multiple JS files on each page
- Authentication handled via localStorage JWT tokens

### After (Server-Side MVC)
- .NET Controllers call backend API via `ApiService`
- Views are rendered server-side using Razor syntax
- Strongly-typed ViewModels with validation
- Authentication handled via ASP.NET Core Identity & Cookies

## Architecture

### New File Structure

```
e360_clone_fe/
├── Services/
│   └── ApiService.cs           # HTTP client for calling backend API
├── Controllers/
│   ├── BaseController.cs       # Base controller with common patterns
│   ├── AuthController.cs       # Authentication (unchanged)
│   └── StudentsController.cs   # Student management (refactored)
├── Models/
│   ├── ViewModels/
│   │   ├── StudentViewModel.cs
│   │   ├── CreateStudentViewModel.cs
│   │   ├── UpdateStudentViewModel.cs
│   │   └── PagedViewModel.cs
│   └── AuthViewModels.cs       # Existing auth models
├── Views/
│   ├── Students/
│   │   ├── Index.cshtml        # List view with Razor rendering
│   │   ├── Create.cshtml       # Create form
│   │   ├── Edit.cshtml         # Edit form
│   │   └── Details.cshtml      # Details view
│   └── Shared/
│       └── _ScriptsPartial.cshtml  # Updated scripts
└── wwwroot/js/
    ├── _archived/              # Old JS modules (kept for reference)
    ├── config.js               # Keep for config
    └── utils.js                # Keep for utilities
```

## Key Components

### 1. ApiService (`Services/ApiService.cs`)

Centralized HTTP client for calling backend API:

```csharp
public interface IApiService
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> DeleteAsync<T>(string endpoint);
    Task<ApiResponse<T>> UploadAsync<T>(string endpoint, IFormFile file, ...);
}
```

**Features:**
- Automatic JWT token handling from user claims
- 401 Unauthorized handling (auto-logout)
- Error handling and logging
- JSON serialization/deserialization
- File upload/download support

**Registration in Program.cs:**
```csharp
builder.Services.AddApiService(builder.Configuration);
```

### 2. BaseController (`Controllers/BaseController.cs`)

Base controller with common patterns:

```csharp
public abstract class BaseController : Controller
{
    protected readonly IApiService _apiService;
    
    // Authentication helpers
    protected bool IsAuthenticated() { ... }
    protected IActionResult RequireAuth() { ... }
    protected bool HasRole(params string[] roles) { ... }
    protected IActionResult RequireRole(params string[] roles) { ... }
    
    // Request helpers
    protected int GetPageNumber(int defaultPage = 1) { ... }
    protected string? GetSearchTerm() { ... }
    
    // Response handling
    protected IActionResult HandleApiResponse<T>(ApiResponse<T> response, ...) { ... }
}
```

### 3. ViewModels (`Models/ViewModels/`)

Strongly-typed models for views:

```csharp
public class StudentViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
    public string StudentCode { get; set; }
    
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    public string FullName { get; set; }
    
    // ... with display helpers and validation
}

public class PagedViewModel<T>
{
    public IEnumerable<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    // ... pagination helpers
}
```

### 4. Controllers (Example: StudentsController)

```csharp
public class StudentsController : BaseController
{
    public StudentsController(IApiService apiService, ILogger<StudentsController> logger)
        : base(apiService, logger) { }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        var authResult = RequireAuth();
        if (authResult != null) return authResult;

        var queryParams = new Dictionary<string, string>
        {
            { "pageNumber", pageNumber.ToString() },
            { "pageSize", pageSize.ToString() }
        };
        if (!string.IsNullOrEmpty(searchTerm))
            queryParams["searchTerm"] = searchTerm;

        var response = await _apiService.GetAsync<PagedResponse<StudentViewModel>>("/students", queryParams);
        
        var pagedModel = new PagedViewModel<StudentViewModel>
        {
            Items = response.Data?.Items ?? new List<StudentViewModel>(),
            PageNumber = response.PageNumber,
            PageSize = response.PageSize,
            TotalRecords = response.TotalRecords,
            SearchTerm = searchTerm
        };

        return View(pagedModel);
    }
}
```

### 5. Views (Example: Students/Index.cshtml)

Razor rendering instead of JavaScript:

```razor
@model PagedViewModel<StudentViewModel>

<table class="table table-hover">
    <thead>
        <tr>
            <th>STT</th>
            <th>Mã SV</th>
            <th>Họ và tên</th>
            <!-- ... -->
        </tr>
    </thead>
    <tbody>
        @if (Model.Items.Any())
        {
            var index = Model.StartRecord;
            @foreach (var student in Model.Items)
            {
                <tr>
                    <td>@index</td>
                    <td>@student.StudentCode</td>
                    <td>@student.FullName</td>
                    <!-- ... -->
                </tr>
                index++;
            }
        }
        else
        {
            <tr>
                <td colspan="9" class="text-center">
                    <i class="ri-inbox-line"></i>
                    <p>Không có sinh viên nào</p>
                </td>
            </tr>
        }
    </tbody>
</table>

<!-- Pagination -->
@if (Model.TotalPages > 1)
{
    <nav>
        <ul class="pagination">
            @if (Model.HasPrevious)
            {
                <li class="page-item">
                    <a class="page-link" asp-action="Index" asp-route-pageNumber="1">
                        <i class="ri-double-left-line"></i>
                    </a>
                </li>
            }
            <!-- ... -->
        </ul>
    </nav>
}
```

## Migration Checklist for Other Modules

To migrate other modules (Lecturers, Exams, etc.), follow these steps:

### 1. Create ViewModels
```csharp
// Models/ViewModels/LecturerViewModel.cs
public class LecturerViewModel { ... }
public class CreateLecturerViewModel { ... }
public class UpdateLecturerViewModel { ... }
```

### 2. Create/Update Controller
```csharp
// Controllers/LecturersController.cs
public class LecturersController : BaseController
{
    public LecturersController(IApiService apiService, ILogger<LecturersController> logger)
        : base(apiService, logger) { }
    
    // Implement CRUD actions similar to StudentsController
}
```

### 3. Create Views
```
Views/Lecturers/
├── Index.cshtml        // List with pagination
├── Create.cshtml       // Create form
├── Edit.cshtml         // Edit form
└── Details.cshtml      // Details view
```

### 4. Archive Old JS Files
Move old JavaScript files to `wwwroot/js/_archived/`:
```bash
robocopy "wwwroot/js/modules/lecturers" "wwwroot/js/_archived/lecturers" /E /MOVE
```

### 5. Update Navigation
Update menu links in `_Sidebar.cshtml` or `_Header.cshtml` to point to new controller actions.

## Benefits of Server-Side MVC

### ✅ Advantages
1. **Better SEO** - Content rendered server-side is indexable
2. **Faster Initial Load** - No need to download and execute JS
3. **Type Safety** - Compile-time checking with ViewModels
4. **Built-in Validation** - ASP.NET Core validation attributes
5. **Simpler Code** - No need for separate API and UI modules
6. **Better Security** - No JWT tokens in localStorage
7. **Easier Debugging** - Server-side stack traces
8. **Progressive Enhancement** - Works without JavaScript

### ⚠️ Considerations
1. **More Server Load** - Rendering happens on server
2. **Page Refreshes** - Full page reload on actions
3. **Less Interactive** - Not as smooth as SPA

## Adding Client-Side Interactivity (Optional)

For features that need JavaScript (e.g., real-time search, modals):

```html
@section Scripts {
    <script>
        // Simple vanilla JS for interactivity
        document.querySelector('#searchInput')?.addEventListener('input', function(e) {
            // Debounced search
            clearTimeout(this.timeout);
            this.timeout = setTimeout(() => {
                window.location.href = `?searchTerm=${e.target.value}`;
            }, 300);
        });
    </script>
}
```

## Testing

### Run the Application
```bash
# Terminal 1 - API
cd e360_clone/e360_clone_api
dotnet run

# Terminal 2 - Frontend
cd e360_clone/e360_clone_fe
dotnet run
```

### Test Student Management
1. Navigate to `http://localhost:5000/Students`
2. Login with `admin@e360.com` / `123456`
3. Test: View list, Create, Edit, Delete students
4. Verify: Pagination, Search, Flash messages

## Troubleshooting

### "Unauthorized" Errors
- Check that user is logged in
- Verify API is running and accessible
- Check JWT token in session

### API Connection Errors
- Verify `ApiSettings:BaseUrl` in `appsettings.json`
- Check CORS settings in API project
- Ensure API is running on correct port

### Model Validation Fails
- Check Data Annotations in ViewModels
- Verify form field names match model properties
- Check `ModelState.IsValid` in controller

## Next Steps

1. **Migrate Other Modules**: Lecturers, Exams, Grades, etc.
2. **Add Partial Views**: Reusable components (tables, forms, pagination)
3. **Implement AJAX Enhancements**: Optional JavaScript for better UX
4. **Add Export/Import**: Excel/PDF export functionality
5. **Create Tag Helpers**: Reusable UI components
6. **Add Unit Tests**: Test controllers and services

## References

- [ASP.NET Core MVC](https://docs.microsoft.com/en-us/aspnet/core/mvc/)
- [Model Validation](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [HttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)
