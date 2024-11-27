using ai_devs_proj.ApiHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace ai_devs_proj.S04E03
{
    internal class ScrapperService
    {
        private readonly HttpClient _aiDevsClient;

        internal ScrapperService()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://centrala.ag3nts.org/");
        }

        internal async Task RunAsync()
        {
            var questions = await GetQuestionsAsync();
            var answers = await ProcessQuestionsWithFirecrawlAsync(questions);
            await Console.Out.WriteLineAsync($"Answers: {answers}");

            var response = await ApiHelper.PostCompletedTask("softo", answers);
            await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());

        }

        internal async Task<JObject> GetQuestionsAsync()
        {
            var questions = await _aiDevsClient.GetStringAsync($"data/{Environment.GetEnvironmentVariable("AI_DEVS_KEY")}/softo.json");
            return JObject.Parse(questions);
        }

        internal async Task<Dictionary<string, string>> ProcessQuestionsWithFirecrawlAsync(JObject questions)
        {
            var answers = new Dictionary<string, string>();

            foreach (var question in questions)
            {
                var originUrl = "https://softo.ag3nts.org/";
                var url = originUrl;
                var questionNumber = question.Key;
                var questionText = question.Value.ToString();
                var visitedUrls = new List<string>();

                while (true)
                {
                    var answer = await ScrappFirecrawlAsync(questionText, url, string.Join(", ", visitedUrls));

                    if (answer.StartsWith('/'))
                    {
                        url = $"{originUrl}{answer}";
                        visitedUrls.Add(answer);
                        continue;
                    }
                    if (answer.Contains(originUrl))
                    {
                        var page = answer.Substring(originUrl.Length);
                        url = $"{originUrl}/{page}";
                        visitedUrls.Add(page);
                        continue;
                    }

                    answers[questionNumber] = answer;
                    await Console.Out.WriteLineAsync($"Scrapped answer for question {questionText}: {answer}");
                    break;
                }
            }

            return answers;
        }

        internal async Task<string> ScrappFirecrawlAsync(string question, string url, string visitedUrl)
        {
            var firecrawlUrl = "https://api.firecrawl.dev/v1/scrape";

            var firecrawlRequest = new
            {
                url = url,
                formats = new[] { "extract" },
                onlyMainContent = true,
                includeTags = new[] { "p", "h1", "h2", "h3", "body", "a", "li"},
                excludeTags = new[] { "script", "style", "head" },
                waitFor = 2000,
                mobile = true,
                timeout = 10000,
                removeBase64Images = true,
                extract = new
                {
                    schema = new { answer = "string" },
                    systemPrompt = "Znajdź odpowiedź na pytanie użytkownika na tej stronie. W przypadku nie znalezienia odpowiedzi zwróc podstronę, która prawdopodobnie może zawierać odpowiedź." +
                    "<rules>" +
                    "- Zwracasz tylko odpowiedź." +
                    "- Jeśli nie możesz znaleźć odpowiedzi, zwracasz najbardziej prawdopodobną podstronę zawierającą odpowiedź." +
                    "- Nie zwracaj takiej samej podstrony, która aktualnie analizujesz." +
                    "- Nie zwracaj już przeanalizowanych podstron." +
                    "</rules>",
                    prompt = $"Znajdź odpowiedź na pytanie: \"{question}\". Jeśli nie znajdziesz odpowiedzi, zwróć url podstrony, która najprawdopodobniej zawiera informację o pytaniu." +
                    $"Aktualnie analizujesz stronę: {url}." +
                    $"Przeanalizowane podstrony: {visitedUrl}"
                },
            };

            var content = new StringContent(JsonConvert.SerializeObject(firecrawlRequest), Encoding.UTF8, "application/json");
           
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("FIRECRAWL_API_KEY"));
            var response = await client.PostAsync(firecrawlUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            try
            {
                var firecrawlResponse = JObject.Parse(responseString);
                var extractedAnswer = firecrawlResponse["data"]?["extract"]?["answer"]?.ToString() ?? string.Empty;
                return extractedAnswer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot process: {ex.Message}");
            }
            return string.Empty;
        }
    }
}
