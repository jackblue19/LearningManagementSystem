namespace LMS.Models.ViewModels.StudentService;

public sealed record StudentRegisteredClassVm(
    long RegistrationId,
    Guid ClassId,
    string ClassName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ScheduleDesc,
    string? RegistrationStatus,
    DateTime RegisteredAt);
