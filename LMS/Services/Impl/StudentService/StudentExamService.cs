using System.Linq.Expressions;
using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Models.ViewModels.StudentService;
using LMS.Repositories;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Services.Impl.StudentService;

public class StudentExamService : IStudentExamService
{
    private readonly IGenericRepository<Exam, Guid> _examRepo;
    private readonly IGenericRepository<ClassRegistration, long> _regRepo;

    public StudentExamService(
        IGenericRepository<Exam, Guid> examRepo,
        IGenericRepository<ClassRegistration, long> regRepo)
    {
        _examRepo = examRepo;
        _regRepo = regRepo;
    }

    public async Task<PagedResult<StudentExamVm>> ListExamsAsync(
        Guid studentId, DateOnly? from = null, DateOnly? to = null, bool upcomingOnly = false,
        int pageIndex = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;
        var skip = (pageIndex - 1) * pageSize;

        var regs = await _regRepo.ListAsync(
                    r => r.StudentId == studentId && r.RegistrationStatus == "approved",
                    asNoTracking: true, ct: ct);

        var clsIds = regs.Select(r => r.ClassId).Distinct().ToArray();
        if (clsIds.Length == 0)
            return new(new List<StudentExamVm>(), 0, pageIndex, pageSize);

        Expression<Func<Exam, bool>> pred = e =>
            clsIds.Contains(e.ClassId) &&
            (!from.HasValue || (e.ExamDate != null && e.ExamDate >= from.Value)) &&
            (!to.HasValue || (e.ExamDate != null && e.ExamDate <= to.Value)) &&
            (!upcomingOnly || (e.ExamDate != null && e.ExamDate >= DateOnly.FromDateTime(DateTime.UtcNow)));

        var exams = await _examRepo.ListAsync(
                            predicate: pred,
                            orderBy: q => q.OrderBy(e => e.ExamDate),
                            skip: skip, take: pageSize,
                            asNoTracking: true,
                            includes: new Expression<Func<Exam, object>>[]
                                        { e => e.Class! },
                            ct: ct);

        var total = await _examRepo.CountAsync(pred, ct);

        var items = exams.Select(e => new StudentExamVm(
            e.ExamId, e.ClassId, e.Class?.ClassName ?? "", e.Title, e.ExamType, e.ExamDate,
            e.MaxScore, e.DurationMin, e.ExamStatus)).ToList();

        return new PagedResult<StudentExamVm>(items, total, pageIndex, pageSize);
    }
}
