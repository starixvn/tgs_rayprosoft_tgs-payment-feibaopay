using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.FeibaoPay;

public class FeibaoPayClient(
    HttpClient httpClient,
    FeibaoPayCryptor cryptor,
    TimeProvider timeProvider,
    ILogger<FeibaoPayClient> logger)
{
    public async ValueTask<FeibaoPayClientDepositResponse> DepositAsync(
        string merchant,
        string hashKey,
        string hashIV,
        DepositOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["amount"] = order.Amount.ToString("#.##"),
            ["callback_url"] = order.CallbackUrl.AbsoluteUri,
            ["device"] = "mobile",
            ["gateway"] = order.GateWay,
            ["merchant_order_num"] = order.FormId.ToString(),
            ["merchant_order_remark"] = "",
            ["merchant_order_time"] = timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString(),
            ["uid"] = order.PlayerCode,
            ["user_ip"] = order.UserIp,
        };

        if (string.IsNullOrWhiteSpace(order.BankCode) is false)
            dict["bank_code"] = order.BankCode;

        var sortDict = dict
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => kvp)
            .ToDictionary();

        var source = JsonSerializer.Serialize(sortDict);
        var encryptData = cryptor.AesEncrypt(source, hashKey, hashIV);

        var request = new
        {
            merchant_slug = merchant,
            data = encryptData
        };

        var response = await httpClient
            .PostAsJsonAsync("/v3/deposit", request, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("FeibaoPayClient Deposit Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Deposit response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException(string.Format("message:{0}", node["msg"]!.ToString()));

        var encryptOrder = node["order"]!.GetValue<string>();
        var decryptOrder = cryptor.AesDecrypt(encryptOrder, hashKey, hashIV);

        var nodeOrder = JsonNode.Parse(decryptOrder) ?? throw new ApiFailException("Deposit response order can't read.");

        return new FeibaoPayClientDepositResponse(
            Amount: decimal.Parse(nodeOrder["amount"]!.GetValue<string>()),
            FormId: order.FormId,
            ThirdPartyOrderNo: node["merchant_order_num"]!.GetValue<string>(),
            RedirectUrl: new Uri(nodeOrder["navigate_url"]!.GetValue<string>()));
    }

    public async ValueTask<FeibaoPayClientWithdrawResponse> WithdrawAsync(
        string merchant,
        string hashKey,
        string hashIV,
        WithdrawOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["amount"] = order.Amount.ToString("#.##"),
            ["area_code"] = "100000",
            ["bank_code"] = order.BankCode,
            ["callback_url"] = order.CallbackUrl.AbsoluteUri,
            ["card_holder"] = order.CardHolder,
            ["card_number"] = order.CardNumber,
            ["city_code"] = "100000",
            ["gateway"] = order.Gateway,
            ["merchant_order_num"] = order.FormId.ToString(),
            ["merchant_order_time"] = timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString(),
            ["merchant_order_remark"] = "",
            ["province_code"] = "beijing",
            ["uid"] = order.UserId,
            ["user_ip"] = order.UserIp,
        };

        var sortDict = dict
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => kvp)
            .ToDictionary();

        var source = JsonSerializer.Serialize(sortDict);
        var encryptData = cryptor.AesEncrypt(source, hashKey, hashIV);

        var request = new
        {
            merchant_slug = merchant,
            data = encryptData
        };

        var response = await httpClient
            .PostAsJsonAsync("/v3/withdraw", request, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("FeibaoPayClient Withdraw Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Withdraw response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException(string.Format("message:{0}", node["msg"]!.ToString()));

        var encryptOrder = node["order"]!.GetValue<string>();
        var decryptOrder = cryptor.AesDecrypt(encryptOrder, hashKey, hashIV);

        var nodeOrder = JsonNode.Parse(decryptOrder) ?? throw new ApiFailException("Withdraw response order can't read.");

        return new FeibaoPayClientWithdrawResponse(
            FormId: order.FormId,
            ThirdPartyOrderNo: node["merchant_order_num"]!.GetValue<string>(),
            Amount: decimal.Parse(nodeOrder["amount"]!.GetValue<string>()));
    }

    public async ValueTask<string> QueryDepositFormStatusAsync(string merchant, string hashKey, string hashIV, Guid formId, CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["merchant_slug"] = timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString(),
            ["merchant_order_num"] = formId.ToString(),
        };

        var sortDict = dict
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => kvp)
            .ToDictionary();

        var source = JsonSerializer.Serialize(sortDict);
        var encryptData = cryptor.AesEncrypt(source, hashKey, hashIV);

        var request = new
        {
            merchant_slug = merchant,
            data = encryptData
        };

        var response = await httpClient
            .PostAsJsonAsync("/v3/check", request, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("FeibaoPayClient Query Deposit Form Status Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit Form Status response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException(string.Format("message:{0}", node["msg"]!.ToString()));

        var encryptOrder = node["order"]!.GetValue<string>();
        var decryptOrder = cryptor.AesDecrypt(encryptOrder, hashKey, hashIV);

        var nodeOrder = JsonNode.Parse(decryptOrder) ?? throw new ApiFailException("Query Deposit Form Status response order can't read.");

        return nodeOrder["status"]!.GetValue<string>();
    }

    public async ValueTask<IReadOnlyDictionary<string, decimal>> QueryBalanceAsync(
        string merchant,
        string hashKey,
        string hashIV,
        IEnumerable<string> gateWays,
        CancellationToken cancellationToken = default)
    {
        var gateways = gateWays.Any() ? gateWays.ToArray() : Array.Empty<string>();

        var dict = new Dictionary<string, object>
        {
            ["merchant_time"] = timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString(),
            ["gateways"] = gateways,
        };

        var sortDict = dict
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => kvp)
            .ToDictionary();

        var source = JsonSerializer.Serialize(sortDict);
        var encryptData = cryptor.AesEncrypt(source, hashKey, hashIV);

        var request = new
        {
            merchant_slug = merchant,
            data = encryptData
        };

        var response = await httpClient
            .PostAsJsonAsync("/v3/balance", request, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("FeibaoPayClient Query Balance Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Balance response can't read.");

        return node["code"]!.GetValue<int>() != 0
            ? throw new ApiFailException(string.Format("message:{0}", node["msg"]!.ToString()))
            : node["wallets"].Deserialize<IReadOnlyDictionary<string, string>>()!
                .ToDictionary(
                    p => p.Key,
                    p => decimal.Parse(p.Value));
    }
}
