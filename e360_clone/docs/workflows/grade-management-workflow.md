# Workflow: Grade Management Process

## Overview

Quy trình quản lý điểm thi từ khi nhập điểm đến công bố kết quả cho sinh viên.

## Actors

| Actor | Responsibilities |
|-------|------------------|
| **Giảng viên** | Chấm thi, nhập điểm, submit điểm |
| **Admin/Nhân viên giáo vụ** | Duyệt điểm, công bố điểm |
| **Sinh viên** | Xem điểm, khiếu nại (nếu có) |

## Grade Status Flow

```mermaid
graph LR
    A[Draft] --> B[Submitted]
    B --> C[Approved]
    C --> D[Published]
    B --> A
    C --> A
    D --> A
```

| Status | Description | Can Edit? | Who Can Edit |
|--------|-------------|-----------|--------------|
| Draft | Nháp, đang nhập | Yes | Lecturer |
| Submitted | Đã submit chờ duyệt | No | - |
| Approved | Đã duyệt | No | - |
| Published | Đã công bố | No | - |

---

## Phase 1: Chấm thi

### 1.1 Nhận bài thi

```mermaid
graph TD
    A[Bài thi từ phòng giáo vụ] --> B[Kiểm tra số lượng]
    B --> C{Complete?}
    C -->|No| D[Báo cáo thiếu bài]
    C -->|Yes| E[Nhận chấm thi]
```

**Checklist:**
- [ ] Số bài thi khớp với biên bản điểm danh
- [ ] Bài thi không rách nát, hư hỏng
- [ ] Đề thi kèm theo (nếu có)

---

### 1.2 Chấm bài

```mermaid
graph TD
    A[Nhận bài thi] --> B[Áp dụng đáp án]
    B --> C[Chấm từng bài]
    C --> D[Tổng hợp điểm]
    D --> E[Kiểm tra lại điểm]
    E --> F[Hoàn tất chấm]
```

**Best Practices:**
- Chấm theo rubric đã công bố
- Chấm 2 vòng để đảm bảo công bằng
- Đánh dấu các bài thi đặc biệt (điểm cao/thấp bất thường)

---

## Phase 2: Nhập điểm

### 2.1 Truy cập hệ thống

```mermaid
graph TD
    A[Giảng viên login] --> B[Vào menu Điểm thi]
    B --> C[Chọn môn thi]
    C --> D[Chọn ca thi]
    D --> E[Nhấn Nhập điểm]
```

---

### 2.2 Nhập điểm chi tiết

```mermaid
graph TD
    A[Form nhập điểm] --> B[Nhập điểm cho từng SV]
    B --> C{Score Valid?}
    C -->|No| D[Show Error]
    C -->|Yes| E[Auto-calculate Letter Grade]
    E --> F[Save to Draft]
    F --> G{More Students?}
    G -->|Yes| B
    G -->|No| H[Review All Scores]
```

**Data Entry Form:**

| Field | Type | Validation |
|-------|------|------------|
| Student | Display | Read-only |
| Score | Number | 0-10, 1 decimal |
| Letter Grade | Auto | Based on score |
| Notes | Text | Optional, max 500 chars |

**Grade Scale:**
```
A:  8.5 - 10.0  (Excellent)
B:  7.0 - 8.4   (Good)
C:  5.5 - 6.9   (Average)
D:  4.0 - 5.4   (Below Average)
F:  0.0 - 3.9   (Fail)
```

---

### 2.3 Kiểm tra điểm trước khi submit

```mermaid
graph TD
    A[Review Screen] --> B[Kiểm tra điểm null]
    B --> C[Kiểm tra điểm bất thường]
    C --> D[Xem thống kê sơ bộ]
    D --> E{All OK?}
    E -->|No| F[Edit Scores]
    E -->|Yes| G[Ready to Submit]
```

**Statistics to Review:**
- Average score (điểm trung bình)
- Standard deviation (độ lệch chuẩn)
- Grade distribution (phân bố điểm)
- Pass rate (tỷ lệ đậu)

**Red Flags:**
- Điểm trung bình < 4.0 hoặc > 9.0
- Độ lệch chuẩn > 3.0
- Tỷ lệ đậu < 50% hoặc = 100%

---

## Phase 3: Submit điểm

### 3.1 Submit để duyệt

