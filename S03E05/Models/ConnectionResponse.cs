using System.Text.Json.Serialization;

namespace ai_devs_proj.S03E05.Models
{
    internal class ConnectionResponse
    {
        [JsonPropertyName("response")]
        public List<Connection> Connections { get; set; }
    }
}
