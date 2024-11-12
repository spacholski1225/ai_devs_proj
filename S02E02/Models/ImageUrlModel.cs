using System.Text.Json.Serialization;

namespace ai_devs_proj.S02E02.Models
{
    internal class ImageUrlModel
    {
        [JsonPropertyName("url")] public string Url { get; set; }
    }
}