```mermaid
graph TD
    A[Click Submit] --> B{All Scores Entered?}
    B -->|No| C[Show Missing Scores]
    B -->|Yes| D[Confirm Submission]
    D --> E[Update Status = Submitted]
    E --> F[Lock Grades]
    F --> G[Send Notification to Admin]
    G --> H[Show Confirmation]
```

**Validation Before Submit:**
```javascript
function validateSubmission(grades) {
  const errors = [];
  
  // Check for null scores
  const nullScores = grades.filter(g => g.score === null);
  if (nullScores.length > 0) {
    errors.push(`${nullScores.length} students have no score`);
  }
  
  // Check for invalid scores
  const invalidScores = grades.filter(g => g.score < 0 || g.score > 10);
  if (invalidScores.length > 0) {
    errors.push(`${invalidScores.length} scores out of range`);
  }
  
  // Check for unusual distribution
  const avg = calculateAverage(grades);
  if (avg < 3 || avg > 9) {
    errors.push(`Unusual average: ${avg}. Please review.`);
  }
  
  return errors;
}
```

---

### 3.2 Email thông báo

**Template: Submission Notification**
```
Subject: Điểm thi cần duyệt - [Môn thi] - [Lớp]

Kính gửi Phòng Giáo vụ,

Giảng viên: [Tên giảng viên]
Môn thi: [Tên môn]
Lớp: [Tên lớp]
Số sinh viên: [Số lượng]
Thời gian submit: [DD/MM/YYYY HH:mm]

Điểm đã được nhập và chờ duyệt.
Vui lòng kiểm tra và duyệt điểm.

Trân trọng,
Hệ thống E360
```

---

## Phase 4: Duyệt điểm

### 4.1 Admin kiểm tra điểm

```mermaid
graph TD
    A[Admin nhận thông báo] --> B[Xem danh sách điểm chờ duyệt]
    B --> C[Chọn môn thi cần duyệt]
    C --> D[Xem chi tiết điểm]
    D --> E[Kiểm tra thống kê]
    E --> F{Valid?}
    F -->|No| G[Return to Lecturer]
    F -->|Yes| H[Approve Grades]
```

**Review Checklist:**
- [ ] Tất cả sinh viên đã có điểm
- [ ] Điểm trong khoảng hợp lệ (0-10)
- [ ] Điểm chữ khớp với điểm số
- [ ] Thống kê điểm bình thường
- [ ] Không có khiếu nại từ sinh viên

---

### 4.2 Duyệt điểm

```mermaid
graph TD
    A[Click Approve] --> B[Enter Approver Info]
    B --> C[Update Status = Approved]
    C --> D[Record Approval Timestamp]
    D --> E[Generate Grade Report]
    E --> F[Ready to Publish]
```

**Approval Record:**
```json
{
  "gradeId": 123,
  "approvedBy": "admin@e360.edu.vn",
  "approvedAt": "2024-06-15T10:30:00Z",
  "previousStatus": "Submitted",
  "newStatus": "Approved"
}
```

---

### 4.3 Từ chối điểm (Return to Lecturer)

```mermaid
graph TD
    A[Click Return] --> B[Enter Reason]
    B --> C[Update Status = Draft]
    C --> D[Notify Lecturer]
    D --> E[Lecturer Reviews]
    E --> F[Edit Scores]
    F --> G[Resubmit]
```

**Template: Return Notification**
```
Subject: Điểm thi cần chỉnh sửa - [Môn thi]

Kính gửi Giảng viên [Tên],

Điểm thi môn [Tên môn] - Lớp [Tên lớp] cần chỉnh sửa:

Lý do: [Chi tiết lý do]

Vui lòng kiểm tra và cập nhật lại điểm.

Trân trọng,
Phòng Giáo vụ
```

---

## Phase 5: Công bố điểm

### 5.1 Chuẩn bị công bố

```mermaid
graph TD
    A[Grades Approved] --> B[Set Publish Date]
    B --> C[Prepare Notifications]
    C --> D[Final Review]
    D --> E[Ready to Publish]
```

**Publishing Rules:**
- Điểm được công bố theo lịch quy định
- Thông báo trước 24 giờ
- Sinh viên được xem điểm trong bao lâu

---

### 5.2 Công bố điểm

```mermaid
graph TD
    A[Click Publish] --> B[Update Status = Published]
    B --> C[Make Visible to Students]
    C --> D[Send Email Notifications]
    D --> E[Generate Transcripts]
    E --> F[Publish Complete]
```

