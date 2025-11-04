using System;

namespace Presentation.ViewModels.Admin;

public record ClassOverviewViewModel(
    string Name,
    string Code,
    string LeadInstructor,
    int Enrolled,
    string NextSession,
    string Status);

public record SubjectOverviewViewModel(
    string Name,
    string Code,
    int ModuleCount,
    string Owner,
    string LastUpdated);

public record ExamScheduleViewModel(
    string Title,
    string Subject,
    DateTime ScheduledAt,
    string Room,
    string Status);

public record PaymentRecordViewModel(
    string Reference,
    string Payer,
    string Category,
    decimal Amount,
    DateTime Date,
    string Status);

public record UserDirectoryViewModel(
    string Name,
    string Role,
    string Email,
    DateTime LastActiveAt,
    bool IsActive);

public record AttendanceRecordViewModel(
    DateTime Date,
    string ClassName,
    int PresentCount,
    int AbsentCount);

public record NotificationViewModel(
    string Title,
    string Audience,
    string SentAt,
    string State);

public record RoomAvailabilityViewModel(
    string Room,
    string Capacity,
    string AvailabilityWindow,
    string Status);

public record TeacherAvailabilityViewModel(
    string Teacher,
    string Speciality,
    string AvailableWindow,
    string Status);

public record AuditLogEntryViewModel(
    DateTime Timestamp,
    string Actor,
    string Action,
    string Target,
    string Channel);

public record ClassMaterialViewModel(
    string Title,
    string Format,
    string Owner,
    DateTime UpdatedAt);

public record RegistrationSnapshotViewModel(
    string ClassName,
    string Segment,
    int Registered,
    int Capacity);

public record ExamResultSummaryViewModel(
    string Exam,
    string Cohort,
    decimal AverageScore,
    decimal PassRate);

public record ScheduleBatchViewModel(
    string Name,
    string Window,
    string Coordinator,
    string Status);
