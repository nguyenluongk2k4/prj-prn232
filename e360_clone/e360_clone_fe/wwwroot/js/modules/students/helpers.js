/**
 * Students Module - Helper Functions
 * UI helpers and utilities specific to Student module
 * Usage: StudentsHelper.formatStudent(), StudentsHelper.getStatusBadge(), etc.
 */

const StudentsHelper = (function() {
    // Format student data for display (uses StudentMapper)
    function formatStudent(student) {
        if (!student) return null;

        // If student is already mapped, use computed properties
        if (typeof Student !== 'undefined' && student instanceof Student) {
            return student;
        }

        // Otherwise use mapper
        if (typeof StudentMapper !== 'undefined') {
            return StudentMapper.fromApi(student);
        }

        // Fallback: basic formatting
        return {
            ...student,
            formattedDateOfBirth: formatDate(student.dateOfBirth),
            statusText: getStatusText(student.status),
            statusBadge: getStatusClass(student.status)
        };
    }

    // Get status badge class (uses StudentStatus enum)
    function getStatusClass(status) {
        return StudentStatusHelper.getBadge(status);
    }

    // Get status text (uses StudentStatus enum)
    function getStatusText(status) {
        return StudentStatusHelper.getText(status);
    }

    // Format date
    function formatDate(dateString) {
        if (typeof StudentMapper !== 'undefined') {
            return StudentMapper.formatDate(dateString);
        }
        if (!dateString) return '';
        return new Date(dateString).toLocaleDateString('vi-VN');
    }

    // Format date for input
    function formatDateForInput(dateString) {
        if (typeof StudentMapper !== 'undefined') {
            return StudentMapper.formatDateForInput(dateString);
        }
        if (!dateString) return '';
        return new Date(dateString).toISOString().split('T')[0];
    }

    // Validate student form
    function validateForm(formData) {
        const errors = [];

        if (!formData.studentCode?.trim()) {
            errors.push('Student code is required');
        }
        if (!formData.fullName?.trim()) {
            errors.push('Full name is required');
        }
        if (!formData.dateOfBirth) {
            errors.push('Date of birth is required');
        }
        if (!formData.email?.trim()) {
            errors.push('Email is required');
        } else if (!isValidEmail(formData.email)) {
            errors.push('Invalid email format');
        }

        // Validate status using enum
        if (formData.status && !StudentStatusHelper.isValid(formData.status)) {
            errors.push('Invalid student status');
        }

        // Validate gender using enum
        if (formData.gender && !GenderHelper.isValid(formData.gender)) {
            errors.push('Invalid gender');
        }

        return {
            isValid: errors.length === 0,
            errors
        };
    }

    // Validate email
    function isValidEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    // Create student row HTML (uses mapped student)
    function createStudentRow(student, index) {
        // Map student if not already mapped
        const mapped = formatStudent(student);
        
        return `
            <tr>
                <td><strong>${escapeHtml(mapped.studentCode)}</strong></td>
                <td>${escapeHtml(mapped.fullName)}</td>
                <td>${mapped.formattedDateOfBirth}</td>
                <td>${escapeHtml(mapped.genderText || mapped.gender)}</td>
                <td>${escapeHtml(mapped.email)}</td>
                <td>${escapeHtml(mapped.phoneNumber || '-')}</td>
                <td>
                    <span class="badge ${mapped.statusBadge}">
                        ${mapped.statusText}
                    </span>
                </td>
                <td>
                    <div class="btn-group">
                        <button class="btn btn-sm btn-outline-primary" onclick="StudentsUI.edit(${mapped.id})">
                            <i class="ri-edit-line"></i> Edit
                        </button>
                        <button class="btn btn-sm btn-outline-danger" onclick="StudentsUI.delete(${mapped.id}, '${escapeHtml(mapped.fullName)}')">
                            <i class="ri-delete-bin-line"></i> Delete
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }

    // Escape HTML to prevent XSS
    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Show loading state
    function showLoading(elementId) {
        const el = document.getElementById(elementId);
        if (el) {
            el.innerHTML = `
                <div class="d-flex justify-content-center align-items-center py-5">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
            `;
        }
    }

    // Show empty state
    function showEmptyState(elementId, message = 'No data available') {
        const el = document.getElementById(elementId);
        if (el) {
            el.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center py-5 text-muted">
                        <i class="ri-inbox-line text-4xl d-block mb-2"></i>
                        ${escapeHtml(message)}
                    </td>
                </tr>
            `;
        }
    }

    // Get status options for dropdown
    function getStatusOptions() {
        return StudentStatusHelper.getAllWithText();
    }

    // Get gender options for dropdown
    function getGenderOptions() {
        return GenderHelper.getAllWithText();
    }

    // Public API
    return {
        formatStudent,
        getStatusClass,
        getStatusText,
        formatDate,
        formatDateForInput,
        validateForm,
        isValidEmail,
        createStudentRow,
        escapeHtml,
        showLoading,
        showEmptyState,
        getStatusOptions,
        getGenderOptions
    };
})();
