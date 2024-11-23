using ai_devs_proj.S03E05;

class Program
{
    static async Task Main(string[] args)
    {
        var neo4j = new Neo4jService();
        await neo4j.Run();
    }
}
