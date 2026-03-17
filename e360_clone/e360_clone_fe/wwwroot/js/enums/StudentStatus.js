/**
 * Student Status Enum
 * Maps to database Student.Status
 */
const StudentStatus = {
    DangHoc: 'Đang học',
    TamNgung: 'Tạm ngưng',
    TotNghiep: 'Tốt nghiệp',
    DinhChi: 'Đình chỉ',
    BuocThoiHoc: 'Buộc thôi học'
};

/**
 * Get display text for student status
 */
const StudentStatusText = {
    [StudentStatus.DangHoc]: 'Đang học',
    [StudentStatus.TamNgung]: 'Tạm ngưng học',
    [StudentStatus.TotNghiep]: 'Đã tốt nghiệp',
    [StudentStatus.DinhChi]: 'Đình chỉ học',
    [StudentStatus.BuocThoiHoc]: 'Buộc thôi học'
};

/**
 * Get badge class for student status
 */
const StudentStatusBadge = {
    [StudentStatus.DangHoc]: 'bg-success',
    [StudentStatus.TamNgung]: 'bg-secondary',
    [StudentStatus.TotNghiep]: 'bg-info',
    [StudentStatus.DinhChi]: 'bg-warning',
    [StudentStatus.BuocThoiHoc]: 'bg-danger'
};

/**
 * Helper functions
 */
const StudentStatusHelper = {
    getText: (status) => StudentStatusText[status] || status,
    getBadge: (status) => StudentStatusBadge[status] || 'bg-primary',
    isValid: (status) => Object.values(StudentStatus).includes(status),
    getAll: () => Object.values(StudentStatus),
    getAllWithText: () => Object.keys(StudentStatus).map(key => ({
        value: StudentStatus[key],
        text: StudentStatusText[StudentStatus[key]],
        badge: StudentStatusBadge[StudentStatus[key]]
    })),
    isActive: (status) => status === StudentStatus.DangHoc,
    isStudying: (status) => [StudentStatus.DangHoc, StudentStatus.TamNgung].includes(status),
    // Constants for comparison
    DANG_HOC: StudentStatus.DangHoc,
    TAM_NGUNG: StudentStatus.TamNgung,
    TOT_NGHIEP: StudentStatus.TotNghiep,
    DINH_CHI: StudentStatus.DinhChi,
    BUOC_THOI_HOC: StudentStatus.BuocThoiHoc
};

window.StudentStatus = StudentStatus;
window.StudentStatusHelper = StudentStatusHelper;
