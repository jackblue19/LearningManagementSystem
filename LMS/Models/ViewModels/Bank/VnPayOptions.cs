namespace LMS.Models.ViewModels.Bank;

public sealed class VnPayOptions
{
    public string TmnCode { get; set; } = "";
    public string HashSecret { get; set; } = "";
    public string PaymentUrl { get; set; } = "";
    public string ReturnUrl { get; set; } = "";
    public string IpnUrl { get; set; } = "";
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
    public string CurrCode { get; set; } = "VND";
    public string Locale { get; set; } = "vn";
}

