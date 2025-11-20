using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories;
using LMS.Repositories.Interfaces.Communication;

namespace LMS.Repositories.Impl.Communication;

public class FeedbackRepository : GenericRepository<Feedback, long>, IFeedbackRepository
{
    public FeedbackRepository(LMS.Data.CenterDbContext db) : base(db)
    {
    }
}
