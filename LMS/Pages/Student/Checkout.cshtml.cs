using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using VNPAY;
using LMS.Data;
using LMS.Models.Entities;

namespace LMS.Pages.Student;

public class CheckoutModel : PageModel
{
    private readonly IVnpayClient _vnpay;
    private readonly CenterDbContext _db;

    public CheckoutModel(IVnpayClient vnpay, CenterDbContext db)
    {
        _vnpay = vnpay;
        _db = db;
    }

    // query
    [BindProperty(SupportsGet = true)] public Guid StudentId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid ClassId { get; set; }

    // form
    [BindProperty] public string Description { get; set; } = string.Empty;

    // view
    public string StudentName { get; private set; } = "";
    public string ClassName { get; private set; } = "";
    public string SubjectName { get; private set; } = "";
    public decimal? UnitPrice { get; private set; }
    public int? TotalSessions { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? ScheduleDesc { get; private set; }
    public decimal Amount { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // ngay đầu OnGetAsync()
        //if (ClassId == Guid.Empty && StudentId != Guid.Empty)
        if (ClassId == Guid.Empty && StudentId.Equals("cc767778-b533-4405-af81-dd4dcb820240"))
        {
            var lastClassId = await _db.ClassRegistrations
            .Where(r => r.StudentId == StudentId && r.RegistrationStatus == "approved")
            .OrderByDescending(r => r.RegisteredAt)
            .Select(r => r.ClassId)
            .FirstOrDefaultAsync();

            if (lastClassId != Guid.Empty) ClassId = lastClassId;
        }

        if (ClassId == Guid.Empty)
        {
            // chưa có class → đưa về MyCourses để chọn 
            //return Redirect($"/Student/MyCourses?studentId={StudentId}");
            return Redirect($"/Student/Courses?studentId=cc767778-b533-4405-af81-dd4dcb820240");
        }

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == StudentId);
        StudentName = user?.FullName ?? user?.Username ?? "(unknown)";

        var cls = await _db.Classes.Include(c => c.Subject)
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(c => c.ClassId == ClassId);
        if (cls == null) return NotFound("Class not found.");

        ClassName = cls.ClassName;
        SubjectName = cls.Subject?.SubjectName ?? "";
        UnitPrice = cls.UnitPrice;
        TotalSessions = cls.TotalSessions;
        StartDate = cls.StartDate;
        EndDate = cls.EndDate;
        ScheduleDesc = cls.ScheduleDesc;
        Amount = (cls.UnitPrice ?? 0m) * (cls.TotalSessions ?? 0);

        if (string.IsNullOrWhiteSpace(Description))
            Description = $"Thanh toán học phí {ClassName}";

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Tính lại để chống sửa form
        var cls = await _db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.ClassId == ClassId);
        if (cls == null) return NotFound("Class not found.");
        var amount = (cls.UnitPrice ?? 0m) * (cls.TotalSessions ?? 0);

        // Tạo URL VNPAY (BaseUrl, TmnCode... lấy từ appsettings qua IVnpayClient)
        var info = _vnpay.CreatePaymentUrl(
            (int)amount,
            string.IsNullOrWhiteSpace(Description) ? $"Thanh toán học phí {cls.ClassName}" : Description,
            VNPAY.Models.Enums.BankCode.ANY // dùng đúng enum, fully-qualified để tránh trùng tên
        );
        var paymentUrl = info.Url;

        // Lấy vnp_TxnRef
        var uri = new Uri(paymentUrl);
        var qs = QueryHelpers.ParseQuery(uri.Query);
        var txnRef = qs.TryGetValue("vnp_TxnRef", out var v) ? v.ToString() : Guid.NewGuid().ToString("N");

        // Lưu Payment PENDING
        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            VnpTxnRef = txnRef,
            Amount = amount,
            PaymentStatus = "PENDING",
            PaidAt = null,                 // để khi callback "PAID" mới set
            StudentId = StudentId,
            ClassId = ClassId,             // cần cột này trên bảng Payments
            BankCode = null,
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return Redirect(paymentUrl);
    }
}
