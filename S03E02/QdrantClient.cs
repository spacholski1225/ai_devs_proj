using System.Text;
using System.Text.Json;

namespace ai_devs_proj.S03E02
{
    internal class QdrantClient
    {
        private readonly HttpClient _httpClient;

        public QdrantClient(string baseUrl = "https://94853e1f-878e-45df-86e2-b01119dbb8d8.us-east4-0.gcp.cloud.qdrant.io:6333")
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient.DefaultRequestHeaders.Add("api-key", $"{Environment.GetEnvironmentVariable("QDRANT_KEY")}");
        }

        public async Task<string> QueryDatabaseAsync(float[] queryEmbedding, string collectionName)
        {
            var query = new
            {
                vector = queryEmbedding,
                top = 1,
            };

            var jsonRequest = JsonSerializer.Serialize(query);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"/collections/{collectionName}/points/search", content);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                return string.Empty;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Database response: ");
            Console.WriteLine(responseBody);

            return responseBody;
        }

        public async Task CreateCollectionAsync(string collectionName, int vectorSize)
        {
            if (await CollectionExistsAsync(collectionName))
            {
                return;
            }

            var request = new
            {
                vectors = new { size = vectorSize, distance = "Cosine" }
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"/collections/{collectionName}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpsertDocumentsAsync(string collectionName, List<(string id, float[] vector, string text)> documents)
        {
            var points = new List<object>();

            var currentIdUUID = 0;

            foreach (var doc in documents)
            {
                points.Add(new
                {
                    id = doc.id,
                    vector =  doc.vector,
                    payload = new
                    {
                        text = doc.text
                    }
                });

                currentIdUUID++;
            }

            var request = new
            {
                points
            };

            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"/collections/{collectionName}/points", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var response = await _httpClient.GetAsync($"/collections/{collectionName}");
            return response.IsSuccessStatusCode;
        }
    }
}
