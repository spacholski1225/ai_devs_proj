using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

public static class PdfImageExtractor
{
    public static string ConvertPdfToPng(byte[] pdf)
    {
        using PdfDocument document = PdfReader.Open(new MemoryStream(pdf));
        if (document.PageCount == 0)
            throw new InvalidOperationException("PDF does not contain any pages.");


        var png = Freeware.Pdf2Png.Convert(pdf, document.PageCount);

        return Convert.ToBase64String(png);
    }
}