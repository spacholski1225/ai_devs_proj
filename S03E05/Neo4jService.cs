using ai_devs_proj.S03E05.Models;
using Neo4j.Driver;
using System.Text.Json;
using ai_devs_proj.ApiHelpers;

namespace ai_devs_proj.S03E05
{
    internal class Neo4jService
    {
        private static readonly IDriver _driver = GraphDatabase.Driver(
            "localhost",
            AuthTokens.Basic("neo4j", ""));

        internal async Task Run()
        {
            var names = await FindShortestPathAsync("Rafał", "Barbara");

            var response = await ApiHelper.PostCompletedTask("connections", string.Join(", ", names));

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        internal async Task ImportDataAsync()
        {
            var jsonUsers = await File.ReadAllTextAsync("C:\\Sources\\ai_devs_proj\\S03E05\\files\\users.json");
            var jsonConnectionsAllTextAsync = await File.ReadAllTextAsync("C:\\Sources\\ai_devs_proj\\S03E05\\files\\connections.json");
            var usersResponse = JsonSerializer.Deserialize<UsersResponse>(jsonUsers);
            var connectionResponse = JsonSerializer.Deserialize<ConnectionResponse>(jsonConnectionsAllTextAsync);

            using (var session = _driver.AsyncSession())
            {
                foreach (var user in usersResponse.Users)
                {
                    await session.RunAsync(@"
                    CREATE (:User {id: $id, username: $username, access_level: $access_level, 
                                   is_active: $is_active, lastlog: date($lastlog)})",
                        new
                        {
                            id = user.Id,
                            username = user.Username,
                            access_level = user.AccessLevel,
                            is_active = user.IsActive == "1",
                            lastlog = user.LastLog
                        });
                }

                foreach (var connection in connectionResponse.Connections)
                {
                    await session.RunAsync(@"
                    MATCH (u1:User {id: $user1_id}), (u2:User {id: $user2_id})
                    CREATE (u1)-[:CONNECTED_TO]->(u2)",
                        new
                        {
                            user1_id = connection.User1Id,
                            user2_id = connection.User2Id
                        });
                }
            }

            await _driver.DisposeAsync();
        }

        public async Task<List<string>> FindShortestPathAsync(string startUsername, string endUsername)
        {
            var query = @"
            MATCH (start:User {username: $startUsername}), (end:User {username: $endUsername})
            MATCH p = shortestPath((start)-[*]-(end))
            RETURN nodes(p) AS pathNodes, relationships(p) AS pathRelationships";

            try
            {
                using var session = _driver.AsyncSession();
                var result = await session.RunAsync(query, new
                {
                    startUsername,
                    endUsername
                });
                    
                var shortestPath = await result.SingleAsync(record =>
                {
                    var nodes = record["pathNodes"].As<IList<INode>>();
                    return nodes.Select(node => node["username"].As<string>()).ToList();
                });

                await _driver.DisposeAsync();
                return shortestPath ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding shortest path: {ex.Message}");

                await _driver.DisposeAsync();
                return new List<string>();
            }
        }
    }
}
