namespace LMS.Models.ViewModels.StudentService;

public sealed record StudentExamResultVm(
    long ResultId,
    Guid ExamId,
    string Title,
    DateOnly? ExamDate,
    decimal? Score,
    string? Note,
    string ClassName);
