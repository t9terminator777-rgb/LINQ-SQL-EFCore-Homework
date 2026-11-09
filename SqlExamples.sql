-- Учебные SQL-запросы для таблиц, которые создаёт EF Core.
-- SELECT + WHERE + ORDER BY: справки дороже 150, сначала самые дорогие.
SELECT "Name", "Price"
FROM "SpravkaTypes"
WHERE "Price" > 150
ORDER BY "Price" DESC;

-- JOIN: справка вместе с именем пациента и видом справки.
SELECT s."Id", p."Name" AS "Patient", t."Name" AS "Spravka", t."Price"
FROM "Spravki" AS s
JOIN "Patients" AS p ON s."PatientId" = p."Id"
JOIN "SpravkaTypes" AS t ON s."SpravkaTypeId" = t."Id";

-- INSERT: пример добавления пациента. Email должен быть уникальным.
INSERT INTO "Patients" ("Name", "BirthDate", "Phone", "Email", "BloodType")
VALUES ('Тестовый пациент', '2001-01-01 00:00:00', '+996700000000', 'sql-demo@example.com', 'B+');

-- UPDATE: изменяет цену только у выбранного вида справки.
UPDATE "SpravkaTypes" SET "Price" = 250 WHERE "Id" = 3;

-- DELETE: удаляет только запись на приём с указанным Id.
DELETE FROM "Appointments" WHERE "Id" = 9999;
