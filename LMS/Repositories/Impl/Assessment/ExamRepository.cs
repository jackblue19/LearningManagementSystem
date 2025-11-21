using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Assessment;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Assessment;

public class ExamRepository : GenericRepository<Exam, Guid>, IExamRepository
{
    public ExamRepository(CenterDbContext db) : base(db) { }

    // Basic queries
    public async Task<IEnumerable<Exam>> GetByClassIdAsync(Guid classId, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .Include(e => e.Teacher)
                        .Include(e => e.Class)
                        .Where(e => e.ClassId == classId)
                        .OrderBy(e => e.ExamDate)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<Exam>> GetByTeacherIdAsync(Guid teacherId, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .Include(e => e.Class)
                        .Where(e => e.TeacherId == teacherId)
                        .OrderBy(e => e.ExamDate)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<Exam>> GetUpcomingExamsAsync(Guid classId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _db.Set<Exam>()
                        .Include(e => e.Teacher)
                        .Where(e => e.ClassId == classId && e.ExamDate >= today)
                        .OrderBy(e => e.ExamDate)
                        .ToListAsync(ct);
    }

    public async Task<bool> HasExamOnDateAsync(Guid classId, DateOnly examDate, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .AnyAsync(e => e.ClassId == classId && e.ExamDate == examDate, ct);
    }

    // Advanced filtering
    public async Task<IEnumerable<Exam>> GetByStatusAsync(Guid classId, string status, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .Include(e => e.Teacher)
                        .Where(e => e.ClassId == classId && e.ExamStatus == status)
                        .OrderBy(e => e.ExamDate)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<Exam>> GetByTypeAsync(Guid classId, string examType, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .Include(e => e.Teacher)
                        .Where(e => e.ClassId == classId && e.ExamType == examType)
                        .OrderBy(e => e.ExamDate)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<Exam>> GetByDateRangeAsync(Guid classId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .Include(e => e.Teacher)
                        .Where(e => e.ClassId == classId &&
                                    e.ExamDate.HasValue &&
                                    e.ExamDate.Value >= startDate &&
                                    e.ExamDate.Value <= endDate)
                        .OrderBy(e => e.ExamDate)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<Exam>> GetPastExamsAsync(Guid classId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _db.Set<Exam>()
                        .Include(e => e.Teacher)
                        .Where(e => e.ClassId == classId && e.ExamDate < today)
                        .OrderByDescending(e => e.ExamDate)
                        .ToListAsync(ct);
    }

    // Teacher specific
    public async Task<IEnumerable<Exam>> GetTeacherUpcomingExamsAsync(Guid teacherId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _db.Set<Exam>()
                        .Include(e => e.Class)
                        .Where(e => e.TeacherId == teacherId && e.ExamDate >= today)
                        .OrderBy(e => e.ExamDate)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<Exam>> GetTeacherExamsByStatusAsync(Guid teacherId, string status, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .Include(e => e.Class)
                        .Where(e => e.TeacherId == teacherId && e.ExamStatus == status)
                        .OrderBy(e => e.ExamDate)
                        .ToListAsync(ct);
    }

    // Statistics and counts
    public async Task<int> CountByClassAsync(Guid classId, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .CountAsync(e => e.ClassId == classId, ct);
    }

    public async Task<int> CountByStatusAsync(Guid classId, string status, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .CountAsync(e => e.ClassId == classId && e.ExamStatus == status, ct);
    }

    public async Task<int> CountUpcomingExamsAsync(Guid classId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _db.Set<Exam>()
                        .CountAsync(e => e.ClassId == classId && e.ExamDate >= today, ct);
    }

    // Single query optimizations
    public async Task<Exam?> GetWithDetailsAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .Include(e => e.Teacher)
                        .Include(e => e.Class)
                        .Include(e => e.ClassMaterials)
                        .FirstOrDefaultAsync(e => e.ExamId == examId, ct);
    }

    public async Task<Exam?> GetWithResultsAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .Include(e => e.Teacher)
                        .Include(e => e.Class)
                        .Include(e => e.ExamResults)
                            .ThenInclude(er => er.Student)
                        .FirstOrDefaultAsync(e => e.ExamId == examId, ct);
    }

    // Validation helpers
    public async Task<bool> ExistsForClassAsync(Guid classId, Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .AnyAsync(e => e.ExamId == examId && e.ClassId == classId, ct);
    }

    public async Task<bool> IsTeacherOwnerAsync(Guid examId, Guid teacherId, CancellationToken ct = default)
    {
        return await _db.Set<Exam>()
                        .AnyAsync(e => e.ExamId == examId && e.TeacherId == teacherId, ct);
    }
}
