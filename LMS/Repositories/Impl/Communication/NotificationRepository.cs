using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Communication;

namespace LMS.Repositories.Impl.Communication;

public class NotificationRepository : GenericRepository<Notification, long>, INotificationRepository
{
    public NotificationRepository(CenterDbContext db) : base(db)
    {
    }
}
