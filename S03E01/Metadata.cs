using System.Dynamic;
using ai_devs_proj.S02E02.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ai_devs_proj.ApiHelpers;

namespace ai_devs_proj.S03E01
{
    internal class Metadata
    {

        internal async Task Run()
        {
            var files = Directory.GetFiles("C:\\Sources\\ai_devs_proj\\S03E01\\Files\\");
            var stringBuilder = new StringBuilder();

            foreach (var file in files)
            {
                var keywords = await GetKeyWords(file);

                stringBuilder.AppendLine(keywords);
            }

            await File.WriteAllTextAsync("C:\\Sources\\ai_devs_proj\\S03E01\\Files\\Keywords\\keywords.txt", stringBuilder.ToString(), Encoding.UTF8);

            var response = await ApiHelper.PostCompletedTask("dokumenty", GetKeyWordsDictionary());

            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }

        internal Dictionary<string, string> GetKeyWordsDictionary()
        {
            var lines = File.ReadAllLines("C:\\Sources\\ai_devs_proj\\S03E01\\Files\\Keywords\\keywords.txt");

            var result = new Dictionary<string, string>();

            foreach (var line in lines)
            {
                var parts = line.Split(':', 2);

                if (parts.Length == 2)
                {
                    var fileName = Path.GetFileName(parts[0].Trim());
                    var keywords = parts[1].Trim();
                    result[fileName] = keywords;
                }
            }

            return result;
        }

        internal async Task<string> GetKeyWords(string pathFile)
        {
            try
            {
                var text = await File.ReadAllTextAsync(pathFile);
                var document = PrepareContext();

                var request = new RequestModel
                {
                    Model = "gpt-4o",
                    Messages = new List<object>
                {
                    new MessageModel
                    {
                        Role = "system",
                        Content = $"Analizujesz dokumenty i opisujesz je w słowami kluczowymi w języku polskim w formie mianownika np. sportowiec, a nie sportowcem lub sportowców. Analize dokonujesz na podstawie zawartości tego pliku: <document> {document} </document>."
                    },
                    new ImageMessageModel
                    {
                        Role = "user",
                        Contents = new List<ContentModel>
                        {
                            new ContentModel()
                            {
                                Type = "text",
                                Text = $"Przeanalizuj ten tekst <text> {text} </text>. I na podstawie tego zwróć słowa kluczowe, bazując na pliku w sekcji <dokument>. Słowa kluczowe mają być w formie mianownika w języku polskim oraz zwróć je po przecinku."
                            }
                        }
                    }
                }
                };

                var jsonRequestBody = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));

                await Task.Delay(3000);

                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);
                var answer = jsonDocument.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()
                    ?.Trim();

                Console.WriteLine($"Keywords for file {pathFile}: {answer}");

                return string.IsNullOrWhiteSpace(answer) ? string.Empty : $"{Path.GetFileName(pathFile)}:{answer}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return string.Empty;
        }

        internal string PrepareContext()
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), "merged_facts.txt");

            try
            {
                var contentBuilder = new StringBuilder();

                var files = Directory.GetFiles("C:\\Sources\\ai_devs_proj\\S03E01\\Files\\Facts\\");

                foreach (var file in files)
                {
                    var content = File.ReadAllText(file);
                    contentBuilder.AppendLine(content);
                }

                return contentBuilder.ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurs: {ex.Message}");
            }

            return string.Empty;
        }
    }
}
