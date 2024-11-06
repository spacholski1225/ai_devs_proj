using ai_devs_proj.ApiHelpers;
using ai_devs_proj.LLMHelpers;
using ai_devs_proj.S01E03.Models;
using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ai_devs_proj.S01E03
{
    internal class ApiCaller(string url)
    {
        internal async Task FinishTask()
        {
            var model = await GetJsonModelAsync();

            var validModel = await GetValidTestDataAsync(model.TestData);

            model.TestData = validModel;
            model.ApiKey = Environment.GetEnvironmentVariable("AI_DEVS_KEY");

            var response = await ApiHelper.PostCompletedTask("JSON", model, $"{url}/report");

            Console.WriteLine($"Finish Task response: {await response.Content.ReadAsStringAsync()}");
        }

        private async Task<JsonResponseModel> GetJsonModelAsync()
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(url) };

            var response = await httpClient.GetAsync($"{url}/data/{Environment.GetEnvironmentVariable("AI_DEVS_KEY")}/json.txt");

            if (!response.IsSuccessStatusCode || response?.Content is null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<JsonResponseModel>(await response.Content.ReadAsStringAsync());
        }

        private async Task<List<TestDataModel>> GetValidTestDataAsync(List<TestDataModel> testData)
        {
            var dataTable = new DataTable();

            foreach (var data in testData)
            {
                var expressionAnswer = Convert.ToInt32(dataTable.Compute(data.Expression, null));


                if (expressionAnswer != data.ExpressionAnswer ) 
                {
                    data.ExpressionAnswer = expressionAnswer;
                }

                if (data.Test is not null)
                {
                    data.Test.Answer = await GPTHelper.GetAnswerFromGPT(data.Test.Question);
                }
            }

            return testData;
        }
    }
}
