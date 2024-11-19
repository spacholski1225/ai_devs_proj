using System.Text;
using System.Text.Json;

namespace ai_devs_proj.S03E02
{
    internal class Embedding
    {
        private readonly HttpClient _httpClient;

        public Embedding()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("OpenAI_KEY")}");
        }

        public async Task<Dictionary<string, float[]>> GetEmbeddingsAsync(Dictionary<string, string> texts)
        {
            var textValues = texts.Values.ToList();

            var request = new
            {
                model = "text-embedding-ada-002",
                input = textValues 
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("embeddings", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            var embeddings = new Dictionary<string, float[]>();
            var i = 0;

            foreach (var item in jsonResponse.GetProperty("data").EnumerateArray())
            {
                var embedding = JsonSerializer.Deserialize<float[]>(item.GetProperty("embedding").ToString());
                embeddings.Add(texts.ElementAt(i).Key, embedding);
                i++;
            }

            return embeddings;
        }
    }
}
