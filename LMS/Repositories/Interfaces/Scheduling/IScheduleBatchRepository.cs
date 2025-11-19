using System;
using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Scheduling;

public interface IScheduleBatchRepository : IGenericRepository<ScheduleBatch, Guid>
{
}
