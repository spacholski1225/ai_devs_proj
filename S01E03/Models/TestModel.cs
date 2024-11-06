using System.Text.Json.Serialization;

namespace ai_devs_proj.S01E03.Models
{
    internal class TestModel
    {
        [JsonPropertyName("q")]
        public string Question { get; set; }

        [JsonPropertyName("a")]
        public string Answer { get; set; }
    }
}
