using System.Net;
using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay;
using Tgs.Payment.FeibaoPay.Client;
using Tgs.Payment.FeibaoPay.MessageQueue;
using Tgs.Payment.Protos;

namespace FeibaoPay.Core.UnitTests;

public class WithdrawProcessorTests
{
    [Fact]
    public async Task 處理提款要求_銀行()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.feibaopay.com");

        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var fakeFeibaoPayClient = new FeibaoPayClient(
            httpClient,
            new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance),
            fakeTimeProvider,
            NullLogger<FeibaoPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var options = new FeibaoPayOptions
        {
            CallbackBaseUrl = new Uri("https://dev-payment.rpgp.cc/feibaopay"),
            PaymentApiUrl = new Uri("https://api.feibaopay.com"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };

        var sut = new WithdrawProcessor(
            fakeFeibaoPayClient,
            fakePaymentClient,
            new OptionsWrapper<FeibaoPayOptions>(options));

        var formId = Guid.Parse("d963d5fc-adbf-4882-b3f0-bf1b104ec20f");
        var merchant = "feibaotest";
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var applySettings = new FeibaoPayWithdrawSettings(
            merchant,
            hashKey,
            hashIV,
            formId,
            BankCode: "GXCHP",
            CardHolder: "testPerson",
            CardNumber: "11112222333344556",
            Gateway: "gcash",
            UserId: "4ucmqplxb029",
            UserIp: "127.0.0.1");

        var request = new WithdrawRequest
        {
            Amount = "100.00",
            ApplyInfo = JsonSerializer.Serialize(applySettings, FeibaoPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.feibaopay.com/v3/withdraw")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "merchant_slug": "feibaotest",
                    "merchant_order_num": "d963d5fc-adbf-4882-b3f0-bf1b104ec20f",
                    "action": "withdraw",
                    "order": "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrHh56R9dD0Nc4EOYMQQ\ntTIMQR774mV+QcgrgZVnYzYMslLDJI4elZLVeMl5jQnJU1WgYqVVf2AeDNRA\n4t/Of+JwRmPGw41n/IUiCz7UaFI/X8HgAtN9filEvbc2eNnZMDZnIXS+KG7w\nzGfnE/s0HL9XUYiLTifHn6zS8asyuZtiQn1YKU5UdiOEM28yC6DVWQ+ZsVdY\nk8cLBA5xzTZ3e1JeTjkDqXb9bpaCtK8kfMZE4pnBhYAZ0yLCmfhLdYDXvpRm\nsZqLLt5/SNVSr7bmWEiCPngeGRzw8fm5nYPiJSDKTg==\n"
                }
                """));

        // Act
        var actual = await sut.HandleAsync(request.ToByteArray());
        var response = WithdrawResponse.Parser.ParseFrom(actual.Span);
        var applyInfo = JsonSerializer.Deserialize<FeibaoPayWithdrawApplyInfo>(response.Result, FeibaoPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                Arg.Is(applySettings.FormId),
                Arg.Is("d963d5fc-adbf-4882-b3f0-bf1b104ec20f"));
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
    }
}
