using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ai_devs_proj.LLMHelpers
{
    public static class GPTHelper
    {
        public static async Task<string> GetAnswerFromGPT(this string question, string additionalContext = "")
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));

            var prompt = $"Answer the following security question. Just return an answer. Question: {question}";

            if (!string.IsNullOrWhiteSpace(additionalContext))
            {
                prompt = $"{prompt} Take in mind this additional context: {additionalContext}";
            }

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 10
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
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
