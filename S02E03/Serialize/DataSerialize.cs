using System.Text.Json.Serialization;

namespace ai_devs_proj.S02E03.Serialize
{
    internal class DataSerialize
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
