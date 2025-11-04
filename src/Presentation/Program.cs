using Application.DependencyInjection;
using Infrastructure.DependencyInjection;
using Presentation.Hubs;

namespace Presentation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages()
            .AddRazorPagesOptions(o =>
            {
                // Protect area folders with policies (role-based)
                o.Conventions.AuthorizeAreaFolder("Students", "/", "StudentPolicy");
                o.Conventions.AuthorizeAreaFolder("Teachers", "/", "TeacherPolicy");
                o.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminPolicy");
                o.Conventions.AuthorizeAreaFolder("CentreManagement", "/", "CentrePolicy");
            });

        // Role-based authorization policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("StudentPolicy", p => p.RequireRole("Student"));
            options.AddPolicy("TeacherPolicy", p => p.RequireRole("Teacher"));
            options.AddPolicy("AdminPolicy", p => p.RequireRole("Admin"));
            options.AddPolicy("CentrePolicy", p => p.RequireRole("CentreManagement"));
        });

        builder.Services.AddControllersWithViews();

        // Authentication - default cookie scheme so area authorization works
        builder.Services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                // Paths for area Razor Pages (placed under Areas/Shared/Pages/Auth)
                options.LoginPath = "/Shared/Auth/Login";
                options.LogoutPath = "/Shared/Auth/Logout";
                options.AccessDeniedPath = "/Shared/Auth/AccessDenied";
            });

        // Blazor Server + SignalR for interactive components and hubs
        builder.Services.AddServerSideBlazor();
        builder.Services.AddSignalR();

        // Application & Infrastructure DI
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Authentication/Authorization for role-based areas
        app.UseAuthentication();
        app.UseAuthorization();

        // Area-aware routing and default routes
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapDefaultControllerRoute();
        app.MapRazorPages();

        // Blazor SignalR endpoint and Notification hub
        app.MapBlazorHub();
        app.MapHub<NotificationHub>("/hubs/notify");

        app.Run();
    }
}
