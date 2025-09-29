namespace Presentation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}

/*  Tham khảo set-up
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorPages()
    .AddRazorPagesOptions(o =>  // bảo vệ Student pages
        o.Conventions.AuthorizeAreaFolder("Student", "/"))
    .Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

// custom DI
builder.Services.AddPresentation();   // nằm trong Extensions

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapDefaultControllerRoute();      // root controllers
app.MapRazorPages();
app.MapBlazorHub();                   // Blazor SignalR
app.MapHub<NotificationHub>("/hubs/notify");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();
*/
