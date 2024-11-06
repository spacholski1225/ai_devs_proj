using System.Text.Json.Serialization;

namespace ai_devs_proj.ApiHelpers.Models
{
    internal class ReturnTaskModel
    {
        [JsonPropertyName("task")]
        public string TaskName { get; set; }

        [JsonPropertyName("apikey")]
        public string ApiKey { get; set; }

        [JsonPropertyName("answer")]
        public object Answer { get; set; }
    }
}
