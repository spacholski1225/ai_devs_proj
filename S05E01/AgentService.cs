using ai_devs_proj.S02E02.Models;
using ai_devs_proj.S03E01;
using ai_devs_proj.Services;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace ai_devs_proj.S05E01
{
    internal class AgentService
    {
        private readonly HttpClient _aiDevsClient;

        internal AgentService()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://centrala.ag3nts.org/");
        }

        internal async Task RunAsync()
        {
            var questions = await GetQuestionsAsync();

            var answers = new Dictionary<string, string>();

            var openAiService = new OpenAiService();

            foreach (var question in questions)
            {
                var questionNumber = question.Key;
                var questionText = question.Value.ToString();

                var answer = await openAiService.CallToGPT(await PrepareDataForModelAsync(questionText));
                answers[questionNumber] = ExtractTextFromAnswer(answer);
                await Console.Out.WriteLineAsync($"Answer for question {questionText}: {answer}");
            }
        }

        internal async Task<List<object>> PrepareDataForModelAsync(string question)
        {
            var messages = new List<object>();

            var data = new Metadata();
            var document = data.PrepareContext();

            messages.Add(new MessageModel
            {
                Role = "system",
                Content = "Jesteś pomocnym asystentem analizującym rozmowy telefoniczne, na podstawie których wyciągasz odpowiedzi. Oto baza rozmów w formacie JSON:" +
                "<data_context>" +
                $"{await File.ReadAllTextAsync("C:\\Sources\\ai_devs_proj\\S05E01\\Files\\sorted.json")}" +
                "</data_context>" +
                "" +
                "<rules>" + 
                "- odpowiadasz na pytanie tylko w oparciu o dane z <data_context>" +
                "- przeanalizuj wszystkie rozmowy w <data_context> pod kątem kto z kim rozmawia." +
                "- przed odpowiedzią analizujesz pytanie, szukasz odpowiedzi bazując na informacjach i powiązaniach w <data_context>" +
                "- zanim odpowiesz zastanów się w sekcji <thinking>. Weź pod uwagę wszystkie rozmowy oraz powiązania między nimi." +
                "- odpowiedź na pytanie zwracasz zawsze w sekcji <answer>" +
                "- odpowiadasz krótko i konkretnie max 4 slowa" +
                "</rules>"
            });
            messages.Add(new MessageModel
            {
                Role = "system",
                Content = $"<data_context> {document} </data_context>"
            });

            messages.Add(new MessageModel
            {
                Role = "user",
                Content = $"Odpowiedz na pytanie bazując na danych w <data_context>. Oto pytanie: {question}"
            });

            return messages;
        }

        private async Task<JObject> GetQuestionsAsync()
        {
            var questions = await _aiDevsClient.GetStringAsync($"data/{Environment.GetEnvironmentVariable("AI_DEVS_KEY")}/phone_questions.json");
            return JObject.Parse(questions);
        }

        private static string ExtractTextFromAnswer(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string cannot be null or empty.", nameof(input));
            }

            string pattern = @"<answer>(.*?)</answer>";

            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return input;
        }
    }
}
