using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.FeibaoPay.ViewModels;

public class CallbackDepositViewModel
{
    [FromForm(Name = "code")]
    public required string Code { get; set; }

    [FromForm(Name = "msg")]
    public required string Msg { get; set; }

    [FromForm(Name = "merchant")]
    public required string Merchant { get; set; }

    [FromForm(Name = "merchant_order_num")]
    public required string MerchantOrderNum { get; set; }

    [FromForm(Name = "action")]
    public required string Action { get; set; }

    [FromForm(Name = "order")]
    public required string Order { get; set; }
}
