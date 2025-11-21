using LMS.Models.Entities;
using LMS.Services.Interfaces;

namespace LMS.Models.ViewModels.StudentService.Api;

public static class UsersApi
{
    public static void MapUsersApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users");

        group.MapGet("/", async (ICrudService<User, Guid> service) =>
        {
            var items = await service.ListAsync();
            return Results.Ok(items.Items.Select(u => new UserListDto(
                u.UserId, u.Username, u.Email, u.FullName, u.Avatar, u.RoleDesc, u.IsActive
            )));
        });

        group.MapGet("/{id:guid}", async (Guid id, ICrudService<User, Guid> service) =>
        {
            var u = await service.GetByIdAsync(id);
            if (u is null) return Results.NotFound();

            return Results.Ok(new UserDetailDto(
                u.UserId, u.Username, u.Email, u.FullName, u.Avatar, u.Phone,
                u.RoleDesc, u.IsActive, u.CreatedAt, u.UpdatedAt, u.CoverImageUrl
            ));
        });

        group.MapPost("/", async (UserCreateDto dto, ICrudService<User, Guid> service) =>
        {
            var u = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = dto.PasswordHash,
                FullName = dto.FullName,
                Phone = dto.Phone,
                RoleDesc = dto.RoleDesc,
                IsActive = true
            };

            await service.CreateAsync(u, true);
            return Results.Created($"/users/{u.UserId}", u.UserId);
        });

        group.MapPut("/{id:guid}", async (Guid id, UserUpdateDto dto, ICrudService<User, Guid> service) =>
        {
            var u = await service.GetByIdAsync(id, asNoTracking: false);
            if (u is null) return Results.NotFound();

            u.FullName = dto.FullName ?? u.FullName;
            u.Phone = dto.Phone ?? u.Phone;
            u.Avatar = dto.Avatar ?? u.Avatar;
            u.CoverImageUrl = dto.CoverImageUrl ?? u.CoverImageUrl;
            if (dto.IsActive is not null) u.IsActive = dto.IsActive.Value;

            await service.UpdateAsync(u, true);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, ICrudService<User, Guid> service) =>
        {
            await service.DeleteByIdAsync(id, true);
            return Results.NoContent();
        });
    }
}
