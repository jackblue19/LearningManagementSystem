using System.Linq;
using LMS.Models.Entities;
using LMS.Repositories;

namespace LMS.Services.Student;

public sealed class StudentExamService : IStudentExamService
{
    private readonly IGenericRepository<ClassRegistration, long> _regs;
    private readonly IGenericRepository<Exam, Guid> _exams;
    private readonly IGenericRepository<ExamResult, long> _results;
    private readonly IGenericRepository<Class, Guid> _classes;

    public StudentExamService(
        IGenericRepository<ClassRegistration, long> regs,
        IGenericRepository<Exam, Guid> exams,
        IGenericRepository<ExamResult, long> results,
        IGenericRepository<Class, Guid> classes)
    {
        _regs = regs;
        _exams = exams;
        _results = results;
        _classes = classes;
    }

    public async Task<IReadOnlyList<ExamDto>> ListExamsForStudentAsync(
        Guid studentId, Guid? classId, CancellationToken ct = default)
    {
        // Lấy các lớp Approved của student (filter thêm classId nếu có)
        var approvedRegs = await _regs.ListAsync(
            r => r.StudentId == studentId
              && r.RegistrationStatus == "Approved"
              && (classId == null || r.ClassId == classId),
            asNoTracking: true,
            ct: ct);

        if (approvedRegs.Count == 0) return Array.Empty<ExamDto>();
        var classIds = approvedRegs.Select(r => r.ClassId).Distinct().ToList();

        // Lấy exams của các lớp trên, sort ExamDate desc
        var examList = await _exams.ListAsync(
            e => classIds.Contains(e.ClassId),
            orderBy: q => q.OrderByDescending(x => x.ExamDate),
            asNoTracking: true,
            ct: ct);

        // Map sang DTO, giữ đúng kiểu số
        return examList.Select(e => new ExamDto(
            ExamId: e.ExamId,
            ClassId: e.ClassId,
            Title: e.Title ?? string.Empty,
            ExamType: e.ExamType,
            ExamStatus: e.ExamStatus
        )).ToList();
    }

    public async Task<IReadOnlyList<ExamScoreDto>> ListScoresAsync(
        Guid studentId, Guid? classId, CancellationToken ct = default)
    {
        // Lấy lớp Approved như trên
        var approvedRegs = await _regs.ListAsync(
            r => r.StudentId == studentId
              && r.RegistrationStatus == "Approved"
              && (classId == null || r.ClassId == classId),
            asNoTracking: true,
            ct: ct);

        if (approvedRegs.Count == 0) return Array.Empty<ExamScoreDto>();
        var classIds = approvedRegs.Select(r => r.ClassId).Distinct().ToList();

        // Exams thuộc các lớp đó
        var examList = await _exams.ListAsync(
            e => classIds.Contains(e.ClassId),
            orderBy: q => q.OrderByDescending(x => x.ExamDate),
            asNoTracking: true,
            ct: ct);

        if (examList.Count == 0) return Array.Empty<ExamScoreDto>();
        var examIds = examList.Select(e => e.ExamId).ToList();

        // Kết quả điểm của chính student trên các exam đó
        var myResults = await _results.ListAsync(
            r => r.StudentId == studentId && examIds.Contains(r.ExamId),
            asNoTracking: true,
            ct: ct);

        if (myResults.Count == 0) return Array.Empty<ExamScoreDto>();

        // Map tên lớp để hiển thị
        var classes = await _classes.ListAsync(c => classIds.Contains(c.ClassId), asNoTracking: true, ct: ct);
        var classMap = classes.ToDictionary(c => c.ClassId, c => c.ClassName ?? string.Empty);
        var examMap = examList.ToDictionary(e => e.ExamId, e => e);

        // Build DTO
        return myResults
            .Where(r => examMap.ContainsKey(r.ExamId))
            .Select(r =>
            {
                var ex = examMap[r.ExamId];
                return new ExamScoreDto(
                    ExamId: r.ExamId,
                    ExamResultId: r.ResultId,
                    Score: r.Score,      
                    Title: ex.Title ?? string.Empty,
                    ClassId: ex.ClassId,
                    ClassName: classMap.TryGetValue(ex.ClassId, out var cn) ? cn : string.Empty
                );
            })
            .OrderByDescending(x => x.Score)
            .ToList();
    }
}
