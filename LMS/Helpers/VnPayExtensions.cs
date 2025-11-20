using VNPAY.Extensions;

namespace LMS.Helpers;

public static class VnPayExtensions
{
    public static IServiceCollection AddVnPayConfig(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        var vnPayConfig = configuration.GetSection("VnPay");

        services.AddVnpayClient(config =>
        {
            config.TmnCode = vnPayConfig["TmnCode"];
            config.HashSecret = vnPayConfig["HashSecret"];
            config.CallbackUrl = vnPayConfig["CallBackUrl"];
            config.BaseUrl = vnPayConfig["BaseUrl"];
            config.Version = vnPayConfig["Version"];
        });

        return services;
    }
}
