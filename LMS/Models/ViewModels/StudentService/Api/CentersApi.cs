using LMS.Models.Entities;
using LMS.Services.Interfaces;

namespace LMS.Models.ViewModels.StudentService.Api;

public static class CentersApi
{
    public static void MapCentersApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/centers");

        group.MapGet("/", async (ICrudService<Center, Guid> service) =>
        {
            var items = await service.ListAsync();
            return Results.Ok(items.Items.Select(c => new CenterListDto(
                c.CenterId, c.CenterName, c.CenterAddress, c.Phone, c.IsActive, c.CenterEmail
            )));
        });

        group.MapGet("/{id:guid}", async (Guid id, ICrudService<Center, Guid> service) =>
        {
            var c = await service.GetByIdAsync(id);
            if (c is null) return Results.NotFound();

            return Results.Ok(new CenterDetailDto(
                c.CenterId, c.CenterName, c.CenterAddress, c.Phone, c.IsActive,
                c.CenterEmail, c.Logo, c.ManagerId, c.CreatedAt
            ));
        });

        group.MapPost("/", async (CenterCreateDto dto, ICrudService<Center, Guid> service) =>
        {
            var c = new Center
            {
                CenterName = dto.CenterName,
                CenterAddress = dto.CenterAddress,
                Phone = dto.Phone,
                CenterEmail = dto.CenterEmail,
                ManagerId = dto.ManagerId,
                IsActive = true
            };

            await service.CreateAsync(c, true);
            return Results.Created($"/centers/{c.CenterId}", c.CenterId);
        });

        group.MapPut("/{id:guid}", async (Guid id, CenterUpdateDto dto, ICrudService<Center, Guid> service) =>
        {
            var c = await service.GetByIdAsync(id, asNoTracking: false);
            if (c is null) return Results.NotFound();

            c.CenterName = dto.CenterName ?? c.CenterName;
            c.CenterAddress = dto.CenterAddress ?? c.CenterAddress;
            c.Phone = dto.Phone ?? c.Phone;
            c.CenterEmail = dto.CenterEmail ?? c.CenterEmail;
            c.Logo = dto.Logo ?? c.Logo;
            if (dto.IsActive is not null) c.IsActive = dto.IsActive.Value;

            await service.UpdateAsync(c, true);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, ICrudService<Center, Guid> service) =>
        {
            await service.DeleteByIdAsync(id, true);
            return Results.NoContent();
        });
    }
}

