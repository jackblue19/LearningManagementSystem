using LMS.Data;
using LMS.Models.Entities;
using LMS.Models.ViewModels.Manager;
using LMS.Repositories.Interfaces.Info;
using LMS.Services.Interfaces.CommonService;
using LMS.Services.Interfaces.ManagerService;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services.Impl.ManagerService;

public class TeacherManagementService : ITeacherManagementService
{
    private readonly IUserRepository _userRepo;
    private readonly CenterDbContext _db;
    private readonly IAuthService _authService;

    public TeacherManagementService(
        IUserRepository userRepo, 
        CenterDbContext db,
        IAuthService authService)
    {
        _userRepo = userRepo;
        _db = db;
        _authService = authService;
    }

    public async Task<(List<User> Teachers, int TotalCount)> GetTeachersAsync(
        string? searchTerm = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        try
        {
            var query = _db.Users.Where(u => u.RoleDesc == "teacher");

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim().ToLower();
                query = query.Where(u => 
                    u.Username.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(search)) ||
                    (u.Phone != null && u.Phone.Contains(search)));
            }

            // Apply active filter
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync(ct);

            var teachers = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (teachers, totalCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeacherManagementService] GetTeachersAsync error: {ex.Message}");
            return (new List<User>(), 0);
        }
    }

    public async Task<User?> GetTeacherByIdAsync(Guid teacherId, CancellationToken ct = default)
    {
        try
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == teacherId && u.RoleDesc == "teacher", ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeacherManagementService] GetTeacherByIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<(bool Success, User? Teacher, string? ErrorMessage)> CreateTeacherAsync(
        CreateTeacherViewModel model,
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

            // Create new teacher
            var teacher = new User
            {
                UserId = Guid.NewGuid(),
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                PasswordHash = _authService.HashPassword(model.Password),
                FullName = model.FullName.Trim(),
                Phone = model.Phone?.Trim(),
                RoleDesc = "teacher",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(teacher, saveNow: true, ct);

            Console.WriteLine($"[TeacherManagementService] Created teacher: {teacher.Username}");
            return (true, teacher, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeacherManagementService] CreateTeacherAsync error: {ex.Message}");
            return (false, null, $"Lỗi tạo tài khoản: {ex.Message}");
        }
    }

    public async Task<(bool Success, User? Teacher, string? ErrorMessage)> UpdateTeacherAsync(
        EditTeacherViewModel model,
        CancellationToken ct = default)
    {
        try
        {
            var teacher = await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == model.UserId && u.RoleDesc == "teacher", ct);

            if (teacher == null)
            {
                return (false, null, "Không tìm thấy giáo viên.");
            }

            // Check if email is changed and already exists
            if (teacher.Email != model.Email)
            {
                var emailExists = await _db.Users
                    .AnyAsync(u => u.Email == model.Email && u.UserId != model.UserId, ct);

                if (emailExists)
                {
                    return (false, null, "Email đã được sử dụng bởi người dùng khác.");
                }
            }

            // Update teacher info
            teacher.Email = model.Email.Trim();
            teacher.FullName = model.FullName.Trim();
            teacher.Phone = model.Phone?.Trim();
            teacher.IsActive = model.IsActive;
            teacher.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateAsync(teacher, saveNow: true, ct);

            Console.WriteLine($"[TeacherManagementService] Updated teacher: {teacher.Username}");
            return (true, teacher, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeacherManagementService] UpdateTeacherAsync error: {ex.Message}");
            return (false, null, $"Lỗi cập nhật thông tin: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteTeacherAsync(
        Guid teacherId,
        CancellationToken ct = default)
    {
        try
        {
            var teacher = await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == teacherId && u.RoleDesc == "teacher", ct);

            if (teacher == null)
            {
                return (false, "Không tìm thấy giáo viên.");
            }

            // Soft delete - just set IsActive to false
            teacher.IsActive = false;
            teacher.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateAsync(teacher, saveNow: true, ct);

            Console.WriteLine($"[TeacherManagementService] Deactivated teacher: {teacher.Username}");
            return (true, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeacherManagementService] DeleteTeacherAsync error: {ex.Message}");
            return (false, $"Lỗi vô hiệu hóa tài khoản: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ActivateTeacherAsync(
        Guid teacherId,
        CancellationToken ct = default)
    {
        try
        {
            var teacher = await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == teacherId && u.RoleDesc == "teacher", ct);

            if (teacher == null)
            {
                return (false, "Không tìm thấy giáo viên.");
            }

            teacher.IsActive = true;
            teacher.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateAsync(teacher, saveNow: true, ct);

            Console.WriteLine($"[TeacherManagementService] Activated teacher: {teacher.Username}");
            return (true, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeacherManagementService] ActivateTeacherAsync error: {ex.Message}");
            return (false, $"Lỗi kích hoạt tài khoản: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ResetTeacherPasswordAsync(
        Guid teacherId,
        string newPassword,
        CancellationToken ct = default)
    {
        try
        {
            var teacher = await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == teacherId && u.RoleDesc == "teacher", ct);

            if (teacher == null)
            {
                return (false, "Không tìm thấy giáo viên.");
            }

            teacher.PasswordHash = _authService.HashPassword(newPassword);
            teacher.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateAsync(teacher, saveNow: true, ct);

            Console.WriteLine($"[TeacherManagementService] Reset password for teacher: {teacher.Username}");
            return (true, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TeacherManagementService] ResetTeacherPasswordAsync error: {ex.Message}");
            return (false, $"Lỗi đặt lại mật khẩu: {ex.Message}");
        }
    }
}
