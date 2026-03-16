# Kiến trúc Fragment - E360 Clone Frontend

## 📁 Cấu trúc thư mục mới

```
e360_clone_fe/
├── Views/
│   ├── Shared/                    # ✅ Fragment chung (MỚI)
│   │   ├── _Layout.cshtml         # Layout chính
│   │   ├── _ViewStart.cshtml      # Default layout config
│   │   ├── _HeadPartial.cshtml    # CSS & Meta tags
│   │   ├── _Header.cshtml         # Top header bar
│   │   ├── _Sidebar.cshtml        # Sidebar navigation
│   │   ├── _Footer.cshtml         # Footer
│   │   ├── _ScriptsPartial.cshtml # JavaScript imports
│   │   └── _ThemeCustomization.cshtml # Theme panel
│   ├── Home/                      # Home views
│   │   └── Index.cshtml           # Dashboard
│   ├── Students/                  # Student module views
│   │   └── Index.cshtml           # Student list
│   ├── Teachers/                  # Teacher module views
│   └── assets/                    # ⚠️ OLD - Không dùng nữa
├── wwwroot/
│   ├── assets/                    # ✅ Assets đã di chuyển vào đây
│   │   ├── css/
│   │   ├── js/
│   │   ├── images/
│   │   ├── fonts/
│   │   └── webfonts/
│   ├── js/
│   │   ├── config.js
│   │   ├── utils.js
│   │   ├── api-client.js
│   │   └── modules/
│   │       ├── students.api.js
│   │       └── students.ui.js
│   └── css/
├── Controllers/
│   ├── HomeController.cs
│   └── StudentsController.cs
└── Program.cs
```

## 🎯 Lợi ích

### 1. Dễ bảo trì
- **Chỉ sửa 1 file**: Khi cần thay đổi header/sidebar, chỉ cần update file partial tương ứng
- **Tự động apply**: Tất cả views đều thừa kế từ `_Layout.cshtml`

### 2. Nhất quán
- **Cùng 1 layout**: Tất cả pages dùng chung `_Layout.cshtml`
- **Cùng CSS/JS**: `_HeadPartial.cshtml` và `_ScriptsPartial.cshtml` đảm bảo tất cả pages có cùng resources

### 3. Module hóa
- **Mỗi feature 1 folder**: `Views/Students/`, `Views/Teachers/`, `Views/Exams/`
- **Dễ tìm kiếm**: Biết ngay view của feature nào ở folder nào

### 4. ASP.NET Core Convention
- **Theo chuẩn MVC**: Sử dụng `_ViewStart.cshtml`, `_Layout.cshtml`, Partial Views
- **Tag Helpers**: Có thể sử dụng ASP.NET Core Tag Helpers

## 📝 Cách sử dụng

### Tạo view mới

1. **Tạo folder cho module** (nếu chưa có):
```
Views/[ModuleName]/
```

2. **Tạo file .cshtml**:
```csharp
@{
    ViewData["Title"] = "Page Title";
}

<div class="main-content__wrapper">
    <!-- Breadcrumb -->
    <div class="breadcrumb-area">
        <h1>@ViewData["Title"]</h1>
    </div>
    
    <!-- Content -->
    <div class="content-area">
        <!-- Your content here -->
    </div>
</div>

@section Scripts {
    <script src="~/js/modules/your-module.api.js"></script>
    <script src="~/js/modules/your-module.ui.js"></script>
}
```

### Override CSS/JS riêng cho page

```csharp
@section Styles {
    <link rel="stylesheet" href="~/css/custom-page.css">
}

@section Scripts {
    <script src="~/js/custom-page.js"></script>
}
```

## 🔧 Đường dẫn assets

Tất cả assets trong `wwwroot/` được truy cập bằng `~/`:

```html
<!-- Images -->
<img src="~/assets/images/logo.png" alt="Logo">

<!-- CSS -->
<link rel="stylesheet" href="~/assets/css/style.css">

<!-- JavaScript -->
<script src="~/assets/js/app.js"></script>
<script src="~/js/modules/students.api.js"></script>
```

## 📋 Checklist tạo module mới

- [ ] Tạo Controller: `Controllers/[ModuleName]Controller.cs`
- [ ] Tạo folder Views: `Views/[ModuleName]/`
- [ ] Tạo views: `Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Details.cshtml`
- [ ] Tạo JS modules: `wwwroot/js/modules/[module].api.js`, `[module].ui.js`
- [ ] Update sidebar: Thêm menu item vào `_Sidebar.cshtml`

## 🚀 Build & Run

```bash
cd e360_clone/e360_clone_fe
dotnet build
dotnet run
```

Truy cập: `http://localhost:5000`

## 📌 Lưu ý

1. **Không edit file trong `Views/assets/`** - Thư mục này đã được di chuyển vào `wwwroot/assets/`
2. **Luôn dùng `~/`** cho đường dẫn static files
3. **Section Scripts là optional** - Chỉ thêm khi cần JS/CSS riêng
4. **Layout tự động apply** - Nhờ `_ViewStart.cshtml`
