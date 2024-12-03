using ai_devs_proj.S05E02;

class Program
{
    static async Task Main(string[] args)
    {
        var service = new LocationService();
        await service.RunAsync();
    }
}
