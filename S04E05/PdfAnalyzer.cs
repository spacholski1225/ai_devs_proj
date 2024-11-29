using ai_devs_proj.ApiHelpers;
using ai_devs_proj.Services;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace ai_devs_proj.S04E05
{
    internal class PdfAnalyzer
    {
        private readonly HttpClient _aiDevsClient;

        internal PdfAnalyzer()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://centrala.ag3nts.org/");
        }

        internal async Task RunAsync()
        {
            var questions = await GetQuestionsAsync();

            await PdfToMarkdownConverter.ConvertPdf("https://centrala.ag3nts.org/dane/notatnik-rafala.pdf");

            var openAiService = new OpenAiService();

            var getTextFromImage = await openAiService.AnalyzeImageGPTAsync("Jesteś pomocnym asystntem. Twoim zadaniem jest odczytanie tekstu z obrazka i zwrócenie go." +
                "<rules>" +
                "- nie wchodzisz w rozmowę z user" +
                "- zwracasz tylko tekst, który widnieje na obrazku" +
                "</rules>", "Odczytaj mi tekst z obrazka." , $"data:image/png;base64,{await File.ReadAllTextAsync("C:\\Sources\\ai_devs_proj\\S04E05\\Files\\imgBase64.txt")}");



            var answers = new Dictionary<string, string>();

            foreach ( var question in questions )
            {
                var questionNumber = question.Key;
                var questionText = question.Value.ToString();

                var answer = await openAiService.CallToGPT("Jesteś pomocnym asystentem zajmującym się analizą dzienników osób chorych psychicznie." +
                "Twoim zadaniem jest odpowiadanie na pytania w opraciu o dane z sekcji <context>. Zanim odpowiesz analizuj pytanie w sekcji <thinking>" +
                "<rules>" +
                "- odpowiadasz na pytanie tylko w oparciu o dane z <context>" +
                "- przed odpowiedzią analizujesz pytanie, szukasz odpowiedzi bazując na wydarzeniach w <context>"+
                "- odpowiedź na pytanie zwracasz zawsze w sekcji <answer>" +
                "- odpowiadasz krótko i konkretnie max 4 slowa" +
                "- jeśli nie ma w tekście jasnej odpowiedzi wywnioskuj z kontekstu lub odwołuj się do wydarzeń" +
                "</rules>" +
                "<context>" +
                $"{await File.ReadAllTextAsync("C:\\Sources\\ai_devs_proj\\S04E05\\Files\\markdown.md")}" +
                $"{getTextFromImage}" +
                "</context>", $"Odpowiedz na pytanie: {questionText}");


                answers[questionNumber] = ExtractTextFromAnswer(answer);
                await Console.Out.WriteLineAsync($"Answer for question {questionText}: {answer}");
            }

            var response = await ApiHelper.PostCompletedTask("notes", answers);

            await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
        }

        private async Task<JObject> GetQuestionsAsync()
        {
            var questions = await _aiDevsClient.GetStringAsync($"data/{Environment.GetEnvironmentVariable("AI_DEVS_KEY")}/notes.json");
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
