/**
 * Student Mapper
 * Maps API response to Student model and applies enums
 */

const StudentMapper = (function() {
    // Map single student from API
    function fromApi(apiData) {
        if (!apiData) return null;

        const student = new Student(apiData);

        // Apply enum mappings
        student.statusText = StudentStatusHelper.getText(apiData.status);
        student.statusBadge = StudentStatusHelper.getBadge(apiData.status);
        student.genderText = GenderHelper.getText(apiData.gender);
        student.genderIcon = GenderHelper.getIcon(apiData.gender);

        // Format dates
        student.formattedDateOfBirth = formatDate(apiData.dateOfBirth);
        student.formattedCreatedAt = formatDate(apiData.createdAt);

        return student;
    }

    // Map student list from API
    function fromApiList(apiDataList) {
        if (!apiDataList || !Array.isArray(apiDataList)) return [];
        return apiDataList.map(fromApi);
    }

    // Map paged response
    function fromPagedResponse(pagedResponse) {
        if (!pagedResponse) return null;

        return {
            ...pagedResponse,
            data: fromApiList(pagedResponse.data),
            items: fromApiList(pagedResponse.items || pagedResponse.data)
        };
    }

    // Map to API create DTO
    function toCreateDto(student) {
        return {
            studentCode: student.studentCode?.trim(),
            fullName: student.fullName?.trim(),
            dateOfBirth: student.dateOfBirth,
            gender: student.gender,
            email: student.email?.trim(),
            phoneNumber: student.phoneNumber?.trim(),
            address: student.address?.trim(),
            classId: parseInt(student.classId) || 0,
            status: student.status || StudentStatus.DANG_HOC
        };
    }

    // Map to API update DTO
    function toUpdateDto(student) {
        return {
            ...toCreateDto(student),
            id: student.id
        };
    }

    // Format date helper
    function formatDate(dateString) {
        if (!dateString) return '';
        return new Date(dateString).toLocaleDateString('vi-VN');
    }

    // Format date for input
    function formatDateForInput(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toISOString().split('T')[0];
    }

    // Public API
    return {
        fromApi,
        fromApiList,
        fromPagedResponse,
        toCreateDto,
        toUpdateDto,
        formatDate,
        formatDateForInput
    };
})();

window.StudentMapper = StudentMapper;
