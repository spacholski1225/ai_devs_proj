using ai_devs_proj.S04E05;

class Program
{
    static async Task Main(string[] args)
    {
        var analyzer = new PdfAnalyzer();
        await analyzer.RunAsync();
    }
}
