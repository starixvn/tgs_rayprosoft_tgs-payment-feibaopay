using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.FeibaoPay;

namespace FeibaoPay.Core.UnitTests;

public class CryptorTests
{
    [Fact]
    public void 查詢餘額加密()
    {
        var data = new
        {
            merchant_time = "1707125205",
            gateways = new string[] { "alipay", "wechat", "remit" }
        };

        var source = JsonSerializer.Serialize(data);

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesEncrypt(source, hashKey, hashIV);

        var expected = "UI6j2cSAQQGQ1giG3Ep4LsFphScfpddD/3qG44cCD1EuSHVRmWAe8IMsFyAA4U4OlcXwdyNebXe41uFCqFLRXVKClpcfYyIOsbs+ecRyufc=";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 查詢餘額解密()
    {
        var source = "UI6j2cSAQQGQ1giG3Ep4LsFphScfpddD/3qG44cCD1EuSHVRmWAe8IMsFyAA4U4OlcXwdyNebXe41uFCqFLRXVKClpcfYyIOsbs+ecRyufc=";

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(source, hashKey, hashIV);

        var expected = """
		{"merchant_time":"1707125205","gateways":["alipay","wechat","remit"]}
		""";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 存款加密()
    {
        var data = new
        {
            amount = "100.00",
            callback_url = "https://dev-payment.rpgp.cc/feibaopay/deposit/callback",
            device = "mobile",
            gateway = "gcash",
            merchant_order_num = "08cc2701-d120-4c43-8f4f-d433e69b1aa3",
            merchant_order_remark = "",
            merchant_order_time = 1707190242,
            uid = "4ucmqplxb029",
            user_ip = "127.0.0.1",
        };

        var source = JsonSerializer.Serialize(data);

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesEncrypt(source, hashKey, hashIV);

        var expected = "tAUAwR76Lcu7BjQ5gGANlVOPemAcXGwHOaAXUiFNL+/GZ2SubwMdMf27yg2l56Aef2uDTaeD09wOYQEJ12XpwwCRWcj7yFJIZbcT31Gyw4TN3jHeurzeqDUIZkN8w0hbP9z+qiPfzpEx3tK+788xg/Pf+nNOM84e1vuO4k4vKzUXKHorbfOSS6hyujejYeDP1TNrfwQTZWME+VZ7pBX+ePfyfklHXK+ASU1pVGhLf1X467bD636ViDTjqY679SgaVa9Nf1GZKGAofrRVu+2B0hkyEBhJs0AlYjzzruxaXSROelq8brb69OeNTtxSU2OVbjFlfJD4G3s7j+LIRmUjSxlmLQO1cu9KGz0udwF5+SrQE6NmAp7vZdWSHkIV6Et0pqBjLFdC7om9OtemEaedKg==";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 存款解密()
    {
        var source = "tAUAwR76Lcu7BjQ5gGANlVOPemAcXGwHOaAXUiFNL+/GZ2SubwMdMf27yg2l56Aef2uDTaeD09wOYQEJ12XpwwCRWcj7yFJIZbcT31Gyw4TN3jHeurzeqDUIZkN8w0hbP9z+qiPfzpEx3tK+788xg/Pf+nNOM84e1vuO4k4vKzUXKHorbfOSS6hyujejYeDP1TNrfwQTZWME+VZ7pBX+ePfyfklHXK+ASU1pVGhLf1X467bD636ViDTjqY679SgaVa9Nf1GZKGAofrRVu+2B0hkyEBhJs0AlYjzzruxaXSROelq8brb69OeNTtxSU2OVbjFlfJD4G3s7j+LIRmUjSxlmLQO1cu9KGz0udwF5+SrQE6NmAp7vZdWSHkIV6Et0pqBjLFdC7om9OtemEaedKg==";

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(source, hashKey, hashIV);

        var expected = """
		{"amount":"100.00","callback_url":"https://dev-payment.rpgp.cc/feibaopay/deposit/callback","device":"mobile","gateway":"gcash","merchant_order_num":"08cc2701-d120-4c43-8f4f-d433e69b1aa3","merchant_order_remark":"","merchant_order_time":1707190242,"uid":"4ucmqplxb029","user_ip":"127.0.0.1"}
		""";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 存款回傳order解密1()
    {
        var source = "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrH+gMPGOGeRNNxP7WdG\n+jmzJDuZovn4rfUEZwaLDkSBM515MSS3nK67nFjKQ7y58zz79RRi+BgV73ZQ\nETpGCMlAER+AESy69JbRTIx7JdMVTOmxFR0SFnRd02T2GIKx5ee8OsCvvK9E\nSwJy0OhEmqRqkssa64UzH01tXcj+1FB3oe2wzJ0rZfKxJpObF2C1GIp80GRI\naRkSUytd6LPZCo7nPpdxqScIr4DCfj5dLqcbd0sVCizz0HzJclIDoVDKZV1c\nIxt6nVvL4QFI7InBVScid9se059ZlDTHD/ab40QA4fWv+2gRj3IZi8xt2Gw7\nEFSKQQ7PkxwT4agPxfcH6gta\n";

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(source, hashKey, hashIV);

        var expected = """
            {"amount":"100.0","gateway":"gcash_qr","merchant_order_num":"a0e5999f-93b1-49ac-a382-97061b3b5a04","merchant_order_time":"1707211110.0","navigate_url":"http://gateway.esilver.top/qrph/ECH20240206171842667042573","status":"waiting","sign":"237278d5c71a74cbbdc8cfc813b66ac50af797ee"}
            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 存款回傳order解密2()
    {
        var source = "WoGfa2weFG91Fi/jHf09rxaDXR7FRJYGtiMTBB8KsRds5y6HJEI7Taja6+Md\n7+lCrwwf/G+eDHoQ65hO2XzlHOcLgx5C7/pXjfPVmH8KeIm/ejvAVh1CiHm+\nI65xJrVLRT/5lmMcvqNiBtAIeEoOmvqLLvyCDgRDPQ1YVy667L0hnnqaqHu6\n0VVlmIjjdixtzHFHmrLvlmtF8mGNT5/tRJYmw1n4ncdvnOpt2kMewr4dOYus\nw1LUPZJQjwps1vD2jgcNhw3nwhS4R8TowQrzIXZ2mN+qCiKf824ruc0Hgk4e\nmK5/7Pmb5ZRZM2iDdDAGI+82oOptjTfUeZVVzKOxE99/xJ6yAd+0qDxRaf4S\nNus5VAIQNs6Eft+/OAM1lI5r\n";

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(source, hashKey, hashIV);

        var expected = """
            {"amount":"200.0","gateway":"gcash_qr","merchant_order_num":"70363ff7-f32d-4e97-a343-ad93359863aa","merchant_order_time":"1707283230.0","navigate_url":"https://pay.qrph.io/07b0b700-6a9d-45f9-b2bd-6e149452ef8b","status":"waiting","sign":"966cd1dc8e6bf6c548004aa64dbab2d5e32a3409"}
            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 存款回調order解密()
    {
        var originOrder = "WoGfa2weFG91Fi%2FjHf09rxaDXR7FRJYGtiMTBB8KsRds5y6HJEI7Taja6%2BMd%0A7%2BlCrwwf%2FG%2BeDHoQ65hO2XzlHOcLgx5C7%2FpXjfPVmH8KeIm%2FejvAVh1CiHm%2B%0AI65xJrVLRT%2F5lmMcvqNiBtAIeEoOmgcULpyOxtrs8R6It9EANuXq93DEpRYh%0AZY9pWFh9BkjHsORf8xu1xhobZjzjGahITrWTTj7EYodu%2BiKgf1Rg34LH0TFV%0ApNUKeaDvh2lPKmmfw99XCEUw616QI%2BXcg69UKQ%3D%3D%0A";

        var source = WebUtility.UrlDecode(originOrder).Replace(@"\n", "");

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(source, hashKey, hashIV);

        var expected = """
            {"amount":"200.0","gateway":"gcash_qr","merchant_order_num":"70363ff7-f32d-4e97-a343-ad93359863aa","merchant_order_remark":"","status":"success","sign":"b1e560e991c339b79cad84cdd69cca2d3e05d4c2"}
            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 提款解密()
    {
        var source = "tAUAwR76Lcu7BjQ5gGANlRcJGwMeoQwHuZTH+G+o4iyRkP1bgL5Dl7Pxfl2VJnKBUhHYos6he4h7y7a8WFIgSZXrDSolfY3ZmBWjmhIZse/L++/LczRw4kf48dNQiXUNMtxtdr5bd/w18nPltBt6RFWh5gzU4HXpvaIrFYWgzQpUhJOtaEwVOdEL2vQ4ODg+eY9lQk+RyzxknL+ntQG8J4ZHyU5zNr0rRPMrK6rilyMHP9YoxOQEq0UswGWxWq24YUBwIq0DpMMnxqHxIa4uQbkuu+4RjDh4G2qwhG7BDklFtBSJhs8YORf6uUkWnRa9149rD0xu3Av2Yr8U7PIXdYeYzYlHnscpGdxfaNf9q30TUStPPLtZAYDPwy/SBmV9zhddneRDJUkoun3ewQp4+mOcpV5XfFLAEMjMa+1BnonkK4o8003C+y78na5eRhAd+XCq0rXxEt8EchuXjBscvmG2B6VyEgfcVpXeffjZZSBoyRSOGzwU9BaU+u/CP5qJc01AbuWd/dPbuLBiar2jtL0hDA4rkD93O5pDVmIuYqou/3/1YjJKvzGxpvVJ6QN7";

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(source, hashKey, hashIV);

        var expected = """
            {"amount":"100.00","area_code":"100000","bank_code":"GXCHP","callback_url":"https://dev-payment.rpgp.cc/feibaopay/withdraw/callback","card_holder":"testPerson","card_number":"11112222333344556","city_code":"100000","gateway":"gcash","merchant_order_num":"d963d5fc-adbf-4882-b3f0-bf1b104ec20f","merchant_order_time":1707295665,"merchant_order_remark":"","province_code":"beijing","uid":"4ucmqplxb029","user_ip":"127.0.0.1"}
            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 提款回調order解密()
    {
        var source = "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrHh56R9dD0Nc4EOYMQQ\ntTIMQR774mV+QcgrgZVnYzYMslLDJI4elZLVeMl5jQnJU1WgYqVVf2AeDNRA\n4t/Of+JwRmPGw41n/IUiCz7UaFI/X8HgAtN9filEvbc2eNnZMDZnIXS+KG7w\nzGfnE/s0HL9XUYiLTifHn6zS8asyuZtiQn1YKU5UdiOEM28yC6DVWQ+ZsVdY\nk8cLBA5xzTZ3e1JeTjkDqXb9bpaCtK8kfMZE4pnBhYAZ0yLCmfhLdYDXvpRm\nsZqLLt5/SNVSr7bmWEiCPngeGRzw8fm5nYPiJSDKTg==\n";

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(source, hashKey, hashIV);

        var expected = """
            {"amount":"100.0","gateway":"gcash","merchant_order_num":"d963d5fc-adbf-4882-b3f0-bf1b104ec20f","merchant_order_remark":"","merchant_order_time":"1707295665.0","status":"prepare","uid":"4ucmqplxb029","sign":"79d7580ec569fcc1e5d32eea753b05fcbf95023f"}
            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 提款回調成功order解密()
    {
        var originOrder = "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrHh56R9dD0Nc4EOYMQQ%0AtTIMguTe5SLuAgSlanI2UD40XbKOtBNFTY8DGivWvgpFGv5MOsLXK%2BGeiiIu%0AhaKSKbDiuTLBrYCRdGxsf2nZuW0XYEBrMRJ7AIBoFi0p9vQbt%2BWNsknD0NRD%0A9BcCzSH5911aA%2B%2FrgWcnkKSpMyWTp5uh%2FAaB9wBKVWdfnXtm4AWJdw2iq5qt%0AS9DXFrQAMwaRU3N9Vy7cVo9c%2B3YdcFp687MYig%3D%3D%0A";

        var decode = WebUtility.UrlDecode(originOrder).Replace(@"\n", "");

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(decode, hashKey, hashIV);

        var expected = """
            {"amount":"100.0","gateway":"gcash","merchant_order_num":"4f3803ab-08e4-43c9-acc3-a06fa426fb1f","merchant_order_remark":"","note":"","status":"success","sign":"b8f6d03eea6fd0b70bff2154552c9fe0f5818575"}
            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 提款回調失敗order解密()
    {
        var originOrder = "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrHh56R9dD0Nc4EOYMQQ%5CntTIMr0XUevoyXrvpd3%2FVpDjtqYMX0SLidyNV%2FbkWlVqkiKGEQynZY1AJ8xHi%5CnnZ9mMicFdSftB7SuyXrcZFJIrXrVU0iN4ul8vo3iCHIaN10izM2dAbzf54JE%5CnwA5I5QsR2Ek%2Fan7CZpio1WfGpGnkSWCKCQTICxw%2Bj%2Bz8wDIkbu9wAZbP3lAp%5Cn8eG01L%2B8q880VWPy3w7labpSBffoON00ggR6vnOziLOf8WXEx7eKxgCWkz52%5Cnord1m4%2FvkE9rY1FhSPoX%5Cn";

        var decode = WebUtility.UrlDecode(originOrder).Replace(@"\n", "");
        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(decode, hashKey, hashIV);

        var expected = """
            {"amount":"100.0","gateway":"gcash","merchant_order_num":"97e075a8-84b9-4822-bd41-a68d50441863","merchant_order_remark":"","note":"remaining balance: 94999.6","status":"fail","sign":"19603b72adcd0a3f9402231e355bbccc6ddcb485"}
            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 查詢存款訂單加密()
    {
        var data = new
        {
            merchant_slug = "feibaotest",
            merchant_order_num = "08cc2701-d120-4c43-8f4f-d433e69b1aa3",
        };

        var source = JsonSerializer.Serialize(data);

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";

        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesEncrypt(source, hashKey, hashIV);

        var expected = "+Utg9c6NbLXu/4ZtqpdWB6pW+yBAWbmudepg3TodMOOGtJgQKqV9D+3KYrxiwjKZbKqrni0aCV2LPuv1EbwtWkl3UFPkQvYAwVXoSQmJ1/ZDXTcsUy/LGcU0+WrUqM7O";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 查詢存款訂單order解密()
    {
        var source = "tAUAwR76Lcu7BjQ5gGANlYl7PHmAJjbWtxtemZNUMrHh56R9dD0Nc4EOYMQQ\ntTIM3TSF+EFevUQptJ0A+c72qloGbpknjHPRNbiAKX8yi2rMh5LRqP0CCMw6\nInM6KFoERV70NFMCjeH2itroFIBLFLXynkzy7hi7g/1tO4KWavRaLX9vM4pr\n9PPvoZ/r5OqOOTBgiGlLqidppsIeFOfBoKoP3E1USTo4cIvONBY4QaNYGj7J\nDIWHUcrXgtIUs6J5a7pW+Eh45AWTW+m3W5cSSg==\n";

        var hashKey = "a8ec90c269c3bd8812739a4d710bbb85";
        var hashIV = "edbeaacdb228dcac";
        var sut = new FeibaoPayCryptor(NullLogger<FeibaoPayCryptor>.Instance);

        var actual = sut.AesDecrypt(source, hashKey, hashIV);

        var expected = """
            {"amount":"100.0","gateway":"gcash","merchant_order_num":"08cc2701-d120-4c43-8f4f-d433e69b1aa3","merchant_order_remark":"","status":"success","sign":"5b110ad7d69703caef5ff1d0ff24c0d348426e35"}
            """;

        Assert.Equal(expected, actual);
    }
}
