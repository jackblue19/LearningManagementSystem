using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Assessment;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.AspNetCore.Http;

namespace LMS.Services.Impl.TeacherService;

public class MaterialService : IMaterialService
{
    private readonly IMaterialRepository _materialRepo;
    private readonly IWebHostEnvironment _env;

    public MaterialService(
        IMaterialRepository materialRepo,
        IWebHostEnvironment env)
    {
        _materialRepo = materialRepo;
        _env = env;
    }

    public async Task<ClassMaterial> UploadMaterialAsync(
        Guid classId,
        Guid uploadedByUserId,
        string title,
        IFormFile file,
        string? materialType = null,
        string? note = null,
        Guid? examId = null,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required");

        // Create uploads directory if not exists
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "materials");
        Directory.CreateDirectory(uploadsPath);

        // Generate unique filename
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsPath, uniqueFileName);

        // Save file to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        // Create database record
        var material = new ClassMaterial
        {
            MaterialId = Guid.NewGuid(),
            ClassId = classId,
            Title = title,
            FileUrl = $"/uploads/materials/{uniqueFileName}",
            UploadedByUserId = uploadedByUserId,
            UploadedAt = DateTime.UtcNow,
            MaterialType = materialType ?? "document",
            Note = note,
            ExamId = examId,
            FileSize = file.Length,
            CloudObjectKey = uniqueFileName
        };

        return await _materialRepo.AddAsync(material, saveNow: true, ct);
    }

    public async Task<IReadOnlyList<ClassMaterial>> GetMaterialsByClassIdAsync(
        Guid classId,
        CancellationToken ct = default)
    {
        return await _materialRepo.GetMaterialsByClassIdAsync(classId, ct);
    }

    public async Task<IReadOnlyList<ClassMaterial>> GetMaterialsByTeacherIdAsync(
        Guid teacherId,
        CancellationToken ct = default)
    {
        return await _materialRepo.GetMaterialsByTeacherIdAsync(teacherId, ct);
    }

    public async Task<bool> DeleteMaterialAsync(
        Guid materialId,
        CancellationToken ct = default)
    {
        var material = await _materialRepo.GetByIdAsync(materialId, asNoTracking: false, ct);
        if (material == null)
            return false;

        // Delete physical file
        if (!string.IsNullOrEmpty(material.FileUrl))
        {
            var filePath = Path.Combine(_env.WebRootPath, material.FileUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        await _materialRepo.DeleteAsync(material, saveNow: true, ct);
        return true;
    }

    public async Task<ClassMaterial?> GetMaterialByIdAsync(
        Guid materialId,
        CancellationToken ct = default)
    {
        return await _materialRepo.GetByIdAsync(materialId, asNoTracking: true, ct);
    }
}
