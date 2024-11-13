using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ai_devs_proj.ApiHelpers;
using ai_devs_proj.LLMHelpers;
using ai_devs_proj.S02E03.Serialize;

namespace ai_devs_proj.S02E03
{
    internal class ImageGenerator
    {
        public async Task FinishTask()
        {
            var robotDescription = await GetDescriptionAsync();
            Console.WriteLine($"Given description: {robotDescription}");

            var correctedDescription = await GPTHelper.GetAnswerFromGPT(
                $"Jesteś analitykiem zeznań w sprawie robotów pilnujących fabryk. Oto opis jednego robota: {robotDescription}. Zwróc sam opis robota po angielsku w 100 słowach.");
            Console.WriteLine($"Corrected description: {correctedDescription}");

            var jsonImage = await GenerateImageAsync(
                $"The outside of factory is guarding by robot whose looks like {correctedDescription}");
            var url = GetUrlOfGeneratedImage(jsonImage);

            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("Cannot get url");
                return;
            }

            var response = await ApiHelper.PostCompletedTask("robotid", url, "https://centrala.ag3nts.org/report");

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private string? GetUrlOfGeneratedImage(string jsonResponse)
        {
            var responseObject = JsonSerializer.Deserialize<ResponseSerialize>(jsonResponse);

            return responseObject?.Data[0].Url;
        }

        private async Task<string> GenerateImageAsync(string prompt, string size = "1024x1024", int n = 1)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));

            var requestBody = new
            {
                prompt = prompt,
                size = size,
                n = n
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/images/generations", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Image generation successful:");
                Console.WriteLine(responseContent);
                return responseContent;
            }
            Console.WriteLine($"Image generation failed with status code: {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Error details:");
            Console.WriteLine(errorContent);
            return string.Empty;
        }

        private async Task<string> GetDescriptionAsync()
        {
            using var client = new HttpClient();

            var response = await client.GetAsync(
                $"https://centrala.ag3nts.org/data/{Environment.GetEnvironmentVariable("AI_DEVS_KEY")}/robotid.json");

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseBody);

            var description = jsonDocument.RootElement
                .GetProperty("description")
                .GetString()
                ?.Trim();

            return description ?? string.Empty;
        }
    }
}
