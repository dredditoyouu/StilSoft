SET NOCOUNT ON;
GO

INSERT INTO dbo.IncidentStatuses (Name, ColorHex)
VALUES
    (N'Новый', N'#9CA3AF'),
    (N'В работе', N'#60A5FA'),
    (N'Эскалирован', N'#F59E0B'),
    (N'Решён', N'#34D399'),
    (N'Закрыт', N'#6B7280');
GO

INSERT INTO dbo.IncidentCategories (Name, Description)
VALUES
    (N'Несанкционированный доступ', N'Попытки доступа без подтверждённых полномочий.'),
    (N'Фишинг', N'Мошеннические письма, ссылки и формы авторизации.'),
    (N'Вредоносное ПО', N'Обнаружение вирусов, троянов, шифровальщиков и иного вредоносного кода.'),
    (N'Сетевые атаки', N'DDoS, сканирование, попытки эксплуатации сетевых сервисов.'),
    (N'Утечка данных', N'Нарушение конфиденциальности и компрометация информации.');
GO

INSERT INTO dbo.Users (FirstName, LastName, Login, PasswordHash, Role, IsActive)
VALUES
    (N'Системный', N'Администратор', N'admin', N'6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b', N'Администратор', 1),
    (N'Иван', N'Оператор', N'operator', N'6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b', N'Оператор', 1),
    (N'Анна', N'Аналитик', N'analyst', N'6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b', N'Аналитик', 1);
GO

INSERT INTO dbo.ResponseResources (Name, ResourceType, Responsible, IsAvailable)
VALUES
    (N'SOC Shift A', N'Группа реагирования', N'Иван Оператор', 1),
    (N'Изолированный стенд', N'Инфраструктура', N'Администратор ИБ', 1),
    (N'Forensics Kit', N'Инструмент', N'Анна Аналитик', 1),
    (N'IR Hotline', N'Коммуникация', N'Дежурный центр', 1);
GO

DECLARE @NewStatusId INT = (SELECT Id FROM dbo.IncidentStatuses WHERE Name = N'Новый');
DECLARE @InProgressStatusId INT = (SELECT Id FROM dbo.IncidentStatuses WHERE Name = N'В работе');
DECLARE @EscalatedStatusId INT = (SELECT Id FROM dbo.IncidentStatuses WHERE Name = N'Эскалирован');
DECLARE @PhishingCategoryId INT = (SELECT Id FROM dbo.IncidentCategories WHERE Name = N'Фишинг');
DECLARE @MalwareCategoryId INT = (SELECT Id FROM dbo.IncidentCategories WHERE Name = N'Вредоносное ПО');
DECLARE @OperatorId INT = (SELECT Id FROM dbo.Users WHERE Login = N'operator');
DECLARE @AdminId INT = (SELECT Id FROM dbo.Users WHERE Login = N'admin');
GO

INSERT INTO dbo.Incidents (Title, Description, Priority, CategoryId, StatusId, OperatorId)
VALUES
    (N'Подозрительное письмо с вложением', N'Поступило письмо с макросом и внешней ссылкой.', N'Высокий', (SELECT Id FROM dbo.IncidentCategories WHERE Name = N'Фишинг'), (SELECT Id FROM dbo.IncidentStatuses WHERE Name = N'Новый'), (SELECT Id FROM dbo.Users WHERE Login = N'operator')),
    (N'Срабатывание EDR на рабочей станции', N'EDR зафиксировал запуск неизвестного исполняемого файла.', N'Критический', (SELECT Id FROM dbo.IncidentCategories WHERE Name = N'Вредоносное ПО'), (SELECT Id FROM dbo.IncidentStatuses WHERE Name = N'Эскалирован'), (SELECT Id FROM dbo.Users WHERE Login = N'operator'));
GO

UPDATE dbo.ResponseResources
SET IsAvailable = 0
WHERE Name = N'Forensics Kit';
GO

INSERT INTO dbo.IncidentResources (IncidentId, ResourceId)
SELECT i.Id, r.Id
FROM dbo.Incidents i
INNER JOIN dbo.ResponseResources r ON r.Name = N'Forensics Kit'
WHERE i.Title = N'Срабатывание EDR на рабочей станции';
GO

INSERT INTO dbo.EventLog (IncidentId, UserId, Action, Comment)
SELECT i.Id, u.Id, N'Создание инцидента', N'Инцидент зарегистрирован в системе.'
FROM dbo.Incidents i
CROSS JOIN dbo.Users u
WHERE i.Title = N'Подозрительное письмо с вложением'
  AND u.Login = N'operator';
GO

INSERT INTO dbo.EventLog (IncidentId, UserId, Action, Comment)
SELECT i.Id, u.Id, N'Создание инцидента', N'Инцидент зарегистрирован в системе.'
FROM dbo.Incidents i
CROSS JOIN dbo.Users u
WHERE i.Title = N'Срабатывание EDR на рабочей станции'
  AND u.Login = N'operator';
GO

INSERT INTO dbo.EventLog (IncidentId, UserId, Action, Comment)
SELECT i.Id, u.Id, N'Эскалация', N'Инцидент эскалирован для углублённого анализа.'
FROM dbo.Incidents i
CROSS JOIN dbo.Users u
WHERE i.Title = N'Срабатывание EDR на рабочей станции'
  AND u.Login = N'admin';
GO

INSERT INTO dbo.EventLog (IncidentId, UserId, Action, Comment)
SELECT i.Id, u.Id, N'Назначение ресурса', N'Назначен ресурс: Forensics Kit.'
FROM dbo.Incidents i
CROSS JOIN dbo.Users u
WHERE i.Title = N'Срабатывание EDR на рабочей станции'
  AND u.Login = N'admin';
GO
