using LMS.Models.Entities;
using LMS.Services.Interfaces;

namespace LMS.Models.ViewModels.StudentService.Api;

public static class ClassesApi
{
    public static void MapClassesApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/classes");

        group.MapGet("/", async (ICrudService<Class, Guid> service) =>
        {
            var items = await service.ListAsync();
            return Results.Ok(items.Items.Select(c => new ClassListDto(
                c.ClassId, c.ClassName, c.SubjectId, c.TeacherId, c.CenterId, c.ClassStatus
            )));
        });

        group.MapGet("/{id:guid}", async (Guid id, ICrudService<Class, Guid> service) =>
        {
            var c = await service.GetByIdAsync(id);
            if (c is null) return Results.NotFound();

            return Results.Ok(new ClassDetailDto(
                c.ClassId, c.ClassName, c.SubjectId, c.TeacherId,
                c.ClassAddress, c.UnitPrice, c.TotalSessions,
                c.StartDate, c.EndDate, c.ScheduleDesc, c.CoverImageUrl,
                c.CreatedAt, c.CenterId, c.ClassStatus
            ));
        });

        group.MapPost("/", async (ClassCreateDto dto, ICrudService<Class, Guid> service) =>
        {
            var c = new Class
            {
                ClassName = dto.ClassName,
                SubjectId = dto.SubjectId,
                TeacherId = dto.TeacherId,
                CenterId = dto.CenterId,
                ClassAddress = dto.ClassAddress,
                UnitPrice = dto.UnitPrice,
                TotalSessions = dto.TotalSessions,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ScheduleDesc = dto.ScheduleDesc,
                CoverImageUrl = dto.CoverImageUrl,
                ClassStatus = dto.ClassStatus
            };

            await service.CreateAsync(c, true);
            return Results.Created($"/classes/{c.ClassId}", c.ClassId);
        });

        group.MapPut("/{id:guid}", async (Guid id, ClassUpdateDto dto, ICrudService<Class, Guid> service) =>
        {
            var c = await service.GetByIdAsync(id, asNoTracking: false);
            if (c is null) return Results.NotFound();

            c.ClassName = dto.ClassName ?? c.ClassName;
            c.SubjectId = dto.SubjectId ?? c.SubjectId;
            c.TeacherId = dto.TeacherId ?? c.TeacherId;
            c.ClassAddress = dto.ClassAddress ?? c.ClassAddress;
            c.UnitPrice = dto.UnitPrice ?? c.UnitPrice;
            c.TotalSessions = dto.TotalSessions ?? c.TotalSessions;
            c.StartDate = dto.StartDate ?? c.StartDate;
            c.EndDate = dto.EndDate ?? c.EndDate;
            c.ScheduleDesc = dto.ScheduleDesc ?? c.ScheduleDesc;
            c.CoverImageUrl = dto.CoverImageUrl ?? c.CoverImageUrl;
            c.ClassStatus = dto.ClassStatus ?? c.ClassStatus;

            await service.UpdateAsync(c, true);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, ICrudService<Class, Guid> service) =>
        {
            await service.DeleteByIdAsync(id, true);
            return Results.NoContent();
        });
    }
}

