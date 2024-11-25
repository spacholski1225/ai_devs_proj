using System.Text.Json.Serialization;

namespace ai_devs_proj.S04E01.Models
{
    internal class InterfaceRequestModel
    {
        [JsonPropertyName("task")]
        public string Task { get; set; }

        [JsonPropertyName("apikey")]
        public string ApiKey { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; }
    }
}
