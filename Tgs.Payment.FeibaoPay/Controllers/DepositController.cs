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
public class DepositController(ILogger<DepositController> logger) : ControllerBase
{
    [HttpPost("callback", Name = "NotifyDeposit")]
    [Consumes(MediaTypeNames.Application.FormUrlEncoded)]
    [Produces(MediaTypeNames.Text.Plain)]
    public async Task<IActionResult> DepositCallbackAsync(
        [FromServices] FeibaoPayCryptor cryptor,
        [FromServices] IGetService<GetDepositFormQuery, DepositForm?> getService,
        [FromServices] IPaymentServiceClient paymentServiceClient,
        [FromServices] IPaymentClient paymentClient,
        [FromForm] CallbackDepositViewModel source)
    {
        logger.LogInformation("{source}", source);

        var formId = Guid.Parse(source.MerchantOrderNum);

        var depositForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (depositForm == null)
        {
            logger.LogWarning("Can't found deposit form({id}).", formId);

            return NotFound();
        }

        var sourceOrder = source.Order;

        var hashKey = depositForm.ApplyInfo!.Settings.HashKey;
        var hashIV = depositForm.ApplyInfo!.Settings.HashIV;

        var orderDecryptJson = cryptor.AesDecrypt(sourceOrder, hashKey, hashIV);

        var order = JsonSerializer.Deserialize<CallbackDepositOrder>(orderDecryptJson)!;

        var paymentReturn = new FeibaoPayDepositReturn()
        {
            Status = order.Status,
            FormId = formId,
            ThirdPartyOrderNo = source.MerchantOrderNum,
            OrderAmount = decimal.Parse(order.Amount),
            GateWay = order.Gateway,
        };

        var fsm = PaymentStateMachineFactory.CreateDepositStateMachine(depositForm.Status);

        switch (paymentReturn.Status)
        {
            case "success":
                if (fsm.CanFire(PaymentTrigger.PaymentDepositCompleted))
                {
                    await fsm.FireAsync(PaymentTrigger.PaymentDepositCompleted).ConfigureAwait(false);

                    await paymentServiceClient.SubmitDepositFormStatusAsync(
                        depositForm.Id,
                        paymentReturn: JsonSerializer.Serialize(
                            paymentReturn,
                            FeibaoPayUtility.SerializerOptions),
                        cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                    logger.LogInformation("FeibaoPay Deposit Callback Submit Deposit Form Status(Deposit Form Id:{depositFormId}) Executed", depositForm.Id);

                    await paymentClient.ApproveDepositAsync(
                        depositForm.Id,
                        depositForm.BrandId,
                        paymentReturn.OrderAmount,
                        HttpContext.RequestAborted).ConfigureAwait(false);

                    logger.LogInformation("FeibaoPay Deposit Callback Approve Deposit(Deposit Form Id:{depositFormId}) Executed", depositForm.Id);
                }

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
