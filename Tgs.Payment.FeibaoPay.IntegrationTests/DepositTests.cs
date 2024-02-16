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

public class DepositTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代收回調_訂單狀態成功()
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

        var formId = Guid.Parse("70363ff7-f32d-4e97-a343-ad93359863aa");
        var brandId = Guid.NewGuid();
        var merchant = "feibaotest";
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var request = new Dictionary<string, string>
            {
                { "code", "0"},
                { "msg", "ok" },
                { "action", "gcash_qr_callback" },
                { "merchant", "feibaotest" },
                { "merchant_order_num","70363ff7-f32d-4e97-a343-ad93359863aa" },
                { "order","WoGfa2weFG91Fi%2FjHf09rxaDXR7FRJYGtiMTBB8KsRds5y6HJEI7Taja6%2BMd%0A7%2BlCrwwf%2FG%2BeDHoQ65hO2XzlHOcLgx5C7%2FpXjfPVmH8KeIm%2FejvAVh1CiHm%2B%0AI65xJrVLRT%2F5lmMcvqNiBtAIeEoOmgcULpyOxtrs8R6It9EANuXq93DEpRYh%0AZY9pWFh9BkjHsORf8xu1xhobZjzjGahITrWTTj7EYodu%2BiKgf1Rg34LH0TFV%0ApNUKeaDvh2lPKmmfw99XCEUw616QI%2BXcg69UKQ%3D%3D%0A" }
            };

        var content = new FormUrlEncodedContent(request);

        var applyInfo = new FeibaoPayDepositApplyInfo(
            new FeibaoPayDepositSettings(
                merchant,
                hashKey,
                hashIV,
                formId,
                "gcash_qr",
                "4ucmqplxb029",
                "127.0.0.1"),
             new FeibaoPayDepositResponse(
                200.0m,
                formId,
                new Uri("https://pay.qrph.io/07b0b700-6a9d-45f9-b2bd-6e149452ef8b"),
                "70363ff7-f32d-4e97-a343-ad93359863aa"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 200.0m,
                ThirdPartyOrderNo = "70363ff7-f32d-4e97-a343-ad93359863aa",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, FeibaoPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var paymentReturn = new FeibaoPayDepositReturn()
        {
            Status = "success",
            FormId = formId,
            ThirdPartyOrderNo = "70363ff7-f32d-4e97-a343-ad93359863aa",
            OrderAmount = 200.0m,
            GateWay = "gcash_qr"
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, FeibaoPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Is(paymentReturn.OrderAmount),
                Arg.Any<CancellationToken>());
    }
}
