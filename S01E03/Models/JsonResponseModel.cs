using System.Text.Json.Serialization;

namespace ai_devs_proj.S01E03.Models
{
    internal class JsonResponseModel
    {
        [JsonPropertyName("apikey")]
        public string ApiKey { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("copyright")]
        public string CopyRight { get; set; }

        [JsonPropertyName("test-data")]
        public List<TestDataModel> TestData { get; set; }
    }
}
