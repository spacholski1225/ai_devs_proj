using ai_devs_proj.S02E03;

class Program
{
    static async Task Main(string[] args)
    {
        var generator = new ImageGenerator();
        await generator.FinishTask();
    }
}
