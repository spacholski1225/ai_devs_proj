using ai_devs_proj.S03E03;

class Program
{
    static async Task Main(string[] args)
    {
        var databaseService = new DatabaseService();
        await databaseService.Run();
    }
}
