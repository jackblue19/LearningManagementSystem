
using System;
using LMS.Data;
using LMS.Helpers;
using LMS.Repositories;
using LMS.Repositories.Impl.Communication;
using LMS.Repositories.Impl.Info;
using LMS.Repositories.Interfaces.Communication;
using LMS.Repositories.Interfaces.Info;
using LMS.Repositories.Impl.Academic;
using LMS.Repositories.Impl.Assessment;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Assessment;
using LMS.Services.Impl;
using LMS.Services.Impl.AdminService;
using LMS.Services.Impl.CommonService;
using LMS.Services.Impl.TeacherService;
using LMS.Services.Interfaces;
using LMS.Services.Interfaces.AdminService;
using LMS.Services.Interfaces.CommonService;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// AuthZN
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

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

app.Run();
