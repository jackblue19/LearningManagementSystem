using LMS.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace LMS.Services.Interfaces.TeacherService;

public interface IMaterialService
{
    Task<ClassMaterial> UploadMaterialAsync(
        Guid classId,
        Guid uploadedByUserId,
        string title,
        IFormFile file,
        string? materialType = null,
        string? note = null,
        Guid? examId = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<ClassMaterial>> GetMaterialsByClassIdAsync(
        Guid classId,
        CancellationToken ct = default);

    Task<IReadOnlyList<ClassMaterial>> GetMaterialsByTeacherIdAsync(
        Guid teacherId,
        CancellationToken ct = default);

    Task<bool> DeleteMaterialAsync(
        Guid materialId,
        CancellationToken ct = default);

    Task<ClassMaterial?> GetMaterialByIdAsync(
        Guid materialId,
        CancellationToken ct = default);
}
