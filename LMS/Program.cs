

using LMS.Data;
using LMS.Helpers;
using LMS.Repositories;
using LMS.Repositories.Impl.Communication;
using LMS.Repositories.Impl.Info;
using LMS.Repositories.Interfaces.Communication;
using LMS.Repositories.Interfaces.Info;
using LMS.Repositories.Impl.Assessment;
using LMS.Repositories.Interfaces.Assessment;
using LMS.Services.Impl;
using LMS.Services.Impl.AdminService;
using LMS.Services.Impl.CommonService;
using LMS.Services.Interfaces;
using LMS.Services.Interfaces.AdminService;
using LMS.Services.Interfaces.CommonService;
using Microsoft.EntityFrameworkCore;
using LMS.Models.Entities;
using System.Linq.Expressions;
using LMS.Models.ViewModels.StudentService.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSignalR();

// DI
var connectionString = builder.Configuration.GetConnectionString("SqlServer");
builder.Services.AddDbContext<LMS.Data.CenterDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

//  Generic
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamResultRepository, ExamResultRepository>();

builder.Services.AddScoped(typeof(ICrudService<,>), typeof(CrudService<,>));

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
});


// Register Repositories
builder.Services.AddRepositories();

// Register Services
builder.Services.AddApplicationServices();

// Register Helpers
builder.Services.AddScoped<EmailHelper>();

// Add MemoryCache for token storage
builder.Services.AddMemoryCache();

builder.Services.AddVnPayConfig(builder.Configuration);
builder.Services.AddStudentServices();

// User Repository & Services

// AuthZN
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddScoped<EmailHelper>();
// Session
builder.Services.AddSessionServices();

var app = builder.Build();

