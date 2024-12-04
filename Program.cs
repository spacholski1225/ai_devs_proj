using ai_devs_proj.S05E03;

class Program
{
    static async Task Main(string[] args)
    {
        var service = new CrackerService();
        await service.RunAsync();
    }
}
