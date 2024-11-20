using System.Text.Json.Serialization;

namespace ai_devs_proj.S03E03.Requests
{
    internal class DatabaseRequest
    {
        [JsonPropertyName("task")] public string Task { get; set; }
        [JsonPropertyName("apikey")] public string ApiKey { get; set; }
        [JsonPropertyName("query")] public string Query { get; set; }
    }
}
