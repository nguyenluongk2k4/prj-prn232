/**
 * Gender Enum
 */
const Gender = {
    Nam: 'Nam',
    Nu: 'Nữ',
    Khac: 'Khác'
};

/**
 * Get display text for gender
 */
const GenderText = {
    [Gender.Nam]: 'Nam',
    [Gender.Nu]: 'Nữ',
    [Gender.Khac]: 'Khác'
};

/**
 * Get icon for gender
 */
const GenderIcon = {
    [Gender.Nam]: 'ri-mars-line',
    [Gender.Nu]: 'ri-venus-line',
    [Gender.Khac]: 'ri-question-line'
};

/**
 * Helper functions
 */
const GenderHelper = {
    getText: (gender) => GenderText[gender] || gender,
    getIcon: (gender) => GenderIcon[gender] || 'ri-question-line',
    isValid: (gender) => Object.values(Gender).includes(gender),
    getAll: () => Object.values(Gender),
    getAllWithText: () => Object.keys(Gender).map(key => ({
        value: Gender[key],
        text: GenderText[Gender[key]],
        icon: GenderIcon[Gender[key]]
    })),
    // Constants for comparison
    NAM: Gender.Nam,
    NU: Gender.Nu,
    KHAC: Gender.Khac
};

window.Gender = Gender;
window.GenderHelper = GenderHelper;
