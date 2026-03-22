-- Update existing class codes from old format to FPT University format
UPDATE "Classes" SET "ClassCode" = 'SE1801', "ClassName" = 'Software Engineering K18 - SE1801', "Cohort" = 18, "CohortYear" = 2018
WHERE "ClassCode" = 'LopCNTT2020A';

UPDATE "Classes" SET "ClassCode" = 'SE1802', "ClassName" = 'Software Engineering K18 - SE1802', "Cohort" = 18, "CohortYear" = 2018
WHERE "ClassCode" = 'LopCNTT2020B';

UPDATE "Classes" SET "ClassCode" = 'AI1801', "ClassName" = 'Artificial Intelligence K18 - AI1801', "Cohort" = 18, "CohortYear" = 2018
WHERE "ClassCode" = 'LopCNTT2020C';

-- Insert remaining sample classes (idempotent on ClassCode)
INSERT INTO "Classes" ("ClassCode", "ClassName", "MajorId", "CourseId", "AcademicYear", "Cohort", "CohortYear", "Semester", "StudentCount", "Status", "CreatedAt")
VALUES
  -- Technology
  ('IA1801', 'Information Assurance K18 - IA1801', 1, 1, '2018-2022', 18, 2018, 1, 24, 'Active', NOW()),
  ('SWD1801', 'Software Development K18 - SWD1801', 1, 1, '2018-2022', 18, 2018, 1, 26, 'Active', NOW()),
  ('PRJ1801', 'Project K18 - PRJ1801', 1, 1, '2018-2022', 18, 2018, 1, 20, 'Active', NOW()),

  -- Business
  ('BUS1801', 'Business K18 - BUS1801', 1, 1, '2018-2022', 18, 2018, 1, 32, 'Active', NOW()),
  ('MKT1801', 'Marketing K18 - MKT1801', 1, 1, '2018-2022', 18, 2018, 1, 30, 'Active', NOW()),
  ('FIN1801', 'Finance K18 - FIN1801', 1, 1, '2018-2022', 18, 2018, 1, 27, 'Active', NOW()),
  ('ACC1801', 'Accounting K18 - ACC1801', 1, 1, '2018-2022', 18, 2018, 1, 29, 'Active', NOW()),

  -- Language
  ('EN1801', 'English K18 - EN1801', 1, 1, '2018-2022', 18, 2018, 1, 35, 'Active', NOW()),
  ('JPN1801', 'Japanese K18 - JPN1801', 1, 1, '2018-2022', 18, 2018, 1, 28, 'Active', NOW()),
  ('KOR1801', 'Korean K18 - KOR1801', 1, 1, '2018-2022', 18, 2018, 1, 26, 'Active', NOW()),
  ('CHI1801', 'Chinese K18 - CHI1801', 1, 1, '2018-2022', 18, 2018, 1, 26, 'Active', NOW()),

  -- Design
  ('GD1801', 'Graphic Design K18 - GD1801', 1, 1, '2018-2022', 18, 2018, 1, 22, 'Active', NOW()),
  ('MM1801', 'Multimedia Communication K18 - MM1801', 1, 1, '2018-2022', 18, 2018, 1, 24, 'Active', NOW())
ON CONFLICT ("ClassCode") DO UPDATE
SET "ClassName" = EXCLUDED."ClassName",
    "AcademicYear" = EXCLUDED."AcademicYear",
    "Cohort" = EXCLUDED."Cohort",
    "CohortYear" = EXCLUDED."CohortYear",
    "Semester" = EXCLUDED."Semester",
    "StudentCount" = EXCLUDED."StudentCount",
    "Status" = EXCLUDED."Status";

-- Update MajorId based on MajorCode prefixes
UPDATE "Classes" c
SET "MajorId" = m."Id"
FROM "Majors" m
WHERE c."ClassCode" LIKE m."MajorCode" || '%';
