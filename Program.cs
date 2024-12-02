using ai_devs_proj.S05E01;

class Program
{
    static async Task Main(string[] args)
    {
        var service = new AgentService();
        await service.RunAsync();
    }
}
