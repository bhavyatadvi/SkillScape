using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

public class RoadmapWebSearchService : IRoadmapWebSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RoadmapWebSearchService> _logger;

    public RoadmapWebSearchService(HttpClient httpClient, ILogger<RoadmapWebSearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(8);
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SkillScape-RoadmapEngine/1.0 (contact@skillscape.org)");
        }
    }

    public async Task<string> SearchCareerKnowledgeAsync(string careerTitle)
    {
        if (string.IsNullOrWhiteSpace(careerTitle)) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"=== LIVE WEB SEARCH KNOWLEDGE FOR: {careerTitle.ToUpperInvariant()} ===");

        try
        {
            // 1. Query Wikipedia Search API for Outline and Curriculum
            var wikiSearchQueries = new[]
            {
                $"Outline of {careerTitle}",
                $"{careerTitle} learning roadmap curriculum skills",
                $"{careerTitle} current tools frameworks standards 2026",
                $"{careerTitle} emerging technologies industry trends 2026"
            };

            var extractedSummaries = new List<string>();

            foreach (var query in wikiSearchQueries)
            {
                var searchUrl = $"https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch={Uri.EscapeDataString(query)}&format=json";
                var responseStr = await _httpClient.GetStringAsync(searchUrl);

                using var doc = JsonDocument.Parse(responseStr);
                if (doc.RootElement.TryGetProperty("query", out var queryElem) &&
                    queryElem.TryGetProperty("search", out var searchArray) &&
                    searchArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in searchArray.EnumerateArray().Take(2))
                    {
                        var title = item.TryGetProperty("title", out var t) ? t.GetString() : null;
                        var snippet = item.TryGetProperty("snippet", out var s) ? s.GetString() : null;

                        if (!string.IsNullOrEmpty(title))
                        {
                            var cleanSnippet = CleanHtmlTags(snippet);
                            // Fetch page extract
                            var extractUrl = $"https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=1&explaintext=1&titles={Uri.EscapeDataString(title)}&format=json";
                            try
                            {
                                var extStr = await _httpClient.GetStringAsync(extractUrl);
                                using var extDoc = JsonDocument.Parse(extStr);
                                if (extDoc.RootElement.TryGetProperty("query", out var qExt) &&
                                    qExt.TryGetProperty("pages", out var pagesElem))
                                {
                                    foreach (var pageProp in pagesElem.EnumerateObject())
                                    {
                                        if (pageProp.Value.TryGetProperty("extract", out var extVal))
                                        {
                                            var extractText = extVal.GetString();
                                            if (!string.IsNullOrWhiteSpace(extractText))
                                            {
                                                var shortExt = extractText.Length > 400 ? extractText.Substring(0, 400) + "..." : extractText;
                                                extractedSummaries.Add($"• [{title}]: {shortExt}");
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug(ex, "Failed fetching Wikipedia extract for {Title}", title);
                                if (!string.IsNullOrEmpty(cleanSnippet))
                                {
                                    extractedSummaries.Add($"• [{title}]: {cleanSnippet}");
                                }
                            }
                        }
                    }
                }
            }

            // 2. Query DuckDuckGo Instant Answer API
            try
            {
                var ddgUrl = $"https://api.duckduckgo.com/?q={Uri.EscapeDataString(careerTitle + " current tools frameworks emerging technologies 2026")}&format=json";
                var ddgStr = await _httpClient.GetStringAsync(ddgUrl);
                using var ddgDoc = JsonDocument.Parse(ddgStr);
                var root = ddgDoc.RootElement;
                if (root.TryGetProperty("AbstractText", out var absElem) && absElem.ValueKind == JsonValueKind.String)
                {
                    var abs = absElem.GetString();
                    if (!string.IsNullOrWhiteSpace(abs))
                    {
                        extractedSummaries.Add($"• [DuckDuckGo Abstract]: {abs}");
                    }
                }

                if (root.TryGetProperty("RelatedTopics", out var relatedTopics) && relatedTopics.ValueKind == JsonValueKind.Array)
                {
                    foreach (var topic in relatedTopics.EnumerateArray().Take(8))
                    {
                        if (topic.TryGetProperty("Text", out var text) && text.ValueKind == JsonValueKind.String)
                        {
                            var relatedText = text.GetString();
                            if (!string.IsNullOrWhiteSpace(relatedText))
                                extractedSummaries.Add($"• [DuckDuckGo Related]: {relatedText}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "DuckDuckGo API search failed for {CareerTitle}", careerTitle);
            }

            if (extractedSummaries.Count > 0)
            {
                foreach (var summary in extractedSummaries.Distinct())
                {
                    sb.AppendLine(summary);
                }
            }
            else
            {
                _logger.LogWarning("No usable web results found for career {CareerTitle}", careerTitle);
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web search failed for career {CareerTitle}. Using standard domain knowledge.", careerTitle);
            return string.Empty;
        }

        return sb.ToString();
    }

    private static string CleanHtmlTags(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var clean = Regex.Replace(input, @"<.*?>", string.Empty);
        return HttpUtility.HtmlDecode(clean).Trim();
    }
}
