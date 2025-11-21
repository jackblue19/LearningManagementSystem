using LMS.Data;
using LMS.Repositories.Interfaces.Info;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class ManagerDashboardModel : PageModel
{
    private readonly IUserRepository _userRepo;
    private readonly IAuthService _authService;
    private readonly CenterDbContext _db;

    public ManagerDashboardModel(
        IUserRepository userRepo, 
        IAuthService authService,
        CenterDbContext db)
    {
        _userRepo = userRepo;
        _authService = authService;
        _db = db;
    }

    // Dashboard Statistics
    public DashboardStats Stats { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public List<RoomStatus> RoomStatuses { get; set; } = new();
    public List<UpcomingClass> UpcomingClasses { get; set; } = new();
    public RevenueData Revenue { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if user needs to setup password (Google OAuth users)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            var user = await _userRepo.GetByIdAsync(userGuid);
            if (user != null && _authService.IsOAuthTempPassword(user.PasswordHash))
            {
                return RedirectToPage("/Common/SetupPassword");
            }
        }

        await LoadDashboardDataAsync();
        return Page();
    }

    private async Task LoadDashboardDataAsync()
    {
        var today = DateTime.Now.Date;
        var thisMonth = new DateTime(today.Year, today.Month, 1);
        var nextMonth = thisMonth.AddMonths(1);

        // Load statistics
        Stats = new DashboardStats
        {
            TotalStudents = await _db.Users.CountAsync(u => u.RoleDesc == "student"),
            ActiveStudents = await _db.ClassRegistrations
                .Where(cr => cr.RegistrationStatus == "approved")
                .Select(cr => cr.StudentId)
                .Distinct()
                .CountAsync(),
            TotalTeachers = await _db.Users.CountAsync(u => u.RoleDesc == "teacher"),
            ActiveTeachers = await _db.Classes
                .Where(c => c.StartDate.HasValue && c.EndDate.HasValue && 
                           c.StartDate.Value <= DateOnly.FromDateTime(today) && 
                           c.EndDate.Value >= DateOnly.FromDateTime(today))
                .Select(c => c.TeacherId)
                .Distinct()
                .CountAsync(),
            TotalClasses = await _db.Classes.CountAsync(),
            ActiveClasses = await _db.Classes
                .Where(c => c.StartDate.HasValue && c.EndDate.HasValue)
                .CountAsync(c => c.StartDate.Value <= DateOnly.FromDateTime(today) && c.EndDate.Value >= DateOnly.FromDateTime(today)),
            TotalRooms = await _db.Rooms.CountAsync(),
            AvailableRooms = await _db.Rooms.CountAsync(r => r.IsActive == true),
            PendingRegistrations = await _db.ClassRegistrations
                .CountAsync(cr => cr.RegistrationStatus == "pending"),
            UnreadNotifications = await _db.Notifications
                .CountAsync(n => !n.IsRead && n.CreatedAt >= today.AddDays(-7))
        };

        // Revenue data
        var thisMonthPayments = await _db.Payments
            .Where(p => p.CreatedAt >= thisMonth && p.CreatedAt < nextMonth)
            .ToListAsync();

        var lastMonth = thisMonth.AddMonths(-1);
        var lastMonthPayments = await _db.Payments
            .Where(p => p.CreatedAt >= lastMonth && p.CreatedAt < thisMonth)
            .ToListAsync();

        Revenue = new RevenueData
        {
            ThisMonth = thisMonthPayments.Sum(p => p.Amount),
            LastMonth = lastMonthPayments.Sum(p => p.Amount),
            TotalPaid = thisMonthPayments.Count(p => p.PaymentStatus == "paid"),
            TotalPending = thisMonthPayments.Count(p => p.PaymentStatus == "pending")
        };

        // Room status for today
        var schedulesToday = await _db.ClassSchedules
            .Include(cs => cs.Room)
            .Include(cs => cs.Class)
            .Where(cs => cs.SessionDate == DateOnly.FromDateTime(today))
            .ToListAsync();

        var allRooms = await _db.Rooms
            .Where(r => r.IsActive == true)
            .ToListAsync();

        RoomStatuses = allRooms.Select(room => new RoomStatus
        {
            RoomName = room.RoomName ?? "",
            Capacity = room.Capacity ?? 0,
            IsOccupied = schedulesToday.Any(s => s.RoomId == room.RoomId),
            CurrentClass = schedulesToday
                .FirstOrDefault(s => s.RoomId == room.RoomId)?.Class?.ClassName
        }).ToList();

        // Upcoming classes (next 7 days)
        var todayOnly = DateOnly.FromDateTime(today);
        var nextWeek = DateOnly.FromDateTime(today.AddDays(7));
        
        UpcomingClasses = await _db.Classes
            .Where(c => c.StartDate.HasValue && c.StartDate.Value >= todayOnly && c.StartDate.Value <= nextWeek)
            .OrderBy(c => c.StartDate)
            .Take(5)
            .Select(c => new UpcomingClass
            {
                ClassName = c.ClassName ?? "",
                StartDate = c.StartDate.HasValue ? c.StartDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.Now,
                TeacherName = c.Teacher != null ? c.Teacher.FullName : "Chưa phân công",
                StudentCount = _db.ClassRegistrations
                    .Count(cr => cr.ClassId == c.ClassId && cr.RegistrationStatus == "approved")
            })
            .ToListAsync();

        // Recent activities (last 10)
        var recentRegistrations = await _db.ClassRegistrations
            .Include(cr => cr.Student)
            .Include(cr => cr.Class)
            .OrderByDescending(cr => cr.RegisteredAt)
            .Take(10)
            .ToListAsync();

        RecentActivities = recentRegistrations.Select(cr => new RecentActivity
        {
            Type = "registration",
            Description = $"{cr.Student?.FullName} đăng ký lớp {cr.Class?.ClassName}",
            Timestamp = cr.RegisteredAt,
            Status = cr.RegistrationStatus ?? "pending"
        }).ToList();
    }

    public class DashboardStats
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveTeachers { get; set; }
        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int PendingRegistrations { get; set; }
        public int UnreadNotifications { get; set; }
    }

    public class RevenueData
    {
        public decimal ThisMonth { get; set; }
        public decimal LastMonth { get; set; }
        public int TotalPaid { get; set; }
        public int TotalPending { get; set; }
        public decimal GrowthPercentage => LastMonth > 0 
            ? Math.Round(((ThisMonth - LastMonth) / LastMonth) * 100, 1) 
            : 0;
    }

    public class RoomStatus
    {
        public string RoomName { get; set; } = "";
        public int Capacity { get; set; }
        public bool IsOccupied { get; set; }
        public string? CurrentClass { get; set; }
    }

    public class UpcomingClass
    {
        public string ClassName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public string? TeacherName { get; set; }
        public int StudentCount { get; set; }
    }

    public class RecentActivity
    {
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "";
    }
}
