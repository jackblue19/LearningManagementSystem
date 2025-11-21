using LMS.Models.Entities;
using LMS.Services.Interfaces;

namespace LMS.Models.ViewModels.StudentService;

public static class ExamResultsApi
{
    public static void MapExamResultsApi(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/exam-results");

        g.MapGet("/exam/{examId:guid}", async (Guid examId, ICrudService<ExamResult, long> svc) =>
        {
            var items = await svc.ListAsync(x => x.ExamId == examId);
            return Results.Ok(items.Items.Select(r => new ExamResultDto(
                r.ResultId, r.ExamId, r.StudentId, r.Score, r.Note
            )));
        });

        g.MapPost("/", async (ExamResultCreateDto dto, ICrudService<ExamResult, long> svc) =>
        {
            var r = new ExamResult
            {
                ExamId = dto.ExamId,
                StudentId = dto.StudentId,
                Score = dto.Score,
                Note = dto.Note
            };

            await svc.CreateAsync(r, true);
            return Results.Created($"/exam-results/{r.ResultId}", r.ResultId);
        });

        g.MapPut("/{id:long}", async (long id, ExamResultUpdateDto dto, ICrudService<ExamResult, long> svc) =>
        {
            var r = await svc.GetByIdAsync(id, asNoTracking: false);
            if (r is null) return Results.NotFound();

            r.Score = dto.Score ?? r.Score;
            r.Note = dto.Note ?? r.Note;

            await svc.UpdateAsync(r, true);
            return Results.NoContent();
        });

        g.MapDelete("/{id:long}", async (long id, ICrudService<ExamResult, long> svc) =>
        {
            await svc.DeleteByIdAsync(id, true);
            return Results.NoContent();
        });
    }
}
