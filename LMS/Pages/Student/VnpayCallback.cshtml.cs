using LMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VNPAY;
using VNPAY.Models.Exceptions;

namespace LMS.Pages.Student;

public class VnpayCallbackModel : PageModel
{
    private readonly IVnpayClient _vnpay;
    private readonly CenterDbContext _db;
    public string Message { get; private set; } = "";
    public string RawQuery { get; private set; } = "";

    public VnpayCallbackModel(IVnpayClient vnpay, CenterDbContext db)
    {
        _vnpay = vnpay;
        _db = db;
    }

    public async Task OnGetAsync()
    {
        try { var _ = _vnpay.GetPaymentResult(Request); }
        catch (VnpayException)
        {
        }

        var rspCode = Request.Query["vnp_ResponseCode"].ToString();
        var txnRef = Request.Query["vnp_TxnRef"].ToString();
        var bankCode = Request.Query["vnp_BankCode"].ToString();
        var amountStr = Request.Query["vnp_Amount"].ToString();
        var amountVnd = string.IsNullOrWhiteSpace(amountStr) ? 0m : (decimal.Parse(amountStr) / 100m);

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.VnpTxnRef == txnRef);
        if (payment is null)
        {
            Message = $"Không tìm thấy Payment với vnp_TxnRef={txnRef}";
            RawQuery = Request.QueryString.Value ?? "";
            return;
        }

        if (payment.PaymentStatus == "PAID")
        {
            Message = "Giao dịch đã được xác nhận trước đó (idempotent).";
            RawQuery = Request.QueryString.Value ?? "";
            return;
        }

        if (rspCode == "00")
        {
            if (payment.Amount != 0 && payment.Amount != amountVnd)
            {
                Message = $"Sai số tiền: DB={payment.Amount} VNP={amountVnd}";
            }
            else
            {
                payment.PaymentStatus = "PAID";
                payment.PaidAt = DateTime.UtcNow;
                payment.BankCode = bankCode;
                if (payment.Amount == 0) payment.Amount = amountVnd;
                await _db.SaveChangesAsync();

                Message = "Thanh toán thành công !!!."; // đang demo đó public thì t fix lại sau
            }
        }
        else
        {
            payment.PaymentStatus = "FAILED";
            await _db.SaveChangesAsync();
            Message = $"Thanh toán chưa thành công (code={rspCode}).";
        }

        //var rsp = Request.Query["vnp_ResponseCode"].ToString();
        //Message = rsp == "00"
        //    ? "Thanh toán thành công. Vui lòng chờ hệ thống xác nhận (IPN)."
        //    : $"Thanh toán chưa thành công (code={rsp}).";
        RawQuery = Request.QueryString.Value ?? "";
    }
}
