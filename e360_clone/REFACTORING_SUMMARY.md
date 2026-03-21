# Refactoring Summary - Auth Module với Repository Pattern & DAO

## ✅ Kiến Trúc Mới (Đúng Chuẩn)

### Folder Structure

```
e360_clone/
├── e360_clone.BusinessObjects/
│   ├── Enums/
│   │   └── Enums.cs              # Gender, StudentStatus, AccountStatus, Role, GradeType
│   ├── Helpers/
│   │   └── PasswordHelper.cs     # Password hashing utilities
│   └── Account.cs                # Entity model
│
├── e360_clone.DataAccess/
│   ├── AppDbContext.cs           # EF Core DbContext
│   ├── DAOs/                     # NEW: Data Access Objects
│   │   ├── BaseDAO.cs            # Base class for all DAOs
│   │   │   - IBaseDAO<T>         # Interface với CRUD cơ bản
│   │   │   - BaseDAO<T>          # Implementation cơ bản
│   │   └── AccountDAO.cs         # DAO cho Account (chứa logic DB trực tiếp)
│   └── Configurations/
│       └── AccountConfiguration.cs
│
├── e360_clone.Repositories/
│   ├── IRepository.cs            # Generic repository interface
│   ├── Repository.cs             # Generic repository implementation
│   ├── IAccountRepository.cs     # Interface cho Account repository
│   └── AccountRepository.cs      # Repository gọi DAO
│
└── e360_clone_fe/
    ├── Controllers/
    │   └── Auth/
    │       └── AuthController.cs  # Controller gọi Repository
    ├── Services/
    │   ├── ApiService.cs
    │   └── ApiSettings.cs
    └── Program.cs                # DI registration
```

---

## 📐 Luồng Dữ Liệu (Data Flow)

```
┌─────────────────┐
│ AuthController  │
│ (FE Controller) │
└────────┬────────┘
         │ Depends on
         ▼
┌─────────────────┐
│ IAccountRepo    │
│ (Interface)     │
└────────┬────────┘
         │ Implements
         ▼
┌─────────────────┐
│ AccountRepo     │
│ (Repository)    │
└────────┬────────┘
         │ Uses
         ▼
┌─────────────────┐
│  AccountDAO     │
│ (Data Access)   │
└────────┬────────┘
         │ Uses
         ▼
┌─────────────────┐
│ AppDbContext    │
│ (EF Core)       │
└────────┬────────┘
         │ Maps to
         ▼
┌─────────────────┐
│   Database      │
│  (Accounts)     │
└─────────────────┘
```

---

## 📝 Chi Tiết Implementation

### 1. DAO Layer (DataAccess/DAOs/)

**BaseDAO.cs** - Base class cho tất cả DAOs:

```csharp
public interface IBaseDAO<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}

public abstract class BaseDAO<T> : IBaseDAO<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    // Implement CRUD operations
}
```

**AccountDAO.cs** - DAO cụ thể cho Account:

```csharp
public class AccountDAO
{
    private readonly AppDbContext _context;
    private readonly DbSet<Account> _dbSet;

    public AccountDAO(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Account>();
    }

    // Business-specific data operations
    public async Task<Account?> FindByEmailOrUsernameAsync(string emailOrUsername)
    {
        return await _dbSet.FirstOrDefaultAsync(
            a => a.Email == emailOrUsername || a.Username == emailOrUsername);
    }

    public async Task UpdateLastLoginAsync(int accountId)
    {
        var account = await GetByIdAsync(accountId);
        if (account != null)
        {
            account.LastLoginAt = DateTime.Now;
            Update(account);
            await SaveChangesAsync();
        }
    }

    // ... other methods
}
```

---

### 2. Repository Layer (Repositories/)

**IAccountRepository.cs** - Interface:

```csharp
public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> FindByEmailOrUsernameAsync(string emailOrUsername);
    Task<Account?> FindByEmailAsync(string email);
    Task<Account?> FindByUsernameAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<IEnumerable<Account>> GetByRoleAsync(string role);
    Task<IEnumerable<Account>> GetActiveAccountsAsync();
    Task UpdateLastLoginAsync(int accountId);
    Task<bool> ChangePasswordAsync(int accountId, string newPasswordHash);
    Task LockAccountAsync(int accountId);
    Task UnlockAccountAsync(int accountId);
}
```

**AccountRepository.cs** - Implementation (gọi DAO):

```csharp
public class AccountRepository : Repository<Account>, IAccountRepository
{
    private readonly AccountDAO _accountDao;

    public AccountRepository(AppDbContext context, AccountDAO accountDao) 
        : base(context)
    {
        _accountDao = accountDao;
    }

    public async Task<Account?> FindByEmailOrUsernameAsync(string emailOrUsername)
    {
        return await _accountDao.FindByEmailOrUsernameAsync(emailOrUsername);
    }

    public async Task UpdateLastLoginAsync(int accountId)
    {
        await _accountDao.UpdateLastLoginAsync(accountId);
    }

    // ... delegate other methods to DAO
}
```

