using ai_devs_proj.S03E05;
using ai_devs_proj.S04E01;

class Program
{
    static async Task Main(string[] args)
    {
        var interfaceService = new InterfaceService();
        await interfaceService.RunAsync();
    }
}
