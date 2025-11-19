using LMS.Models.Entities;
using LMS.Models.ViewModels.Auth;

namespace LMS.Services.Interfaces.CommonService;

public interface IAuthService
{
    /// <summary>
    /// Authenticate user with username/email and password
    /// </summary>
    Task<(bool Success, User? User, string? ErrorMessage)> LoginAsync(
        string usernameOrEmail, 
        string password, 
        CancellationToken ct = default);

    /// <summary>
    /// Register new user (default role: student)
    /// </summary>
    Task<(bool Success, User? User, string? ErrorMessage)> RegisterAsync(
        RegisterViewModel model, 
        CancellationToken ct = default);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(
        Guid userId, 
        string currentPassword, 
        string newPassword, 
        CancellationToken ct = default);

    /// <summary>
    /// Send password reset email
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(
        string email, 
        CancellationToken ct = default);

    /// <summary>
    /// Update user profile
    /// </summary>
    Task<(bool Success, User? User, string? ErrorMessage)> UpdateProfileAsync(
        EditProfileViewModel model, 
        CancellationToken ct = default);

    /// <summary>
    /// Verify password hash
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);

    /// <summary>
    /// Hash password
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Handle Google OAuth login
    /// </summary>
    Task<(bool Success, User? User, string? ErrorMessage, bool IsNewUser)> GoogleLoginAsync(
        string email, 
        string name, 
        string? picture, 
        CancellationToken ct = default);

    /// <summary>
    /// Check if password is temporary OAuth marker
    /// </summary>
    bool IsOAuthTempPassword(string passwordHash);
}
