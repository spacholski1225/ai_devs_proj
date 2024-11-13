using System.Text.Json.Serialization;

namespace ai_devs_proj.S02E03.Serialize
{
    internal class ResponseSerialize
    {
        [JsonPropertyName("data")] public List<DataSerialize> Data { get; set; }
    }
}
