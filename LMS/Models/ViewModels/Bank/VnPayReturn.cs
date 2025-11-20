namespace LMS.Models.ViewModels.Bank;

public sealed record VnPayReturn(
    Guid PaymentId,          // map từ vnp_TxnRef (Guid "N")
    decimal AmountVnd,       // vnp_Amount / 100
    string ResponseCode,     // "00" = success
    string? BankCode,
    string? TransactionNo    // vnp_TransactionNo
);
