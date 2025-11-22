using LMS.Models.Entities;

namespace LMS.Services.Student;

public interface IStudentExamService
{
    Task<IReadOnlyList<ExamDto>> ListExamsForStudentAsync(Guid studentId, Guid? classId, CancellationToken ct = default);
    Task<IReadOnlyList<ExamScoreDto>> ListScoresAsync(Guid studentId, Guid? classId, CancellationToken ct = default);
}

public sealed record ExamDto(
    Guid ExamId,
    Guid ClassId,
    string Title,
    string? ExamType,
    string? ExamStatus);

public sealed record ExamScoreDto(
    Guid ExamId,
    long ExamResultId,
    decimal? Score,
    string Title,
    Guid ClassId,
    string ClassName);
