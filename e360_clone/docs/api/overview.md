# API Documentation Overview

## Base URL

```
Development: http://localhost:5104/api
Production: https://your-domain.com/api
```

## Authentication

All API endpoints require authentication using JWT Bearer tokens.

```
Authorization: Bearer <your-jwt-token>
```

## Response Format

All responses follow a consistent format:

### Success Response

```json
{
  "success": true,
  "message": "Thành công",
  "data": { ... }
}
```

### Paged Response

```json
{
  "success": true,
  "message": "Lấy danh sách thành công",
  "data": [ ... ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 100,
  "totalPages": 10
}
```

### Error Response

```json
{
  "success": false,
  "message": "Có lỗi xảy ra",
  "data": null
}
```

### Validation Error Response

```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "errors": {
    "studentCode": ["Mã sinh viên đã tồn tại"],
    "email": ["Email không hợp lệ"]
  }
}
```

---

## HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Missing/invalid token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource already exists |
| 500 | Internal Server Error |

---

## Modules

### 1. Students API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /students | Get all students (paged) |
| GET | /students/{id} | Get student by ID |
| POST | /students | Create student |
| PUT | /students/{id} | Update student |
| DELETE | /students/{id} | Delete student |

### 2. Lecturers API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /lecturers | Get all lecturers |
| GET | /lecturers/{id} | Get lecturer by ID |
| POST | /lecturers | Create lecturer |
| PUT | /lecturers/{id} | Update lecturer |
| DELETE | /lecturers/{id} | Delete lecturer |

### 3. Subjects API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /subjects | Get all subjects |
| GET | /subjects/{id} | Get subject by ID |
| POST | /subjects | Create subject |
| PUT | /subjects/{id} | Update subject |
| DELETE | /subjects/{id} | Delete subject |

### 4. Exams API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /exams | Get all exams |
| GET | /exams/{id} | Get exam by ID |
| POST | /exams | Create exam |
| PUT | /exams/{id} | Update exam |
| DELETE | /exams/{id} | Delete exam |

### 5. Exam Schedules API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /exam-schedules | Get all schedules |
| GET | /exam-schedules/{id} | Get schedule by ID |
| POST | /exam-schedules | Create schedule |
| PUT | /exam-schedules/{id} | Update schedule |
| DELETE | /exam-schedules/{id} | Cancel schedule |

### 6. Proctor Assignments API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /proctor-assignments | Get all assignments |
| POST | /proctor-assignments | Create assignment |
| PUT | /proctor-assignments/{id}/confirm | Confirm assignment |
| PUT | /proctor-assignments/{id}/decline | Decline assignment |

### 7. Grades API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /grades | Get all grades |
| POST | /grades | Create grade |
| PUT | /grades/{id} | Update grade |
| POST | /grades/submit | Submit grades |
| PUT | /grades/{id}/approve | Approve grade |
| PUT | /grades/{id}/publish | Publish grade |

### 8. Attendance API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /attendance | Get all attendance records |
| POST | /attendance | Create attendance |
| POST | /attendance/checkin | QR check-in |
| PUT | /attendance/{id} | Update attendance |

---

## Rate Limiting

| Tier | Requests/minute |
|------|-----------------|
| Standard | 100 |
| Premium | 500 |
| Admin | 1000 |

Rate limit headers:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1625140800
```

---

## Versioning

API version is included in the Accept header:

```
Accept: application/vnd.e360.v1+json
```

---

## CORS

Allowed origins:
- http://localhost:5000
- https://localhost:5001
- http://localhost:3000

---

## Swagger UI

Available at: `http://localhost:5104/swagger`

---

## Example Request/Response

### Create Student

**Request:**
```http
POST /api/students
Content-Type: application/json
Authorization: Bearer <token>

{
  "studentCode": "SV001",
  "fullName": "Nguyễn Văn A",
  "dateOfBirth": "2000-01-15",
  "gender": "Nam",
  "email": "sv001@e360.edu.vn",
  "phoneNumber": "0901234567",
  "address": "Hà Nội",
  "classId": 1,
  "status": "Active"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Thêm sinh viên thành công",
  "data": {
    "id": 1,
    "studentCode": "SV001",
    "fullName": "Nguyễn Văn A",
    "dateOfBirth": "2000-01-15",
    "gender": "Nam",
    "email": "sv001@e360.edu.vn",
    "phoneNumber": "0901234567",
    "address": "Hà Nội",
    "classId": 1,
    "status": "Active",
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

---

## Error Handling

### Client Errors

```json
{
  "success": false,
  "message": "Mã sinh viên đã tồn tại",
  "errorCode": "STUDENT_CODE_EXISTS"
}
```

### Server Errors

```json
{
  "success": false,
  "message": "Lỗi máy chủ nội bộ",
  "errorCode": "INTERNAL_ERROR",
  "traceId": "abc123xyz"
}
```

---

## Webhooks

Configure webhooks to receive notifications:

| Event | Payload |
|-------|---------|
| grade.published | { examId, studentId, grade } |
| schedule.created | { scheduleId, examId, date } |
| assignment.created | { assignmentId, lecturerId, scheduleId } |

Webhook endpoint configuration:
```json
{
  "url": "https://your-server.com/webhook",
  "events": ["grade.published", "schedule.created"],
  "secret": "your-secret-key"
}
```
