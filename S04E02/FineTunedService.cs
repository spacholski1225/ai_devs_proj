using ai_devs_proj.ApiHelpers;
using ai_devs_proj.Services;

namespace ai_devs_proj.S04E02
{
    internal class FineTunedService
    {
        private readonly HttpClient _aiDevsClient;

        public FineTunedService()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://centrala.ag3nts.org/");
        }

        public async Task RunAsync()
        {
            var valuesToClassification = ReadFileToDictionary("C:\\Sources\\ai_devs_proj\\S04E02\\Files\\verify.txt");

            var correctData = new List<string>();

            foreach (var value in valuesToClassification)
            {
                var openaiService = new OpenAiService();
                var classification = await openaiService.CallToGPT("Klasyfikuj poprawność danych.", value.Value, "ft:gpt-4o-mini-2024-07-18:personal:aidevs-classification:AXo9UqlM");

                if (classification.Equals("1"))
                {
                    correctData.Add(value.Key);
                }
            }

            var response = await ApiHelper.PostCompletedTask("research", correctData);

            await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
        }

        static Dictionary<string, string> ReadFileToDictionary(string filePath)
        {
            var result = new Dictionary<string, string>();

            try
            {
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();

                        result[key] = value;
                    }
                    else
                    {
                        Console.WriteLine($"Incorrect line: {line}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return result;
        }
    }
}
