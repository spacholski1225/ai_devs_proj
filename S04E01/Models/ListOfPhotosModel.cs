using System.Text.Json.Serialization;

namespace ai_devs_proj.S04E01.Models
{
    internal class ListOfPhotosModel
    {
        [JsonPropertyName("thinking")]
        public string Thinking { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
