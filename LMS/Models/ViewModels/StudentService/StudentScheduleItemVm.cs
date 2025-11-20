namespace LMS.Models.ViewModels.StudentService;

public sealed record StudentScheduleItemVm(
    long ScheduleId,
    Guid ClassId,
    string ClassName,
    DateOnly SessionDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? SlotOrder,
    string? RoomName,
    string? ScheduleLabel);
