/**
 * Grade Type Enum
 * Maps to database Grade.ScoreType
 */
const GradeType = {
    KiemTra15Phut: 'Kiểm tra 15 phút',
    KiemTra1Tiet: 'Kiểm tra 1 tiết',
    GiuaKy: 'Giữa kỳ',
    CuoiKy: 'Cuối kỳ',
    BaiTap: 'Bài tập',
    DoAn: 'Đồ án',
    ChuyenCan: 'Chuyên cần',
    Khac: 'Khác'
};

/**
 * Get display text for grade type
 */
const GradeTypeText = {
    [GradeType.KiemTra15Phut]: 'Kiểm tra 15 phút',
    [GradeType.KiemTra1Tiet]: 'Kiểm tra 1 tiết',
    [GradeType.GiuaKy]: 'Giữa kỳ',
    [GradeType.CuoiKy]: 'Cuối kỳ',
    [GradeType.BaiTap]: 'Bài tập',
    [GradeType.DoAn]: 'Đồ án',
    [GradeType.ChuyenCan]: 'Chuyên cần',
    [GradeType.Khac]: 'Khác'
};

/**
 * Weight coefficients for grade types
 */
const GradeTypeWeight = {
    [GradeType.KiemTra15Phut]: 0.1,
    [GradeType.KiemTra1Tiet]: 0.2,
    [GradeType.GiuaKy]: 0.3,
    [GradeType.CuoiKy]: 0.5,
    [GradeType.BaiTap]: 0.2,
    [GradeType.DoAn]: 0.4,
    [GradeType.ChuyenCan]: 0.1,
    [GradeType.Khac]: 0.1
};

/**
 * Helper functions
 */
const GradeTypeHelper = {
    getText: (type) => GradeTypeText[type] || type,
    getWeight: (type) => GradeTypeWeight[type] || 0.1,
    isValid: (type) => Object.values(GradeType).includes(type),
    getAll: () => Object.values(GradeType),
    getAllWithText: () => Object.keys(GradeType).map(key => ({
        value: GradeType[key],
        text: GradeTypeText[GradeType[key]],
        weight: GradeTypeWeight[GradeType[key]]
    })),
    // Constants for comparison
    KIEM_TRA_15_PHUT: GradeType.KiemTra15Phut,
    KIEM_TRA_1_TIET: GradeType.KiemTra1Tiet,
    GIUA_KY: GradeType.GiuaKy,
    CUOI_KY: GradeType.CuoiKy,
    BAI_TAP: GradeType.BaiTap,
    DO_AN: GradeType.DoAn,
    CHUYEN_CAN: GradeType.ChuyenCan,
    KHAC: GradeType.Khac
};

window.GradeType = GradeType;
window.GradeTypeHelper = GradeTypeHelper;
