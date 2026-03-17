/**
 * Role Enum
 * Maps to database Account.Role
 */
const Role = {
    QuanTriCaoCap: 'Quản trị cao cấp',
    QuanTriVien: 'Quản trị viên',
    SinhVien: 'Sinh viên',
    GiangVien: 'Giảng viên',
    PhuHuynh: 'Phụ huynh',
    Thuthu: 'Thủ thư',
    NhanVien: 'Nhân viên'
};

/**
 * Get display text for role
 */
const RoleText = {
    [Role.QuanTriCaoCap]: 'Quản trị cao cấp',
    [Role.QuanTriVien]: 'Quản trị viên',
    [Role.SinhVien]: 'Sinh viên',
    [Role.GiangVien]: 'Giảng viên',
    [Role.PhuHuynh]: 'Phụ huynh',
    [Role.Thuthu]: 'Thủ thư',
    [Role.NhanVien]: 'Nhân viên'
};

/**
 * Get icon for role
 */
const RoleIcon = {
    [Role.QuanTriCaoCap]: 'ri-shield-star-line',
    [Role.QuanTriVien]: 'ri-admin-line',
    [Role.SinhVien]: 'ri-graduation-cap-line',
    [Role.GiangVien]: 'ri-user-star-line',
    [Role.PhuHuynh]: 'ri-user-follow-line',
    [Role.Thuthu]: 'ri-book-open-line',
    [Role.NhanVien]: 'ri-user-line'
};

/**
 * Role permissions matrix
 */
const RolePermissions = {
    [Role.QuanTriCaoCap]: ['*'], // All permissions
    [Role.QuanTriVien]: ['dashboard.view', 'users.manage', 'students.manage', 'teachers.manage', 'exams.manage'],
    [Role.SinhVien]: ['dashboard.view', 'exams.view', 'grades.view', 'schedule.view'],
    [Role.GiangVien]: ['dashboard.view', 'grades.manage', 'attendance.manage', 'exams.proctor'],
    [Role.PhuHuynh]: ['dashboard.view', 'children.view', 'grades.view'],
    [Role.Thuthu]: ['dashboard.view', 'books.manage'],
    [Role.NhanVien]: ['dashboard.view']
};

/**
 * Helper functions
 */
const RoleHelper = {
    getText: (role) => RoleText[role] || role,
    getIcon: (role) => RoleIcon[role] || 'ri-user-line',
    hasPermission: (role, permission) => {
        const permissions = RolePermissions[role] || [];
        return permissions.includes('*') || permissions.includes(permission);
    },
    isAdmin: (role) => [Role.QuanTriCaoCap, Role.QuanTriVien].includes(role),
    isStudent: (role) => role === Role.SinhVien,
    isTeacher: (role) => role === Role.GiangVien,
    getAll: () => Object.values(Role),
    getAllWithText: () => Object.keys(Role).map(key => ({
        value: Role[key],
        text: RoleText[Role[key]],
        icon: RoleIcon[Role[key]]
    })),
    // Constants for comparison
    QUAN_TRI_CAO_CAP: Role.QuanTriCaoCap,
    QUAN_TRI_VIEN: Role.QuanTriVien,
    SINH_VIEN: Role.SinhVien,
    GIANG_VIEN: Role.GiangVien,
    PHU_HUYNH: Role.PhuHuynh,
    THUTHU: Role.Thuthu,
    NHAN_VIEN: Role.NhanVien
};

window.Role = Role;
window.RoleHelper = RoleHelper;
