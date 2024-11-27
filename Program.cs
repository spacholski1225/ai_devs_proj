using ai_devs_proj.S04E03;

class Program
{
    static async Task Main(string[] args)
    {
        var scrapper = new ScrapperService();
        await scrapper.RunAsync();
    }
}
