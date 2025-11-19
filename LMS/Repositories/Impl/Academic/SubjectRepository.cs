using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;

namespace LMS.Repositories.Impl.Academic;

public class SubjectRepository : GenericRepository<Subject, long>, ISubjectRepository
{
    public SubjectRepository(CenterDbContext db) : base(db)
    {
    }
}
