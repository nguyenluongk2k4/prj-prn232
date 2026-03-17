/**
 * Students Module - UI Controller
 * Handles UI logic, events, and rendering for Student module
 * Usage: StudentsUI.init(), StudentsUI.loadStudents(), etc.
 */

const StudentsUI = (function() {
    // State
    let currentPage = 1;
    let pageSize = 10;
    let currentSearchTerm = '';
    let currentFilters = {};

    // DOM elements
    let elements = {};

    // Initialize
    function init() {
        cacheElements();
        bindEvents();
        loadStudents(1);
    }

    // Cache DOM elements
    function cacheElements() {
        elements = {
            tableBody: document.querySelector('#studentsTable tbody'),
            pagination: document.getElementById('pagination'),
            searchInput: document.getElementById('searchInput'),
            searchBtn: document.querySelector('[data-action="search"]'),
            addBtn: document.querySelector('[data-action="add"]'),
            modal: document.getElementById('studentModal'),
            modalTitle: document.getElementById('modalTitle'),
            form: document.getElementById('studentForm'),
            studentId: document.getElementById('studentId'),
            studentCode: document.getElementById('studentCode'),
            fullName: document.getElementById('fullName'),
            dateOfBirth: document.getElementById('dateOfBirth'),
            gender: document.getElementById('gender'),
            email: document.getElementById('email'),
            phoneNumber: document.getElementById('phoneNumber'),
            address: document.getElementById('address'),
            classId: document.getElementById('classId'),
            status: document.getElementById('status')
        };
    }

    // Bind events
    function bindEvents() {
        // Search
        elements.searchInput?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') search();
        });

        elements.searchBtn?.addEventListener('click', search);

        // Add new
        elements.addBtn?.addEventListener('click', prepareAdd);

        // Form submit
        elements.form?.addEventListener('submit', (e) => {
            e.preventDefault();
            save();
        });
    }

    // Load students
    async function loadStudents(page = 1) {
        showLoading();
        
        try {
            const response = await StudentsApi.getAll(page, pageSize, currentFilters);
            
            if (response.success) {
                renderTable(response.data);
                renderPagination(response.pageNumber, response.totalPages);
            } else {
                showToast(response.message, 'danger');
            }
        } catch (error) {
            showToast('Error loading students: ' + error.message, 'danger');
        } finally {
            hideLoading();
        }
    }

    // Render table
    function renderTable(students) {
        if (!elements.tableBody) return;

        if (!students || students.length === 0) {
            StudentsHelper.showEmptyState(elements.tableBody.id, 'No students found');
            return;
        }

        elements.tableBody.innerHTML = students
            .map((student, index) => StudentsHelper.createStudentRow(student, index))
            .join('');
    }

    // Render pagination
    function renderPagination(current, total) {
        if (!elements.pagination) return;

        elements.pagination.innerHTML = '';

        // Previous button
        const prevLi = createPaginationItem('Previous', current === 1, () => {
            if (current > 1) loadStudents(current - 1);
        });
        elements.pagination.appendChild(prevLi);

        // Page numbers
        for (let i = 1; i <= total; i++) {
            if (shouldShowPage(i, current, total)) {
                const li = createPaginationItem(i, i === current, () => {
                    loadStudents(i);
                });
                elements.pagination.appendChild(li);
            }
        }

        // Next button
        const nextLi = createPaginationItem('Next', current === total, () => {
            if (current < total) loadStudents(current + 1);
        });
        elements.pagination.appendChild(nextLi);
    }

    // Create pagination item
    function createPaginationItem(label, disabled, onClick) {
        const li = document.createElement('li');
        li.className = `page-item ${disabled ? 'disabled' : ''}`;
        li.innerHTML = `<a class="page-link" href="#">${label}</a>`;
        li.onclick = (e) => {
            e.preventDefault();
            if (!disabled) onClick();
        };
        return li;
    }

    // Determine if page should be shown
    function shouldShowPage(page, current, total) {
        return page === 1 || page === total || (page >= current - 1 && page <= current + 1);
    }

    // Search
    function search() {
        currentSearchTerm = elements.searchInput?.value.trim() || '';
        currentFilters = currentSearchTerm ? { searchTerm: currentSearchTerm } : {};
        currentPage = 1;
        loadStudents(1);
    }

    // Prepare add form
    function prepareAdd() {
        if (elements.modalTitle) elements.modalTitle.textContent = 'Add Student';
        if (elements.form) elements.form.reset();
        if (elements.studentId) elements.studentId.value = '';
        
        const modal = new bootstrap.Modal(elements.modal);
        modal.show();
    }

    // Edit student
    async function edit(id) {
        showLoading();
        
        try {
            const response = await StudentsApi.getById(id);
            
            if (response.success) {
                const student = response.data;
                fillForm(student);
                
                if (elements.modalTitle) elements.modalTitle.textContent = 'Edit Student';
                const modal = new bootstrap.Modal(elements.modal);
                modal.show();
            }
        } catch (error) {
            showToast('Error loading student: ' + error.message, 'danger');
        } finally {
            hideLoading();
        }
    }

    // Fill form with student data
    function fillForm(student) {
        if (elements.studentId) elements.studentId.value = student.id;
        if (elements.studentCode) elements.studentCode.value = student.studentCode;
        if (elements.fullName) elements.fullName.value = student.fullName;
        if (elements.dateOfBirth) elements.dateOfBirth.value = StudentsHelper.formatDateForInput(student.dateOfBirth);
        if (elements.gender) elements.gender.value = student.gender;
        if (elements.email) elements.email.value = student.email;
        if (elements.phoneNumber) elements.phoneNumber.value = student.phoneNumber;
        if (elements.address) elements.address.value = student.address || '';
        if (elements.classId) elements.classId.value = student.classId;
        if (elements.status) elements.status.value = student.status;
    }

    // Save student
    async function save() {
        const formData = {
            studentCode: elements.studentCode?.value.trim(),
            fullName: elements.fullName?.value.trim(),
            dateOfBirth: elements.dateOfBirth?.value,
            gender: elements.gender?.value,
            email: elements.email?.value.trim(),
            phoneNumber: elements.phoneNumber?.value.trim(),
            address: elements.address?.value.trim(),
            classId: parseInt(elements.classId?.value) || 0,
            status: elements.status?.value
        };

        // Validate
        const validation = StudentsHelper.validateForm(formData);
        if (!validation.isValid) {
            showToast(validation.errors.join(', '), 'warning');
            return;
        }

        showLoading();
        
        try {
            const id = elements.studentId?.value;
            const response = id
                ? await StudentsApi.update(parseInt(id), formData)
                : await StudentsApi.create(formData);

            if (response.success) {
                showToast(response.message, 'success');
                bootstrap.Modal.getInstance(elements.modal)?.hide();
                loadStudents(currentPage);
            } else {
                showToast(response.message, 'danger');
            }
        } catch (error) {
            showToast('Error saving student: ' + error.message, 'danger');
        } finally {
            hideLoading();
        }
    }

    // Delete student
    async function deleteStudent(id, name) {
        if (!confirm(`Are you sure you want to delete student "${name}"?`)) {
            return;
        }

        showLoading();
        
        try {
            const response = await StudentsApi.delete(id);
            
            if (response.success) {
                showToast(response.message, 'success');
                loadStudents(currentPage);
            } else {
                showToast(response.message, 'danger');
            }
        } catch (error) {
            showToast('Error deleting student: ' + error.message, 'danger');
        } finally {
            hideLoading();
        }
    }

    // Show loading
    function showLoading() {
        if (typeof Utils !== 'undefined' && Utils.showLoading) {
            Utils.showLoading();
        } else {
            document.body.style.cursor = 'wait';
        }
    }

    // Hide loading
    function hideLoading() {
        if (typeof Utils !== 'undefined' && Utils.hideLoading) {
            Utils.hideLoading();
        } else {
            document.body.style.cursor = 'default';
        }
    }

    // Show toast
    function showToast(message, type = 'info') {
        if (typeof Utils !== 'undefined' && Utils.showToast) {
            Utils.showToast(message, type);
        } else {
            alert(message);
        }
    }

    // Public API
    return {
        init,
        loadStudents,
        search,
        prepareAdd,
        edit,
        save,
        delete: deleteStudent
    };
})();

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Check if we're on a students page
    if (document.getElementById('studentsTable')) {
        StudentsUI.init();
    }
});
