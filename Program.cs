using ai_devs_proj.S01E02;

class Program
{
    static async Task Main(string[] args)
    {
        var verify = new Verify("/verify");
        await verify.StartVerificationAsync();
    }
}
