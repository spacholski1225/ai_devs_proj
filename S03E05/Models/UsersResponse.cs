using System.Text.Json.Serialization;

namespace ai_devs_proj.S03E05.Models
{
    internal class UsersResponse
    {
        [JsonPropertyName("response")]
        public List<User> Users { get; set; }
    }
}
