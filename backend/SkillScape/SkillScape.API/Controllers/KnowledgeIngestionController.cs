using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeIngestionController : ControllerBase
{
    private readonly IKnowledgeIngestionService _ingestionService;

    public KnowledgeIngestionController(IKnowledgeIngestionService ingestionService)
    {
        _ingestionService = ingestionService;
    }

    [HttpPost("node")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> IngestNode([FromBody] IngestNodeRequest request)
    {
        if (request.Provenance == null || string.IsNullOrWhiteSpace(request.Provenance.SourceName))
        {
            return BadRequest("Strict provenance metadata is required.");
        }

        var id = await _ingestionService.IngestNodeAsync(
            request.Id,
            request.Label,
            request.Name,
            request.Description ?? string.Empty,
            request.PropertiesJson ?? "{}",
            request.Provenance
        );

        return Ok(new { Message = "Node ingested successfully.", NodeId = id });
    }

    [HttpPost("edge")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LinkNodes([FromBody] IngestEdgeRequest request)
    {
        if (request.Provenance == null || string.IsNullOrWhiteSpace(request.Provenance.SourceName))
        {
            return BadRequest("Strict provenance metadata is required.");
        }

        await _ingestionService.LinkNodesAsync(
            request.SourceNodeId,
            request.TargetNodeId,
            request.RelationshipType,
            request.PropertiesJson ?? "{}",
            request.Provenance
        );

        return Ok(new { Message = "Nodes linked successfully." });
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _ingestionService.GetGraphStatsAsync();
        return Ok(stats);
    }
}

public class IngestNodeRequest
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PropertiesJson { get; set; }
    public ProvenanceMetadata Provenance { get; set; } = null!;
}

public class IngestEdgeRequest
{
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public string? PropertiesJson { get; set; }
    public ProvenanceMetadata Provenance { get; set; } = null!;
}
