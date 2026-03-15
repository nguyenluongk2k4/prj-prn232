// Student UI Module
const StudentUI = {
    currentPage: 1,
    pageSize: 10,
    currentSearchTerm: '',

    init() {
        this.loadStudents();
        this.bindEvents();
    },

    bindEvents() {
        document.getElementById('searchInput')?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.search();
        });
    },

    async loadStudents(page = 1) {
        showLoading();
        try {
            const response = await StudentApi.getAll(page, this.pageSize, this.currentSearchTerm);
            if (response.success) {
                this.renderTable(response.data);
                this.renderPagination(response.pageNumber, response.totalPages);
            } else {
                showToast(response.message, 'danger');
            }
        } catch (error) {
            showToast('Không thể tải dữ liệu: ' + error.message, 'danger');
        } finally {
            hideLoading();
        }
    },

    renderTable(students) {
        const tbody = document.querySelector('#studentsTable tbody');
        if (!tbody) return;

        if (students.length === 0) {
            tbody.innerHTML = '<tr><td colspan="8" class="text-center">Không có dữ liệu</td></tr>';
            return;
        }

        tbody.innerHTML = students.map(s => `
            <tr>
                <td><strong>${s.studentCode}</strong></td>
                <td>${s.fullName}</td>
                <td>${formatDate(s.dateOfBirth)}</td>
                <td>${s.gender}</td>
                <td>${s.email}</td>
                <td>${s.phoneNumber}</td>
                <td><span class="badge bg-${this.getStatusBadge(s.status)}">${this.getStatusText(s.status)}</span></td>
                <td>
                    <button class="btn btn-sm btn-outline-primary" onclick="StudentUI.edit(${s.id})">Sửa</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="StudentUI.delete(${s.id}, '${s.fullName}')">Xóa</button>
                </td>
            </tr>
        `).join('');
    },

    getStatusBadge(status) {
        return { Active: 'success', Inactive: 'secondary', Graduated: 'info' }[status] || 'primary';
    },

    getStatusText(status) {
        return { Active: 'Đang học', Inactive: 'Tạm ngưng', Graduated: 'Tốt nghiệp' }[status] || status;
    },

    renderPagination(current, total) {
        const pagination = document.getElementById('pagination');
        if (!pagination) return;

        pagination.innerHTML = '';
        
        const prevLi = document.createElement('li');
        prevLi.className = `page-item ${current === 1 ? 'disabled' : ''}`;
        prevLi.innerHTML = `<a class="page-link" href="#">Trước</a>`;
        prevLi.onclick = (e) => { e.preventDefault(); if (current > 1) this.loadStudents(current - 1); };
        pagination.appendChild(prevLi);

        for (let i = 1; i <= total; i++) {
            if (i === 1 || i === total || (i >= current - 1 && i <= current + 1)) {
                const li = document.createElement('li');
                li.className = `page-item ${i === current ? 'active' : ''}`;
                li.innerHTML = `<a class="page-link" href="#">${i}</a>`;
                li.onclick = (e) => { e.preventDefault(); this.loadStudents(i); };
                pagination.appendChild(li);
            }
        }

        const nextLi = document.createElement('li');
        nextLi.className = `page-item ${current === total ? 'disabled' : ''}`;
        nextLi.innerHTML = `<a class="page-link" href="#">Sau</a>`;
        nextLi.onclick = (e) => { e.preventDefault(); if (current < total) this.loadStudents(current + 1); };
        pagination.appendChild(nextLi);
    },

    search() {
        this.currentSearchTerm = document.getElementById('searchInput').value.trim();
        this.currentPage = 1;
        this.loadStudents(1);
    },

    prepareAdd() {
        document.getElementById('modalTitle').textContent = 'Thêm sinh viên';
        document.getElementById('studentForm').reset();
        document.getElementById('studentId').value = '';
    },

    async edit(id) {
        showLoading();
        try {
            const response = await StudentApi.getById(id);
            if (response.success) {
                const s = response.data;
                document.getElementById('modalTitle').textContent = 'Sửa sinh viên';
                document.getElementById('studentId').value = s.id;
                document.getElementById('studentCode').value = s.studentCode;
                document.getElementById('fullName').value = s.fullName;
                document.getElementById('dateOfBirth').value = formatDateForInput(s.dateOfBirth);
                document.getElementById('gender').value = s.gender;
                document.getElementById('email').value = s.email;
                document.getElementById('phoneNumber').value = s.phoneNumber;
                document.getElementById('address').value = s.address || '';
                document.getElementById('classId').value = s.classId;
                document.getElementById('status').value = s.status;
                new bootstrap.Modal(document.getElementById('studentModal')).show();
            }
        } catch (error) {
            showToast('Lỗi: ' + error.message, 'danger');
        } finally {
            hideLoading();
        }
    },

    async save() {
        const student = {
            studentCode: document.getElementById('studentCode').value.trim(),
            fullName: document.getElementById('fullName').value.trim(),
            dateOfBirth: document.getElementById('dateOfBirth').value,
            gender: document.getElementById('gender').value,
            email: document.getElementById('email').value.trim(),
            phoneNumber: document.getElementById('phoneNumber').value.trim(),
            address: document.getElementById('address').value.trim(),
            classId: parseInt(document.getElementById('classId').value) || 0,
            status: document.getElementById('status').value
        };

        if (!student.studentCode || !student.fullName || !student.dateOfBirth || !student.email) {
            showToast('Vui lòng điền đầy đủ thông tin bắt buộc', 'warning');
            return;
        }

        showLoading();
        try {
            const id = document.getElementById('studentId').value;
            const response = id 
                ? await StudentApi.update(parseInt(id), student)
                : await StudentApi.create(student);

            if (response.success) {
                showToast(response.message, 'success');
                bootstrap.Modal.getInstance(document.getElementById('studentModal'))?.hide();
                this.loadStudents(this.currentPage);
            } else {
                showToast(response.message, 'danger');
            }
        } catch (error) {
            showToast('Lỗi: ' + error.message, 'danger');
        } finally {
            hideLoading();
        }
    },

    async delete(id, name) {
        if (!confirm(`Xóa sinh viên "${name}"?`)) return;

        showLoading();
        try {
            const response = await StudentApi.delete(id);
            if (response.success) {
                showToast(response.message, 'success');
                this.loadStudents(this.currentPage);
            }
        } catch (error) {
            showToast('Lỗi: ' + error.message, 'danger');
        } finally {
            hideLoading();
        }
    }
};
