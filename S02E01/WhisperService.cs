using System.Diagnostics;
using System.Text;

internal class WhisperService
{
    private const string pythonInterpreter = @"C:\Users\spach\AppData\Local\Programs\Python\Python312\python.exe";
    private const string scriptPath = @"C:\Sources\ai_devs_proj\S02E01\Scripts\whisper_transcribe.py";

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
