using ai_devs_proj.LLMHelpers;
using ai_devs_proj.S01E02.Models;
using System.Text;
using System.Text.Json;

namespace ai_devs_proj.S01E02
{
    internal class Verify(string url)
    {
        private const string ADDITIONAL_CONTEXT = "You answer only in English, the capital of Poland is Krakow. The current year is 1999. The known number from the book Hitchhiker's Guide to the Galaxy is 69";
        public async Task StartVerificationAsync()
        {

            using var httpClient = new HttpClient { BaseAddress = new Uri(url) };

            var startMessage = new Message
            {
                Text = "READY",
                MessageId = 0
            };

            var startMessageJson = JsonSerializer.Serialize(startMessage);

            var startResponse = await httpClient.PostAsync(url, new StringContent(startMessageJson, Encoding.UTF8, "application/json"));

            Console.WriteLine($"Response: {startResponse.Content.ReadAsStringAsync().Result}");
            var responseMessage = JsonSerializer.Deserialize<Message>(startResponse.Content.ReadAsStringAsync().Result);

            if (responseMessage is null)
            {
                Console.WriteLine("Cannot deserialize response message");
                return;
            }


            var answer = await GPTHelper.GetAnswerFromGPT(ADDITIONAL_CONTEXT);
            Console.WriteLine($"Answer: {answer}");

            var answerMessage = new Message
            {
                MessageId = responseMessage.MessageId,
                Text = answer
            };

            var answerJson = JsonSerializer.Serialize(answerMessage);

            var response = await httpClient.PostAsync(url, new StringContent(answerJson, Encoding.UTF8, "application/json"));

            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }
    }
}
