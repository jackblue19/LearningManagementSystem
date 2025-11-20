using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Assessment;

namespace LMS.Repositories.Impl.Assessment;

public class ExamRepository : GenericRepository<Exam, Guid>, IExamRepository
{
    public ExamRepository(CenterDbContext db) : base(db)
    {
    }
}
