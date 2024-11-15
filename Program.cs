using ai_devs_proj.S02E04;
using ai_devs_proj.S02E05;

class Program
{
    static async Task Main(string[] args)
    {
            var analyzer = new HtmlAnalyzer("https://centrala.ag3nts.org/dane/");
            await analyzer.Run();
    }
}
