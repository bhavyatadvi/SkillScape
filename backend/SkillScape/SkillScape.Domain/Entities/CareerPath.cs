using System;
using System.Collections.Generic;

namespace SkillScape.Domain.Entities;

/// <summary>
/// Represents a node in the hierarchical career tree (e.g. Science -> PCM -> Engineering -> CS -> AI Engineer)
/// </summary>
public class CareerPath
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public int Level { get; set; } // 1, 2, 3, 4, 5
    
    public string? ParentCareerId { get; set; }
    
    public string StreamType { get; set; } = string.Empty; // After10th, After12th, AfterGraduation, CareerSwitch, SkillEnhancement
    
    public string Tags { get; set; } = string.Empty; // Comma-separated tags (e.g. "tech,pcm,development")
    
    public string SkillsRequired { get; set; } = "[]"; // JSON array of skills (e.g. ["React", "HTML"])
    
    public string FutureScope { get; set; } = string.Empty;
    
    public string SalaryDataJson { get; set; } = "{}"; // JSON map: experience/years -> salary
    
    public double IndustryGrowth { get; set; } // expected CAGR percentage (e.g. 15.0)
    
    public string RoadmapJson { get; set; } = "[]"; // JSON array of roadmap steps
    
    public string Certifications { get; set; } = string.Empty; // Comma-separated
    
    public string Colleges { get; set; } = string.Empty; // Comma-separated
    
    public double DemandIndex { get; set; } // 0 to 100 percentage
    
    public string Icon { get; set; } = "briefcase"; // Lucide icon identifier
    
    public string Color { get; set; } = "blue"; // Theme color code (e.g. "cyan", "indigo")
    
    public string WhatYouWillStudy { get; set; } = string.Empty;
    
    public string RecommendedSubjects { get; set; } = "[]"; // JSON array of subjects
    
    public string TopCompanies { get; set; } = "[]"; // JSON array of companies
    
    public string RequiredDegrees { get; set; } = "[]"; // JSON array of degrees
    
    public string WorkEnvironment { get; set; } = "Office"; // Office, Hybrid, Field, Laboratory, Remote
    
    public double RemoteWorkPossibility { get; set; } // 0 to 100 percentage
    
    public double EntrepreneurshipPotential { get; set; } // 0 to 100 percentage
    
    public double CareerFlexibility { get; set; } // 0 to 100 percentage
    
    public string LearningResources { get; set; } = "[]"; // JSON array of resources
    
    public string YoutubeResources { get; set; } = "[]"; // JSON array of YouTube channels/links
    
    public string RoadmapTimeline { get; set; } = string.Empty; // e.g. "4-6 Years"
    
    public string DifficultyLevel { get; set; } = "Medium"; // Easy, Medium, Hard
    
    public string PersonalityMatch { get; set; } = string.Empty; // MBTI or Holland Codes
    
    public string RelatedCareerIds { get; set; } = string.Empty; // Comma-separated related career path IDs
    
    public double AutomationRisk { get; set; } // 0 to 100 percentage
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── V2 Taxonomy Cross-link Fields ─────────────────────────────────────────
    /// <summary>Short domain code matching CareerTaxonomyV2.Id (e.g. "sci.eng.civil")</summary>
    public string TaxonomyNodeId { get; set; } = string.Empty;

    /// <summary>Human-readable hierarchy path (e.g. "Science / Engineering / Civil Engineering")</summary>
    public string TaxonomyPath { get; set; } = string.Empty;

    /// <summary>JSON array of phase keys where this path is valid (e.g. ["After12th","AfterGraduation"])</summary>
    public string PhaseApplicabilityJson { get; set; } = "[]";

    /// <summary>JSON map of signal keys this path primarily listens to in the Bayesian engine</summary>
    public string CanonicalSignalsJson { get; set; } = "{}";

    // Navigation properties for tree structure
    public CareerPath? ParentCareer { get; set; }
    public ICollection<CareerPath> SubCareers { get; set; } = new List<CareerPath>();
}
