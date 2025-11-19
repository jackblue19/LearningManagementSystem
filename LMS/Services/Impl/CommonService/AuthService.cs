using LMS.Data;
using LMS.Models.Entities;
using LMS.Models.ViewModels.Auth;
using LMS.Repositories.Interfaces.Info;
using LMS.Services.Interfaces.CommonService;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services.Impl.CommonService;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly CenterDbContext _db;

    public AuthService(IUserRepository userRepo, CenterDbContext db)
    {
        _userRepo = userRepo;
        _db = db;
    }

    public async Task<(bool Success, User? User, string? ErrorMessage)> LoginAsync(
        string usernameOrEmail, 
        string password, 
        CancellationToken ct = default)
    {
        try
        {
            // Find user by username or email
            var user = await _db.Users
                .FirstOrDefaultAsync(u => 
                    (u.Username == usernameOrEmail || u.Email == usernameOrEmail) 
                    && u.IsActive, ct);

            if (user == null)
            {
                return (false, null, "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
            {
                return (false, null, "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            return (true, user, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Lỗi đăng nhập: {ex.Message}");
        }
    }

    public async Task<(bool Success, User? User, string? ErrorMessage)> RegisterAsync(
        RegisterViewModel model, 
        CancellationToken ct = default)
    {
        try
        {
            // Check if username exists
            var existingUsername = await _db.Users
                .AnyAsync(u => u.Username == model.Username, ct);

            if (existingUsername)
            {
                return (false, null, "Tên đăng nhập đã tồn tại.");
            }

            // Check if email exists
            var existingEmail = await _db.Users
                .AnyAsync(u => u.Email == model.Email, ct);

            if (existingEmail)
            {
                return (false, null, "Email đã được sử dụng.");
            }

            // Create new user
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                PasswordHash = HashPassword(model.Password),
                FullName = model.FullName.Trim(),
                Phone = model.Phone?.Trim(),
                RoleDesc = "student", // Default role
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user, saveNow: true, ct);

            return (true, user, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Lỗi đăng ký: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(
        Guid userId, 
        string currentPassword, 
        string newPassword, 
        CancellationToken ct = default)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId, asNoTracking: false, ct);

            if (user == null)
            {
                return (false, "Không tìm thấy người dùng.");
            }

            // Verify current password
            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                return (false, "Mật khẩu hiện tại không đúng.");
            }

            // Update password
            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateAsync(user, saveNow: true, ct);

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi đổi mật khẩu: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(
        string email, 
        CancellationToken ct = default)
    {
        try
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, ct);

            if (user == null)
            {
                // Don't reveal if email exists for security
                return (true, null);
            }

            // TODO: Implement email sending logic
            // Generate reset token, save to database, send email
            // For now, just return success

            // In production, you would:
            // 1. Generate a secure token
            // 2. Save token with expiration to database
            // 3. Send email with reset link
            // var resetToken = Guid.NewGuid().ToString();
            // await _emailService.SendPasswordResetEmailAsync(email, resetToken);

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, User? User, string? ErrorMessage)> UpdateProfileAsync(
        EditProfileViewModel model, 
        CancellationToken ct = default)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(model.UserId, asNoTracking: false, ct);

            if (user == null)
            {
                return (false, null, "Không tìm thấy người dùng.");
            }

            // Check if email is changed and already exists
            if (user.Email != model.Email)
            {
                var emailExists = await _db.Users
                    .AnyAsync(u => u.Email == model.Email && u.UserId != model.UserId, ct);

                if (emailExists)
                {
                    return (false, null, "Email đã được sử dụng bởi người dùng khác.");
                }
            }

            // Update user info
            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim();
            user.Phone = model.Phone?.Trim();

            // Handle avatar upload
            if (model.AvatarFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream, ct);
                }

                user.Avatar = $"/uploads/avatars/{fileName}";
            }

            // Handle cover image upload
            if (model.CoverImageFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.CoverImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImageFile.CopyToAsync(stream, ct);
                }

                user.CoverImageUrl = $"/uploads/covers/{fileName}";
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user, saveNow: true, ct);

            return (true, user, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Lỗi cập nhật thông tin: {ex.Message}");
        }
    }

    public async Task<(bool Success, User? User, string? ErrorMessage, bool IsNewUser)> GoogleLoginAsync(
        string email, 
        string name, 
        string? picture, 
        CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"[AuthService] GoogleLoginAsync called - Email: {email}");
            
            // Check if user exists with this email
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user != null)
            {
                Console.WriteLine($"[AuthService] Existing user found - Username: {user.Username}, Role: {user.RoleDesc}");
                
                // User exists, update last login and avatar if needed
                if (!string.IsNullOrEmpty(picture) && string.IsNullOrEmpty(user.Avatar))
                {
                    user.Avatar = picture;
                }
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepo.UpdateAsync(user, saveNow: true, ct);

                return (true, user, null, false);
            }
            
            Console.WriteLine($"[AuthService] New user, creating account...");

            // Generate unique username from email
            var baseUsername = email.Split('@')[0];
            var username = baseUsername;
            var counter = 1;
            
            // Check if username exists and append number if needed
            while (await _db.Users.AnyAsync(u => u.Username == username, ct))
            {
                username = $"{baseUsername}{counter}";
                counter++;
            }

            // Create new user with Google info
            user = new User
            {
                UserId = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = HashPassword(OAUTH_TEMP_PASSWORD_MARKER), // Temporary marker for OAuth users
                FullName = name,
                Avatar = picture,
                RoleDesc = "student", // Default role
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            Console.WriteLine($"[AuthService] Creating user - Username: {username}, Role: student");
            
            await _userRepo.AddAsync(user, saveNow: true, ct);

            // Verify user was saved
            var savedUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId, ct);
            if (savedUser == null)
            {
                Console.WriteLine($"[AuthService] ERROR: User not saved to database!");
                return (false, null, "Không thể lưu thông tin người dùng vào database.", false);
            }

            Console.WriteLine($"[AuthService] User created successfully - UserId: {savedUser.UserId}");
            return (true, savedUser, null, true); // isNewUser = true
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Exception: {ex.Message}");
            Console.WriteLine($"[AuthService] StackTrace: {ex.StackTrace}");
            return (false, null, $"Lỗi đăng nhập Google: {ex.Message}", false);
        }
    }

    private const string OAUTH_TEMP_PASSWORD_MARKER = "__OAUTH_TEMP_PASSWORD__";

    public bool IsOAuthTempPassword(string passwordHash)
    {
        // Check if password is the temporary OAuth marker
        return passwordHash == HashPassword(OAUTH_TEMP_PASSWORD_MARKER);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        // Simple hash comparison (in production, use BCrypt or similar)
        return HashPassword(password) == passwordHash;
    }

    public string HashPassword(string password)
    {
        // Simple hash (in production, use BCrypt, Argon2, or PBKDF2)
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "LMS_SALT_2025");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
