using ai_devs_proj.S02E02.Models;
using HtmlAgilityPack;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using ai_devs_proj.ApiHelpers;

namespace ai_devs_proj.S02E05
{
    internal class HtmlAnalyzer(string baseUrl)
    {
        internal async Task Run()
        {
            if (Path.Exists("C:\\Sources\\ai_devs_proj\\S02E05\\answer.txt"))
            {
                var answersDictionary = ParseAnswers(await File.ReadAllTextAsync("C:\\Sources\\ai_devs_proj\\S02E05\\answer.txt"));
                var response = await ApiHelper.PostCompletedTask("arxiv", answersDictionary);

                Console.WriteLine(await response.Content.ReadAsStringAsync());
                return;
            }

            var html = await GetHtmlPageAsync($"{baseUrl}/arxiv-draft.html");
            var questions = await GetQuestions();

            var text = GetTextFromHtml(html);
            var images = GetImagesSourcesFromHtml(html);
            var audios = GetAudioSourcesFromHtml(html);

            var audioTranscription = string.Empty;

            foreach (var audioUrl in audios)
            {
                audioTranscription = await GetAudioTranscriptionAsync(audioUrl);
            }

            var imageDescription = new List<string>();

            foreach (var imageUrl in images)
            {
                imageDescription.Add(await GetDescriptionOfImageAsync(imageUrl));
            }

            var answer = await AskModelAsync(text, audioTranscription, imageDescription, questions);

            Console.WriteLine(answer);

            await File.WriteAllTextAsync("C:\\Sources\\ai_devs_proj\\S02E05\\answer.txt", answer, Encoding.UTF8);

            var res = await ApiHelper.PostCompletedTask("arxiv", ParseAnswers(answer));

            Console.WriteLine(await res.Content.ReadAsStringAsync());
        }

        internal async Task<string> GetQuestions()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(new Uri(
                $"https://centrala.ag3nts.org/data/{Environment.GetEnvironmentVariable("AI_DEVS_KEY")}/arxiv.txt"));

            return await response.Content.ReadAsStringAsync();
        }

        internal async Task<string> AskModelAsync(string text, string transcription, List<string> imageDesc, string questions)
        {
            var request = new RequestModel
            {
                Model = "gpt-4o",
                Messages =
                [
                    new MessageModel
                    {
                        Role = "system",
                        Content = "You are the best analyzer. You can answer for questions only based on user provided data. You are answering in one sentence on each question."
                    },
                    new MessageModel
                    {
                        Role = "user",
                        Content = "I will give you a information to analyze. After that I will ask you some question. While answering please think loudly about each question in <thinking> section. After that answer in max one sentence for every question in <answer>."
                    },
                    new MessageModel
                    {
                        Role = "user",
                        Content = $"It is an information from text: {text}"
                    },
                    new MessageModel
                    {
                        Role = "user",
                        Content = $"It is an information from audio transcription: {transcription}"
                    },
                    new MessageModel
                    {
                        Role = "user",
                        Content = $"It is an information from image description: {string.Join(" | ", imageDesc)}"
                    },
                    new MessageModel
                    {
                        Role = "user",
                        Content = $"There are questions: {questions}. Analyze them and use <thinking> to think about questions and answer. Answers return in <answer> section for example:" +
                                  $"<answer>" +
                                  $"01-answer" +
                                  $"02-answer" +
                                  $"NN-answer" +
                                  $"</answer>"
                    }
                ]
            };

            var jsonRequestBody = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));

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

            return answer ?? string.Empty;
        }


        internal async Task<string> GetDescriptionOfImageAsync(string imageUrl)
        {
            var request = new RequestModel
            {
                Model = "gpt-4o",
                Messages =
                [
                    new MessageModel
                    {
                        Role = "system",
                        Content = "You are the best analyzer images. You are focusing on that what is on the image."
                    },

                    new ImageMessageModel
                    {
                        Role = "user",
                        Contents =
                        [
                            new ContentModel
                            {
                                ImageUrl = new ImageUrlModel
                                {
                                    Url = $"{imageUrl}"
                                },
                                Type = "image_url"
                            },

                            new ContentModel
                            {
                                Type = "text",
                                Text = "Analyze image and describe it in max 10 sentences. Remember one of the picture is not a cake or pie it is pizza. Focus also on fruits, for example strawberries"
                            }
                        ]
                    }
                ]
            };


            var jsonRequestBody = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));

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

            return answer ?? string.Empty;
        }

        internal async Task<string> GetHtmlPageAsync(string uri)
        {
            using var client = new HttpClient();
            return await client.GetStringAsync(uri);
        }

        internal async Task<string> GetAudioTranscriptionAsync(string audioUrl)
        {
            var filePath = "C:\\Sources\\ai_devs_proj\\S02E05\\Transcription\\";

            using (var client = new HttpClient())
            {
                var audioData = await client.GetByteArrayAsync(audioUrl);
                await File.WriteAllBytesAsync($"{filePath}temp.mp3", audioData);
                Console.WriteLine($"Audio saved: {filePath}temp.mp3");
            }

            

            if (!Path.Exists($"{filePath}transcription.md"))
            {
                var whisper = new WhisperService();
                var transcription = whisper.TranscribeAudio($"{filePath}temp.mp3");

                whisper.SaveTranscriptionToMarkdown(transcription, $"{filePath}transcription.md");

                return transcription;
            }

            return await File.ReadAllTextAsync($"{filePath}transcription.md");
        }

        internal IEnumerable<string> GetImagesSourcesFromHtml(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var images = htmlDoc.DocumentNode.SelectNodes("//img")?.Select(node => node.GetAttributeValue("src", null))
                .Where(src => src != null).Select(src => new Uri(new Uri(baseUrl), src).ToString());
            
            return images;
        }

        internal IEnumerable<string> GetAudioSourcesFromHtml(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var audios = htmlDoc.DocumentNode.SelectNodes("//audio/source")
                ?.Select(node => node.GetAttributeValue("src", null))
                .Where(src => src != null).Select(src => new Uri(new Uri(baseUrl), src).ToString());

            return audios;
        }

        internal string GetTextFromHtml(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            return htmlDoc.DocumentNode.SelectSingleNode("//body")?.InnerText;
        }

        private static Dictionary<string, string> ParseAnswers(string response)
        {
            var answers = new Dictionary<string, string>();

            var answerSectionMatch = Regex.Match(response, @"<answer>(.*?)</answer>", RegexOptions.Singleline);
            if (answerSectionMatch.Success)
            {
                var answerSection = answerSectionMatch.Groups[1].Value;

                var regex = new Regex(@"(\d{2})-(.+?)(?=(\d{2}-|$))", RegexOptions.Singleline);
                foreach (Match match in regex.Matches(answerSection))
                {
                    var questionId = $"{match.Groups[1].Value}";
                    var shortAnswer = match.Groups[2].Value.Trim();
                    answers[questionId] = shortAnswer;
                }
            }

            return answers;
        }
    }
}
