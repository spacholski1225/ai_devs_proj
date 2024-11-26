using ai_devs_proj.S02E02.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ai_devs_proj.Services
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

        public async Task<string> CallToGPT(string systemPrompt, string userPrompt, string model = "gpt-4o")
        {
            var requestBody = new
            {
                model = model,
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

        public async Task<string> AnalyzeImageGPTAsync(string systemPrompt, string userPrompt, string imageUrl)
        {
            var request = new RequestModel
            {
                Model = "gpt-4o",
                Messages = new List<object>
                {
                    new MessageModel
                    {
                        Role = "system",
                        Content = systemPrompt
                    },
                    new ImageMessageModel
                    {
                        Role = "user",
                        Contents = new List<ContentModel>
                        {
                            new ContentModel()
                            {
                                ImageUrl = new ImageUrlModel
                                {
                                    Url = $"{imageUrl}"
                                },
                                Type = "image_url"
                            },
                            new ContentModel()
                            {
                                Type = "text",
                                Text = userPrompt
                            }
                        }
                    }
                }
            };

            var jsonRequestBody = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

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
