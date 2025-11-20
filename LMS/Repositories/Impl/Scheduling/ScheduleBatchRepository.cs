using System;
using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Repositories;

namespace LMS.Repositories.Impl.Scheduling;

public class ScheduleBatchRepository : GenericRepository<ScheduleBatch, Guid>, IScheduleBatchRepository
{
    public ScheduleBatchRepository(CenterDbContext db) : base(db)
    {
    }
}
