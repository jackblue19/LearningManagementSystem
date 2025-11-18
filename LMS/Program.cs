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

// AuthZN
builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                // nào có path login ui đồ ok thì bỏ vô sau
                //options.LoginPath = "/SystemAccounts/Login";
                //options.AccessDeniedPath = "/SystemAccounts/AccessDenied";
                //options.LogoutPath = "/SystemAccounts/Logout";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
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

builder.Services.AddAuthorization();

var app = builder.Build();

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
