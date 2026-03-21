-- =====================================================
-- E360 Clone - Seed Accounts Data
-- =====================================================
-- Run this in Supabase SQL Editor or pgAdmin
-- Password for all accounts: 123456
-- SHA256 Hash (Base64): jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=
-- =====================================================

-- Insert Super Admin
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('superadmin', 'superadmin@e360.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'SuperAdmin', 'Super Administrator', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Admin
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('admin', 'admin@e360.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Admin', 'System Administrator', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Student
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('student', 'student@e360.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Student', 'Nguyen Van Student', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Teacher
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('teacher', 'teacher@e360.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Teacher', 'Tran Van Teacher', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Parent
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('parent', 'parent@e360.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Parent', 'Le Van Parent', 'Active', NOW())
ON CONFLICT ("Username") DO NOTHING;

-- Insert Librarian
INSERT INTO "Accounts" ("Username", "Email", "PasswordHash", "Role", "FullName", "Status", "CreatedAt")
VALUES ('librarian', 'librarian@e360.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Librarian', 'Pham Van Librarian', 'Active', NOW())
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
