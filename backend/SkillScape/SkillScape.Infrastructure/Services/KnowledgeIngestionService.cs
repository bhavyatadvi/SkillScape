using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class KnowledgeIngestionService : IKnowledgeIngestionService
{
    private readonly ApplicationDbContext _context;

    public KnowledgeIngestionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> IngestNodeAsync(string id, string label, string name, string description, string propertiesJson, ProvenanceMetadata provenance)
    {
        if (string.IsNullOrWhiteSpace(provenance.SourceName))
            throw new ArgumentException("Provenance SourceName is strictly required.");

        var existingNode = await _context.KgNodes.FirstOrDefaultAsync(n => n.Id == id);
        if (existingNode != null)
        {
            // Update node
            existingNode.Label = label;
            existingNode.Name = name;
            existingNode.Description = description;
            existingNode.PropertiesJson = propertiesJson;
            existingNode.SourceUrl = provenance.SourceUrl;
            existingNode.SourceName = provenance.SourceName;
            existingNode.SourceType = provenance.SourceType;
            existingNode.PublicationDate = provenance.PublicationDate;
            existingNode.ExtractionTimestamp = DateTime.UtcNow;
            existingNode.Confidence = provenance.Confidence;
            existingNode.LicenseMetadata = provenance.LicenseMetadata;
            
            _context.KgNodes.Update(existingNode);
        }
        else
        {
            // Insert node
            var node = new KgNode
            {
                Id = id,
                Label = label,
                Name = name,
                Description = description,
                PropertiesJson = propertiesJson,
                SourceUrl = provenance.SourceUrl,
                SourceName = provenance.SourceName,
                SourceType = provenance.SourceType,
                PublicationDate = provenance.PublicationDate,
                ExtractionTimestamp = DateTime.UtcNow,
                Confidence = provenance.Confidence,
                LicenseMetadata = provenance.LicenseMetadata
            };
            _context.KgNodes.Add(node);
        }

        await _context.SaveChangesAsync();
        return id;
    }

    public async Task<bool> LinkNodesAsync(string sourceNodeId, string targetNodeId, string relationshipType, string propertiesJson, ProvenanceMetadata provenance)
    {
        if (string.IsNullOrWhiteSpace(provenance.SourceName))
            throw new ArgumentException("Provenance SourceName is strictly required.");

        var edgeId = $"{sourceNodeId}_{relationshipType}_{targetNodeId}";
        var existingEdge = await _context.KgEdges.FirstOrDefaultAsync(e => e.Id == edgeId);

        if (existingEdge != null)
        {
            existingEdge.PropertiesJson = propertiesJson;
            existingEdge.ExtractionTimestamp = DateTime.UtcNow;
            existingEdge.SourceUrl = provenance.SourceUrl;
            existingEdge.SourceName = provenance.SourceName;
            existingEdge.SourceType = provenance.SourceType;
            existingEdge.PublicationDate = provenance.PublicationDate;
            existingEdge.Confidence = provenance.Confidence;
            existingEdge.LicenseMetadata = provenance.LicenseMetadata;
            
            _context.KgEdges.Update(existingEdge);
        }
        else
        {
            var edge = new KgEdge
            {
                Id = edgeId,
                SourceNodeId = sourceNodeId,
                TargetNodeId = targetNodeId,
                RelationshipType = relationshipType,
                PropertiesJson = propertiesJson,
                SourceUrl = provenance.SourceUrl,
                SourceName = provenance.SourceName,
                SourceType = provenance.SourceType,
                PublicationDate = provenance.PublicationDate,
                ExtractionTimestamp = DateTime.UtcNow,
                Confidence = provenance.Confidence,
                LicenseMetadata = provenance.LicenseMetadata
            };
            _context.KgEdges.Add(edge);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetGraphStatsAsync()
    {
        var nodeCount = await _context.KgNodes.CountAsync();
        var edgeCount = await _context.KgEdges.CountAsync();
        
        var nodesByLabel = await _context.KgNodes
            .GroupBy(n => n.Label)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .ToListAsync();

        var edgesByType = await _context.KgEdges
            .GroupBy(e => e.RelationshipType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return new
        {
            TotalNodes = nodeCount,
            TotalEdges = edgeCount,
            NodesByLabel = nodesByLabel,
            EdgesByType = edgesByType
        };
    }
}
