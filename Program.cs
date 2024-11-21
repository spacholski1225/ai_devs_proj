using ai_devs_proj.S03E04;

class Program
{
    static async Task Main(string[] args)
    {
        var finder = new FindLocation();
        await finder.RunAsync();
    }
}
