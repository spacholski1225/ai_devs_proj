using ai_devs_proj.ApiHelpers;
using ai_devs_proj.S04E01.Models;
using ai_devs_proj.Services;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ai_devs_proj.S04E01
{
    internal class InterfaceService
    {
        private readonly HttpClient _aiDevsClient;

        public InterfaceService()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://centrala.ag3nts.org/");
        }

        internal async Task RunAsync()
        {
            var getInfoAboutPhotos = await GetPhotosResponseAsync();

            var openAiService = new OpenAiService();
            var getListOfUrl = await openAiService.CallToGPT("Jesteś pomocnym asystntem specjalizującym się w tworzeniu URL i zwracaniu JSONA.",
                $"Oto odpowiedź z zewnętrznego systemu {getInfoAboutPhotos}. Twoim zadaniem jest na podstawie wiadomości utworzyć JSONa, który będzie zawierał kolekcję URL (powinny być cztery URL). " +
                $"<example>" +
                "{ \"result\": [" +
                "{" +
                "   \"thinking\": \"\" - tutaj wstawiasz proces analizy na podstawie której tworzysz URL" +
                "   \"url\": \"https://centrala.ag3nts.org/barbara/IMG_001.PNG\" - tutaj znajduje się URL stworzony na podstawie dostarczonej przez użytkownika wiadomości " +
                "}" +
                "]}" +
                $"</example>" +
                $"<rules>" +
                $"- Zwracasz tylko kolekcje JSON (bez ```json) w której są obiekty jak w <example>, nie wchodzisz w dyskusje z użytkownikiem" +
                $"- Zawsze pierwszy parametr JSON to thinking w którym opisujesz swój tok rozumowania." +
                $"</rules>");

            await Console.Out.WriteLineAsync(getListOfUrl);



            var urlsResponse = JsonSerializer.Deserialize<UrlsResponseModel>(getListOfUrl);

            var correctPhotosUrl = new List<string>();

            foreach (var img in urlsResponse.Result)
            {
                var whatsWrongWithImg = string.Empty;
                while (true)
                {
                    whatsWrongWithImg = await openAiService.AnalyzeImageGPTAsync("Jesteś pomocnym asystentem analizującym wady obrazów. Możesz zwracać tylko wartości:" +
                    "REPAIR\r\n\r\nDARKEN\r\n\r\nBRIGHTEN", "Przeanalizuj co jest nie tak z tym zdjęciem. Zdjęcie może być zbyt ciemne, zbyt jasne, zakłócone lub w porządku." +
                    "<rules>" +
                    "- Nie wchodzisz w dyskusję z użytkownikiem." +
                    "- Zwracasz tylko jedna z wartości: REPAIR, DARKEN, BRIGHTEN w zależności od problemu z obrazem." +
                    "- REPAIR - jeśli zdjęcie jest zakłócone." +
                    "- DARKEN - jeśli zdjęcie jest zbyt jasne." +
                    "- BRIGHTEN - jeśli zdjęcie jest zbyt ciemne." +
                    "- OK - jeśli jest w porządku" +
                    "</rules>", img.Url);

                    if (whatsWrongWithImg.Equals("OK"))
                    {
                        correctPhotosUrl.Add(img.Url);
                        break;
                    }

                    var request = new InterfaceRequestModel
                    {
                        Answer = $"{whatsWrongWithImg} {Path.GetFileName(img.Url)}",
                        ApiKey = Environment.GetEnvironmentVariable("AI_DEVS_KEY"),
                        Task = "photos"
                    };

                    var jsonRequestBody = JsonSerializer.Serialize(request);
                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                    var getFixedPhoto = await _aiDevsClient.PostAsync("/report", content);
                    await Console.Out.WriteLineAsync(await getFixedPhoto.Content.ReadAsStringAsync());
                    var fileNameOfFixedPhoto = ExtractFilenameFromResponse(await getFixedPhoto.Content.ReadAsStringAsync());

                    if (string.IsNullOrWhiteSpace(fileNameOfFixedPhoto))
                    {
                        break;
                    }

                    img.Url = ReplaceFilenameInUrl(img.Url, fileNameOfFixedPhoto);

                }
            }

            var listOfImageDescriptions = new List<string>();

            foreach (var photoUrl in correctPhotosUrl)
            {
                var response = await openAiService.AnalyzeImageGPTAsync("Jestes pomocnym asystentem specjalizującym się w opisie zdjęć kobiet.",
                    "Opisz to zdjęcie kobiety w języku polskim.", photoUrl);
                listOfImageDescriptions.Add(response);
                await Console.Out.WriteLineAsync(response);
            }

            var description = await openAiService.CallToGPT("Jesteś pomocnym asystentem tworzącym rysopis postaci ze zdjęć.",
                $"Na podstawie tych czterech opisów : {string.Join(", ", listOfImageDescriptions)}. Napisz rysopis jednej osoby. Weż pod uwagę tylko te opisy, które się powtarzają, zwróc uwagę na szczegóły." +
                $"<rules>" +
                $"- nie wchodzisz w dyskusje z użytkownikiem" +
                $"- zwracasz TYLKO rysopis" +
                $"- zwracasz uwagę na szczegółu np. jaki ma kolor włosów, jaki ma oczy, czy ma jakieś cechy charakterystyczne. Wszystko to co powinien mieć rysopis." +
                $"</rules>");

            await Console.Out.WriteLineAsync(description);

            var aidevResponse = await ApiHelper.PostCompletedTask("photos", description);

            await Console.Out.WriteLineAsync(await aidevResponse.Content.ReadAsStringAsync());

        }

        internal async Task<string> GetPhotosResponseAsync()
        {
            var request = new InterfaceRequestModel
            {
                Answer = "START",
                ApiKey = Environment.GetEnvironmentVariable("AI_DEVS_KEY"),
                Task = "photos"
            };


            var jsonRequestBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await _aiDevsClient.PostAsync("/report", content);
            await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStringAsync();
        }

        string ExtractFilenameFromResponse(string jsonResponse)
        {
            try
            {
                var jsonDocument = JsonDocument.Parse(jsonResponse);

                var message = jsonDocument.RootElement.GetProperty("message").GetString();

                if (!string.IsNullOrEmpty(message))
                {
                    var regex = new Regex(@"IMG[\w\d_-]+\.PNG", RegexOptions.IgnoreCase);

                    var match = regex.Match(message);

                    if (match.Success)
                    {
                        return match.Value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing response: {ex.Message}");
                return null;
            }
        }


        static string ReplaceFilenameInUrl(string url, string newFileName)
        {
            try
            {
                int lastSlashIndex = url.LastIndexOf('/');
                if (lastSlashIndex != -1)
                {
                    string baseUrl = url.Substring(0, lastSlashIndex + 1);
                    return baseUrl + newFileName;
                }

                return url;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error replacing filename: {ex.Message}");
                return null;
            }
        }
    }
}
