using System.Linq.Expressions;
using LMS.Data;
using LMS.Helpers;
using LMS.Models.Entities;
using LMS.Models.ViewModels.Bank;
using LMS.Repositories;
using LMS.Services.Impl;
using LMS.Services.Impl.StudentService;
using LMS.Services.Interfaces;
using LMS.Services.Interfaces.StudentService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VNPAY;
using VNPAY.Extensions;
using VNPAY.Models;
using VNPAY.Models.Enums;
using VNPAY.Models.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// DI
var connectionString = builder.Configuration.GetConnectionString("SqlServer");
builder.Services.AddDbContext<CenterDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

//  Generic
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
builder.Services.AddScoped(typeof(ICrudService<,>), typeof(CrudService<,>));

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

// Register Helpers
builder.Services.AddScoped<LMS.Helpers.EmailHelper>();

// Add MemoryCache for token storage
builder.Services.AddMemoryCache();

builder.Services.AddVnPayConfig(builder.Configuration);
builder.Services.AddStudentServices();

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

app.Run();
