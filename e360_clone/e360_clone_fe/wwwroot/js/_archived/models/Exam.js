/**
 * Exam Model
 * Maps to database Exam entity
 */

class Exam {
    constructor(data = {}) {
        this.id = data.id ?? 0;
        this.examCode = data.examCode ?? '';
        this.examName = data.examName ?? '';
        this.examType = data.examType ?? '';
        this.subjectId = data.subjectId ?? 0;
        this.subjectName = data.subjectName ?? '';
        this.classId = data.classId ?? 0;
        this.className = data.className ?? '';
        this.examDate = data.examDate ?? null;
        this.startTime = data.startTime ?? null;
        this.endTime = data.endTime ?? null;
        this.duration = data.duration ?? 0;
        this.roomId = data.roomId ?? 0;
        this.roomName = data.roomName ?? '';
        this.academicYear = data.academicYear ?? '';
        this.semester = data.semester ?? '';
        this.status = data.status ?? '';
        this.createdAt = data.createdAt ?? null;
        this.updatedAt = data.updatedAt ?? null;
        
        // Computed properties (from mappers)
        this.formattedDate = '';
        this.formattedTime = '';
        this.statusText = '';
        this.statusBadge = '';
    }

    get isUpcoming() {
        return ['Planned', 'Scheduled'].includes(this.status);
    }

    get isCompleted() {
        return this.status === 'Completed';
    }

    get isInProgress() {
        return this.status === 'InProgress';
    }

    get dateTimeString() {
        if (!this.examDate) return '';
        const date = new Date(this.examDate).toLocaleDateString('vi-VN');
        const time = this.startTime ? this.startTime.substring(0, 5) : '';
        return `${date} ${time}`;
    }

    static create(data) {
        return new Exam(data);
    }

    static fromApi(data) {
        return Exam.create(data);
    }

    static fromApiList(dataList) {
        return dataList.map(data => Exam.fromApi(data));
    }
}

window.Exam = Exam;
