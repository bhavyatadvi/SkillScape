using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface IProfileParserService
{
    /// <summary>
    /// Natural Language Profile Parser:
    /// Parses free-text Current Status and Primary Goal (or form inputs) into structured data.
    /// Extracted profile includes degree, specialization, role, employment status, experience, skills, and target domain.
    /// </summary>
    ParsedProfileDto ParseProfile(string phaseKey, string? currentStatusRaw, string? primaryGoalRaw, Dictionary<string, string>? rawFields = null);
}

public interface IOcrIngestionService
{
    /// <summary>
    /// OCR Support Pipeline: Converts uploaded images/PDFs (resumes, marksheets, certificates) to text.
    /// </summary>
    Task<string> ExtractTextFromDocumentAsync(byte[] documentBytes, string fileName);
}
