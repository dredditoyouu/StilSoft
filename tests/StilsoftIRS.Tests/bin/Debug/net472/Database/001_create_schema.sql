IF OBJECT_ID('dbo.EventLog', 'U') IS NOT NULL DROP TABLE dbo.EventLog;
IF OBJECT_ID('dbo.IncidentResources', 'U') IS NOT NULL DROP TABLE dbo.IncidentResources;
IF OBJECT_ID('dbo.Incidents', 'U') IS NOT NULL DROP TABLE dbo.Incidents;
IF OBJECT_ID('dbo.ResponseResources', 'U') IS NOT NULL DROP TABLE dbo.ResponseResources;
IF OBJECT_ID('dbo.IncidentStatuses', 'U') IS NOT NULL DROP TABLE dbo.IncidentStatuses;
IF OBJECT_ID('dbo.IncidentCategories', 'U') IS NOT NULL DROP TABLE dbo.IncidentCategories;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Login NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(64) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CONSTRAINT UQ_Users_Login UNIQUE (Login),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Администратор', N'Оператор', N'Аналитик'))
);
GO

CREATE TABLE dbo.IncidentCategories
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IncidentCategories PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL
);
GO

CREATE TABLE dbo.IncidentStatuses
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IncidentStatuses PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    ColorHex NCHAR(7) NOT NULL CONSTRAINT DF_IncidentStatuses_ColorHex DEFAULT (N'#CCCCCC'),
    CONSTRAINT UQ_IncidentStatuses_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.Incidents
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Incidents PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(2000) NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Incidents_CreatedAt DEFAULT (GETDATE()),
    ClosedAt DATETIME NULL,
    Priority NVARCHAR(20) NOT NULL,
    CategoryId INT NOT NULL,
    StatusId INT NOT NULL,
    OperatorId INT NOT NULL,
    CONSTRAINT FK_Incidents_Category FOREIGN KEY (CategoryId) REFERENCES dbo.IncidentCategories(Id),
    CONSTRAINT FK_Incidents_Status FOREIGN KEY (StatusId) REFERENCES dbo.IncidentStatuses(Id),
    CONSTRAINT FK_Incidents_Operator FOREIGN KEY (OperatorId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Incidents_Priority CHECK (Priority IN (N'Критический', N'Высокий', N'Средний', N'Низкий'))
);
GO

CREATE TABLE dbo.ResponseResources
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ResponseResources PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    ResourceType NVARCHAR(50) NULL,
    Responsible NVARCHAR(100) NULL,
    IsAvailable BIT NOT NULL CONSTRAINT DF_ResponseResources_IsAvailable DEFAULT (1)
);
GO

CREATE TABLE dbo.IncidentResources
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IncidentResources PRIMARY KEY,
    IncidentId INT NOT NULL,
    ResourceId INT NOT NULL,
    AssignedAt DATETIME NOT NULL CONSTRAINT DF_IncidentResources_AssignedAt DEFAULT (GETDATE()),
    CONSTRAINT FK_IncidentResources_Incident FOREIGN KEY (IncidentId) REFERENCES dbo.Incidents(Id) ON DELETE CASCADE,
    CONSTRAINT FK_IncidentResources_Resource FOREIGN KEY (ResourceId) REFERENCES dbo.ResponseResources(Id)
);
GO

CREATE TABLE dbo.EventLog
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventLog PRIMARY KEY,
    IncidentId INT NULL,
    UserId INT NOT NULL,
    Action NVARCHAR(200) NOT NULL,
    Comment NVARCHAR(1000) NULL,
    OccurredAt DATETIME NOT NULL CONSTRAINT DF_EventLog_OccurredAt DEFAULT (GETDATE()),
    CONSTRAINT FK_EventLog_Incident FOREIGN KEY (IncidentId) REFERENCES dbo.Incidents(Id) ON DELETE SET NULL,
    CONSTRAINT FK_EventLog_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE INDEX IX_Incidents_StatusId ON dbo.Incidents(StatusId);
CREATE INDEX IX_Incidents_CategoryId ON dbo.Incidents(CategoryId);
CREATE INDEX IX_Incidents_OperatorId ON dbo.Incidents(OperatorId);
CREATE INDEX IX_Incidents_CreatedAt ON dbo.Incidents(CreatedAt);
CREATE INDEX IX_Incidents_Priority ON dbo.Incidents(Priority);
CREATE INDEX IX_IncidentResources_IncidentId ON dbo.IncidentResources(IncidentId);
CREATE INDEX IX_IncidentResources_ResourceId ON dbo.IncidentResources(ResourceId);
CREATE INDEX IX_EventLog_IncidentId ON dbo.EventLog(IncidentId);
CREATE INDEX IX_EventLog_UserId ON dbo.EventLog(UserId);
CREATE INDEX IX_EventLog_OccurredAt ON dbo.EventLog(OccurredAt);
GO
