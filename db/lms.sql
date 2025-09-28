/*====================================================================
  LMS DATABASE
====================================================================*/
USE MASTER;
GO

CREATE DATABASE CenterLMS;
GO

USE CenterLMS;
GO

/*====================================================================
  USERS 
====================================================================*/
CREATE TABLE Users (
    UserId         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username       NVARCHAR(100)  NOT NULL,
    Email          NVARCHAR(120)  NOT NULL,
    PasswordHash   NVARCHAR(256)  NOT NULL,
    FullName       NVARCHAR(120),
    Avatar         NVARCHAR(300),
    Phone          NVARCHAR(30),
    RoleDesc       NVARCHAR(40),    -- manager, teacher, student    (admin trong appsettings.json)
    IsActive       BIT            NOT NULL DEFAULT 0,
    CreatedAt      DATETIME2(0)   NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt      DATETIME2(0),
    CoverImageUrl  NVARCHAR(300)
);
GO
/*====================================================================
  CENTERS 
====================================================================*/
CREATE TABLE Centers (
    CenterId      UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CenterName    NVARCHAR(200) NOT NULL,
    CenterAddress NVARCHAR(300),
    Phone         NVARCHAR(30),
    CreatedAt     DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    IsActive      BIT          NOT NULL DEFAULT 1,
    CenterEmail   NVARCHAR(120),
    Logo          NVARCHAR(300),
    ManagerId        UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_Centers_Users FOREIGN KEY (ManagerId)
        REFERENCES Users(UserId)
);
GO
/*====================================================================
  SUBJECTS
====================================================================*/
CREATE TABLE Subjects (
    SubjectId   BIGINT IDENTITY(1,1) PRIMARY KEY,
    SubjectName NVARCHAR(120) NOT NULL,
    GradeLevel  NVARCHAR(40),
    CreatedAt   DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CenterId    UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_Subjects_Centers FOREIGN KEY (CenterId)
        REFERENCES Centers(CenterId)
);
GO
/*====================================================================
  CLASSES
====================================================================*/
CREATE TABLE Classes (
    ClassId        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClassName      NVARCHAR(150) NOT NULL,
    SubjectId      BIGINT           NOT NULL,
    TeacherId         UNIQUEIDENTIFIER NOT NULL,
    ClassAddress   NVARCHAR(300),
    UnitPrice      DECIMAL(18,2),
    TotalSessions  INT,
    StartDate      DATE,
    EndDate        DATE,
    ScheduleDesc   NVARCHAR(500),
    CoverImageUrl  NVARCHAR(300),
    CreatedAt      DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CenterId       UNIQUEIDENTIFIER NOT NULL,
    ClassStatus    NVARCHAR(40),    -- on progress, not start, finished
    CONSTRAINT FK_Classes_Subjects FOREIGN KEY (SubjectId) REFERENCES Subjects(SubjectId),
    CONSTRAINT FK_Classes_Users    FOREIGN KEY (TeacherId) REFERENCES Users(UserId),
    CONSTRAINT FK_Classes_Centers  FOREIGN KEY (CenterId)  REFERENCES Centers(CenterId)
);
GO
/*====================================================================
  CLASS SCHEDULES
====================================================================*/
CREATE TABLE ClassSchedules (
    ScheduleId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    ClassId       UNIQUEIDENTIFIER NOT NULL,
    SessionDate   DATE            NOT NULL,
    RoomName      NVARCHAR(100),
    StartTime     TIME,
    EndTime       TIME,
    SlotOrder     INT,
    CreatedAt     DATETIME2(0)    NOT NULL DEFAULT SYSUTCDATETIME(),
    ScheduleLabel NVARCHAR(150),
    ScheduleNote  NVARCHAR(150),
    CONSTRAINT FK_ClassSchedules_Classes FOREIGN KEY (ClassId)
        REFERENCES Classes(ClassId)
);
GO
/*====================================================================
  CLASS REGISTRATIONS
====================================================================*/
CREATE TABLE ClassRegistrations (
    RegistrationId BIGINT IDENTITY(1,1)  PRIMARY KEY,
    ClassId             UNIQUEIDENTIFIER NOT NULL,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    RegisteredAt        DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    RegistrationStatus  NVARCHAR(40),   -- approved, rejected
    CONSTRAINT FK_ClassRegistrations_Classes FOREIGN KEY (ClassId)    REFERENCES Classes(ClassId),
    CONSTRAINT FK_ClassRegistrations_Users   FOREIGN KEY (StudentId)  REFERENCES Users(UserId)
);
GO
/*====================================================================
  ATTENDANCES
====================================================================*/
CREATE TABLE Attendances (
    AttendanceId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    ScheduleId      BIGINT               NOT NULL,
    StudentId       UNIQUEIDENTIFIER     NOT NULL,
    StudentStatus   NVARCHAR(40),   -- absent, present, NG
    Note            NVARCHAR(300),
    CONSTRAINT FK_Attendances_Schedules FOREIGN KEY (ScheduleId) REFERENCES ClassSchedules(ScheduleId),
    CONSTRAINT FK_Attendances_Users     FOREIGN KEY (StudentId)  REFERENCES Users(UserId)
);
GO
/*====================================================================
  EXAMS
====================================================================*/
CREATE TABLE Exams (
    ExamId       UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClassId      UNIQUEIDENTIFIER NOT NULL,
    Title        NVARCHAR(150)    NOT NULL,
    ExamType     NVARCHAR(40),
    ExamDate     DATE,
    TeacherId    UNIQUEIDENTIFIER NOT NULL,
    MaxScore     DECIMAL(6,2),
    DurationMin  INT,
    ExamDesc     NVARCHAR(500),
    ExamStatus   NVARCHAR(40),
    CONSTRAINT FK_Exams_Classes FOREIGN KEY (ClassId)   REFERENCES Classes(ClassId),
    CONSTRAINT FK_Exams_Users   FOREIGN KEY (TeacherId) REFERENCES Users(UserId)
);
GO
/*====================================================================
  EXAM RESULTS
====================================================================*/
CREATE TABLE ExamResults (
    ResultId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    ExamId      UNIQUEIDENTIFIER NOT NULL,
    StudentId   UNIQUEIDENTIFIER NOT NULL,
    Score       DECIMAL(6,2),
    Note        NVARCHAR(300),
    CONSTRAINT FK_ExamResults_Exams FOREIGN KEY (ExamId)    REFERENCES Exams(ExamId),
    CONSTRAINT FK_ExamResults_Users FOREIGN KEY (StudentId) REFERENCES Users(UserId)
);
GO
/*====================================================================
  PAYMENTS
====================================================================*/
CREATE TABLE Payments (
    PaymentId      UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StudentId      UNIQUEIDENTIFIER NOT NULL,
    ClassId        UNIQUEIDENTIFIER,
    Amount         DECIMAL(18,2)    NOT NULL,
    PaymentStatus  NVARCHAR(40),
    PaymentMethod  NVARCHAR(40),
    CreatedAt      DATETIME2(0)     NOT NULL DEFAULT SYSUTCDATETIME(),
    PaidAt         DATETIME2(0),
    RegistrationId BIGINT,
    CONSTRAINT FK_Payments_Users         FOREIGN KEY (StudentId)      REFERENCES Users(UserId),
    CONSTRAINT FK_Payments_Classes       FOREIGN KEY (ClassId)        REFERENCES Classes(ClassId),
    CONSTRAINT FK_Payments_Registrations FOREIGN KEY (RegistrationId) REFERENCES ClassRegistrations(RegistrationId)
);
GO
/*====================================================================
  FEEDBACKS
====================================================================*/
CREATE TABLE Feedbacks (
    FeedbackId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId         UNIQUEIDENTIFIER NOT NULL,
    ClassId        UNIQUEIDENTIFIER NOT NULL,
    Content        NVARCHAR(MAX),
    FbStatus       NVARCHAR(40),
    CreatedAt      DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    Rating         INT,
    IsVisible      BIT DEFAULT 1,
    RegistrationId BIGINT,
    CONSTRAINT FK_Feedbacks_Users         FOREIGN KEY (UserId)        REFERENCES Users(UserId),
    CONSTRAINT FK_Feedbacks_Classes       FOREIGN KEY (ClassId)       REFERENCES Classes(ClassId),
    CONSTRAINT FK_Feedbacks_Registrations FOREIGN KEY (RegistrationId)REFERENCES ClassRegistrations(RegistrationId)
);
GO
/*====================================================================
  AUDIT LOGS
====================================================================*/
CREATE TABLE AuditLogs (
    LogId       BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    ActionType  NVARCHAR(40),
    EntityName  NVARCHAR(100),
    RecordId    NVARCHAR(100),
    CreatedAt   DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    NewData     NVARCHAR(MAX),
    OldData     NVARCHAR(MAX),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO
/*====================================================================
  NOTIFICATIONS
====================================================================*/
CREATE TABLE Notifications (
    NotificationId BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId         UNIQUEIDENTIFIER     NULL,
    ClassId        UNIQUEIDENTIFIER     NULL,
    CenterId       UNIQUEIDENTIFIER     NULL,
    Content        NVARCHAR(400),
    NotiType       NVARCHAR(40),
    CreatedAt      DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    IsRead         BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Notifications_Classes FOREIGN KEY (ClassId) REFERENCES Classes(ClassId),
    CONSTRAINT FK_Notifications_Centers FOREIGN KEY (CenterId) REFERENCES Centers(CenterId)
);
GO
/*====================================================================
  CLASS MATERIALS
====================================================================*/
CREATE TABLE ClassMaterials (
    MaterialId          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClassId             UNIQUEIDENTIFIER NOT NULL,
    Title               NVARCHAR(150),
    FileUrl             NVARCHAR(500),
    UploadedByUserId    UNIQUEIDENTIFIER NOT NULL,
    UploadedAt          DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    MaterialType        NVARCHAR(40),
    Note                NVARCHAR(300),
    ExamId              UNIQUEIDENTIFIER,
    FileSize            BIGINT,
    CloudObjectKey      NVARCHAR(300),
    CONSTRAINT FK_ClassMaterials_Classes FOREIGN KEY (ClassId)          REFERENCES Classes(ClassId),
    CONSTRAINT FK_ClassMaterials_Users   FOREIGN KEY (UploadedByUserId) REFERENCES Users(UserId),
    CONSTRAINT FK_ClassMaterials_Exams   FOREIGN KEY (ExamId)           REFERENCES Exams(ExamId)
);
GO
