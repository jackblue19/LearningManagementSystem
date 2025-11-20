namespace LMS.Helpers;

public class VnPayDeploy
{
    //  VnPay API call back
    // IPN (server-to-server)

    /*app.MapMethods("/api/payments/vnpay-ipn", new[] { "GET", "POST" },
    async ([FromServices] IVnpayClient vnpay,
           [FromServices] CenterDbContext db, // đổi theo DbContext thực tế
           HttpContext ctx,
           ILoggerFactory lf) =>
    {
        var log = lf.CreateLogger("VNPAY-IPN");
        try
        {
            // Verify chữ ký & parse kết quả (ném VnpayException nếu sai)
            var _ = vnpay.GetPaymentResult(ctx.Request);

            // Đọc các field chuẩn từ query
            var rspCode = ctx.Request.Query["vnp_ResponseCode"].ToString();
            var txnRef = ctx.Request.Query["vnp_TxnRef"].ToString();    // mã tham chiếu giao dịch VNPAY
            var amount = ctx.Request.Query["vnp_Amount"].ToString();    // *100 (theo chuẩn VNPAY)

            // Idempotent: tìm payment theo txnRef bạn đã lưu ở bước Checkout
            var payment = await db.Payments.FirstOrDefaultAsync(p => p.VnpTxnRef == txnRef);
            if (payment is null)
            {
                log.LogWarning("IPN: Not found payment with vnp_TxnRef={TxnRef}", txnRef);
                return Results.BadRequest("TXN_REF_NOT_FOUND");
            }

            // Nếu đã xử lý rồi thì thoát sớm (idempotent)
            if (payment.PaymentStatus == "PAID")
                return Results.Ok("OK");

            // Kiểm tra số tiền (VNPAY trả về *100)
            if (long.TryParse(amount, out var vnpAmount100))
            {
                var vndFromVnp = vnpAmount100 / 100m;
                if (payment.Amount != vndFromVnp)
                {
                    log.LogWarning("IPN: amount mismatch. DB={DbAmt}, VNP={VnpAmt}", payment.Amount, vndFromVnp);
                    return Results.BadRequest("AMOUNT_MISMATCH");
                }
            }

            if (rspCode == "00")
            {
                payment.PaymentStatus = "PAID";
                payment.PaidAt = DateTime.UtcNow;
                payment.BankCode = ctx.Request.Query["vnp_BankCode"];
                payment.TnxNo = ctx.Request.Query["vnp_TransactionNo"];
            }
            else
            {
                payment.PaymentStatus = "FAILED";
            }

            await db.SaveChangesAsync();
            return Results.Ok("OK");
        }
        catch (VnpayException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    });*/


    /*app.MapGet("/api/vnpay/callback", (IVnpayClient vnpay, HttpContext ctx) =>
    {
        var _ = vnpay.GetPaymentResult(ctx.Request);
        var rspCode = ctx.Request.Query["vnp_ResponseCode"].ToString();
        var message = rspCode == "00"
            ? "Thanh toán thành công. Vui lòng chờ hệ thống xác nhận (IPN)."
            : $"Thanh toán chưa thành công (code={rspCode}).";
        return Results.Ok(new { message, query = ctx.Request.QueryString.Value });
    });*/

}
