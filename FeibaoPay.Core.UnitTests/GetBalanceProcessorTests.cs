using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.FeibaoPay.MessageQueue;
using Tgs.Payment.FeibaoPay;
using Tgs.Payment.Protos;
using Xunit;

namespace FeibaoPay.Core.UnitTests;

public class GetBalanceProcessorTests
{
    [Fact]
    public async Task GetBalanceProcessor_取得商戶餘額()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.feibaopay.com");

        var cryptor = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);
        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var fakeFeibaoPayClient = new FeibaoPayClient(
            httpClient,
            cryptor,
            fakeTimeProvider,
            NullLogger<FeibaoPayClient>.Instance);

        var sut = new GetBalanceProcessor(fakeFeibaoPayClient);

        var merchant = "feibaotest";
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var request = new PaymentBalanceRequest
        {
            BrandPaymentSettings = $$"""
                {
                    "merchant": "{{merchant}}",
                    "hashKey": "{{hashKey}}",
                    "hashIV": "{{hashIV}}",
                    "gateWay": "gcash"
                }
                """
        };

        _ = fakeTimeProvider.GetUtcNow()
            .Returns(DateTimeOffset.FromUnixTimeMilliseconds(1707208766));

        _ = mockHttp.Expect(HttpMethod.Post, "https://api.feibaopay.com/v3/balance")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "action": "balance",
                    "wallets": {
                        "gcash": "94899.6"
                    }
                }
                """));

        // Act
        var actual = await sut.HandleAsync("test", request, null);

        // Assert
        Assert.Equal(94899.6M, actual.Balance.ToDecimal());
    }
}
