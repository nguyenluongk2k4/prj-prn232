-- =====================================================
-- E360 Clone - Seed Accounts Data
-- =====================================================
-- Run this in Supabase SQL Editor or pgAdmin
-- Password for all accounts: 123456
-- =====================================================

-- SHA256 hash of "123456" in Base64
-- Hash: 2jw2Xp3U6Ltf1OQ4jGWxqzPU53iHw9TfjGwZ0elGt+U=

-- Insert Super Admin
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('superadmin', 'superadmin@e360.com', '2jw2Xp3U6Ltf1OQ4jGWxqzPU53iHw9TfjGwZ0elGt+U=', 'SuperAdmin', 'Super Administrator', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Admin
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('admin', 'admin@e360.com', '2jw2Xp3U6Ltf1OQ4jGWxqzPU53iHw9TfjGwZ0elGt+U=', 'Admin', 'System Administrator', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Student
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('student', 'student@e360.com', '2jw2Xp3U6Ltf1OQ4jGWxqzPU53iHw9TfjGwZ0elGt+U=', 'Student', 'Nguyen Van Student', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Teacher
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('teacher', 'teacher@e360.com', '2jw2Xp3U6Ltf1OQ4jGWxqzPU53iHw9TfjGwZ0elGt+U=', 'Teacher', 'Tran Van Teacher', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Parent
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('parent', 'parent@e360.com', '2jw2Xp3U6Ltf1OQ4jGWxqzPU53iHw9TfjGwZ0elGt+U=', 'Parent', 'Le Van Parent', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Librarian
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('librarian', 'librarian@e360.com', '2jw2Xp3U6Ltf1OQ4jGWxqzPU53iHw9TfjGwZ0elGt+U=', 'Librarian', 'Pham Van Librarian', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- =====================================================
-- Verify inserted data
-- =====================================================
SELECT 
    "Id", 
    "Username", 
    "Email", 
    "Role", 
    "FullName", 
    "Status", 
    "CreatedAt" 
FROM "Accounts" 
ORDER BY "Id";

-- =====================================================
-- Count total accounts
-- =====================================================
SELECT COUNT(*) AS "Total Accounts" FROM "Accounts";