**Email Template: Grade Published**
```
Subject: Điểm thi đã công bố - Học kỳ 2/2024

Kính chào sinh viên,

Điểm thi học kỳ 2 đã được công bố.

Xem điểm tại: [Link to portal]

Thời gian khiếu nại: 7 ngày từ hôm nay.

Trân trọng,
Phòng Giáo vụ
```

---

## Phase 6: Khiếu nại điểm (Nếu có)

### 6.1 Sinh viên khiếu nại

```mermaid
graph TD
    A[Sinh viên xem điểm] --> B{Disagree?}
    B -->|Yes| C[Submit Appeal]
    B -->|No| D[End]
    C --> E[Enter Reason]
    E --> F[Attach Evidence (optional)]
    F --> G[Submit to System]
    G --> H[Notify Admin & Lecturer]
```

**Appeal Form:**
- Môn thi
- Điểm hiện tại
- Lý do khiếu nại
- Bằng chứng đính kèm (optional)

**Valid Reasons:**
- Điểm không khớp với bài làm
- Tổng hợp điểm sai
- Thiếu điểm thành phần

**Invalid Reasons:**
- Không đồng ý với đáp án
- Xin điểm không có căn cứ

---

### 6.2 Xử lý khiếu nại

```mermaid
graph TD
    A[Nhận khiếu nại] --> B[Admin xem xét]
    B --> C{Valid?}
    C -->|No| D[Reject Appeal]
    C -->|Yes| E[Request Re-grade]
    E --> F[Lecturer Reviews]
    F --> G[Submit New Score]
    G --> H[Admin Approves]
    H --> I[Update Grade]
    I --> J[Notify Student]
```

**Timeline:**
- Sinh viên khiếu nại: Trong vòng 7 ngày
- Admin xử lý: Trong vòng 3 ngày
- Giảng viên chấm lại: Trong vòng 5 ngày
- Công bố kết quả: Trong vòng 2 ngày

---

## Phase 7: Lưu trữ và báo cáo

### 7.1 Xuất báo cáo

```mermaid
graph TD
    A[Select Report Type] --> B[Set Filters]
    B --> C[Generate Report]
    C --> D[Preview]
    D --> E{OK?}
    E -->|No| B
    E -->|Yes| F[Export PDF/Excel]
    F --> G[Print/Save]
```

**Available Reports:**
1. Bảng điểm môn thi
2. Thống kê điểm theo lớp
3. Thống kê điểm theo môn
4. Học bạ sinh viên
5. Báo cáo tổng kết kỳ thi

---

### 7.2 Lưu trữ hồ sơ

**Retention Policy:**
| Document | Retention Period |
|----------|------------------|
| Bài thi | 1 năm |
| Bảng điểm | 5 năm |
| Biên bản điểm danh | 1 năm |
| Khiếu nại điểm | 1 năm sau khi xử lý |

---

## API Integration Points

| Endpoint | Phase | Description |
|----------|-------|-------------|
| POST /api/grades | 2.2 | Create grade entry |
| PUT /api/grades/{id} | 2.2 | Update grade |
| POST /api/grades/submit | 3.1 | Submit for approval |
| PUT /api/grades/{id}/approve | 4.2 | Approve grade |
| PUT /api/grades/{id}/publish | 5.2 | Publish grade |
| POST /api/grades/appeal | 6.1 | Submit appeal |
| GET /api/grades/transcript | 7.1 | Generate transcript |

---

## KPIs & Metrics

| Metric | Target | Formula |
|--------|--------|---------|
| Thời gian nhập điểm | < 7 ngày | Days from exam to submission |
| Thời gian duyệt điểm | < 3 ngày | Days from submission to approval |
| Tỷ lệ khiếu nại | < 5% | Appeals / Total grades |
| Tỷ lệ đậu | 80-95% | Passed / Total students |
| Điểm trung bình | 6.0-8.0 | Average score |

---

## Audit Trail

All grade changes are logged:

```json
{
  "gradeId": 123,
  "action": "UPDATE",
  "oldValue": { "score": 7.5, "letterGrade": "B" },
  "newValue": { "score": 8.5, "letterGrade": "A" },
  "changedBy": "lecturer@e360.edu.vn",
  "changedAt": "2024-06-10T14:30:00Z",
  "reason": "Re-grade request"
}
```
