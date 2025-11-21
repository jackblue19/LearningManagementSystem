using LMS.Models.Entities;
using LMS.Services.Interfaces;

namespace LMS.Models.ViewModels.StudentService.Api;

public static class ExamsApi
{
    public static void MapExamsApi(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/exams");

        g.MapGet("/", async (ICrudService<Exam, Guid> svc) =>
        {
            var items = await svc.ListAsync();
            return Results.Ok(items.Items.Select(e => new ExamListDto(
                e.ExamId, e.ClassId, e.Title,
                e.ExamType,
                e.ExamDate is null ? null : (e.ExamDate.Value),
                e.TeacherId
            )));
        });

        g.MapGet("/{id:guid}", async (Guid id, ICrudService<Exam, Guid> svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            if (e is null) return Results.NotFound();

            return Results.Ok(new ExamDetailDto(
                e.ExamId, e.ClassId, e.Title,
                e.ExamType,
                e.ExamDate is null ? null : (e.ExamDate.Value),
                e.TeacherId,
                e.MaxScore, e.DurationMin,
                e.ExamDesc, e.ExamStatus
            ));
        });

        g.MapPost("/", async (ExamCreateDto dto, ICrudService<Exam, Guid> svc) =>
        {
            var e = new Exam
            {
                ClassId = dto.ClassId,
                Title = dto.Title,
                ExamType = dto.ExamType,
                ExamDate = dto.ExamDate,
                TeacherId = dto.TeacherId,
                MaxScore = dto.MaxScore,
                DurationMin = dto.DurationMin,
                ExamDesc = dto.ExamDesc,
                ExamStatus = dto.ExamStatus
            };

            await svc.CreateAsync(e, true);
            return Results.Created($"/exams/{e.ExamId}", e.ExamId);
        });

        g.MapPut("/{id:guid}", async (Guid id, ExamUpdateDto dto, ICrudService<Exam, Guid> svc) =>
        {
            var e = await svc.GetByIdAsync(id, asNoTracking: false);
            if (e is null) return Results.NotFound();

            e.Title = dto.Title ?? e.Title;
            e.ExamType = dto.ExamType ?? e.ExamType;
            e.ExamDate = dto.ExamDate ?? e.ExamDate;
            e.MaxScore = dto.MaxScore ?? e.MaxScore;
            e.DurationMin = dto.DurationMin ?? e.DurationMin;
            e.ExamDesc = dto.ExamDesc ?? e.ExamDesc;
            e.ExamStatus = dto.ExamStatus ?? e.ExamStatus;

            await svc.UpdateAsync(e, true);
            return Results.NoContent();
        });

        g.MapDelete("/{id:guid}", async (Guid id, ICrudService<Exam, Guid> svc) =>
        {
            await svc.DeleteByIdAsync(id, true);
            return Results.NoContent();
        });
    }
}

