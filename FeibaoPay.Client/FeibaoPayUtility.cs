using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Payment.FeibaoPay.Client;

public static class FeibaoPayUtility
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters = {
        }
    };
}
