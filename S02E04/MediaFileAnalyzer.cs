using ai_devs_proj.ApiHelpers;
using ai_devs_proj.LLMHelpers;
using ai_devs_proj.S02E02.Models;
using ai_devs_proj.S02E04.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ai_devs_proj.S02E04
{
    internal class MediaFileAnalyzer
    {
        private const string REGEX_PATTERN_HUMAN = @"<answer>(.*?human.*?)</answer>";
        private const string REGEX_PATTERN_MACHINE = @"<answer>(.*?machine.*?)</answer>";
        private readonly string _folderPath;

        public MediaFileAnalyzer(string folderPath)
        {
            _folderPath = folderPath;
        }

        internal async Task FinishTask()
        {
            var request = new ApiRequestModel
            {
                Hardwares = [],
                People = []
            };

            var mp3Files = Directory.GetFiles(_folderPath, "*.mp3");
            var pngFiles = Directory.GetFiles(_folderPath, "*.png");
            var txtFiles = Directory.GetFiles(_folderPath, "*.txt");

            await AnalyzeAudioFiles(mp3Files, request);
            await AnalyzeTextFiles(txtFiles, request);
            await AnalyzeImageFiles(pngFiles, request);

            Console.WriteLine(JsonSerializer.Serialize(request));

            var response = await ApiHelper.PostCompletedTask("kategorie", request);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private async Task AnalyzeImageFiles(string[] pngFiles, ApiRequestModel request)
        {
            foreach (var pngFile in pngFiles)
            {
                var isAboutHuman = await IsInformationAboutHumanOrMachineFromPng(pngFile);
                var isAboutMachine = await IsInformationAboutHumanOrMachineFromPng(pngFile);
                if (Regex.Match(isAboutHuman, REGEX_PATTERN_HUMAN, RegexOptions.IgnoreCase).Success)
                {
                    request.People.Add(Path.GetFileName(pngFile));
                }
                if (Regex.Match(isAboutMachine, REGEX_PATTERN_MACHINE, RegexOptions.IgnoreCase).Success)
                {
                    request.Hardwares.Add(Path.GetFileName(pngFile));
                }
            }
        }

        private async Task AnalyzeTextFiles(string[] txtFiles, ApiRequestModel request)
        {
            foreach (var txtFile in txtFiles)
            {
                var isAboutHuman = await IsInformationAboutHumanOrMachine(await File.ReadAllTextAsync(txtFile));
                var isAboutMachine = await IsInformationAboutHumanOrMachine(await File.ReadAllTextAsync(txtFile));
                if (Regex.Match(isAboutHuman, REGEX_PATTERN_HUMAN, RegexOptions.IgnoreCase).Success)
                {
                    request.People.Add(Path.GetFileName(txtFile));
                }
                if (Regex.Match(isAboutMachine, REGEX_PATTERN_MACHINE, RegexOptions.IgnoreCase).Success)
                {
                    request.Hardwares.Add(Path.GetFileName(txtFile));
                }
            }
        }

        private async Task AnalyzeAudioFiles(string[] mp3Files, ApiRequestModel request)
        {
            foreach (var mp3File in mp3Files)
            {
                var transcribeAudio = TranscribeAudio(Path.GetFileName(mp3File));
                var isAboutHuman = await IsInformationAboutHumanOrMachine(transcribeAudio);
                var isAboutMachine = await IsInformationAboutHumanOrMachine(transcribeAudio);
                if (Regex.Match(isAboutHuman, REGEX_PATTERN_HUMAN, RegexOptions.IgnoreCase).Success)
                {
                    request.People.Add(Path.GetFileName(mp3File));
                }
                if (Regex.Match(isAboutMachine, REGEX_PATTERN_MACHINE, RegexOptions.IgnoreCase).Success)
                {
                    request.Hardwares.Add(Path.GetFileName(mp3File));
                }
            }
        }

        internal string TranscribeAudio(string fileName)
        {
            if (File.Exists($"C:\\Sources\\ai_devs_proj\\S02E04\\Transcription\\{fileName}.md"))
            {
                return File.ReadAllText($"C:\\Sources\\ai_devs_proj\\S02E04\\Transcription\\{fileName}.md");
            }

            var whisper = new WhisperService();
            var transcription = whisper.TranscribeAudio($"{_folderPath}/{fileName}");
            whisper.SaveTranscriptionToMarkdown(transcription, $"C:\\Sources\\ai_devs_proj\\S02E04\\Transcription\\{fileName}.md");

            return transcription;
        }

        internal async Task<string> IsInformationAboutHumanOrMachine(string text)
        {
            var response = await GPTHelper.GetAnswerFromGPT(
                $"You are a great specialist in capturing information about human or machine. Analyze the received text and answer the question whether there is description about human or machine in the file. It it possible that the text is about something else in that case you should return false. Think about answer loudly in <thinking> section and the answer return in <answer> section. For example <answer> human </answer>. Given text: {text}");

           // Console.WriteLine($"Analyzing {text}: {response}");

            return response.ToLower();
        }

        internal async Task<string> IsInformationAboutHumanOrMachineFromPng(string filePath)
        {
            if (File.Exists($"C:\\Sources\\ai_devs_proj\\S02E04\\Ocr\\{Path.GetFileName(filePath)}.md"))
            {
                var text = File.ReadAllText($"C:\\Sources\\ai_devs_proj\\S02E04\\Ocr\\{Path.GetFileName(filePath)}.md");

                return await IsInformationAboutHumanOrMachine(text);
            }

            var fileStream = File.OpenRead(filePath);
            if (!fileStream.CanRead)
            {
                Console.WriteLine("Cannot read file");
                return string.Empty;
            }

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
                $"C:\\Sources\\ai_devs_proj\\S02E04\\Ocr\\{Path.GetFileName(filePath)}.md");

            return await IsInformationAboutHumanOrMachine(answer.ToLower());
        }
    }
}
