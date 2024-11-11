using System.Text;
using ai_devs_proj.ApiHelpers;
using ai_devs_proj.LLMHelpers;

class Program
{
    static async Task Main(string[] args)
    {
        if (!File.Exists("C:\\Sources\\ai_devs_proj\\S02E01\\Transcription\\transcription.md"))
        {
            var whisperService = new WhisperService();

            var agnieszka = whisperService.TranscribeAudio("C:\\Users\\spach\\Downloads\\przesluchania\\agnieszka.m4a");
            var adam = whisperService.TranscribeAudio("C:\\Users\\spach\\Downloads\\przesluchania\\adam.m4a");
            var rafal = whisperService.TranscribeAudio("C:\\Users\\spach\\Downloads\\przesluchania\\rafal.m4a");
            var ardian = whisperService.TranscribeAudio("C:\\Users\\spach\\Downloads\\przesluchania\\ardian.m4a");
            var michal = whisperService.TranscribeAudio("C:\\Users\\spach\\Downloads\\przesluchania\\michal.m4a");
            var monika = whisperService.TranscribeAudio("C:\\Users\\spach\\Downloads\\przesluchania\\monika.m4a");

            var transcription = new StringBuilder();
            transcription.AppendLine("### Transkrybcja wypowiedzi Agnieszki");
            transcription.AppendLine(agnieszka);
            transcription.AppendLine("### Transkrybcja wypowiedzi Adama");
            transcription.AppendLine(adam);
            transcription.AppendLine("### Transkrybcja wypowiedzi Rafała");
            transcription.AppendLine(rafal);
            transcription.AppendLine("### Transkrybcja wypowiedzi Ardiana");
            transcription.AppendLine(ardian);
            transcription.AppendLine("### Transkrybcja wypowiedzi Michała");
            transcription.AppendLine(michal);
            transcription.AppendLine("### Transkrybcja wypowiedzi Moniki");
            transcription.AppendLine(monika);

            whisperService.SaveTranscriptionToMarkdown(transcription.ToString(), "C:\\Sources\\ai_devs_proj\\S02E01\\Transcription\\transcription.md");
        }

        var text = string.Empty;
        try
        {
            text = await File.ReadAllTextAsync("C:\\Sources\\ai_devs_proj\\S02E01\\Transcription\\transcription.md");
        }
        catch
        {
            Console.WriteLine("Cannot read transcription");
            return;
        }

        var answer = await GPTHelper.GetAnswerFromGPT(
             $"Jesteś narzędziem do analizy zeznań świadków. Przeanalizuj podany tekst i zastanów się nad nim. Analizuj tekst w oparciu o poniższe pytania." +
             $"<thinking>" +
             $"- O co chodzi w tym tekscie" +
             $"- Czego dotyczy ten tekst?" +
             $"- Kogo opisuje ten tekst?" +
             $"- Na jakie pytanie mam odpowiedzieć?" +
             $"</thinking> " +
             $"Na koniec odpowiedz na pytanie: Na jakiej ulicy znajduje się uczelnia, na której wykłada Andrzej Maj?. Znajdź dokładny adres tej uczelni korzystając z internetu." +
             $"Zwróć sam adres uczelni. Tekst do analizy: {text}");

        Console.WriteLine($"Odpowiedz: {answer}");

        var response = await ApiHelper.PostCompletedTask("mp3", answer, "https://centrala.ag3nts.org/report");

        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
}
