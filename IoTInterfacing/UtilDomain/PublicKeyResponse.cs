using System.Text.Json.Serialization;

namespace IoTInterfacing.UtilDomain;

public class PublicKeyResponse
{
    [JsonPropertyName("requestType")]
    public string RequestType { get; set; }

    [JsonPropertyName("key")]
    public byte[] Key { get; set; }

    [JsonConstructor]
    public PublicKeyResponse(string requestType, byte[] key)
    {
        RequestType = requestType;
        Key = key;
    }
}
