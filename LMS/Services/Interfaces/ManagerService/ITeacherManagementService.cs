using LMS.Models.Entities;
using LMS.Models.ViewModels.Manager;

namespace LMS.Services.Interfaces.ManagerService;

public interface ITeacherManagementService
{
    /// <summary>
    /// Get all teachers with optional search and pagination
    /// </summary>
    Task<(List<User> Teachers, int TotalCount)> GetTeachersAsync(
        string? searchTerm = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default);
    
    /// <summary>
    /// Get teacher by ID
    /// </summary>
    Task<User?> GetTeacherByIdAsync(Guid teacherId, CancellationToken ct = default);
    
    /// <summary>
    /// Create new teacher account
    /// </summary>
    Task<(bool Success, User? Teacher, string? ErrorMessage)> CreateTeacherAsync(
        CreateTeacherViewModel model,
        CancellationToken ct = default);
    
    /// <summary>
    /// Update teacher information
    /// </summary>
    Task<(bool Success, User? Teacher, string? ErrorMessage)> UpdateTeacherAsync(
        EditTeacherViewModel model,
        CancellationToken ct = default);
    
    /// <summary>
    /// Delete teacher (soft delete - set IsActive = false)
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> DeleteTeacherAsync(
        Guid teacherId,
        CancellationToken ct = default);
    
    /// <summary>
    /// Activate teacher account
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> ActivateTeacherAsync(
        Guid teacherId,
        CancellationToken ct = default);
    
    /// <summary>
    /// Reset teacher password
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> ResetTeacherPasswordAsync(
        Guid teacherId,
        string newPassword,
        CancellationToken ct = default);
}
