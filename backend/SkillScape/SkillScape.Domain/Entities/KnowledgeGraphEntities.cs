using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillScape.Domain.Entities;

public class KgNode
{
    [Key]
    [MaxLength(255)]
    public string Id { get; set; } = string.Empty; // e.g. "skill_python", "occ_software_engineer"

    [Required]
    [MaxLength(100)]
    public string Label { get; set; } = string.Empty; // "Skill", "Occupation", "EducationStage"

    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string PropertiesJson { get; set; } = "{}";

    // Provenance Metadata
    [MaxLength(2000)]
    public string SourceUrl { get; set; } = string.Empty;

    [MaxLength(255)]
    public string SourceName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string SourceType { get; set; } = string.Empty; // "Tier 1 - Government", "Tier 2 - Taxonomy"

    public DateTime? PublicationDate { get; set; }

    public DateTime ExtractionTimestamp { get; set; }

    public double Confidence { get; set; }

    public string LicenseMetadata { get; set; } = string.Empty;

    // Navigation
    [InverseProperty("SourceNode")]
    public ICollection<KgEdge> OutgoingEdges { get; set; } = new List<KgEdge>();

    [InverseProperty("TargetNode")]
    public ICollection<KgEdge> IncomingEdges { get; set; } = new List<KgEdge>();
}

public class KgEdge
{
    [Key]
    [MaxLength(255)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string SourceNodeId { get; set; } = string.Empty;
    
    [ForeignKey("SourceNodeId")]
    public KgNode SourceNode { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string TargetNodeId { get; set; } = string.Empty;
    
    [ForeignKey("TargetNodeId")]
    public KgNode TargetNode { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string RelationshipType { get; set; } = string.Empty; // "requires", "belongs_to", "relevant_to"

    public string PropertiesJson { get; set; } = "{}";

    // Provenance Metadata for the relationship
    [MaxLength(2000)]
    public string SourceUrl { get; set; } = string.Empty;

    [MaxLength(255)]
    public string SourceName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string SourceType { get; set; } = string.Empty;

    public DateTime? PublicationDate { get; set; }

    public DateTime ExtractionTimestamp { get; set; }

    public double Confidence { get; set; }

    public string LicenseMetadata { get; set; } = string.Empty;
}
