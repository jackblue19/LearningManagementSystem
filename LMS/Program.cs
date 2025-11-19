using System.Linq.Expressions;
using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// DI
var connectionString = builder.Configuration.GetConnectionString("SqlServer");
builder.Services.AddDbContext<CenterDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

// Register Repositories
// Assessment
builder.Services.AddScoped<LMS.Repositories.Interfaces.Assessment.IMaterialRepository, LMS.Repositories.Impl.Assessment.MaterialRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Assessment.IExamRepository, LMS.Repositories.Impl.Assessment.ExamRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Assessment.IExamResultRepository, LMS.Repositories.Impl.Assessment.ExamResultRepository>();

// Academic
builder.Services.AddScoped<LMS.Repositories.Interfaces.Academic.IAttendanceRepository, LMS.Repositories.Impl.Academic.AttendanceRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Academic.IClassRepository, LMS.Repositories.Impl.Academic.ClassRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Academic.IClassRegistrationRepository, LMS.Repositories.Impl.Academic.ClassRegistrationRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Academic.ISubjectRepository, LMS.Repositories.Impl.Academic.SubjectRepository>();

// Scheduling
builder.Services.AddScoped<LMS.Repositories.Interfaces.Scheduling.IRoomRepository, LMS.Repositories.Impl.Scheduling.RoomRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Scheduling.IClassScheduleRepository, LMS.Repositories.Impl.Scheduling.ClassScheduleRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Scheduling.ITimeSlotRepository, LMS.Repositories.Impl.Scheduling.TimeSlotRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Scheduling.IRoomAvailabilityRepository, LMS.Repositories.Impl.Scheduling.RoomAvailabilityRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Scheduling.ITeacherAvailabilityRepository, LMS.Repositories.Impl.Scheduling.TeacherAvailabilityRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Scheduling.IScheduleBatchRepository, LMS.Repositories.Impl.Scheduling.ScheduleBatchRepository>();

// Info
builder.Services.AddScoped<LMS.Repositories.Interfaces.Info.IUserRepository, LMS.Repositories.Impl.Info.UserRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Info.ICenterRepository, LMS.Repositories.Impl.Info.CenterRepository>();

// Communication
builder.Services.AddScoped<LMS.Repositories.Interfaces.Communication.IFeedbackRepository, LMS.Repositories.Impl.Communication.FeedbackRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Communication.INotificationRepository, LMS.Repositories.Impl.Communication.NotificationRepository>();
builder.Services.AddScoped<LMS.Repositories.Interfaces.Communication.IAuditLogRepository, LMS.Repositories.Impl.Communication.AuditLogRepository>();

// Finance
builder.Services.AddScoped<LMS.Repositories.Interfaces.Finance.IPaymentRepository, LMS.Repositories.Impl.Finance.PaymentRepository>();

// Register Teacher Services
builder.Services.AddScoped<LMS.Services.Interfaces.TeacherService.IMaterialService, LMS.Services.Impl.TeacherService.MaterialService>();
builder.Services.AddScoped<LMS.Services.Interfaces.TeacherService.IRoomService, LMS.Services.Impl.TeacherService.RoomService>();
builder.Services.AddScoped<LMS.Services.Interfaces.TeacherService.IAttendanceService, LMS.Services.Impl.TeacherService.AttendanceService>();
builder.Services.AddScoped<LMS.Services.Interfaces.TeacherService.IClassScheduleService, LMS.Services.Impl.TeacherService.ClassScheduleService>();
builder.Services.AddScoped<LMS.Services.Interfaces.TeacherService.IClassManagementService, LMS.Services.Impl.TeacherService.ClassManagementService>();
builder.Services.AddScoped<LMS.Services.Interfaces.TeacherService.IExamService, LMS.Services.Impl.TeacherService.ExamService>();
builder.Services.AddScoped<LMS.Services.Interfaces.TeacherService.IExamResultService, LMS.Services.Impl.TeacherService.ExamResultService>();
builder.Services.AddScoped<LMS.Services.Interfaces.TeacherService.ITimeSlotService, LMS.Services.Impl.TeacherService.TimeSlotService>();
builder.Services.AddScoped<LMS.Services.Interfaces.TeacherService.ITeacherAvailabilityService, LMS.Services.Impl.TeacherService.TeacherAvailabilityService>();

