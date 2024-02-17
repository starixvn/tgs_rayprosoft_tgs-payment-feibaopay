using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay.ApplicationServices;
using Tgs.Payment.FeibaoPay.Client;
using Tgs.Payment.FeibaoPay.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.FeibaoPay.Controllers;

[Route("[controller]")]
[ApiController]
public class WithdrawController(ILogger<WithdrawController> logger) : ControllerBase
{
    [HttpPost("callback", Name = "NotifyWithdraw")]
    [Consumes(MediaTypeNames.Application.FormUrlEncoded)]
    [Produces(MediaTypeNames.Text.Plain)]
    public async Task<IActionResult> WithdrawCallbackAsync(
        [FromServices] FeibaoPayCryptor cryptor,
        [FromServices] IGetService<GetWithdrawFormQuery, WithdrawForm?> getService,
        [FromServices] IPaymentServiceClient paymentServiceClient,
        [FromServices] IPaymentClient paymentClient,
        [FromForm] CallbackWithdrawViewModel source)
    {
        logger.LogInformation("{source}", source);

        var formId = Guid.Parse(source.MerchantOrderNum);

        var withdrawForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (withdrawForm == null)
        {
            logger.LogWarning("Can't found withdraw form({id}).", formId);
            return NotFound();
        }

        var sourceOrder = source.Order;

        var hashKey = withdrawForm.ApplyInfo!.Settings.HashKey;
        var hashIV = withdrawForm.ApplyInfo!.Settings.HashIV;

        var orderDecryptJson = cryptor.AesDecrypt(sourceOrder, hashKey, hashIV);

        var order = JsonSerializer.Deserialize<CallbackWithdrawOrder>(orderDecryptJson)!;

        var paymentReturn = new FeibaoPayWithdrawReturn()
        {
            FormId = formId,
            Amount = decimal.Parse(order.Amount),
            ThirdPartyOrderNo = source.MerchantOrderNum,
            Status = order.Status,
            Gateway = order.Gateway,
            MerchantOrderRemark = order.MerchantOrderRemark,
            Note = order.Note,
        };

        var fsm = PaymentStateMachineFactory.CreateWithdrawStateMachine(withdrawForm.Status);

        switch (paymentReturn.Status)
        {
            case "success":
                if (fsm.CanFire(PaymentTrigger.PaymentWithdrawCompleted))
                {
                    await fsm.FireAsync(PaymentTrigger.PaymentWithdrawCompleted).ConfigureAwait(false);

                    await paymentServiceClient.SubmitWithdrawFormStatusAsync(
                        withdrawForm.Id,
                        paymentReturn: JsonSerializer.Serialize(
                            paymentReturn,
                            FeibaoPayUtility.SerializerOptions),
                        cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                    await paymentClient.PaidWithdrawAsync(
                        withdrawForm.Id,
                        withdrawForm.BrandId).ConfigureAwait(false);
                }

                break;

            case "reverted":
            case "fail":
                if (fsm.CanFire(PaymentTrigger.Reject))
                {
                    await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

                    await paymentServiceClient.SubmitWithdrawFormStatusAsync(
                    withdrawForm.Id,
                        paymentReturn: JsonSerializer.Serialize(
                            paymentReturn,
                            FeibaoPayUtility.SerializerOptions),
                        cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                    await paymentClient.RejectWithdrawAsync(
                        withdrawForm.Id,
                        withdrawForm.BrandId,
                        HttpContext.RequestAborted).ConfigureAwait(false);
                }

                logger.LogWarning("FormId:{formId}, ThirdPartyOrderNo:{ThirdPartyOrderNo}, Msg:{Msg}",
                    formId,
                    paymentReturn.ThirdPartyOrderNo,
                    source.Msg);
                break;

            default:
                logger.LogWarning("FormId:{formId}, ThirdPartyOrderNo:{ThirdPartyOrderNo} , payment Status:{status}",
                    formId,
                    paymentReturn.ThirdPartyOrderNo,
                    paymentReturn.Status);
                return BadRequest();
        }

        return Ok();
    }
}
