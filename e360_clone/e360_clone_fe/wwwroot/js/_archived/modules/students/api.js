/**
 * Students Module - API Service
 * All API calls related to Students
 * Usage: StudentsApi.getAll(), StudentsApi.create(), etc.
 */

const StudentsApi = (function() {
    const ENDPOINTS = {
        BASE: '/students',
        BY_ID: (id) => `/students/${id}`,
        SEARCH: '/students/search',
        EXPORT: '/students/export',
        IMPORT: '/students/import'
    };

    // Get all students with pagination
    async function getAll(page = 1, pageSize = 10, filters = {}) {
        const params = {
            pageNumber: page,
            pageSize,
            ...filters
        };
        const response = await Http.get(ENDPOINTS.BASE, params);
        // Map response using StudentMapper
        return StudentMapper.fromPagedResponse(response);
    }

    // Get student by ID
    async function getById(id) {
        const response = await Http.get(ENDPOINTS.BY_ID(id));
        if (response.success && response.data) {
            return {
                ...response,
                data: StudentMapper.fromApi(response.data)
            };
        }
        return response;
    }

    // Create new student
    async function create(studentData) {
        const dto = StudentMapper.toCreateDto(studentData);
        return Http.post(ENDPOINTS.BASE, dto);
    }

    // Update student
    async function update(id, studentData) {
        const dto = StudentMapper.toUpdateDto({ ...studentData, id });
        return Http.put(ENDPOINTS.BY_ID(id), dto);
    }

    // Delete student
    async function deleteStudent(id) {
        return Http.delete(ENDPOINTS.BY_ID(id));
    }

    // Search students
    async function search(searchTerm, page = 1, pageSize = 10) {
        return getAll(page, pageSize, { searchTerm });
    }

    // Export students to Excel/PDF
    async function exportData(format = 'excel', filters = {}) {
        return Http.download(`${ENDPOINTS.EXPORT}?format=${format}`, filters);
    }

    // Import students from Excel
    async function importData(formData) {
        return Http.upload(ENDPOINTS.IMPORT, formData);
    }

    // Get students by class
    async function getByClass(classId, page = 1, pageSize = 50) {
        return getAll(page, pageSize, { classId });
    }

    // Get students by status
    async function getByStatus(status, page = 1, pageSize = 10) {
        return getAll(page, pageSize, { status });
    }

    // Public API
    return {
        getAll,
        getById,
        create,
        update,
        delete: deleteStudent,
        search,
        exportData,
        importData,
        getByClass,
        getByStatus
    };
})();
