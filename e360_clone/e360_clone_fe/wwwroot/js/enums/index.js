/**
 * Index file - Import all enums
 * Usage: import { Role, Gender, StudentStatus } from '../enums';
 */

// Account
const AccountStatus = window.AccountStatus;
const AccountStatusHelper = window.AccountStatusHelper;

// Role
const Role = window.Role;
const RoleHelper = window.RoleHelper;

// Student
const StudentStatus = window.StudentStatus;
const StudentStatusHelper = window.StudentStatusHelper;

// Gender
const Gender = window.Gender;
const GenderHelper = window.GenderHelper;

// Exam
const ExamStatus = window.ExamStatus;
const ExamStatusHelper = window.ExamStatusHelper;

// Grade
const GradeType = window.GradeType;
const GradeTypeHelper = window.GradeTypeHelper;

// Attendance
const AttendanceStatus = window.AttendanceStatus;
const AttendanceStatusHelper = window.AttendanceStatusHelper;

// Export
window.Enums = {
    AccountStatus,
    AccountStatusHelper,
    Role,
    RoleHelper,
    StudentStatus,
    StudentStatusHelper,
    Gender,
    GenderHelper,
    ExamStatus,
    ExamStatusHelper,
    GradeType,
    GradeTypeHelper,
    AttendanceStatus,
    AttendanceStatusHelper
};
