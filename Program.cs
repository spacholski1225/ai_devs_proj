using ai_devs_proj.S02E04;

class Program
{
    static async Task Main(string[] args)
    {
        var analyzer = new MediaFileAnalyzer("C:\\Users\\spach\\Downloads\\pliki_z_fabryki (1)");
        await analyzer.FinishTask();
    }
}
