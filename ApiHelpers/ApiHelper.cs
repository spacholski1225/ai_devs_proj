using ai_devs_proj.ApiHelpers.Models;
using System.Text.Json;

namespace ai_devs_proj.ApiHelpers
{
    internal static class ApiHelper
    {
        internal static async Task<HttpResponseMessage> PostCompletedTask(string taskName, object answer, string uri = "https://centrala.ag3nts.org/report")
        {
            var returnTask = new ReturnTaskModel
            {
                TaskName = taskName,
                ApiKey = Environment.GetEnvironmentVariable("AI_DEVS_KEY"),
                Answer = answer
            };

            var request = JsonSerializer.Serialize(returnTask, new JsonSerializerOptions { WriteIndented = true });

            using var client = new HttpClient();
            Console.WriteLine(request);
            return await client.PostAsync(uri, new StringContent(request));
        }
    }
}
