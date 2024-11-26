using ai_devs_proj.S04E02;

class Program
{
    static async Task Main(string[] args)
    {
        var finetuned = new FineTunedService();
        await finetuned.RunAsync();
    }
}
