using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ai_devs_proj.LLMHelpers;

namespace ai_devs_proj.S01E01
{
    public class Login(string url, string username, string password)
    {
        public async Task TryToLoginAsync()
        {

            using var httpClient = new HttpClient { BaseAddress = new Uri(url) };

            var question = await FetchSecurityQuestion(httpClient);

            var answer = await GPTHelper.GetAnswerFromGPT(question);

            await LoginAsync(httpClient, url, username, password, answer);
        }

        private static async Task<string> FetchSecurityQuestion(HttpClient httpClient)
        {
            try
            {
                var response = await httpClient.GetAsync("/");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                var questionStart = responseBody.IndexOf("<p id=\"human-question\">Question:<br />") + 35;
                var questionEnd = responseBody.IndexOf("</p>", questionStart);
                var question = responseBody.Substring(questionStart, questionEnd - questionStart).Trim();

                Console.WriteLine($"Downloaded Question: {question}");
                return question;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot Get Question: {ex.Message}");
                return null;
            }
        }

        private static async Task LoginAsync(HttpClient httpClient, string loginEndpoint, string username, string password, string answer)
        {
            var loginData = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("answer", answer)
        });

            try
            {
                var response = await httpClient.PostAsync(loginEndpoint, loginData);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Logged correctly");
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                else
                {
                    Console.WriteLine($"Logging Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error occurs: {ex.Message}");
            }
        }
    }
}