// Register Common Services
builder.Services.AddScoped<LMS.Services.Interfaces.CommonService.IAuthService, LMS.Services.Impl.CommonService.AuthService>();

// AuthZN
builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Common/Login";
                options.AccessDeniedPath = "/Common/AccessDenied";
                options.LogoutPath = "/Common/Logout";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.SaveTokens = true;
                options.Scope.Add("profile");
                options.Scope.Add("email");
            });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDistributedMemoryCache();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("manager"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("teacher"));
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("student"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("admin", "manager"));
    options.AddPolicy("TeacherOrManager", policy => policy.RequireRole("teacher", "manager"));
});

var app = builder.Build();

// Seed data on startup (only in development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
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

app.MapRazorPages();

/*  Test API
app.MapGet("/api/users", async (
    string? q,
    string? role,
    bool? isActive,
    int? skip,
    int? take,
    IGenericRepository<User, Guid> repo,
    CancellationToken ct) =>
{
    Expression<Func<User, bool>> predicate = u =>
        (q == null || u.Username.Contains(q) || u.Email.Contains(q) || (u.FullName ?? "").Contains(q)) &&
        (role == null || u.RoleDesc == role) &&
        (isActive == null || u.IsActive == isActive);

    var items = await repo.ListAsync(
        predicate: predicate,
        orderBy: qy => qy.OrderByDescending(x => x.CreatedAt),
        skip: skip ?? 0,
        take: take ?? 50,
        asNoTracking: true,
        includes: null,
        ct);

    var result = items.Select(u => new UserSummaryDto(
        u.UserId, u.Username, u.Email, u.FullName, u.Avatar, u.Phone, u.RoleDesc, u.IsActive,
        u.CreatedAt, u.UpdatedAt, u.CoverImageUrl));

    return Results.Ok(result);
});

app.MapGet("/api/users/{id:guid}", async (
    Guid id,
    IGenericRepository<User, Guid> repo,
    CancellationToken ct) =>
{
    var entity = await repo.GetByIdAsync(id, asNoTracking: true, ct);
    if (entity is null) return Results.NotFound();

    var dto = new UserSummaryDto(
        entity.UserId, entity.Username, entity.Email, entity.FullName, entity.Avatar, entity.Phone,
        entity.RoleDesc, entity.IsActive, entity.CreatedAt, entity.UpdatedAt, entity.CoverImageUrl);

    return Results.Ok(dto);
});

app.MapPost("/api/users", async (
    CreateUserDto req,
    IGenericRepository<User, Guid> repo,
    CancellationToken ct) =>
{
    var user = new User
    {
        UserId = Guid.NewGuid(),
        Username = req.Username.Trim(),
        Email = req.Email.Trim(),
        PasswordHash = req.PasswordHash, // demo only
        FullName = req.FullName,
        Phone = req.Phone,
        RoleDesc = req.RoleDesc,
        IsActive = req.IsActive ?? false,
        CreatedAt = DateTime.UtcNow
    };

    await repo.AddAsync(user, saveNow: true, ct);

    var dto = new UserSummaryDto(
        user.UserId, user.Username, user.Email, user.FullName, user.Avatar, user.Phone,
        user.RoleDesc, user.IsActive, user.CreatedAt, user.UpdatedAt, user.CoverImageUrl);

    return Results.Created($"/api/users/{user.UserId}", dto);
});*/

app.Run();

/*
public record CreateUserDto(
    string Username,
    string Email,
    string PasswordHash,
    string? FullName,
    string? Phone,
    string? RoleDesc,
    bool? IsActive);

public record UserSummaryDto(
    Guid UserId,
    string Username,
    string Email,
    string? FullName,
    string? Avatar,
    string? Phone,
    string? RoleDesc,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CoverImageUrl);
*/
