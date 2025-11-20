using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleBatches",
                columns: table => new
                {
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Schedule__5D55CE584AAE1748", x => x.BatchId);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    SlotId = table.Column<byte>(type: "tinyint", nullable: false),
                    SlotLabel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    SlotOrder = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TimeSlot__0A124AAFF9165456", x => x.SlotId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RoleDesc = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4C5FAC8AE7", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecordId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    NewData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditLog__5E5486487A336F7B", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Centers",
                columns: table => new
                {
                    CenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CenterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CenterAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CenterEmail = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Centers__398FC7F745A25F02", x => x.CenterId);
                    table.ForeignKey(
                        name: "FK_Centers_Users",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TeacherAvailabilities",
                columns: table => new
                {
                    AvailabilityId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<byte>(type: "tinyint", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TeacherA__DA3979B19117FA2C", x => x.AvailabilityId);
                    table.ForeignKey(
                        name: "FK_TAv_Users",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Rooms__32863939D1D5D956", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Rooms_Centers",
                        column: x => x.CenterId,
                        principalTable: "Centers",
                        principalColumn: "CenterId");
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Subjects__AC1BA3A8A4D30879", x => x.SubjectId);
                    table.ForeignKey(
                        name: "FK_Subjects_Centers",
                        column: x => x.CenterId,
                        principalTable: "Centers",
                        principalColumn: "CenterId");
                });

            migrationBuilder.CreateTable(
                name: "RoomAvailabilities",
                columns: table => new
                {
                    AvailabilityId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<byte>(type: "tinyint", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RoomAvai__DA3979B10CF81786", x => x.AvailabilityId);
                    table.ForeignKey(
                        name: "FK_RAv_Rooms",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId");
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ClassName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SubjectId = table.Column<long>(type: "bigint", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalSessions = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ScheduleDesc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Classes__CB1927C0743FBE81", x => x.ClassId);
                    table.ForeignKey(
                        name: "FK_Classes_Centers",
                        column: x => x.CenterId,
                        principalTable: "Centers",
                        principalColumn: "CenterId");
                    table.ForeignKey(
                        name: "FK_Classes_Subjects",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId");
                    table.ForeignKey(
                        name: "FK_Classes_Users",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ClassRegistrations",
                columns: table => new
                {
                    RegistrationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    RegistrationStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ClassReg__6EF588108FFE0997", x => x.RegistrationId);
                    table.ForeignKey(
                        name: "FK_ClassRegistrations_Classes",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId");
                    table.ForeignKey(
                        name: "FK_ClassRegistrations_Users",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ClassSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    SlotOrder = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ScheduleLabel = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ScheduleNote = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SlotId = table.Column<byte>(type: "tinyint", nullable: true),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsAutoGenerated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ClassSch__9C8A5B496FFD3435", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_ClassSchedules_Batches",
                        column: x => x.BatchId,
                        principalTable: "ScheduleBatches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_ClassSchedules_Classes",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId");
                    table.ForeignKey(
                        name: "FK_ClassSchedules_Rooms",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId");
                    table.ForeignKey(
                        name: "FK_ClassSchedules_TimeSlots",
                        column: x => x.SlotId,
                        principalTable: "TimeSlots",
                        principalColumn: "SlotId");
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ExamType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxScore = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    DurationMin = table.Column<int>(type: "int", nullable: true),
                    ExamDesc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExamStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Exams__297521C7BF71C2CC", x => x.ExamId);
                    table.ForeignKey(
                        name: "FK_Exams_Classes",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId");
                    table.ForeignKey(
                        name: "FK_Exams_Users",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    NotiType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E12892C69B4", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Centers",
                        column: x => x.CenterId,
                        principalTable: "Centers",
                        principalColumn: "CenterId");
                    table.ForeignKey(
                        name: "FK_Notifications_Classes",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId");
                    table.ForeignKey(
                        name: "FK_Notifications_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FbStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    RegistrationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Feedback__6A4BEDD6D692A1E6", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Classes",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId");
                    table.ForeignKey(
                        name: "FK_Feedbacks_Registrations",
                        column: x => x.RegistrationId,
                        principalTable: "ClassRegistrations",
                        principalColumn: "RegistrationId");
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    VnpTxnRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TnxNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    PaidAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    RegistrationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payments__9B556A3867C0C89A", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Classes",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId");
                    table.ForeignKey(
                        name: "FK_Payments_Registrations",
                        column: x => x.RegistrationId,
                        principalTable: "ClassRegistrations",
                        principalColumn: "RegistrationId");
                    table.ForeignKey(
                        name: "FK_Payments_Users",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    AttendanceId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Attendan__8B69261C571FE25E", x => x.AttendanceId);
                    table.ForeignKey(
                        name: "FK_Attendances_Schedules",
                        column: x => x.ScheduleId,
                        principalTable: "ClassSchedules",
                        principalColumn: "ScheduleId");
                    table.ForeignKey(
                        name: "FK_Attendances_Users",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ClassMaterials",
                columns: table => new
                {
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    MaterialType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    CloudObjectKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ClassMat__C50610F76C6897A5", x => x.MaterialId);
                    table.ForeignKey(
                        name: "FK_ClassMaterials_Classes",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId");
                    table.ForeignKey(
                        name: "FK_ClassMaterials_Exams",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "ExamId");
                    table.ForeignKey(
                        name: "FK_ClassMaterials_Users",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    ResultId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ExamResu__97690208B563FFED", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_ExamResults_Exams",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "ExamId");
                    table.ForeignKey(
                        name: "FK_ExamResults_Users",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ScheduleId",
                table: "Attendances",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId",
                table: "Attendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Centers_ManagerId",
                table: "Centers",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_CenterId",
                table: "Classes",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_SubjectId",
                table: "Classes",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherId",
                table: "Classes",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMaterials_ClassId",
                table: "ClassMaterials",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMaterials_ExamId",
                table: "ClassMaterials",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMaterials_UploadedByUserId",
                table: "ClassMaterials",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassRegistrations_ClassId",
                table: "ClassRegistrations",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassRegistrations_StudentId",
                table: "ClassRegistrations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSched_Batch",
                table: "ClassSchedules",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSched_SlotRoom",
                table: "ClassSchedules",
                columns: new[] { "SessionDate", "SlotId", "RoomId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_ClassId",
                table: "ClassSchedules",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_RoomId",
                table: "ClassSchedules",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_SlotId",
                table: "ClassSchedules",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ExamId",
                table: "ExamResults",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ClassId",
                table: "Exams",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_TeacherId",
                table: "Exams",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ClassId",
                table: "Feedbacks",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_RegistrationId",
                table: "Feedbacks",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CenterId",
                table: "Notifications",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ClassId",
                table: "Notifications",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClassId",
                table: "Payments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RegistrationId",
                table: "Payments",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAvail",
                table: "RoomAvailabilities",
                columns: new[] { "RoomId", "DayOfWeek", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "UQ_Room_Center",
                table: "Rooms",
                columns: new[] { "CenterId", "RoomName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CenterId",
                table: "Subjects",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAvail",
                table: "TeacherAvailabilities",
                columns: new[] { "TeacherId", "DayOfWeek", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "UQ__TimeSlot__F57364B47815BB11",
                table: "TimeSlots",
                column: "SlotOrder",
                unique: true,
                filter: "[SlotOrder] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ClassMaterials");

            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "RoomAvailabilities");

            migrationBuilder.DropTable(
                name: "TeacherAvailabilities");

            migrationBuilder.DropTable(
                name: "ClassSchedules");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "ClassRegistrations");

            migrationBuilder.DropTable(
                name: "ScheduleBatches");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "TimeSlots");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Centers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
