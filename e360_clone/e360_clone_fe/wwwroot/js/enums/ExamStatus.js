/**
 * Exam Status Enum
 * Maps to database Exam.Status
 */
const ExamStatus = {
    LenKeHoach: 'Lên kế hoạch',
    DaXepLich: 'Đã xếp lịch',
    DangDienRa: 'Đang diễn ra',
    HoanThanh: 'Hoàn thành',
    DaHuy: 'Đã hủy'
};

/**
 * Get display text for exam status
 */
const ExamStatusText = {
    [ExamStatus.LenKeHoach]: 'Đã lên kế hoạch',
    [ExamStatus.DaXepLich]: 'Đã xếp lịch',
    [ExamStatus.DangDienRa]: 'Đang diễn ra',
    [ExamStatus.HoanThanh]: 'Hoàn thành',
    [ExamStatus.DaHuy]: 'Đã hủy'
};

/**
 * Get badge class for exam status
 */
const ExamStatusBadge = {
    [ExamStatus.LenKeHoach]: 'bg-secondary',
    [ExamStatus.DaXepLich]: 'bg-primary',
    [ExamStatus.DangDienRa]: 'bg-warning',
    [ExamStatus.HoanThanh]: 'bg-success',
    [ExamStatus.DaHuy]: 'bg-danger'
};

/**
 * Helper functions
 */
const ExamStatusHelper = {
    getText: (status) => ExamStatusText[status] || status,
    getBadge: (status) => ExamStatusBadge[status] || 'bg-primary',
    isValid: (status) => Object.values(ExamStatus).includes(status),
    getAll: () => Object.values(ExamStatus),
    getAllWithText: () => Object.keys(ExamStatus).map(key => ({
        value: ExamStatus[key],
        text: ExamStatusText[ExamStatus[key]],
        badge: ExamStatusBadge[ExamStatus[key]]
    })),
    isUpcoming: (status) => [ExamStatus.LenKeHoach, ExamStatus.DaXepLich].includes(status),
    isCompleted: (status) => status === ExamStatus.HoanThanh,
    // Constants for comparison
    LEN_KE_HOACH: ExamStatus.LenKeHoach,
    DA_XEP_LICH: ExamStatus.DaXepLich,
    DANG_DIEN_RA: ExamStatus.DangDienRa,
    HOAN_THANH: ExamStatus.HoanThanh,
    DA_HUY: ExamStatus.DaHuy
};

window.ExamStatus = ExamStatus;
window.ExamStatusHelper = ExamStatusHelper;
