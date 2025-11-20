using LMS.Data;
using LMS.Helpers;
using LMS.Repositories;
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
builder.Services.AddRepositories();

// Register Services
builder.Services.AddApplicationServices();

// Register Helpers
builder.Services.AddScoped<EmailHelper>();

// Add MemoryCache for token storage
builder.Services.AddMemoryCache();

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
