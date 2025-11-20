using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Assessment;

namespace LMS.Repositories.Impl.Assessment;

public class ExamResultRepository : GenericRepository<ExamResult, long>, IExamResultRepository
{
    public ExamResultRepository(CenterDbContext db) : base(db)
    {
    }
}
