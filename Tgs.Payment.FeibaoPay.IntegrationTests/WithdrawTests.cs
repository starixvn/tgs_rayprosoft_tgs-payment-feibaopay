using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay.Client;
using Xunit.Abstractions;

namespace Tgs.Payment.FeibaoPay.IntegrationTests;

public class WithdrawTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代付回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = FeibaoPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("4f3803ab-08e4-43c9-acc3-a06fa426fb1f");
        var brandId = Guid.NewGuid();
        var merchant = "feibaotest";
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var settings = new FeibaoPayWithdrawSettings(
            merchant,
            hashKey,
            hashIV,
            formId,
            "GXCHP",
            "testPerson",
            "11112222333344556",
            Gateway: "gcash",
            UserId: "4ucmqplxb029",
            UserIp: "127.0.0.1");

        var withdrawResponse = new FeibaoPayWithdrawResponse(formId);

        var applyInfo = new FeibaoPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, FeibaoPayUtility.SerializerOptions)
            });

        var request = new Dictionary<string, string>
        {
            { "code", "0"},
            { "msg", "ok" },
            { "action", "gcash_callback" },
            { "merchant", "feibaotest" },
            { "merchant_order_num", "4f3803ab-08e4-43c9-acc3-a06fa426fb1f" },
            { "order", "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrHh56R9dD0Nc4EOYMQQ%0AtTIMguTe5SLuAgSlanI2UD40XbKOtBNFTY8DGivWvgpFGv5MOsLXK%2BGeiiIu%0AhaKSKbDiuTLBrYCRdGxsf2nZuW0XYEBrMRJ7AIBoFi0p9vQbt%2BWNsknD0NRD%0A9BcCzSH5911aA%2B%2FrgWcnkKSpMyWTp5uh%2FAaB9wBKVWdfnXtm4AWJdw2iq5qt%0AS9DXFrQAMwaRU3N9Vy7cVo9c%2B3YdcFp687MYig%3D%3D%0A" }
        };

        var content = new FormUrlEncodedContent(request);

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var paymentReturn = new FeibaoPayWithdrawReturn()
        {
            FormId = formId,
            Amount = 100.0m,
            ThirdPartyOrderNo = "4f3803ab-08e4-43c9-acc3-a06fa426fb1f",
            Status = "success",
            Gateway = "gcash",
            Note = "",
            MerchantOrderRemark = ""
        };

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, FeibaoPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .PaidWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 代付回調_訂單狀態失敗()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = FeibaoPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("97e075a8-84b9-4822-bd41-a68d50441863");
        var brandId = Guid.NewGuid();
        var merchant = "feibaotest";
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var settings = new FeibaoPayWithdrawSettings(
            merchant,
            hashKey,
            hashIV,
            formId,
            "GXCHP",
            "testPerson",
            "11112222333344556",
            Gateway: "gcash",
            UserId: "4ucmqplxb029",
            UserIp: "127.0.0.1");

        var withdrawResponse = new FeibaoPayWithdrawResponse(formId);

        var applyInfo = new FeibaoPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, FeibaoPayUtility.SerializerOptions)
            });

        var request = new Dictionary<string, string>
        {
            { "code", "0"},
            { "msg", "ok" },
            { "action", "gcash_callback" },
            { "merchant", "feibaotest" },
            { "merchant_order_num", "97e075a8-84b9-4822-bd41-a68d50441863" },
            { "order", "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrHh56R9dD0Nc4EOYMQQ%5CntTIMr0XUevoyXrvpd3%2FVpDjtqYMX0SLidyNV%2FbkWlVqkiKGEQynZY1AJ8xHi%5CnnZ9mMicFdSftB7SuyXrcZFJIrXrVU0iN4ul8vo3iCHIaN10izM2dAbzf54JE%5CnwA5I5QsR2Ek%2Fan7CZpio1WfGpGnkSWCKCQTICxw%2Bj%2Bz8wDIkbu9wAZbP3lAp%5Cn8eG01L%2B8q880VWPy3w7labpSBffoON00ggR6vnOziLOf8WXEx7eKxgCWkz52%5Cnord1m4%2FvkE9rY1FhSPoX%5Cn" }
        };

        var content = new FormUrlEncodedContent(request);

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var paymentReturn = new FeibaoPayWithdrawReturn()
        {
            FormId = formId,
            Amount = 100.0m,
            ThirdPartyOrderNo = "97e075a8-84b9-4822-bd41-a68d50441863",
            Status = "fail",
            Gateway = "gcash",
            Note = "remaining balance: 94999.6",
            MerchantOrderRemark = ""
        };

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, FeibaoPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}
