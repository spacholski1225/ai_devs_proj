using DroneApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace DroneApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DroneController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PostInstruction([FromBody] InstructionRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Instruction))
            {
                return BadRequest(new { error = "Instruction cannot be null or empty" });
            }

            Console.WriteLine($"Received instruction: {request.Instruction}");

            var answer = await CallToGPT("Jesteś pomocnym asystentem. Wyobraź sobie mapę jako siatke 4x4 , gdzie kolumny oznaczono literami (A, B, C, D), a wiersze cyframi (1, 2, 3, 4). Każde pole tej mapy zawiera różne elementy. Oto szczegóły każdego pola:" +
                "A1 - Miejsce startowe\r\nA2 - Łąka\r\nA3 - Łąka\r\nA4 - Skały\r\nB1 - Łąka\r\nB2 - Młyn\r\nB3 - Łąka\r\nB4 - Skały\r\nC1 - Drzewo\r\nC2 - Łąka\r\nC3 - Skały\r\nC4 - Samochód\r\nD1 - Dom\r\nD2 - Łąka\r\nD3 - Dwa drzewa\r\nD4 - Jaskinia" +
                "Twoim zadaniem jest podanie miejsca końcowego w postaci JSON." +
                "<example>" +
                "user: Dwa w prawo i na sam dół." +
                "assistant: {" +
                "\"thinking\": \"proces myślenia\"" +
                "\"description:\" samochód" +
                "}" +
                "" +
                "user: trzy w prawo, dwa w dół, jeden w lewo." +
                "assistant: {" +
                "\"thinking\": \"proces myślenia\"" +
                "\"description:\" skały" +
                "}" +
                "</example>" +
                "<rules>" +
                "- zawsze zaczynasz od pola A1 - miejsce startowe" +
                "- zwracasz odpowiedz w postaci tekstu o formacie json. Nie poprzedzasz znakami ```json oraz nie konczysz ```" +
                "- właściwość thinking zawiera proces Twojej analizy, jest on generowany pierwszy" +
                "- właściowść description zawiera odpowiedź co znajduje się na danym polu" +
                "- analizujesz krok po kroku instrukcje poruszania się dronem po mapie, po każdym kroku analizujesz poprzedni krok oraz następny." +
                "</rules>",
                $"Znajdz co znajduje się na danym polu bazując na tej treści: {request.Instruction}");


            await Console.Out.WriteLineAsync(answer);

            return Ok(JsonSerializer.Deserialize<JsonDocument>(answer));
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

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/chat/completions")
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
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
