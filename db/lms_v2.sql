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
    ManagerId     UNIQUEIDENTIFIER NOT NULL,
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
    VnpTxnRef      NVARCHAR(MAX),
    BankCode       NVARCHAR(MAX),
    TnxNo          NVARCHAR(MAX),
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


/* =============================================================
   LMS Hybrid Timetable Schema  (SlotId + Legacy Start/EndTime)
   ============================================================= */

/* ---------- 0. TIME SLOTS  (khung giờ cố định) ---------------*/
IF OBJECT_ID(N'dbo.TimeSlots', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TimeSlots (
        SlotId     TINYINT       PRIMARY KEY,
        SlotLabel  NVARCHAR(40)  NOT NULL,
        StartTime  TIME          NOT NULL,
        EndTime    TIME          NOT NULL,
        SlotOrder  TINYINT       UNIQUE
    );
END
GO

/*  Seed 6 slot mặc định (07-09 … 19-21) - chạy một lần */
IF NOT EXISTS (SELECT 1 FROM dbo.TimeSlots)
BEGIN
    INSERT dbo.TimeSlots (SlotId, SlotLabel, StartTime, EndTime, SlotOrder)
    VALUES (0, N'07-09', '07:00', '09:00', 0),
           (1, N'09-11', '09:00', '11:00', 1),
           (2, N'13-15', '13:00', '15:00', 2),
           (3, N'15-17', '15:00', '17:00', 3),
           (4, N'17-19', '17:00', '19:00', 4),
           (5, N'19-21', '19:00', '21:00', 5);
END
GO

/* ---------- 1. ROOMS (nếu chưa có) ---------------------------*/
IF OBJECT_ID(N'dbo.Rooms', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Rooms (
        RoomId   UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CenterId UNIQUEIDENTIFIER NOT NULL,
        RoomName NVARCHAR(120) NOT NULL,
        Capacity INT,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_Rooms_Centers FOREIGN KEY (CenterId)
            REFERENCES dbo.Centers(CenterId)
    );
END
GO

/* ---------- 2. ROOM AVAILABILITIES ---------------------------*/
IF OBJECT_ID(N'dbo.RoomAvailabilities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RoomAvailabilities (
        AvailabilityId BIGINT IDENTITY(1,1) PRIMARY KEY,
        RoomId    UNIQUEIDENTIFIER NOT NULL,
        DayOfWeek TINYINT NOT NULL,        -- 1=Mon … 7=Sun
        StartTime TIME    NOT NULL,
        EndTime   TIME    NOT NULL,
        Note      NVARCHAR(150),
        CONSTRAINT FK_RAv_Rooms FOREIGN KEY (RoomId)
            REFERENCES dbo.Rooms(RoomId)
    );
    CREATE INDEX IX_RoomAvail
      ON dbo.RoomAvailabilities(RoomId, DayOfWeek, StartTime, EndTime);
END
GO

/* ---------- 3. TEACHER AVAILABILITIES ------------------------*/
IF OBJECT_ID(N'dbo.TeacherAvailabilities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeacherAvailabilities (
        AvailabilityId BIGINT IDENTITY(1,1) PRIMARY KEY,
        TeacherId UNIQUEIDENTIFIER NOT NULL,
        DayOfWeek TINYINT NOT NULL,        -- 1=Mon … 7=Sun
        StartTime TIME NOT NULL,
        EndTime   TIME NOT NULL,
        Note      NVARCHAR(150),
        CONSTRAINT FK_TAv_Users FOREIGN KEY (TeacherId)
            REFERENCES dbo.Users(UserId)
    );
    CREATE INDEX IX_TeacherAvail
      ON dbo.TeacherAvailabilities(TeacherId, DayOfWeek, StartTime, EndTime);
END
GO

/* ---------- 4. SCHEDULE BATCHES (audit/rollback) -------------*/
IF OBJECT_ID(N'dbo.ScheduleBatches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScheduleBatches (
        BatchId     UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CreatedAt   DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        Description NVARCHAR(200)
    );
END
GO

/* ---------- 5. ALTER ClassSchedules  ------------------------ */
/*  (giữ StartTime/EndTime/RoomName cũ, chỉ thêm cột mới)       */
IF COL_LENGTH('dbo.ClassSchedules', 'SlotId') IS NULL
    ALTER TABLE dbo.ClassSchedules ADD SlotId TINYINT NULL;
IF COL_LENGTH('dbo.ClassSchedules', 'RoomId') IS NULL
    ALTER TABLE dbo.ClassSchedules ADD RoomId UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.ClassSchedules', 'BatchId') IS NULL
    ALTER TABLE dbo.ClassSchedules ADD BatchId UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.ClassSchedules', 'IsAutoGenerated') IS NULL
    ALTER TABLE dbo.ClassSchedules ADD IsAutoGenerated BIT NOT NULL DEFAULT 0;
GO

/*  FK mới */
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_ClassSchedules_TimeSlots')
    ALTER TABLE dbo.ClassSchedules
        ADD CONSTRAINT FK_ClassSchedules_TimeSlots
            FOREIGN KEY (SlotId) REFERENCES dbo.TimeSlots(SlotId);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_ClassSchedules_Rooms')
    ALTER TABLE dbo.ClassSchedules
        ADD CONSTRAINT FK_ClassSchedules_Rooms
            FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_ClassSchedules_Batches')
    ALTER TABLE dbo.ClassSchedules
        ADD CONSTRAINT FK_ClassSchedules_Batches
            FOREIGN KEY (BatchId) REFERENCES dbo.ScheduleBatches(BatchId);
GO

/* ---------- 6. INDEX phụ trợ --------------------------------*/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ClassSched_SlotRoom')
    CREATE INDEX IX_ClassSched_SlotRoom
      ON dbo.ClassSchedules(SessionDate, SlotId, RoomId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ClassSched_Batch')
    CREATE INDEX IX_ClassSched_Batch
      ON dbo.ClassSchedules(BatchId) INCLUDE (ClassId, SlotId, RoomId, SessionDate);
GO

/* ---------- 7. TRIGGER đồng bộ dữ liệu legacy ---------------*/
/*  Ghi StartTime/EndTime/RoomName theo SlotId / RoomId         */
IF OBJECT_ID(N'dbo.trg_ClassSchedules_SyncLegacy', N'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_ClassSchedules_SyncLegacy;
GO
CREATE TRIGGER dbo.trg_ClassSchedules_SyncLegacy
ON dbo.ClassSchedules
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE cs
      SET  StartTime = ts.StartTime,
           EndTime   = ts.EndTime,
           RoomName  = ISNULL(r.RoomName, cs.RoomName)
    FROM dbo.ClassSchedules cs
    JOIN inserted i ON i.ScheduleId = cs.ScheduleId
    LEFT JOIN dbo.TimeSlots ts ON ts.SlotId = cs.SlotId
    LEFT JOIN dbo.Rooms     r  ON r.RoomId = cs.RoomId
    WHERE cs.IsAutoGenerated = 1       -- đồng bộ record sinh tự động
      AND (cs.StartTime IS NULL OR cs.EndTime IS NULL OR cs.RoomName IS NULL);
END
GO

/* ---------- 8. STORED PROCEDURE ROLLBACK --------------------*/
IF OBJECT_ID(N'dbo.usp_Schedule_Rollback', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Schedule_Rollback;
GO
CREATE PROCEDURE dbo.usp_Schedule_Rollback
    @BatchId UNIQUEIDENTIFIER,
    @HardDelete BIT = 1            -- 1 = DELETE, 0 = chỉ huỷ flag Auto
AS
BEGIN
    SET NOCOUNT ON;

    IF @HardDelete = 1
        DELETE FROM dbo.ClassSchedules WHERE BatchId = @BatchId;
    ELSE
        UPDATE dbo.ClassSchedules
           SET IsAutoGenerated = 0
         WHERE BatchId = @BatchId;
END
GO

/* ---------- 9. CHECK CONSTRAINT khớp giờ ↔ SlotId -----------*/
-- IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name='CK_ClassSched_TimeMatch')
-- BEGIN
--     ALTER TABLE dbo.ClassSchedules  WITH NOCHECK
--       ADD CONSTRAINT CK_ClassSched_TimeMatch
--       CHECK (
--         SlotId IS NULL
--         OR EXISTS (
--             SELECT 1
--             FROM dbo.TimeSlots t
--             WHERE t.SlotId = SlotId
--               AND t.StartTime = StartTime
--               AND t.EndTime   = EndTime
--         )
--       );
-- END
-- GO

/* ====================== DONE PATCH ==========================
   - TimeSlots, Rooms, Avail tables
   - ClassSchedules chứa SlotId, RoomId, BatchId, IsAutoGenerated
   - Trigger giữ đồng bộ giờ / tên phòng cũ
   - Proc rollback
============================================================== */

-- CONSTRAINT DATA

ALTER TABLE dbo.Rooms
  ADD CONSTRAINT UQ_Room_Center UNIQUE (CenterId, RoomName);

ALTER TABLE dbo.TeacherAvailabilities
  ADD CONSTRAINT CK_TAv_Day CHECK (DayOfWeek BETWEEN 1 AND 7);
ALTER TABLE dbo.RoomAvailabilities
  ADD CONSTRAINT CK_RAv_Day CHECK (DayOfWeek BETWEEN 1 AND 7);



