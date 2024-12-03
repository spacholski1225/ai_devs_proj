using Newtonsoft.Json;

namespace ai_devs_proj.S05E02.Models
{
    public class IdsResponse
    {
        [JsonProperty("reply")]
        public List<ReplyItem> Reply { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class ReplyItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }

}
