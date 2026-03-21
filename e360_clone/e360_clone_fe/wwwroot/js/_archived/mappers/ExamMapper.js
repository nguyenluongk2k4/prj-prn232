/**
 * Exam Mapper
 * Maps API response to Exam model and applies enums
 */

const ExamMapper = (function() {
    // Map single exam from API
    function fromApi(apiData) {
        if (!apiData) return null;

        const exam = new Exam(apiData);

        // Apply enum mappings
        exam.statusText = ExamStatusHelper.getText(apiData.status);
        exam.statusBadge = ExamStatusHelper.getBadge(apiData.status);

        // Format dates and times
        exam.formattedDate = formatDate(apiData.examDate);
        exam.formattedTime = formatTimeRange(apiData.startTime, apiData.endTime);
        exam.formattedCreatedAt = formatDate(apiData.createdAt);

        return exam;
    }

    // Map exam list from API
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

    // Format date helper
    function formatDate(dateString) {
        if (!dateString) return '';
        return new Date(dateString).toLocaleDateString('vi-VN');
    }

    // Format time range helper
    function formatTimeRange(startTime, endTime) {
        if (!startTime) return '';
        const start = startTime.substring(0, 5);
        const end = endTime ? endTime.substring(0, 5) : '...';
        return `${start} - ${end}`;
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
        formatDate,
        formatTimeRange,
        formatDateForInput
    };
})();

window.ExamMapper = ExamMapper;
