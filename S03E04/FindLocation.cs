using ai_devs_proj.S02E02.Models;
using ai_devs_proj.S03E03;
using ai_devs_proj.S03E03.Requests;
using System.Text.Json;
using ai_devs_proj.ApiHelpers;

namespace ai_devs_proj.S03E04
{
    internal class FindLocation
    {
        private readonly HttpClient _aiDevsClient;

        public FindLocation()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://centrala.ag3nts.org/");
        }

        internal async Task RunAsync()
        {
            var textResponse = await _aiDevsClient.GetAsync("dane/barbara.txt");
            var textFile = await textResponse.Content.ReadAsStringAsync();

            var openAiService = new OpenAiService();

            var namesAndLocations = await openAiService.CallToGPT("Jesteś pomocnym asystentem. Przeanalizuj dany tekst i zwróc uwagę na połączenia pomiędzy lokalizacjami oraz osobami. Zwróć listę osób i lokalizacji, które pojawiły się w tekście. " +
                                    "<rules>" +
                                    "- Zwracasz TYLKO osoby i lokalizacje oddzielone spacjami. Przykład: ANDRZEJ KRAKOW GDYNIA MARCIN" +
                                    "- Osoba i lokalizacja jest w formie mianownika. Przykład GRZESIEK, a nie grześkowi" +
                                    "- Imiona i lokalizacje nie mają polskich znaków." +
                                    "</rules>", $"O to tekst do analizy: {textFile}");


            var messages = new List<object>
            {
                new MessageModel
                {
                    Role = "system",
                    Content = "Jesteś pomocnym asystentem wykluczającym dane kombinacje na podstawie danych. " +
                              "<rules>" +
                              "- Zwracasz tylko jedno imie lub lokalizacje w formie mianownika np. ANDRZEJ" +
                              "- Jeżeli dana lokalizacja i imie jest powiązane - zingoruj je i wybierz inne." +
                              "</rules>"
                },
                new MessageModel
                {
                    Role = "user",
                    Content =
                        "Podam ci listę imion lub lokalizacji w formie mianownika. Chcę żebyś wybrał jedno imie lub lokalizację i zwrócił je. Jeśli wybierzesz imię ja podeślę Ci lokalizację w postaci kilu miast na przykład: POZNAN KRAKOW." +
                        "Jeśli wybierzesz lokalizację, podeślę Ci listę imion na przykład: MARCIN RAFAL. Twoim zadaniem jest wykluczać różne opcje tak długo aż nie podam Ci imienia BARBARA. Dodatkowo możesz ode mnie dostać oznaczenie [**RESTRICTED DATA**]. Wtedy musisz się cofnąć, bo ta ścieżka jest zablokowana." +
                        $"O to lista: {namesAndLocations}"
                }
            };

            while (true)
            {
                var res = await openAiService.CallToGPT(messages);

                Console.WriteLine($"Response OpenAI [[{res}]]");

                messages.Add(new MessageModel
                {
                    Role = "assistant",
                    Content = res
                });

                var name = await GetLocationOrNameAsync(res, "places");
                var location = await GetLocationOrNameAsync(res, "people");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (name.ToLower().Equals("barbara"))
                    {
                        var response = await ApiHelper.PostCompletedTask("loop", res);
                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                        break;
                    }

                    messages.Add(new MessageModel
                    {
                        Role = "user",
                        Content = name
                    });
                }
                if (!string.IsNullOrWhiteSpace(location))
                {
                    messages.Add(new MessageModel
                    {
                        Role = "user",
                        Content = location
                    });
                }

            }

        }

        internal async Task<string> GetLocationOrNameAsync(string locationOrName, string uri)
        {
            var request = new DatabaseRequest
            {
                ApiKey = Environment.GetEnvironmentVariable("AI_DEVS_KEY"),
                Query = $"{locationOrName}"
            };

            var jsonRequest = JsonSerializer.Serialize(request);

            var response = await _aiDevsClient.PostAsync($"{uri}", new StringContent(jsonRequest));

            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            Console.WriteLine($"Response with table struct: {await response.Content.ReadAsStringAsync()}");

            var responseBody = await response.Content.ReadAsStringAsync();

            var jsonDocument = JsonDocument.Parse(responseBody);
            var answer = jsonDocument.RootElement
                .GetProperty("message")
                .GetString()
                ?.Trim();

            if (!string.IsNullOrWhiteSpace(answer) && answer.Contains("{{"))
            {
                return answer;
            }

            return answer ?? string.Empty;
        }
    }
}
