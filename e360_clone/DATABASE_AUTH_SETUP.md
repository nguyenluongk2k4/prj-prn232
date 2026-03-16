# Database & Authentication Setup - E360 Clone

## ✅ Hoàn thành

### 1. Database Schema

#### Bảng `Accounts`
Đã được tạo qua migration `20260316170242_AddAccountTable`.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Identity | Primary key |
| Username | varchar(50) | NOT NULL, UNIQUE | Tên đăng nhập |
| Email | varchar(255) | NOT NULL, UNIQUE | Email |
| PasswordHash | varchar(255) | NOT NULL | Mật khẩu hash (SHA256) |
| Role | varchar(50) | NOT NULL | Role: SuperAdmin, Admin, Student, Teacher, Parent, Librarian |
| FullName | varchar(100) | NULL | Họ tên đầy đủ |
| PhoneNumber | varchar(20) | NULL | Số điện thoại |
| AvatarUrl | text | NULL | URL ảnh đại diện |
| Status | varchar(20) | DEFAULT 'Active' | Active, Inactive, Locked |
| CreatedAt | timestamp | NOT NULL | Ngày tạo |
| LastLoginAt | timestamp | NULL | Lần đăng nhập cuối |
| UpdatedAt | timestamp | NULL | Ngày cập nhật |
| StudentId | int | FK, NULL | Link to Students table |
| LecturerId | int | FK, NULL | Link to Lecturers table |

#### Foreign Keys
- `StudentId` → `Students(Id)` ON DELETE SET NULL
- `LecturerId` → `Lecturers(Id)` ON DELETE SET NULL

#### Indexes
- `IX_Accounts_Username` (UNIQUE)
- `IX_Accounts_Email` (UNIQUE)
- `IX_Accounts_StudentId`
- `IX_Accounts_LecturerId`

### 2. Default Accounts (Seeded)

Tất cả tài khoản có password: **`123456`**

| Username | Email | Role | FullName |
|----------|-------|------|----------|
| superadmin | superadmin@e360.com | SuperAdmin | Super Administrator |
| admin | admin@e360.com | Admin | System Administrator |
| student | student@e360.com | Student | Nguyen Van Student |
| teacher | teacher@e360.com | Teacher | Tran Van Teacher |
| parent | parent@e360.com | Parent | Le Van Parent |
| librarian | librarian@e360.com | Librarian | Pham Van Librarian |

### 3. Files Created/Updated

#### Business Objects
- `e360_clone.BusinessObjects/Account.cs` - Entity model
- `e360_clone.BusinessObjects/Helpers/PasswordHelper.cs` - Password hashing

#### Data Access
- `e360_clone.DataAccess/AppDbContext.cs` - Added `DbSet<Account>` and configuration

#### Frontend
- `e360_clone_fe/Models/AuthViewModels.cs` - LoginViewModel, RegisterViewModel
- `e360_clone_fe/Controllers/Auth/AuthController.cs` - Login, Logout, QuickLogin
- `e360_clone_fe/Program.cs` - Added DbContext, Authentication, CORS
- `e360_clone_fe/appsettings.json` - Added ConnectionStrings

#### API
- `e360_clone_api/Seeders/AccountSeeder.cs` - Seed default accounts
- `e360_clone_api/Program.cs` - Call seeder on startup

### 4. Migration Commands

```bash
# Create migration
cd e360_clone/e360_clone_api
dotnet ef migrations add AddAccountTable

# Apply migration
dotnet ef database update
```

### 5. Running the Application

#### Step 1: Run API (to seed data)
```bash
cd e360_clone/e360_clone_api
dotnet run
```
Output: `✅ Database seeded successfully!`

#### Step 2: Run Frontend
```bash
cd e360_clone/e360_clone_fe
dotnet run
```

#### Step 3: Access Login Page
```
http://localhost:5000/Auth/Login
```

### 6. Login Flow

```
┌─────────────────┐
│  Login Page     │
│  /Auth/Login    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Enter Email     │
│ Password        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ AuthController  │
│ POST Login      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Query Database  │
│ SELECT * FROM   │
│ Accounts WHERE  │
│ Email = ?       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Verify Password │
│ SHA256 Hash     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Create Claims   │
│ - Name          │
│ - Email         │
│ - Role          │
│ - FullName      │
│ - UserId        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Sign In         │
│ Cookie Auth     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Redirect to     │
│ Home/Index      │
└─────────────────┘
```

### 7. Password Hashing

Currently using SHA256 (for demo):
```csharp
public static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(password);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}
```

**Production Recommendation:** Use BCrypt or Argon2
```
PM> Install-Package BCrypt.Net-Next
```

### 8. Testing

1. **Login with email/password:**
   - Email: `admin@e360.com`
   - Password: `123456`
   - → Redirects to School Dashboard

2. **Quick Login (no password):**
   - Click "Admin" button
   - → Redirects to School Dashboard

3. **Role-based Dashboard:**
   - Student → Student Dashboard
   - Teacher → Teacher Dashboard
   - Parent → Parent Dashboard
   - Admin/SuperAdmin → School Dashboard

### 9. Next Steps

- [ ] Add Register functionality
- [ ] Add Forgot Password flow
- [ ] Implement 2FA
- [ ] Add password strength validation
- [ ] Add account lockout after failed attempts
- [ ] Add refresh token for remember me
- [ ] Upgrade to BCrypt/Argon2
