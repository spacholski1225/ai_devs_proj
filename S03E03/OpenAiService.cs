using ai_devs_proj.S02E02.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ai_devs_proj.S03E03
{
    internal class OpenAiService
    {
        private readonly HttpClient _openAiClient;

        public OpenAiService()
        {
            _openAiClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/chat/completions")
            };
            _openAiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));
        }

        public async Task<string> CallToGPT(string systemPrompt, string userPrompt)
        {
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new
                    {
                        role = "system", content = systemPrompt
                    },
                    new
                    {
                        role = "user", content = userPrompt
                    }
                }
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await _openAiClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseBody);
            var answer = jsonDocument.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?.Trim();

            return answer ?? string.Empty;
        }

        public async Task<string> CallToGPT(List<object> messages)
        {
            var request = new RequestModel
            {
                Model = "gpt-4o-mini",
                Messages = messages
            };

            var jsonRequestBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await _openAiClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseBody);
            var answer = jsonDocument.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?.Trim();

            return answer ?? string.Empty;
        }
    }
}
