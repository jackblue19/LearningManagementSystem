/*using LMS.Models.Entities;
using LMS.Pages;
using LMS.Services.Interfaces;
using System.Linq.Expressions;
using System;

namespace LMS;

public class Wraping
{
}

app.MapGet("/api/subjects", async(
    ICrudService<Subject, long> svc,
    string? search,
    int pageIndex = 1,
    int pageSize = 20,
    CancellationToken ct = default) =>
{
    Expression<Func<Subject, bool>>? predicate = null;
    if (!string.IsNullOrWhiteSpace(search))
        predicate = s => s.SubjectName.Contains(search);

    var page = await svc.ListAsync(
        predicate: predicate,
        orderBy: q => q.OrderBy(s => s.SubjectId),
        pageIndex: pageIndex,
        pageSize: pageSize,
        asNoTracking: true,
        includes: null,
        ct: ct);

    return Results.Ok(page);
});

// Get by id
app.MapGet("/api/subjects/{id:long}", async (
    long id,
    ICrudService<Subject, long> svc,
    CancellationToken ct) =>
{
    var found = await svc.GetByIdAsync(id, asNoTracking: true, includes: null, ct);
    return found is null ? Results.NotFound() : Results.Ok(found);
});

// Create
app.MapPost("/api/subjects", async (
    ICrudService<Subject, long> svc,
    SubjectCreateDto dto,
    CancellationToken ct) =>
{
    var entity = new Subject
    {
        SubjectName = dto.SubjectName,
        GradeLevel = dto.GradeLevel,
        CenterId = dto.CenterId
    };

    await svc.CreateAsync(entity, saveNow: false, ct);
    await svc.SaveChangesAsync(ct);
    return Results.Created($"/api/subjects/{entity.SubjectId}", entity);
});

// Update
app.MapPut("/api/subjects/{id:long}", async (
    long id,
    ICrudService<Subject, long> svc,
    SubjectUpdateDto dto,
    CancellationToken ct) =>
{
    var found = await svc.GetByIdAsync(id, asNoTracking: false, includes: null, ct);
    if (found is null) return Results.NotFound();

    found.SubjectName = dto.SubjectName ?? found.SubjectName;
    found.GradeLevel = dto.GradeLevel ?? found.GradeLevel;

    await svc.UpdateAsync(found, saveNow: false, ct);
    await svc.SaveChangesAsync(ct);
    return Results.NoContent();
});

// Delete
app.MapDelete("/api/subjects/{id:long}", async (
    long id,
    ICrudService<Subject, long> svc,
    CancellationToken ct) =>
{
    var found = await svc.GetByIdAsync(id, asNoTracking: false, includes: null, ct);
    if (found is null) return Results.NotFound();

    await svc.DeleteAsync(found, saveNow: false, ct);
    await svc.SaveChangesAsync(ct);
    return Results.NoContent();
});

// Count + Exists
app.MapGet("/api/subjects/count", async (
    ICrudService<Subject, long> svc,
    string? search,
    CancellationToken ct) =>
{
    Expression<Func<Subject, bool>>? predicate = null;
    if (!string.IsNullOrWhiteSpace(search))
        predicate = s => s.SubjectName.Contains(search);

    var total = await svc.CountAsync(predicate, ct);
    return Results.Ok(new { total });
});

app.MapGet("/api/subjects/exists", async (
    ICrudService<Subject, long> svc,
    string name,
    CancellationToken ct) =>
{
    var yes = await svc.ExistsAsync(s => s.SubjectName == name, ct);
    return Results.Ok(new { exists = yes });
});

 // ========================= Users (GUID key) =========================

app.MapPost("/api/users", async (
    ICrudService<User, Guid> svc,
    UserCreateDto dto,
    CancellationToken ct) =>
{
    var user = new User
    {
        Username = dto.Username,
        Email = dto.Email,
        PasswordHash = dto.PasswordHash,
        FullName = dto.FullName,
        RoleDesc = dto.RoleDesc,
        IsActive = dto.IsActive
    };

    await svc.CreateAsync(user, saveNow: false, ct);
    await svc.SaveChangesAsync(ct);
    return Results.Created($"/api/users/{user.UserId}", user);
});

app.MapGet("/api/users/{id:guid}", async (
    Guid id,
    ICrudService<User, Guid> svc,
    CancellationToken ct) =>
{
    var found = await svc.GetByIdAsync(id, asNoTracking: true, includes: null, ct);
    return found is null ? Results.NotFound() : Results.Ok(found);
});


// ===== Centers (GUID key) – cần User (ManagerId) đã tồn tại ===== 

app.MapPost("/api/centers", async (
    ICrudService<Center, Guid> svc,
    CenterCreateDto dto,
    CancellationToken ct) =>
{
    var center = new Center
    {
        CenterName = dto.CenterName,
        CenterAddress = dto.CenterAddress,
        CenterEmail = dto.CenterEmail,
        Phone = dto.Phone,
        Logo = dto.Logo,
        ManagerId = dto.ManagerId,
        IsActive = dto.IsActive
    };

    await svc.CreateAsync(center, saveNow: false, ct);
    await svc.SaveChangesAsync(ct);
    return Results.Created($"/api/centers/{center.CenterId}", center);
});


// ===== Classes (GUID key) – includes Subject, Teacher(User) ===== 

app.MapPost("/api/classes", async (
    ICrudService<Class, Guid> svc,
    ClassCreateDto dto,
    CancellationToken ct) =>
{
    var cls = new Class
    {
        ClassName = dto.ClassName,
        SubjectId = dto.SubjectId,
        TeacherId = dto.TeacherId,
        ClassAddress = dto.ClassAddress,
        UnitPrice = dto.UnitPrice,
        TotalSessions = dto.TotalSessions,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        ScheduleDesc = dto.ScheduleDesc,
        CenterId = dto.CenterId,
        ClassStatus = dto.ClassStatus
    };

    await svc.CreateAsync(cls, saveNow: false, ct);
    await svc.SaveChangesAsync(ct);
    return Results.Created($"/api/classes/{cls.ClassId}", cls);
});

app.MapGet("/api/classes", async (
    ICrudService<Class, Guid> svc,
    int pageIndex = 1,
    int pageSize = 20,
    CancellationToken ct = default) =>
{
    var includes = new Expression<Func<Class, object>>[]
    {
        c => c.Subject!,
        c => c.Teacher!
    };

    var page = await svc.ListAsync(
        predicate: null,
        orderBy: q => q.OrderBy(c => c.ClassName),
        pageIndex: pageIndex,
        pageSize: pageSize,
        asNoTracking: true,
        includes: includes,
        ct: ct);

    return Results.Ok(page);
});


// ===== ClassSchedules (BIGINT key) =====

app.MapPost("/api/schedules", async (
    ICrudService<ClassSchedule, long> svc,
    ScheduleCreateDto dto,
    CancellationToken ct) =>
{
    var sch = new ClassSchedule
    {
        ClassId = dto.ClassId,
        SessionDate = dto.SessionDate,
        RoomName = dto.RoomName,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        SlotOrder = dto.SlotOrder,
        ScheduleLabel = dto.ScheduleLabel,
        ScheduleNote = dto.ScheduleNote
    };

    await svc.CreateAsync(sch, saveNow: false, ct);
    await svc.SaveChangesAsync(ct);
    return Results.Created($"/api/schedules/{sch.ScheduleId}", sch);
});

app.MapGet("/api/schedules", async (
    ICrudService<ClassSchedule, long> svc,
    Guid classId,
    CancellationToken ct) =>
{
    var items = await svc.ListAsync(
        predicate: s => s.ClassId == classId,
        orderBy: q => q.OrderBy(s => s.SessionDate).ThenBy(s => s.SlotOrder),
        pageIndex: 1, pageSize: 100,
        asNoTracking: true,
        includes: null,
        ct: ct);

    return Results.Ok(items);
});

// ================= DTOs =================
public sealed record SubjectCreateDto(string SubjectName, Guid CenterId, string? GradeLevel);
public sealed record SubjectUpdateDto(string? SubjectName, string? GradeLevel);

public sealed record UserCreateDto(
    string Username,
    string Email,
    string PasswordHash,
    string? FullName,
    string? RoleDesc,
    bool IsActive = true);

public sealed record CenterCreateDto(
    string CenterName,
    Guid ManagerId,
    string? CenterAddress,
    string? CenterEmail,
    string? Phone,
    string? Logo,
    bool IsActive = true);

public sealed record ClassCreateDto(
    string ClassName,
    long SubjectId,
    Guid TeacherId,
    Guid CenterId,
    decimal? UnitPrice,
    int? TotalSessions,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ScheduleDesc,
    string? ClassAddress,
    string? ClassStatus);

public sealed record ScheduleCreateDto(
    Guid ClassId,
    DateOnly SessionDate,
    string? RoomName,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? SlotOrder,
    string? ScheduleLabel,
    string? ScheduleNote);

*/
