using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.FeibaoPay;

namespace FeibaoPay.Core.UnitTests;

public class FeibaoClientTests
{
    [Fact]
    public async Task 代收接口_成功()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.feibaopay.com");
        var cryptor = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);
        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var sut = new FeibaoPayClient(httpClient, cryptor, fakeTimeProvider, NullLogger<FeibaoPayClient>.Instance);

        var formId = Guid.Parse("a0e5999f-93b1-49ac-a382-97061b3b5a04");
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var currentTime = DateTimeOffset.FromUnixTimeSeconds(1707190242);

        _ = fakeTimeProvider
            .GetUtcNow()
            .Returns(currentTime);

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
        var merchant = "feibaotest";
        var order = new DepositOrder(
            100M,
            formId,
            new Uri("https://dev-payment.rpgp.cc/feibaopay/deposit/callback"),
            "gcash_qr",
            null,
            "4ucmqplxb029",
            "127.0.0.1");

        var actual = await sut.DepositAsync(merchant, hashKey, hashIV, order);

        Assert.Equal(formId, actual.FormId);
        Assert.Equal(new Uri("http://gateway.esilver.top/qrph/ECH20240206171842667042573"), actual.RedirectUrl);
    }

    [Fact]
    public async Task 代付接口_成功()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.feibaopay.com");
        var cryptor = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);
        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var sut = new FeibaoPayClient(httpClient, cryptor, fakeTimeProvider, NullLogger<FeibaoPayClient>.Instance);

        var formId = Guid.Parse("d963d5fc-adbf-4882-b3f0-bf1b104ec20f");
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var currentTime = DateTimeOffset.FromUnixTimeSeconds(1707295665);

        _ = fakeTimeProvider
            .GetUtcNow()
            .Returns(currentTime);

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
        var merchant = "feibaotest";
        var order = new WithdrawOrder(
            formId,
            100.0M,
            new Uri("https://dev-payment.rpgp.cc/feibaopay/withdraw/callback"),
            "gcash",
            "testPerson",
            "11112222333344556",
            "gcash",
            "4ucmqplxb029",
            "127.0.0.1");

        var actual = await sut.WithdrawAsync(merchant, hashKey, hashIV, order);

        Assert.Equal(formId, actual.FormId);
        Assert.Equal("d963d5fc-adbf-4882-b3f0-bf1b104ec20f", actual.ThirdPartyOrderNo);
    }

    [Fact]
    public async Task 查詢代收訂單()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.feibaopay.com");
        var cryptor = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);
        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var sut = new FeibaoPayClient(httpClient, cryptor, fakeTimeProvider, NullLogger<FeibaoPayClient>.Instance);
        var formId = Guid.Parse("08cc2701-d120-4c43-8f4f-d433e69b1aa3");

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var currentTime = DateTimeOffset.FromUnixTimeSeconds(1707208766);

        _ = fakeTimeProvider
            .GetUtcNow()
            .Returns(currentTime);

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
        var merchant = "feibaotest";

        var actual = await sut.QueryDepositFormStatusAsync(merchant, hashKey, hashIV, formId);

        Assert.Equal("success", actual);
    }

    [Fact]
    public async Task 查詢各錢包餘額()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.feibaopay.com");
        var cryptor = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);
        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var sut = new FeibaoPayClient(httpClient, cryptor, fakeTimeProvider, NullLogger<FeibaoPayClient>.Instance);

        var merchant = "feibaotest";
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var currentTime = DateTimeOffset.FromUnixTimeSeconds(1707208766);

        _ = fakeTimeProvider
            .GetUtcNow()
            .Returns(currentTime);

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.feibaopay.com/v3/balance")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "action": "balance",
                    "wallets": {
                        "bank": "230302.0",
                        "bank_qr": "0.0",
                        "coinsph": "0.0",
                        "credit_card": "0.0",
                        "cstore": "0.0",
                        "gcash": "94899.6",
                        "gcash_qr": "142983.4",
                        "gcash_vivamax": "0.0",
                        "grabpay": "567533.0",
                        "grabpay_qr": "0.0",
                        "paymaya": "1150508.0",
                        "paymaya_qr": "0.0",
                        "paypal": "0.0",
                        "qrph": "0.0",
                        "remit": "0.0",
                        "rusty_p2p": "0.0"
                    }
                }
                """));
        
        var expected = JsonNode.Parse("""
            {
                "bank": 230302.0,
                "bank_qr": 0.0,
                "coinsph": 0.0,
                "credit_card": 0.0,
                "cstore": 0.0,
                "gcash": 94899.6,
                "gcash_qr": 142983.4,
                "gcash_vivamax": 0.0,
                "grabpay": 567533.0,
                "grabpay_qr": 0.0,
                "paymaya": 1150508.0,
                "paymaya_qr": 0.0,
                "paypal": 0.0,
                "qrph": 0.0,
                "remit": 0.0,
                "rusty_p2p": 0.0
            }
            """);

        var actual = await sut.QueryBalanceAsync(merchant, hashKey, hashIV, Array.Empty<string>());

        Assert.NotNull(actual);
        Assert.Equal(expected!.ToJsonString(), JsonSerializer.Serialize(actual));
    }
}
