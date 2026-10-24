using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using SkillScape.Application.Interfaces;
using UglyToad.PdfPig;

namespace SkillScape.Infrastructure.Services;

public class ResumeTextExtractor : IResumeTextExtractor
{
    private readonly ILogger<ResumeTextExtractor> _logger;

    public ResumeTextExtractor(ILogger<ResumeTextExtractor> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        if (stream == null || stream.Length == 0)
        {
            throw new ArgumentException("Uploaded file is empty.");
        }

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;

        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        string extractedText = string.Empty;

        try
        {
            if (ext == ".pdf")
            {
                extractedText = ExtractFromPdf(ms);
            }
            else if (ext == ".docx")
            {
                extractedText = ExtractFromDocx(ms);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported file format '{ext}'. Only PDF and DOCX files are allowed.");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from file: {FileName}", fileName);
            throw new InvalidOperationException("Unable to extract readable text from this resume. Please upload a text-based PDF/DOCX or fill the profile manually.", ex);
        }

        if (string.IsNullOrWhiteSpace(extractedText) || extractedText.Trim().Length < 15)
        {
            throw new InvalidOperationException("Unable to extract readable text from this resume. Please upload a text-based PDF or fill the profile manually.");
        }

        return extractedText.Trim();
    }

    private string ExtractFromPdf(Stream stream)
    {
        var sb = new StringBuilder();
        using var document = PdfDocument.Open(stream);
        foreach (var page in document.GetPages())
        {
            if (!string.IsNullOrWhiteSpace(page.Text))
            {
                sb.AppendLine(page.Text);
            }
        }
        return sb.ToString();
    }

    private string ExtractFromDocx(Stream stream)
    {
        using var wordDoc = WordprocessingDocument.Open(stream, false);
        var body = wordDoc.MainDocumentPart?.Document?.Body;
        if (body == null) return string.Empty;

        var sb = new StringBuilder();
        foreach (var paragraph in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
        {
            var text = paragraph.InnerText;
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text);
            }
        }

        return sb.Length > 0 ? sb.ToString() : body.InnerText;
    }
}
