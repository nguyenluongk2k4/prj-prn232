/**
 * Student Model
 * Maps to database Student entity
 */

class Student {
    constructor(data = {}) {
        this.id = data.id ?? 0;
        this.studentCode = data.studentCode ?? '';
        this.fullName = data.fullName ?? '';
        this.dateOfBirth = data.dateOfBirth ?? null;
        this.gender = data.gender ?? '';
        this.email = data.email ?? '';
        this.phoneNumber = data.phoneNumber ?? '';
        this.address = data.address ?? '';
        this.classId = data.classId ?? 0;
        this.className = data.className ?? '';
        this.status = data.status ?? '';
        this.avatarUrl = data.avatarUrl ?? '';
        this.createdAt = data.createdAt ?? null;
        this.updatedAt = data.updatedAt ?? null;
        
        // Computed properties (from mappers)
        this.formattedDateOfBirth = '';
        this.statusText = '';
        this.statusBadge = '';
        this.genderText = '';
        this.genderIcon = '';
    }

    get isActive() {
        return this.status === 'Active';
    }

    get isStudying() {
        return ['Active', 'Inactive'].includes(this.status);
    }

    get fullNameUpperCase() {
        return this.fullName.toUpperCase();
    }

    get initials() {
        return this.fullName
            .split(' ')
            .map(word => word[0])
            .join('')
            .toUpperCase()
            .slice(0, 2);
    }

    get age() {
        if (!this.dateOfBirth) return 0;
        const today = new Date();
        const birthDate = new Date(this.dateOfBirth);
        let age = today.getFullYear() - birthDate.getFullYear();
        const monthDiff = today.getMonth() - birthDate.getMonth();
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
            age--;
        }
        return age;
    }

    static create(data) {
        return new Student(data);
    }

    static fromApi(data) {
        return Student.create(data);
    }

    static fromApiList(dataList) {
        return dataList.map(data => Student.fromApi(data));
    }

    toUpdateDto() {
        return {
            id: this.id,
            studentCode: this.studentCode,
            fullName: this.fullName,
            dateOfBirth: this.dateOfBirth,
            gender: this.gender,
            email: this.email,
            phoneNumber: this.phoneNumber,
            address: this.address,
            classId: this.classId,
            status: this.status
        };
    }

    toCreateDto() {
        return {
            studentCode: this.studentCode,
            fullName: this.fullName,
            dateOfBirth: this.dateOfBirth,
            gender: this.gender,
            email: this.email,
            phoneNumber: this.phoneNumber,
            address: this.address,
            classId: this.classId,
            status: this.status
        };
    }
}

window.Student = Student;
