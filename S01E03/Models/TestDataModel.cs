using System.Text.Json.Serialization;

namespace ai_devs_proj.S01E03.Models
{
    internal class TestDataModel
    {
        [JsonPropertyName("question")]
        public string Expression { get; set; }

        [JsonPropertyName("answer")]
        public int ExpressionAnswer { get; set; }

        [JsonPropertyName("test")]
        public TestModel Test { get; set; }
    }
}
