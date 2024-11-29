using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;

public static class PdfToMarkdownConverter
{
    public static async Task ConvertPdf(string pdfUrl)
    {
        byte[] pdfData;
        using (var client = new HttpClient())
        {
            pdfData = await client.GetByteArrayAsync(pdfUrl);
        }

        var tempPdfPath = System.IO.Path.GetTempFileName();
        File.WriteAllBytes(tempPdfPath, pdfData);

        var pdfText = ExtractTextFromPdf(tempPdfPath);

        File.Delete(tempPdfPath);

        var markdownText = ConvertTextToMarkdown(pdfText);
        await File.WriteAllTextAsync("C:\\Sources\\ai_devs_proj\\S04E05\\Files\\markdown.md", markdownText);

        var lastPageImg = PdfImageExtractor.ConvertPdfToPng(pdfData);
        await File.WriteAllTextAsync("C:\\Sources\\ai_devs_proj\\S04E05\\Files\\imgBase64.txt", lastPageImg);
    }

    private static string ExtractTextFromPdf(string pdfPath)
    {
        var text = new StringBuilder();

        using (var reader = new PdfReader(pdfPath))
        {
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                text.Append("\n");
            }
        }

        return text.ToString();
    }

    private static string ConvertTextToMarkdown(string text)
    {
        var markdown = new StringBuilder();

        string[] paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var paragraph in paragraphs)
        {
            markdown.AppendLine(paragraph.Trim());
            markdown.AppendLine();
        }

        return markdown.ToString();
    }
}
