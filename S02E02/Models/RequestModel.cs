using System.Text.Json.Serialization;

namespace ai_devs_proj.S02E02.Models
{
    internal class RequestModel
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }
        
        [JsonPropertyName("messages")]
        public List<object> Messages { get; set; }
    }
}
