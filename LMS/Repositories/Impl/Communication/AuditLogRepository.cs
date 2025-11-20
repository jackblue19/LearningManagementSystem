using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories;
using LMS.Repositories.Interfaces.Communication;

namespace LMS.Repositories.Impl.Communication;

public class AuditLogRepository : GenericRepository<AuditLog, long>, IAuditLogRepository
{
    public AuditLogRepository(LMS.Data.CenterDbContext db) : base(db)
    {
    }
}
