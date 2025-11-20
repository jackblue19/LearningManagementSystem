namespace LMS.Helpers;

public static class SessionExtensions
{
    public static IServiceCollection AddSessionServices(this IServiceCollection services)
    {
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        services.AddDistributedMemoryCache();

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Lax;
        });

        return services;
    }
}
