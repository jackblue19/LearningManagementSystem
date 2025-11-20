using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Communication;

namespace LMS.Repositories.Impl.Communication;

public class FeedbackRepository : GenericRepository<Feedback, long>, IFeedbackRepository
{
    public FeedbackRepository(CenterDbContext db) : base(db)
    {
    }
}
