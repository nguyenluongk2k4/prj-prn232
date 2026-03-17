/**
 * Attendance Status Enum
 * Maps to database Attendance.Status
 */
const AttendanceStatus = {
    CoMat: 'Có mặt',
    VangMat: 'Vắng mặt',
    DiMuon: 'Đi muộn',
    VangCoPhep: 'Vắng có phép',
    VeSom: 'Về sớm'
};

/**
 * Get display text for attendance status
 */
const AttendanceStatusText = {
    [AttendanceStatus.CoMat]: 'Có mặt',
    [AttendanceStatus.VangMat]: 'Vắng mặt',
    [AttendanceStatus.DiMuon]: 'Đi muộn',
    [AttendanceStatus.VangCoPhep]: 'Vắng có phép',
    [AttendanceStatus.VeSom]: 'Về sớm'
};

/**
 * Get badge class for attendance status
 */
const AttendanceStatusBadge = {
    [AttendanceStatus.CoMat]: 'bg-success',
    [AttendanceStatus.VangMat]: 'bg-danger',
    [AttendanceStatus.DiMuon]: 'bg-warning',
    [AttendanceStatus.VangCoPhep]: 'bg-info',
    [AttendanceStatus.VeSom]: 'bg-secondary'
};

/**
 * Helper functions
 */
const AttendanceStatusHelper = {
    getText: (status) => AttendanceStatusText[status] || status,
    getBadge: (status) => AttendanceStatusBadge[status] || 'bg-primary',
    isValid: (status) => Object.values(AttendanceStatus).includes(status),
    getAll: () => Object.values(AttendanceStatus),
    getAllWithText: () => Object.keys(AttendanceStatus).map(key => ({
        value: AttendanceStatus[key],
        text: AttendanceStatusText[AttendanceStatus[key]],
        badge: AttendanceStatusBadge[AttendanceStatus[key]]
    })),
    isPresent: (status) => status === AttendanceStatus.CoMat,
    isAbsent: (status) => status === AttendanceStatus.VangMat,
    // Constants for comparison
    CO_MAT: AttendanceStatus.CoMat,
    VANG_MAT: AttendanceStatus.VangMat,
    DI_MUON: AttendanceStatus.DiMuon,
    VANG_CO_PHEP: AttendanceStatus.VangCoPhep,
    VE_SOM: AttendanceStatus.VeSom
};

window.AttendanceStatus = AttendanceStatus;
window.AttendanceStatusHelper = AttendanceStatusHelper;
