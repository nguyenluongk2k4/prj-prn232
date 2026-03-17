/**
 * Account Status Enum
 * Maps to database Account.Status
 */
const AccountStatus = {
    HoatDong: 'Hoạt động',
    KhongHoatDong: 'Không hoạt động',
    BiKhoa: 'Bị khóa',
    DinhChi: 'Đình chỉ'
};

/**
 * Get display text for account status
 */
const AccountStatusText = {
    [AccountStatus.HoatDong]: 'Hoạt động',
    [AccountStatus.KhongHoatDong]: 'Không hoạt động',
    [AccountStatus.BiKhoa]: 'Bị khóa',
    [AccountStatus.DinhChi]: 'Đình chỉ'
};

/**
 * Get badge class for account status
 */
const AccountStatusBadge = {
    [AccountStatus.HoatDong]: 'bg-success',
    [AccountStatus.KhongHoatDong]: 'bg-secondary',
    [AccountStatus.BiKhoa]: 'bg-danger',
    [AccountStatus.DinhChi]: 'bg-warning'
};

/**
 * Helper functions
 */
const AccountStatusHelper = {
    getText: (status) => AccountStatusText[status] || status,
    getBadge: (status) => AccountStatusBadge[status] || 'bg-primary',
    isValid: (status) => Object.values(AccountStatus).includes(status),
    getAll: () => Object.values(AccountStatus),
    getAllWithText: () => Object.keys(AccountStatus).map(key => ({
        value: AccountStatus[key],
        text: AccountStatusText[AccountStatus[key]]
    })),
    // Constants for comparison
    HOAT_DONG: AccountStatus.HoatDong,
    KHONG_HOAT_DONG: AccountStatus.KhongHoatDong,
    BI_KHOA: AccountStatus.BiKhoa,
    DINH_CHI: AccountStatus.DinhChi
};

window.AccountStatus = AccountStatus;
window.AccountStatusHelper = AccountStatusHelper;
