using System.Text.Json.Serialization;

namespace ai_devs_proj.S03E05.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }


        [JsonPropertyName("access_level")]
        public string AccessLevel { get; set; }


        [JsonPropertyName("is_active")]
        public string IsActive { get; set; }


        [JsonPropertyName("lastlog")]
        public string LastLog { get; set; }
    }
}
