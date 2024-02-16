using System.Net;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay;
using Tgs.Payment.FeibaoPay.Client;
using Tgs.Payment.FeibaoPay.MessageQueue;
using Tgs.Payment.Protos;

namespace FeibaoPay.Core.UnitTests;

public class DepositTimeoutCheckHandlerTests
{
    [Fact]
    public async Task 當三方代收結果為success時不會繼續計時詢問()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

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

        var sut = new DepositTimeoutCheckHandler(fakePaymentServiceClient, fakeFeibaoPayClient, fakePaymentClient, NullLogger<DepositTimeoutCheckHandler>.Instance);

        var formId = Guid.Parse("08cc2701-d120-4c43-8f4f-d433e69b1aa3");
        var brandId = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af");
        var request = new ScheduleRequest
        {
            FormId = formId,
            Subject = "tgs.payment.feibaopay.deposit.schedule.reject",
            ScheduleTime = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        var applyInfo = new FeibaoPayDepositApplyInfo(
            new FeibaoPayDepositSettings(
                "feibaotest",
                "a8ec90c269c3bd8812739a4d710bbb85",
                "edbeaacdb228dcac",
                formId,
                "gcash",
                "4ucmqplxb029",
                "127.0.0.1"),
            new FeibaoPayDepositResponse(
                100.00m,
                formId,
                new Uri("https://api.pay.live.swiftpay.ph/api/bootstrap?institution_code=GCASH\u0026x_access_key=BC2785DAC4D6406CA81FCDBBEA1F6CF2\u0026x_amount=100.00\u0026x_currency=PHP\u0026x_reference_no=g20240206113110000010000b8d\u0026signature=24083fbafdf71d084c864d453582ddad754d2f298d95e29811eea5f1b1b35c66"),
                "08cc2701-d120-4c43-8f4f-d433e69b1aa3"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(request.FormId.ToGuid()))
            .Returns(new DepositFormInfo
            {
                Id = formId,
                Amount = 100.00m,
                BrandId = brandId,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, FeibaoPayUtility.SerializerOptions)
            });

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.feibaopay.com/v3/check")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "merchant_slug": "feibaotest",
                    "merchant_order_num": "08cc2701-d120-4c43-8f4f-d433e69b1aa3",
                    "action": "check",
                    "order": "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrHh56R9dD0Nc4EOYMQQ\ntTIM3TSF+EFevUQptJ0A+c72qloGbpknjHPRNbiAKX8yi2rMh5LRqP0CCMw6\nInM6KFoERV70NFMCjeH2itroFIBLFLXynkzy7hi7g/1tO4KWavRaLX9vM4pr\n9PPvoZ/r5OqOOTBgiGlLqidppsIeFOfBoKoP3E1USTo4cIvONBY4QaNYGj7J\nDIWHUcrXgtIUs6J5a7pW+Eh45AWTW+m3W5cSSg==\n"
                }
                """));

        // Act
        await sut.HandleAsync(request.ToByteArray());

        // Assert
        _ = fakePaymentClient.Received(0)
        .ScheduleAsync(
                Arg.Is("tgs.payment.feibaopay.deposit.schedule.reject"),
                Arg.Is(request.FormId.ToGuid()),
                Arg.Any<DateTime>());
    }
}
