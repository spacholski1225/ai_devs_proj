using System.Text.Json.Serialization;

namespace ai_devs_proj.S01E02.Models
{
    public class Message
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("msgID")]
        public long MessageId { get; set; }
    }
}