// Seed data on startup (only in development)
if (app.Environment.IsDevelopment())
{
    var services = app.Services.CreateScope().ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CenterDbContext>();
        await DataSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapControllers();
app.MapHub<LMS.Hubs.NotificationHub>("/notificationHub");

app.MapClassesApi();
app.MapUsersApi();
app.MapSubjectsApi();
app.MapCentersApi();
app.MapExamsApi();
app.MapClassSchedulesApi();


///

//app.MapGet("/", () => Results.Ok(new { ok = true, name = "LMS Mock Checkout API" }));

///* ========================== USERS ========================== */
//app.MapPost("/api/users", async (
//    IGenericRepository<User, Guid> repo,
//    CreateUserDto dto,
//    CancellationToken ct) =>
//{
//    var u = new User
//    {
//        Username = dto.Username,
//        Email = dto.Email,
//        PasswordHash = dto.PasswordHash,
//        FullName = dto.FullName,
//        RoleDesc = dto.RoleDesc,      // manager | teacher | student
//        IsActive = dto.IsActive
//    };
//    await repo.AddAsync(u, saveNow: false, ct);
//    await repo.SaveChangesAsync(ct);
//    return Results.Created($"/api/users/{u.UserId}", u);
//});

//app.MapGet("/api/users", async (IGenericRepository<User, Guid> repo, string? role, CancellationToken ct) =>
//{
//    var list = await repo.ListAsync(u => string.IsNullOrEmpty(role) || u.RoleDesc == role,
//                                    orderBy: q => q.OrderByDescending(x => x.CreatedAt),
//                                    asNoTracking: true, ct: ct);
//    return Results.Ok(list);
//});

///* ========================== CENTERS ========================== */
//app.MapPost("/api/centers", async (
//    IGenericRepository<Center, Guid> repo,
//    CreateCenterDto dto,
//    CancellationToken ct) =>
//{
//    var c = new Center
//    {
//        CenterName = dto.CenterName,
//        CenterAddress = dto.CenterAddress,
//        Phone = dto.Phone,
//        CenterEmail = dto.CenterEmail,
//        Logo = dto.Logo,
//        ManagerId = dto.ManagerId,
//        IsActive = dto.IsActive
//    };
//    await repo.AddAsync(c, saveNow: false, ct);
//    await repo.SaveChangesAsync(ct);
//    return Results.Created($"/api/centers/{c.CenterId}", c);
//});

//app.MapGet("/api/centers", async (IGenericRepository<Center, Guid> repo, CancellationToken ct) =>
//{
//    var list = await repo.ListAsync(asNoTracking: true, ct: ct);
//    return Results.Ok(list);
//});

///* ========================== SUBJECTS & GRADES ========================== */
//app.MapPost("/api/subjects", async (
//    IGenericRepository<Subject, long> repo,
//    CreateSubjectDto dto,
//    CancellationToken ct) =>
//{
//    var s = new Subject
//    {
//        SubjectName = dto.SubjectName,
//        GradeLevel = dto.GradeLevel,
//        CenterId = dto.CenterId
//    };
//    await repo.AddAsync(s, saveNow: false, ct);
//    await repo.SaveChangesAsync(ct);
//    return Results.Created($"/api/subjects/{s.SubjectId}", s);
//});

//app.MapGet("/api/subjects", async (
//    IGenericRepository<Subject, long> repo,
//    Guid? centerId,
//    string? grade,
//    CancellationToken ct) =>
//{
//    Expression<Func<Subject, bool>> pred = s =>
//        (!centerId.HasValue || s.CenterId == centerId.Value) &&
//        (string.IsNullOrEmpty(grade) || s.GradeLevel == grade);

//    var list = await repo.ListAsync(pred, orderBy: q => q.OrderBy(s => s.SubjectName),
//                                    asNoTracking: true, ct: ct);
//    return Results.Ok(list);
//});

//// Grades: lấy danh sách GradeLevel distinct từ Subjects
//app.MapGet("/api/grades", async (IGenericRepository<Subject, long> repo, Guid? centerId, CancellationToken ct) =>
//{
//    var list = await repo.ListAsync(s => !centerId.HasValue || s.CenterId == centerId.Value,
//                                    asNoTracking: true, ct: ct);
//    var grades = list.Where(s => !string.IsNullOrWhiteSpace(s.GradeLevel))
//                     .Select(s => s.GradeLevel!)
//                     .Distinct()
//                     .OrderBy(x => x)
//                     .ToList();
//    return Results.Ok(grades);
//});

///* ========================== CLASSES ========================== */
//app.MapPost("/api/classes", async (
//    IGenericRepository<Class, Guid> repo,
//    CreateClassDto dto,
//    CancellationToken ct) =>
//{
//    var cls = new Class
//    {
//        ClassName = dto.ClassName,
//        SubjectId = dto.SubjectId,      // BIGINT
//        TeacherId = dto.TeacherId,      // GUID
//        ClassAddress = dto.ClassAddress,
//        UnitPrice = dto.UnitPrice,
//        TotalSessions = dto.TotalSessions,
//        StartDate = dto.StartDate,      // DATE (DateOnly)
//        EndDate = dto.EndDate,          // DATE (DateOnly)
//        ScheduleDesc = dto.ScheduleDesc,
//        CenterId = dto.CenterId,
//        ClassStatus = dto.ClassStatus
//    };
//    await repo.AddAsync(cls, saveNow: false, ct);
//    await repo.SaveChangesAsync(ct);
//    return Results.Created($"/api/classes/{cls.ClassId}", cls);
//});

//app.MapGet("/api/classes", async (
//    IGenericRepository<Class, Guid> repo,
//    Guid? centerId, long? subjectId, string? search,
//    CancellationToken ct) =>
//{
//    Expression<Func<Class, bool>> pred = c =>
//        (!centerId.HasValue || c.CenterId == centerId.Value) &&
//        (!subjectId.HasValue || c.SubjectId == subjectId.Value) &&
//        (string.IsNullOrEmpty(search) || c.ClassName.Contains(search));

//    var list = await repo.ListAsync(pred,
//        orderBy: q => q.OrderBy(c => c.ClassName),
//        asNoTracking: true,
//        includes: new Expression<Func<Class, object>>[] { c => c.Subject! },
//        ct: ct);

//    return Results.Ok(list.Select(c => new
//    {
//        c.ClassId,
//        c.ClassName,
//        c.CenterId,
//        c.SubjectId,
//        SubjectName = c.Subject?.SubjectName,
//        c.UnitPrice,
//        c.TotalSessions,
//        c.StartDate,
//        c.EndDate,
//        c.ClassStatus
//    }));
//});

//app.MapGet("/api/classes/{id:guid}", async (
//    Guid id,
//    IGenericRepository<Class, Guid> repo,
//    CancellationToken ct) =>
//{
//    var cls = await repo.FirstOrDefaultAsync(c => c.ClassId == id, asNoTracking: true,
//        includes: new Expression<Func<Class, object>>[] { c => c.Subject! }, ct: ct);
//    return cls is null ? Results.NotFound() : Results.Ok(cls);
//});

///* ========================== CLASS SCHEDULES ========================== */
//app.MapPost("/api/schedules", async (
//    IGenericRepository<ClassSchedule, long> repo,
//    CreateScheduleDto dto,
//    CancellationToken ct) =>
//{
//    var sch = new ClassSchedule
//    {
//        ClassId = dto.ClassId,
//        SessionDate = dto.SessionDate,    // DATE
//        RoomName = dto.RoomName,
//        StartTime = dto.StartTime,        // TIME
//        EndTime = dto.EndTime,            // TIME
//        SlotOrder = dto.SlotOrder,
//        ScheduleLabel = dto.ScheduleLabel,
//        ScheduleNote = dto.ScheduleNote
//    };
//    await repo.AddAsync(sch, saveNow: false, ct);
//    await repo.SaveChangesAsync(ct);
//    return Results.Created($"/api/schedules/{sch.ScheduleId}", sch);
//});

//app.MapGet("/api/schedules", async (
//    IGenericRepository<ClassSchedule, long> repo,
//    Guid classId,
//    CancellationToken ct) =>
//{
//    var items = await repo.ListAsync(s => s.ClassId == classId,
//        orderBy: q => q.OrderBy(s => s.SessionDate).ThenBy(s => s.SlotOrder),
//        asNoTracking: true, ct: ct);
//    return Results.Ok(items);
//});

///* ========================== REGISTRATIONS ========================== */
//app.MapPost("/api/registrations", async (
//    IGenericRepository<ClassRegistration, long> repo,
//    CreateRegistrationDto dto,
//    CancellationToken ct) =>
//{
//    // Nếu đã có approved thì không tạo trùng
//    var dup = await repo.ExistsAsync(r => r.StudentId == dto.StudentId
//                                       && r.ClassId == dto.ClassId
//                                       && r.RegistrationStatus == "approved", ct);
//    if (dup) return Results.Conflict(new { message = "Already registered." });

//    var reg = new ClassRegistration
//    {
//        StudentId = dto.StudentId,
//        ClassId = dto.ClassId,
//        RegisteredAt = DateTime.UtcNow,
//        RegistrationStatus = "approved" // theo schema
//    };
//    await repo.AddAsync(reg, saveNow: false, ct);
//    await repo.SaveChangesAsync(ct);
//    return Results.Created($"/api/registrations/{reg.RegistrationId}", reg);
//});

//app.MapGet("/api/registrations", async (
//    IGenericRepository<ClassRegistration, long> repo,
//    Guid studentId,
//    CancellationToken ct) =>
//{
//    var list = await repo.ListAsync(r => r.StudentId == studentId,
//        orderBy: q => q.OrderByDescending(r => r.RegisteredAt),
//        asNoTracking: true,
//        includes: new Expression<Func<ClassRegistration, object>>[] { r => r.Class! },
//        ct: ct);

//    var shaped = list.Select(r => new
//    {
//        r.RegistrationId,
//        r.StudentId,
//        r.ClassId,
//        ClassName = r.Class?.ClassName,
//        r.RegisteredAt,
//        r.RegistrationStatus
//    });

//    return Results.Ok(shaped);
//});
///
app.Run();


// ✅ Property-based DTOs (binder thân thiện, tránh lỗi Guid/DateOnly ở positional records)

//public sealed record CreateUserDto
//{
//    public string Username { get; init; } = "";
//    public string Email { get; init; } = "";
//    public string PasswordHash { get; init; } = "";
//    public string? FullName { get; init; }
//    public string? RoleDesc { get; init; }  // manager | teacher | student
//    public bool IsActive { get; init; } = true;
//}

//public sealed record CreateCenterDto
//{
//    public string CenterName { get; init; } = "";
//    public Guid ManagerId { get; init; }
//    public string? CenterAddress { get; init; }
//    public string? Phone { get; init; }
//    public string? CenterEmail { get; init; }
//    public string? Logo { get; init; }
//    public bool IsActive { get; init; } = true;
//}

//public sealed record CreateSubjectDto
//{
//    public string SubjectName { get; init; } = "";
//    public Guid CenterId { get; init; }
//    public string? GradeLevel { get; init; }
//}

//public sealed record CreateClassDto
//{
//    public string ClassName { get; init; } = "";
//    public long SubjectId { get; init; }   // BIGINT
//    public Guid TeacherId { get; init; }   // GUID
//    public Guid CenterId { get; init; }   // GUID
//    public decimal? UnitPrice { get; init; }   // DECIMAL(18,2)
//    public int? TotalSessions { get; init; }
//    public DateOnly? StartDate { get; init; }   // DATE
//    public DateOnly? EndDate { get; init; }   // DATE
//    public string? ScheduleDesc { get; init; }
//    public string? ClassAddress { get; init; }
//    public string? ClassStatus { get; init; }
//}

//public sealed record CreateScheduleDto
//{
//    public Guid ClassId { get; init; }
//    public DateOnly SessionDate { get; init; }   // DATE
//    public string? RoomName { get; init; }
//    public TimeOnly? StartTime { get; init; }   // TIME
//    public TimeOnly? EndTime { get; init; }   // TIME
//    public int? SlotOrder { get; init; }
//    public string? ScheduleLabel { get; init; }
//    public string? ScheduleNote { get; init; }
//}

//public sealed record CreateRegistrationDto
//{
//    public Guid StudentId { get; init; }
//    public Guid ClassId { get; init; }
//}

//public sealed record CheckoutDto
//{
//    public Guid StudentId { get; init; }
//    public Guid ClassId { get; init; }
//    public string PaymentMethod { get; init; } = "cash";
//}
