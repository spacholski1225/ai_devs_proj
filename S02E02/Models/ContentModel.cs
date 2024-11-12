using System.Text.Json.Serialization;

namespace ai_devs_proj.S02E02.Models
{
    internal class ContentModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("image_url")]
        public ImageUrlModel ImageUrl { get; set; }
    }
}
