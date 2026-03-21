-- =====================================================
-- E360 Clone - Fix Account Passwords
-- =====================================================
-- Password: 123456
-- SHA256 Hash (Base64): jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=
-- =====================================================

-- Update all account passwords to correct SHA256 hash
UPDATE "Accounts" 
SET "PasswordHash" = 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI='
WHERE "PasswordHash" = '2jw2Xp3U6Ltf1OQ4jGWxqzPU53iHw9TfjGwZ0elGt+U=';

-- Verify
SELECT "Username", "Email", "Role", 
       CASE 
           WHEN "PasswordHash" = 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=' 
           THEN '✓ Correct' 
           ELSE '✗ Wrong' 
       END AS "PasswordStatus"
FROM "Accounts"
ORDER BY "Id";
