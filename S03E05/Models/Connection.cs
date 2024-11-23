using System.Text.Json.Serialization;

namespace ai_devs_proj.S03E05.Models
{
    public class Connection
    {
        [JsonPropertyName("user1_id")]
        public string User1Id { get; set; }


        [JsonPropertyName("user2_id")]
        public string User2Id { get; set; }
    }
}
