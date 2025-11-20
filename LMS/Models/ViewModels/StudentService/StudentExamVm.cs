namespace LMS.Models.ViewModels.StudentService;

public sealed record StudentExamVm(
    Guid ExamId,
    Guid ClassId,
    string ClassName,
    string Title,
    string? ExamType,
    DateOnly? ExamDate,
    decimal? MaxScore,
    int? DurationMin,
    string? ExamStatus);
