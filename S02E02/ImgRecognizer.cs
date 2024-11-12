using ai_devs_proj.S02E02.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ai_devs_proj.S02E02
{
    internal class ImgRecognizer
    {
        internal async Task<string> GetLocationBaseOnMapImg(string filePath)
        {
            var fileStream = File.OpenRead(filePath);
            if (!fileStream.CanRead)
            {
                Console.WriteLine("Cannot read file");
                return string.Empty;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));

            var imgBase64 = Convert.ToBase64String(File.ReadAllBytes(filePath));


            var request = new RequestModel
            {
                Model = "gpt-4o",
                Messages = new List<object>
                {
                    new MessageModel
                    {
                        Role = "system",
                        Content = "Jesteś wybitnym i skrupulatnym analitykiem obrazów specjalizującym się w znajdowaniu lokalizacji miasta bazując na mapie. Twoim zadaniem jest analiza obrazu oraz zwrócenie nazwy miasta."
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
                                    Url = $"data:image/jpeg;base64,{imgBase64}"
                                },
                                Type = "image_url"
                            },
                            new ContentModel()
                            {
                                Type = "text",
                                Text =
                                    "Przeanalizuj obraz, a następnie w sekcji <answer> umieść odpowiedź które miasta przedstawia mapa. Najpierw analizuj opraz i lokalizacje w sekcji <thinking>. Następnie zwróć odpwoiedź"
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
