using ai_devs_proj.S02E02.Models;
using ai_devs_proj.S03E02;
using ai_devs_proj.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ai_devs_proj.S05E05
{
    internal class StoryService
    {

        private readonly HttpClient _aiDevsClient;

        internal StoryService()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://centrala.ag3nts.org/");
        }

        internal async Task RunAsync()
        {
            var openAiService = new OpenAiService();
            var questions = await GetQuestionsAsync();

            var folderPath = "C:\\Sources\\ai_devs_proj\\S05E05\\Files\\";

            var index = 0;

            foreach (var file in Directory.GetFiles(folderPath, "*.md"))
            {
                var response = await openAiService.CallToGPT("Jesteś pomocnym asystentem, odpowiadasz na pytania zadanie przez użytkownika. Przestrzegasz zasad:" +
                    "<rules>" +
                    "- nie wchodzisz w dyskusje z użytkownikiem," +
                    "- odpowiadasz na pytania w formacie JSON, w ten sam sposób jak zapyta użytkownik," +
                    "- jeżeli w <facts> nie ma informacji o danym pytaniu, odpowiedz ze nie wiesz" +
                    "</rules>" +
                    "<facts>" +
                    $"{await File.ReadAllTextAsync(file)}" +
                    "</facts>", questions);

                var whisper = new WhisperService();
                whisper.SaveTranscriptionToMarkdown(response.ToLower(),
                    $"C:\\Sources\\ai_devs_proj\\S05E05\\Files\\Answers\\{Path.GetFileName(index.ToString())}.md");
                index++;
            }

        }


        private async Task<string> GetQuestionsAsync()
        {
            var question = await _aiDevsClient.GetStringAsync($"data/{Environment.GetEnvironmentVariable("AI_DEVS_KEY")}/story.json");
            return question;
        }

        internal async Task IndexStoryAsync()
        {

            var folderPath = "C:\\Sources\\ai_devs_proj\\S05E05\\Files\\";
            var documents = new Dictionary<string, string>();
            foreach (var file in Directory.GetFiles(folderPath, "*.md"))
            {
                documents.Add(Guid.NewGuid().ToString(), $"{Path.GetFileNameWithoutExtension(file)}:{await File.ReadAllTextAsync(file)}");
            }

            var openAiEmbedding = new Embedding();
            var embeddedDocuments = await openAiEmbedding.GetEmbeddingsAsync(documents);

            var qdrantClient = new QdrantClient();
            await qdrantClient.CreateCollectionAsync("story", embeddedDocuments.First().Value.Length);

            var documentsWithEmbeddings = embeddedDocuments.Select(entry =>
            (
                id: entry.Key,
                vector: entry.Value,
                text: documents[entry.Key]
            )).ToList();

            await qdrantClient.UpsertDocumentsAsync("story", documentsWithEmbeddings);

            Console.WriteLine("Zakończono indeksowanie raportów w Qdrant.");

        }

        internal async Task Ocr()
        {
            var pngFiles = Directory.GetFiles("C:\\Sources\\ai_devs_proj\\S05E05\\Files\\zygfryd_notatnik\\", "*.png");

            foreach (var pngFile in pngFiles)
            {
                await DescribeAndSave(pngFile);
            }
        }

        private async Task DescribeAndSave(string filePath)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));

            var imgBase64 = Convert.ToBase64String(File.ReadAllBytes(filePath));


            var request = new RequestModel
            {
                Model = "gpt-4o",
                Messages = new List<object>
                {
                    new MessageModel
                    {
                        Role = "system",
                        Content = $"You are an outstanding and meticulous image. Specialized in converting Polish sentences to the text."
                    },
                    new ImageMessageModel
                    {
                        Role = "user",
                        Contents = new List<ContentModel>
                        {
                            new ContentModel()
                            {
                                ImageUrl = new ImageUrlModel
                                {
                                    Url = $"data:image/jpeg;base64,{imgBase64}"
                                },
                                Type = "image_url"
                            },
                            new ContentModel()
                            {
                                Type = "text",
                                Text =
                                    $"Parse the image and return text written on this image"
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

            var whisper = new WhisperService();
            whisper.SaveTranscriptionToMarkdown(answer.ToLower(),
                $"C:\\Sources\\ai_devs_proj\\S05E05\\Files\\Ocr\\{Path.GetFileName(filePath)}.md");
        }
    }
}