---

### 3. Controller Layer (FE Controllers)

**AuthController.cs** - Controller gọi Repository:

```csharp
public class AuthController : Controller
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAccountRepository accountRepository,
        ILogger<AuthController> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        // Find account via Repository
        var account = await _accountRepository.FindByEmailOrUsernameAsync(model.Email);

        if (account == null)
        {
            TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng";
            return View(model);
        }

        // Verify password
        if (!PasswordHelper.VerifyPassword(model.Password, account.PasswordHash))
        {
            TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng";
            return View(model);
        }

        // Create claims & sign in
        // ...

        // Update last login via Repository
        await _accountRepository.UpdateLastLoginAsync(account.Id);

        return RedirectToAction("Index", "Home");
    }
}
```

---

### 4. Dependency Injection (Program.cs)

```csharp
using e360_clone.DataAccess;
using e360_clone.DataAccess.DAOs;
using e360_clone.Repositories;

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(...));

// Register DAOs
builder.Services.AddScoped<AccountDAO>();

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

// Register ApiService
builder.Services.AddApiService(builder.Configuration);

// Register Authentication
builder.Services.AddAuthentication(...);
```

---

## 🎯 Lợi Ích Của Kiến Trúc Này

### 1. Phân Tầng Rõ Ràng

| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| **DAO** | Direct DB operations via EF Core | BusinessObjects, EF Core |
| **Repository** | Business logic, orchestration | DAOs, BusinessObjects |
| **Controller** | HTTP handling, validation | Repositories, ViewModels |

### 2. Testability

- Dễ dàng mock `IAccountRepository` để test Controller
- Dễ dàng mock `AccountDAO` để test Repository

### 3. Maintainability

- Thay đổi logic DB → chỉ sửa DAO
- Thay đổi business logic → chỉ sửa Repository
- Thay đổi HTTP handling → chỉ sửa Controller

### 4. Reusability

- `BaseDAO<T>` có thể reuse cho tất cả entities
- `Repository<T>` cung cấp CRUD cơ bản cho mọi entity
- `AccountDAO` chỉ chứa logic đặc thù của Account

---

## 📋 Checklist Khi Tạo Module Mới

### 1. BusinessObjects
- [ ] Tạo entity class (e.g., `Student.cs`)
- [ ] Thêm enum (nếu cần)

### 2. DataAccess
- [ ] Thêm `DbSet<T>` vào `AppDbContext`
- [ ] Tạo entity configuration
- [ ] Tạo DAO class kế thừa `BaseDAO<T>`:
  ```csharp
  public class StudentDAO : BaseDAO<Student>
  {
      public StudentDAO(AppDbContext context) : base(context) { }
      
      // Add student-specific methods
  }
  ```
- [ ] Tạo migration: `dotnet ef migrations add AddStudents`

### 3. Repositories
- [ ] Tạo `IStudentRepository : IRepository<Student>`
- [ ] Tạo `StudentRepository` sử dụng `StudentDAO`
- [ ] Register DI: `builder.Services.AddScoped<StudentDAO>();`

### 4. Frontend
- [ ] Tạo ViewModels
- [ ] Tạo Controller sử dụng `IStudentRepository`
- [ ] Tạo Views (Index, Create, Edit, Details)
- [ ] Test

---

## 🔧 Build Commands

```bash
# Build từng project
cd e360_clone
dotnet build e360_clone.BusinessObjects/e360_clone.BusinessObjects.csproj
dotnet build e360_clone.DataAccess/e360_clone.DataAccess.csproj
dotnet build e360_clone.Repositories/e360_clone.Repositories.csproj
dotnet build e360_clone_fe/e360_clone_fe.csproj

# Build tất cả
dotnet build e360_clone.slnx

# Clean and rebuild
dotnet clean
dotnet build
```

---

## 🚀 Running The Application

**Lưu ý:** Stop Visual Studio process trước khi build để tránh file lock.

```bash
# Terminal 1 - Frontend
cd e360_clone/e360_clone_fe
dotnet run

# Access
http://localhost:5000/Auth/Login

# Login credentials
Email: admin@e360.com
Password: 123456

# Or use Quick Login buttons
```

---

## 📚 Next Steps

1. ✅ **Auth Module** - Hoàn thành với Repository pattern & DAO
2. ⏳ **Students Module** - Refactor để sử dụng DAO pattern
3. ⏳ **Lecturers Module** - Implement mới với DAO pattern
4. ⏳ **Classes & Subjects** - Implement mới
5. ⏳ **Exams & Schedules** - Implement mới
6. ⏳ **Grades & Attendance** - Implement mới

---

## 📖 Tài Liệu Tham Khảo

- [Repository Pattern](https://docs.microsoft.com/en-us/aspnet/core/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-implementation)
- [DAO Pattern](https://en.wikipedia.org/wiki/Data_access_object)
- [ASP.NET Core Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
