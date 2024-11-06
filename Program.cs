using ai_devs_proj.S01E03;

class Program
{
    static async Task Main(string[] args)
    {
        var caller = new ApiCaller("https://centrala.ag3nts.org");

        await caller.FinishTask();
    }
}
