using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPaymentClient()
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig => natsConfig
        .ConfigQueueOptions((opt, _) => opt.Url = "nats://localhost:4222"))
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

var merchant = "feibaotest";
var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
var hashIV = "edbeaacdb228dcac";

//代收GCash QR
var gateway = "gcash_qr";

var depositSettings = new FeibaoPayDepositSettings(merchant, hashKey, hashIV, Guid.NewGuid(), gateway, null, "4ucmqplxb029", "127.0.0.1");

var depositReturn = await paymentClient.DepositAsync(
    "tgs.payment.feibaopay.deposit",
    200M,
    JsonSerializer.Serialize(depositSettings, FeibaoPayUtility.SerializerOptions)).ConfigureAwait(false);

logger.LogInformation("DepositReturn: {depositReturn}", depositReturn);

//代付
//var bankCode = "GXCHP";
//var withdrawSettings = new FeibaoPayWithdrawSettings(merchant, hashKey, hashIV, Guid.NewGuid(), bankCode, "testPerson", "11112222333344556", "gcash", "4ucmqplxb029", "127.0.0.1");

//var withdrawReturn = await paymentClient.WithdrawAsync(
//    "tgs.payment.feibaopay.withdraw",
//    100M,
//    JsonSerializer.Serialize(withdrawSettings, FeibaoPayUtility.SerializerOptions)).ConfigureAwait(false);

//logger.LogInformation("WithdrawReturn: {withdrawReturn}", withdrawReturn);

//查詢存款訂單
