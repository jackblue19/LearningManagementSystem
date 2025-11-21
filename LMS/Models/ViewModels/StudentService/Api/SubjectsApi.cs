using LMS.Models.Entities;
using LMS.Services.Interfaces;

namespace LMS.Models.ViewModels.StudentService.Api;

public static class SubjectsApi
{
    public static void MapSubjectsApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/subjects");

        group.MapGet("/", async (ICrudService<Subject, long> service) =>
        {
            var items = await service.ListAsync();
            return Results.Ok(items.Items.Select(s => new SubjectListDto(
                s.SubjectId, s.SubjectName, s.GradeLevel, s.CenterId
            )));
        });

        group.MapGet("/{id:long}", async (long id, ICrudService<Subject, long> service) =>
        {
            var s = await service.GetByIdAsync(id);
            if (s is null) return Results.NotFound();

            return Results.Ok(new SubjectDetailDto(
                s.SubjectId, s.SubjectName, s.GradeLevel, s.CenterId, s.CreatedAt
            ));
        });

        group.MapPost("/", async (SubjectCreateDto dto, ICrudService<Subject, long> service) =>
        {
            var s = new Subject
            {
                SubjectName = dto.SubjectName,
                GradeLevel = dto.GradeLevel,
                CenterId = dto.CenterId
            };

            await service.CreateAsync(s, true);
            return Results.Created($"/subjects/{s.SubjectId}", s.SubjectId);
        });

        group.MapPut("/{id:long}", async (long id, SubjectUpdateDto dto, ICrudService<Subject, long> service) =>
        {
            var s = await service.GetByIdAsync(id, asNoTracking: false);
            if (s is null) return Results.NotFound();

            s.SubjectName = dto.SubjectName ?? s.SubjectName;
            s.GradeLevel = dto.GradeLevel ?? s.GradeLevel;

            await service.UpdateAsync(s, true);
            return Results.NoContent();
        });

        group.MapDelete("/{id:long}", async (long id, ICrudService<Subject, long> service) =>
        {
            await service.DeleteByIdAsync(id, true);
            return Results.NoContent();
        });
    }
}

