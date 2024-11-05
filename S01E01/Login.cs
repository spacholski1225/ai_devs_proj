using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ai_devs_proj.S01E01
{
    internal class Login(string url, string username, string password)
    {
        public async Task TryToLoginAsync()
        {

            using var httpClient = new HttpClient { BaseAddress = new Uri(url) };

            var question = await FetchSecurityQuestion(httpClient);

            var answer = await GetAnswerFromGPT(question);

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

        private static async Task<string> GetAnswerFromGPT(string question)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OpenAI_Key"));

            var prompt = $"Answer the following security question. Just return a number. Question: {question}";
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                new { role = "user", content = prompt }
            },
                max_tokens = 10
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
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

            return answer ?? string.Empty;
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
