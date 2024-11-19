using ai_devs_proj.S03E02;

class Program
{
    static async Task Main(string[] args)
    {
        var embeddingTask = new Finisher();
        await embeddingTask.Finish();
    }
}
