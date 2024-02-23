using System.Net;
using System.Text.Json;
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

public class DepositProcessorTests
{
    [Fact]
    public async Task 儲值要求處理_GCash_Qr()
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
        var formId = Guid.Parse("a0e5999f-93b1-49ac-a382-97061b3b5a04");
        var merchant = "feibaotest";
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var sut = new DepositProcessor(
            fakeFeibaoPayClient,
            fakePaymentClient,
            new OptionsWrapper<FeibaoPayOptions>(options));

        var applySettings = new FeibaoPayDepositSettings(
            merchant,
            hashKey,
            hashIV,
            formId,
            "gcash_qr",
            null,
            "4ucmqplxb029",
            "127.0.0.1");

        var request = new DepositRequest
        {
            Amount = "100.00",
            ApplyInfo = JsonSerializer.Serialize(applySettings, FeibaoPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.feibaopay.com/v3/deposit")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "merchant_slug": "feibaotest",
                    "merchant_order_num": "a0e5999f-93b1-49ac-a382-97061b3b5a04",
                    "action": "deposit",
                    "order": "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrH+gMPGOGeRNNxP7WdG\n+jmzJDuZovn4rfUEZwaLDkSBM515MSS3nK67nFjKQ7y58zz79RRi+BgV73ZQ\nETpGCMlAER+AESy69JbRTIx7JdMVTOmxFR0SFnRd02T2GIKx5ee8OsCvvK9E\nSwJy0OhEmqRqkssa64UzH01tXcj+1FB3oe2wzJ0rZfKxJpObF2C1GIp80GRI\naRkSUytd6LPZCo7nPpdxqScIr4DCfj5dLqcbd0sVCizz0HzJclIDoVDKZV1c\nIxt6nVvL4QFI7InBVScid9se059ZlDTHD/ab40QA4fWv+2gRj3IZi8xt2Gw7\nEFSKQQ7PkxwT4agPxfcH6gta\n",
                    "msg": "success"
                }
                """));

        // Act
        var actual = await sut.HandleAsync("test", request, null);
        var applyInfo = JsonSerializer.Deserialize<FeibaoPayDepositApplyInfo>(actual.Result, FeibaoPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .ScheduleAsync(
                Arg.Is("tgs.payment.feibaopay.deposit.schedule.reject"),
                Arg.Is(applySettings.FormId),
                Arg.Any<DateTime>());
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new FeibaoPayDepositResponse(
                100.00m,
                applySettings.FormId,
                new Uri("http://gateway.esilver.top/qrph/ECH20240206171842667042573"),
                "a0e5999f-93b1-49ac-a382-97061b3b5a04"),
            applyInfo.Response);
    }

    [Fact]
    public async Task 儲值要求處理_GCash()
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
        var formId = Guid.Parse("08cc2701-d120-4c43-8f4f-d433e69b1aa3");
        var merchant = "feibaotest";
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var sut = new DepositProcessor(
            fakeFeibaoPayClient,
            fakePaymentClient,
            new OptionsWrapper<FeibaoPayOptions>(options));

        var applySettings = new FeibaoPayDepositSettings(
            merchant,
            hashKey,
            hashIV,
            formId,
            "gcash",
            null,
            "4ucmqplxb029",
            "127.0.0.1");

        var request = new DepositRequest
        {
            Amount = "100.0",
            ApplyInfo = JsonSerializer.Serialize(applySettings, FeibaoPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.feibaopay.com/v3/deposit")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "merchant_slug": "feibaotest",
                    "merchant_order_num": "08cc2701-d120-4c43-8f4f-d433e69b1aa3",
                    "action": "deposit",
                    "order": "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrHh56R9dD0Nc4EOYMQQ\ntTIM3TSF+EFevUQptJ0A+c72qloGbpknjHPRNbiAKX8yi2rMh5LRqP0CCMw6\nInM6KFoERV70NFMCjeH2itroFIBLFHVmGGTXEqPfAPKU7E08PvbReMlZNbII\nbnGnGHpHId7tpHIvhkXD3RSMeY7Kd4abVF9cH3M8V5+t6kJYqpmqAcFkHpdR\nYhxxTcrw023mJFzHIlz4JtxrELefK+FrGN0SstuYiZUOzFrWY7enVuFWZ0Jr\n4VmKN+B/lEaeoSmI95E/N3iX5Op9lQvP2VfTQRuUWyYU1MwfCPheAgopBkni\nyNyN9HL2HkB8+J905Q/Tew3jYusyjJCSvDKIPiPWXiDLGJJ7SzS44SAGWZXK\ncwS7vBpaR94YUV98oF5abr3U43NG+AU7daVcM6ejpw8wjnlG2wQFxEQ5vavI\nDGBwu8UqFzTnVlT6NxKY9fM6v1uaSiGcrTi1jEdTV3xrchCJWYLs+rwQ82JN\nKTF4CDIKgYkBVeEN8/wbOXyCVipfIA1tz/zUPyTqSmlAj8WU61BkQQUQ5GJo\nkMZypUZbzCjwnAlDWiTLfJZEHvs9jW2Rg3Upp5Y8sPnVSXZ6scujpHy6CNeU\nS9Glvq7OSsIORJ1/L7jiVPc=\n",
                    "msg": "success"
                }
                """));

        // Act
        var actual = await sut.HandleAsync("test", request, null);
        var applyInfo = JsonSerializer.Deserialize<FeibaoPayDepositApplyInfo>(actual.Result, FeibaoPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .ScheduleAsync(
                Arg.Is("tgs.payment.feibaopay.deposit.schedule.reject"),
                Arg.Is(applySettings.FormId),
                Arg.Any<DateTime>());
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new FeibaoPayDepositResponse(
                100.00m,
                applySettings.FormId,
                new Uri("https://api.pay.live.swiftpay.ph/api/bootstrap?institution_code=GCASH\u0026x_access_key=BC2785DAC4D6406CA81FCDBBEA1F6CF2\u0026x_amount=100.00\u0026x_currency=PHP\u0026x_reference_no=g20240206113110000010000b8d\u0026signature=24083fbafdf71d084c864d453582ddad754d2f298d95e29811eea5f1b1b35c66"),
                "08cc2701-d120-4c43-8f4f-d433e69b1aa3"),
            applyInfo.Response);
    }
}
