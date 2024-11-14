using System.Text.Json.Serialization;

namespace ai_devs_proj.S02E04.Models
{
    internal class ApiRequestModel
    {
        [JsonPropertyName("people")] 
        public List<string> People { get; set; }

        [JsonPropertyName("hardware")]
        public List<string> Hardwares { get; set; }
    }
}
