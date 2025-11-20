namespace LMS.Models.ViewModels.StudentService;

public sealed record StudentCourseListItemVm(
    Guid ClassId,
    string ClassName,
    Guid CenterId,
    long SubjectId,
    string? SubjectName,
    decimal? UnitPrice,
    int? TotalSessions,
    string? ScheduleDesc,
    string? ClassStatus);

