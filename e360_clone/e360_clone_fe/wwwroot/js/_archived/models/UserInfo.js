/**
 * User Info Model
 * For storing authenticated user info in localStorage
 */

class UserInfo {
    constructor(data = {}) {
        this.username = data.username ?? '';
        this.email = data.email ?? '';
        this.fullName = data.fullName ?? '';
        this.role = data.role ?? '';
        this.avatarUrl = data.avatarUrl ?? '';
        this.userId = data.userId ?? 0;
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

    get isAdmin() {
        return ['Admin', 'SuperAdmin'].includes(this.role);
    }

    get isStudent() {
        return this.role === 'Student';
    }

    get isTeacher() {
        return this.role === 'Teacher';
    }

    static create(data) {
        return new UserInfo(data);
    }

    static fromJwtResponse(data) {
        return UserInfo.create({
            username: data.username,
            email: data.email,
            fullName: data.fullName,
            role: data.role,
            avatarUrl: data.avatarUrl,
            userId: data.userId
        });
    }

    toStorage() {
        return JSON.stringify(this);
    }

    static fromStorage(storageString) {
        try {
            const data = JSON.parse(storageString);
            return UserInfo.create(data);
        } catch {
            return null;
        }
    }
}

window.UserInfo = UserInfo;
