using ai_devs_proj.S01E05;

class Program
{
    static async Task Main(string[] args)
    {
        var caller = new OllamaCaller("/");

        await caller.DoTask();
    }
}
