using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.FeibaoPay;

public class FeibaoPayCryptor(ILogger<FeibaoPayCryptor> logger)
{
    public string AesEncrypt(string source, string hashKey, string hashIV)
    {
        if (string.IsNullOrEmpty(source))
        {
            logger.LogWarning("this source is empty.");
            throw new ArgumentException("this source is empty.");
        }

        var toEncryptArray = Encoding.UTF8.GetBytes(source);

        var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(hashKey);
        aes.IV = Encoding.UTF8.GetBytes(hashIV);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var crypto = aes.CreateEncryptor();

        var resultArray = crypto.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    public string AesDecrypt(string source, string hashKey, string hashIV)
    {
        if (string.IsNullOrEmpty(source))
        {
            logger.LogWarning("this source is empty.");
            throw new ArgumentException("this source is empty.");
        }

        var toEncryptArray = Convert.FromBase64String(source);

        var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(hashKey);
        aes.IV = Encoding.UTF8.GetBytes(hashIV);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var crypto = aes.CreateDecryptor();

        var resultArray = crypto.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Encoding.UTF8.GetString(resultArray);
    }
}
