using System.Text;
using System.Text.Json;

namespace ai_devs_proj.S01E05
{
    internal class OllamaCaller(string url)
    {
        internal async Task DoTask()
        {
            var uncensoredText = await GetPlainText();
            Console.WriteLine($"Download text: {uncensoredText}");

            var censoredText = await GetCensoredText(uncensoredText);
            Console.WriteLine($"Censored text: {censoredText}");

            var response = await ApiHelpers.ApiHelper.PostCompletedTask("CENZURA", censoredText, "https://centrala.ag3nts.org/report");
            Console.WriteLine($"Response: {await response.Content.ReadAsStringAsync()}");
        }

        internal async Task<string> GetCensoredText(string uncensoredText)
        {
            using var httpClient = new HttpClient();
            var prompt = $"<context> Dbasz o to aby zastępować wrażliwe dane osobowe słowem \"CENZURA\". Wrażliwe dane to Imię i Nazwisko, miasto, nazwa ulicy i numer oraz wiek. Zwróć tylko poprawiony tekst. Nie zmieniaj jego formatu, ma być dokładnie taki sam. </context> " +
                         $"<examples> User: Jan Kowalski mieszka w Warszawie przy ulicy Stromej 23. Output: CENZURA mieszka w CENZURA przy ulicy CENZURA." +
                         $"User: Krzysztof Kononowicz ma 41 lat i mieszka w Zielonej Górze na ulicy Palacza 3. Output: CENZURA ma CENZURA lat i mieszka w CENZURA na ulicy CENZURA." +
                         $"User: Marcin Wahadało mieszkał w Poznaniu na ulicy Ptasiej 9 i ma 19 lat. Output: CENZURA mieszkał w CENZURA na ulicy CENZURA i ma CENZURA lat. </examples>" +
                         $"Oto text, który masz zmienić: {uncensoredText}";

            
            var requestBody = new
            {
                model = "llama3.1",
                stream = false,
                temperature = 0.1,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 10
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:11434/api/chat", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseBody);
            var answer = jsonDocument.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?.Trim();

            return answer ?? string.Empty;
        }

        internal async Task<string> GetPlainText()
        {
            using var client = new HttpClient { BaseAddress = new Uri(url) };

            var response = await client.GetAsync($"/data/{Environment.GetEnvironmentVariable("AI_DEVS_KEY")}/cenzura.txt");

            return await response.Content.ReadAsStringAsync();
        }
    }
}
