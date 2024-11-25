using ai_devs_proj.ApiHelpers;
using ai_devs_proj.S03E03.Requests;
using ai_devs_proj.Services;
using System.Text.Json;

namespace ai_devs_proj.S03E03
{
    internal class DatabaseService
    {
        private readonly HttpClient _aiDevsClient;

        public DatabaseService()
        {
            _aiDevsClient = new HttpClient();
            _aiDevsClient.BaseAddress = new Uri("https://centrala.ag3nts.org");
        }

        internal async Task Run()
        {
            var datacentersStruct = await GetDatabaseTableStruct("datacenters");
            var usersStruct = await GetDatabaseTableStruct("users");

            var openaiService = new OpenAiService();

            var response = await openaiService.CallToGPT("You are helpful assistant, specialist in mysql database queries. You are returning just SQL Query in one line. For example: SELECT * FROM USERS WHERE ID = 1",
                $"Take this table structure for table datacenters: {datacentersStruct} and for table users {usersStruct}. Create query that will return an answer for this question: które aktywne datacenter (DC_ID) są zarządzane przez pracowników, którzy są na urlopie (is_active=0). Return just a sql query.");

            var datacentersIdsResponse = await GetIdDatacenters(response);

            var jsonDatacentersIdsResponse = JsonSerializer.Deserialize<JsonElement>(datacentersIdsResponse);
            if (jsonDatacentersIdsResponse.TryGetProperty("reply", out JsonElement replyElement) && 
                replyElement.ValueKind == JsonValueKind.Array)
            {
                var datacenterIds = replyElement.EnumerateArray()
                    .Select(item => item.GetProperty("dc_id").GetString())
                    .ToList();


                var aiDevsResponse = await ApiHelper.PostCompletedTask("database", datacenterIds);
                Console.WriteLine($"AI DEVS RESPONSE: {await aiDevsResponse.Content.ReadAsStringAsync()}");
            }
        }

        internal async Task<string> GetDatabaseTableStruct(string tableName)
        {
            var request = new DatabaseRequest
            {
                Task = "database",
                ApiKey = Environment.GetEnvironmentVariable("AI_DEVS_KEY"),
                Query = $"show create table {tableName}"
            };

            var jsonRequest = JsonSerializer.Serialize(request);

            var response = await _aiDevsClient.PostAsync("/apidb/", new StringContent(jsonRequest));

            Console.WriteLine($"Response with table struct: {await response.Content.ReadAsStringAsync()}");

            return await response.Content.ReadAsStringAsync();
        }

        internal async Task<string> GetIdDatacenters(string query)
        {
            var request = new DatabaseRequest
            {
                Task = "database",
                ApiKey = Environment.GetEnvironmentVariable("AI_DEVS_KEY"),
                Query = $"{query}"
            };

            var jsonRequest = JsonSerializer.Serialize(request);

            var response = await _aiDevsClient.PostAsync("/apidb/", new StringContent(jsonRequest));

            Console.WriteLine($"Datacenters IDs: {await response.Content.ReadAsStringAsync()}");

            return await response.Content.ReadAsStringAsync();
        }
    }
}
