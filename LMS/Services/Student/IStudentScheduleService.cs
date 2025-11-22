using LMS.Models.Entities;

namespace LMS.Services.Student;

public interface IStudentScheduleService
{
    Task<IReadOnlyList<StudentScheduleItem>> ListSchedulesByStudentAsync(
        Guid studentId,
        DateOnly? from,
        DateOnly? to,
        Guid? classId,
        CancellationToken ct = default);

    Task<IReadOnlyList<MyClassItem>> ListMyClassesAsync(
        Guid studentId,
        CancellationToken ct = default);
}

public sealed record StudentScheduleItem(
    long ScheduleId,
    Guid ClassId,
    string ClassName,
    DateOnly SessionDate,
    string? StartTime,
    string? EndTime,
    string? RoomName,
    int? SlotOrder,
    string? Label,
    string? Note);

public sealed record MyClassItem(Guid ClassId, string ClassName);
