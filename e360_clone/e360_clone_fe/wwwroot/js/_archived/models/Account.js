/**
 * Account Model
 * Maps to database Account entity
 */

class Account {
    constructor(data = {}) {
        this.id = data.id ?? 0;
        this.username = data.username ?? '';
        this.email = data.email ?? '';
        this.passwordHash = data.passwordHash ?? '';
        this.role = data.role ?? '';
        this.fullName = data.fullName ?? '';
        this.phoneNumber = data.phoneNumber ?? '';
        this.avatarUrl = data.avatarUrl ?? '';
        this.status = data.status ?? '';
        this.studentId = data.studentId ?? null;
        this.lecturerId = data.lecturerId ?? null;
        this.createdAt = data.createdAt ?? null;
        this.lastLoginAt = data.lastLoginAt ?? null;
        this.updatedAt = data.updatedAt ?? null;
        
        // Computed properties (from mappers)
        this.roleText = '';
        this.roleIcon = '';
        this.statusText = '';
        this.statusBadge = '';
    }

    get isActive() {
        return this.status === 'Active';
    }

    get isAdmin() {
        return ['Admin', 'SuperAdmin'].includes(this.role);
    }

    get isStudent() {
        return this.role === 'Student';
    }

    get isTeacher() {
        return this.role === 'Teacher';
    }

    get displayName() {
        return this.fullName || this.username || this.email;
    }

    get initials() {
        return this.fullName
            .split(' ')
            .map(word => word[0])
            .join('')
            .toUpperCase()
            .slice(0, 2);
    }

    static create(data) {
        return new Account(data);
    }

    static fromApi(data) {
        return Account.create(data);
    }

    static fromApiList(dataList) {
        return dataList.map(data => Account.fromApi(data));
    }

    toLoginDto() {
        return {
            email: this.email,
            password: '' // Password is not stored in model
        };
    }

    toUpdateDto() {
        return {
            id: this.id,
            fullName: this.fullName,
            phoneNumber: this.phoneNumber,
            avatarUrl: this.avatarUrl,
            status: this.status
        };
    }
}

window.Account = Account;
