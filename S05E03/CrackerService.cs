using ai_devs_proj.S02E05;
using ai_devs_proj.Services;
using System.Text.Json;

namespace ai_devs_proj.S05E03
{
    internal class CrackerService
    {
        private readonly HttpClient _aiDevsClient;
        private readonly OpenAiService _openAiService;
        private readonly string _envKey;

        internal CrackerService()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://rafal.ag3nts.org");

            _openAiService = new OpenAiService();
            _envKey = Environment.GetEnvironmentVariable("AI_DEVS_KEY");
        }

        internal async Task RunAsync()
        {
            var htmlAnalyzer = new HtmlAnalyzer(string.Empty);
            var html = await htmlAnalyzer.GetHtmlPageAsync("https://centrala.ag3nts.org/dane/arxiv-draft.html");

            var text = htmlAnalyzer.GetTextFromHtml(html);
            var brief = await _openAiService.CallToGPT("Jestes pomocnym asystentem, robiacym notatki na podstawie tekstu. Twoim zadaniem jest skracanie tekstu, tak żeby zawchować wszystkie ważne informacje.",
                $"Skróć ten tekst, ale zachowaj ważne infromacje. {text}");

            var getHash = await _aiDevsClient.PostAsync("/b46c3", new StringContent("{\"password\": \"NONOMNISMORI\"}"));

            using JsonDocument document = JsonDocument.Parse(await getHash.Content.ReadAsStringAsync());

            var hash = string.Empty;
            
            if (document.RootElement.TryGetProperty("message", out JsonElement messageElement))
            {
                hash = messageElement.GetString() ?? string.Empty;
            }
            var getSources = await _aiDevsClient.PostAsync("/b46c3", new StringContent(string.Concat("{\"sign\": \"", hash, "\"}")));

            var (signature, timestamp) = ExtractDetails(await getSources.Content.ReadAsStringAsync());

            var questions0 = await _aiDevsClient.GetStringAsync("https://rafal.ag3nts.org/source0");
            var questions1 = await _aiDevsClient.GetStringAsync("https://rafal.ag3nts.org/source1");

            var answer = await _openAiService.CallToGPT(string.Empty, string.Concat($"Facts: {text}" ,"Odpowiedzi zwróć jako zwykły tekst, każda odpowiedz oddziel przecinkiem bez znaków specjalnych. Przykład: odpowiedz1, odzpowiedz2, odpowiedz3", questions0, questions1));

            var json = string.Concat("{\"apikey\":\"", _envKey, "\",\"timestamp\":\"", timestamp, "\",\"signature\": \"", signature, "\",\"answer\": \"", answer, "\"}");
            var response = await _aiDevsClient.PostAsync("/b46c3", new StringContent(json));

            await Console.Out.WriteLineAsync(json);
            await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
        }

        public static (string Signature, long Timestamp) ExtractDetails(string jsonResponse)
        {   
            using JsonDocument document = JsonDocument.Parse(jsonResponse);
            
            if (document.RootElement.TryGetProperty("message", out JsonElement messageElement))
            {
                string signature = messageElement.GetProperty("signature").GetString() ?? string.Empty;
                
                long timestamp = messageElement.GetProperty("timestamp").GetInt64();

                return (signature, timestamp);
            }
            else
            {
                throw new Exception("Pole 'message' nie zostało znalezione w odpowiedzi JSON.");
            }
        }
    }
}
