using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Models.ViewModels.StudentService;
using LMS.Repositories;
using LMS.Services.Interfaces.StudentService;
using System.Linq.Expressions;

namespace LMS.Services.Impl.StudentService;

public class StudentExamResultService : IStudentExamResultService
{
    private readonly IGenericRepository<ExamResult, long> _resultRepo;

    public StudentExamResultService(IGenericRepository<ExamResult, long> resultRepo)
    {
        _resultRepo = resultRepo;
    }

    public async Task<PagedResult<StudentExamResultVm>> ListMyResultsAsync(
        Guid studentId, int pageIndex = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;
        var skip = (pageIndex - 1) * pageSize;

        var results = await _resultRepo.ListAsync(
                        r => r.StudentId == studentId,
                        orderBy: q => q.OrderByDescending(r => r.Exam!.ExamDate),
                        skip: skip, take: pageSize,
                        asNoTracking: true,
                        includes: new Expression<Func<ExamResult, object>>[]
                                    { r => r.Exam!, r => r.Exam!.Class! },
                        ct: ct);

        var total = await _resultRepo.CountAsync(r => r.StudentId == studentId, ct);

        var items = results.Select(r => new StudentExamResultVm(
                r.ResultId, r.ExamId, r.Exam?.Title ?? "",
                r.Exam?.ExamDate, r.Score, r.Note, r.Exam?.Class?.ClassName ?? ""))
                .ToList();

        return new PagedResult<StudentExamResultVm>(items, total, pageIndex, pageSize);
    }
}
