using ai_devs_proj.S03E01;

class Program
{
    static async Task Main(string[] args)
    {
        var metaData = new Metadata();
        await metaData.Run();
    }
}
