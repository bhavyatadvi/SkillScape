using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkillScape.Application.Interfaces;

public class ProvenanceMetadata
{
    public string SourceUrl { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public System.DateTime? PublicationDate { get; set; }
    public string LicenseMetadata { get; set; } = string.Empty;
    public double Confidence { get; set; } = 1.0;
}

public interface IKnowledgeIngestionService
{
    /// <summary>
    /// Ingests a new node into the Knowledge Graph with mandatory provenance tracking.
    /// </summary>
    Task<string> IngestNodeAsync(string id, string label, string name, string description, string propertiesJson, ProvenanceMetadata provenance);

    /// <summary>
    /// Links two nodes in the Knowledge Graph with mandatory provenance tracking.
    /// </summary>
    Task<bool> LinkNodesAsync(string sourceNodeId, string targetNodeId, string relationshipType, string propertiesJson, ProvenanceMetadata provenance);
    
    /// <summary>
    /// Returns graph statistics.
    /// </summary>
    Task<object> GetGraphStatsAsync();
}
