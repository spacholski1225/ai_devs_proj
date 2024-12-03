using ai_devs_proj.ApiHelpers;
using ai_devs_proj.S03E03;
using ai_devs_proj.S03E04;
using ai_devs_proj.S05E02.Models;
using ai_devs_proj.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ai_devs_proj.S05E02
{
    internal class LocationService
    {
        private readonly HttpClient _aiDevsClient;

        internal LocationService()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://centrala.ag3nts.org/");
        }

        internal async Task RunAsync()
        {
            var question = await GetQuestionsAsync();

            var openAiService = new OpenAiService();

            var getLocation = await openAiService.CallToGPT("Jesteś pomocnym asystentem. Na podstawie treści Twoim zadaniem jest wyciągnięcie nazwy miasta z podanej przez użytkownika treści." +
                "Zawsze przestrzegasz zasad opisanych w <rules>." +
                "<rules>" +
                "- zwracasz tylko nazwę miasta" +
                "- odpowiedź umieszczasz w sekcji <answer>" +
                "- możesz myśleć na głos w sekcji <thinking> przed udzieleniem odpowiedzi" +
                "</rules>" +
                "<examples>" +
                "<answer>Kraków</answer>" +
                "</examples>", question);

            await Console.Out.WriteLineAsync(getLocation);

            var location = ExtractTextFromAnswer(getLocation);

            var findLocation = new FindLocation();
            var jsonNames = await findLocation.GetLocationOrNameAsync(location, "/places");

            var getStringOfNames = await openAiService.CallToGPT("Jesteś pomocnym asystentem. Na podstawie treści Twoim zadaniem jest wyciągnięcie imion w apostrofach oraz oddzielonych przecinkami." +
                "Zawsze przestrzegasz zasad opisanych w <rules>." +
                "<rules>" +
                "- zwracasz listę imion zgodnych z <examples>" +
                "- odpowiedź umieszczasz w sekcji <answer>" +
                "- możesz myśleć na głos w sekcji <thinking> przed udzieleniem odpowiedzi" +
                "- jeżeli w imionach dostarczonych przez użytkownika, pojawi się imie Barbara - omijasz je" +
                "</rules>" +
                "<examples>" +
                "<answer>'Julia', 'Robert', 'Rafał'</answer>" +
                "</examples>", jsonNames);

            await Console.Out.WriteLineAsync(getStringOfNames);

            var names = ExtractTextFromAnswer(getStringOfNames);

            var databaseService = new DatabaseService();

            var ids = await databaseService.GetIdDatacenters($"SELECT id, username FROM users WHERE username IN ({names})");



            var dicIdUsername = ExtractIdUsernameDictionary(ids);

            var request = await FetchGpsDataAsync(dicIdUsername);

            Console.WriteLine(JsonConvert.SerializeObject(request));

            var response = await ApiHelper.PostCompletedTask("gps", request);

            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }

        public async Task<Dictionary<string, Coordinates>> FetchGpsDataAsync(Dictionary<string, string> users)
        {
            var gpsResults = new Dictionary<string, Coordinates>();

            foreach (var kvp in users)
            {
                var userId = kvp.Key;
                var userName = kvp.Value;

                var payload = new { userID = userId };
                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    var response = await _aiDevsClient.PostAsync("/gps", content);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var gpsResponse = JsonConvert.DeserializeObject<GpsResponse>(jsonResponse);

                    if (gpsResponse?.Code == 0 && gpsResponse.Message != null)
                    {
                        gpsResults[userName] = gpsResponse.Message;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error for user {userName}: {ex.Message}");
                }
            }

            return gpsResults;
        }

        public static Dictionary<string, string> ExtractIdUsernameDictionary(string jsonResponse)
        {
            var parsedResponse = JsonConvert.DeserializeObject<IdsResponse>(jsonResponse);
            var dictionary = new Dictionary<string, string>();

            if (parsedResponse?.Reply != null)
            {
                foreach (var replyItem in parsedResponse.Reply)
                {
                    if (!string.IsNullOrEmpty(replyItem.Id) && !string.IsNullOrEmpty(replyItem.Username))
                    {
                        dictionary[replyItem.Id] = replyItem.Username;
                    }
                }
            }

            return dictionary;
        }

        private async Task<string> GetQuestionsAsync()
        {
            var question = await _aiDevsClient.GetStringAsync($"data/{Environment.GetEnvironmentVariable("AI_DEVS_KEY")}/gps_question.json");
            return question;
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
