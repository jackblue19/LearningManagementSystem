using Microsoft.AspNetCore.Authentication.Cookies;

namespace LMS.Helpers;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
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
                options.ClientId = configuration["Authentication:Google:ClientId"] ?? "";
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.SaveTokens = true;
                options.Scope.Add("profile");
                options.Scope.Add("email");
            });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
            options.AddPolicy("ManagerOnly", policy => policy.RequireRole("manager"));
            options.AddPolicy("TeacherOnly", policy => policy.RequireRole("teacher"));
            options.AddPolicy("StudentOnly", policy => policy.RequireRole("student"));
            options.AddPolicy("StaffOnly", policy => policy.RequireRole("admin", "manager"));
            options.AddPolicy("TeacherOrManager", policy => policy.RequireRole("teacher", "manager"));
        });

        return services;
    }
}
