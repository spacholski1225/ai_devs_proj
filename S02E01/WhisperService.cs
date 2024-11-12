using ai_devs_proj.ApiHelpers;
using ai_devs_proj.LLMHelpers;
using System.Diagnostics;
using System.Text;

internal class WhisperService
{
    private const string pythonInterpreter = @"C:\Users\spach\AppData\Local\Programs\Python\Python312\python.exe";
    private const string scriptPath = @"C:\Sources\ai_devs_proj\S02E01\Scripts\whisper_transcribe.py";

    internal async Task<string> GetAnswerFromAudioFiles()
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
            return string.Empty;
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

        return answer;
    }

    internal string TranscribeAudio(string audioFilePath)
    {
        var start = new ProcessStartInfo
        {
            FileName = pythonInterpreter,
            Arguments = $"{scriptPath} \"{audioFilePath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            EnvironmentVariables = { ["PYTHONIOENCODING"] = "utf-8" },
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        var output = string.Empty;
        var error = string.Empty;

        using (var process = Process.Start(start))
        {
            process.WaitForExit();

            using (var reader = process.StandardOutput)
            {
                output = reader.ReadToEnd();
            }

            using (var reader = process.StandardError)
            {
                error = reader.ReadToEnd();
            }
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            Console.WriteLine($"ERRORS: {error}");
        }

        return output;
    }

    internal void SaveTranscriptionToMarkdown(string transcriptionText, string markdownFilePath)
    {
        var markdownContent = new StringBuilder();

        markdownContent.AppendLine("# Transcription Result");
        markdownContent.AppendLine($"**Date:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        markdownContent.AppendLine();
        markdownContent.AppendLine("## Transcription Text");
        markdownContent.AppendLine();
        markdownContent.AppendLine(transcriptionText);

        File.WriteAllText(markdownFilePath, markdownContent.ToString(), Encoding.UTF8);
    }
}
