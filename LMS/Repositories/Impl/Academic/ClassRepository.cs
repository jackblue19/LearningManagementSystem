using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using Microsoft.EntityFrameworkCore;
using System;
namespace LMS.Repositories.Impl.Academic;

public class ClassRepository
    : GenericRepository<Class, Guid>, IClassRepository
{
    public ClassRepository(CenterDbContext db) : base(db)
    {
    }
}
