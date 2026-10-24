using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Loads and queries Career Intelligence data from JSON files.
/// Registered as a singleton to avoid repeated file reads.
/// </summary>
public class CareerIntelligenceService : ICareerIntelligenceService
{
    private readonly ILogger<CareerIntelligenceService> _logger;

    // Cached data loaded once at startup
    private List<RawDomain> _domains = new();
    private List<RawSubDomain> _subDomains = new();
    private List<RawCareer> _careers = new();
    private List<RawSkill> _skills = new();
    private List<RawCareerSkillMapping> _mappings = new();
    private RawTaxonomy? _taxonomy;
    private bool _loaded;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public CareerIntelligenceService(ILogger<CareerIntelligenceService> logger)
    {
        _logger = logger;
    }

    // ─── Ensure data is loaded ──────────────────────────────────────────────

    private void EnsureLoaded()
    {
        if (_loaded) return;
        lock (_lock)
        {
            if (_loaded) return;
            try
            {
                var baseDir = FindCareerIntelligenceDir();
                _domains = LoadJson<List<RawDomain>>(Path.Combine(baseDir, "domains.json")) ?? new();
                _subDomains = LoadJson<List<RawSubDomain>>(Path.Combine(baseDir, "subdomains.json")) ?? new();
                _careers = LoadJson<List<RawCareer>>(Path.Combine(baseDir, "careers.json")) ?? new();
                _skills = LoadJson<List<RawSkill>>(Path.Combine(baseDir, "skills.json")) ?? new();
                _mappings = LoadJson<List<RawCareerSkillMapping>>(Path.Combine(baseDir, "career-skill-mapping.json")) ?? new();
                _taxonomy = LoadJson<RawTaxonomy>(Path.Combine(baseDir, "career-taxonomy.json"));
                MergeTaxonomyExtension();
                foreach (var extension in LoadJson<List<RawTaxonomy>>(Path.Combine(baseDir, "career-taxonomy-expansion.json")) ?? new())
                    MergeTaxonomyExtension(extension);
                foreach (var extension in LoadJson<List<RawTaxonomy>>(Path.Combine(baseDir, "career-taxonomy-specialization-roles.json")) ?? new())
                    MergeTaxonomyExtension(extension);
                _loaded = true;
                _logger.LogInformation("CareerIntelligence: loaded {D} domains, {SD} sub-domains, {C} careers, {S} skills, {M} mappings.",
                    _domains.Count, _subDomains.Count, _careers.Count, _skills.Count, _mappings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Career Intelligence JSON files.");
                _domains = new(); _subDomains = new(); _careers = new(); _skills = new(); _mappings = new();
                _loaded = true; // prevent retry loop
            }
        }
    }

    // The extension is deliberately merged into the established flat catalog. This keeps
    // legacy Career Switch identifiers stable while allowing the analyzer to use a richer
    // JSON hierarchy at runtime.
    private void MergeTaxonomyExtension()
    {
        if (_taxonomy != null) MergeTaxonomyExtension(_taxonomy);
    }

    private void MergeTaxonomyExtension(RawTaxonomy taxonomy)
    {
        if (taxonomy.Domains == null) return;

        foreach (var domain in taxonomy.Domains)
        {
            if (!_domains.Any(d => d.Id.Equals(domain.Id, StringComparison.OrdinalIgnoreCase)))
                _domains.Add(new RawDomain { Id = domain.Id, Name = domain.Name, AlternateNames = domain.AlternateNames });

            foreach (var specialization in domain.Specializations ?? new())
            {
                if (!_subDomains.Any(s => s.Id.Equals(specialization.Id, StringComparison.OrdinalIgnoreCase)))
                    _subDomains.Add(new RawSubDomain
                    {
                        Id = specialization.Id, DomainId = domain.Id, Name = specialization.Name,
                        Description = specialization.Description, Order = specialization.Order
                    });

                foreach (var role in specialization.TargetRoles ?? new())
                {
                    if (_careers.Any(c => c.Id.Equals(role.Id, StringComparison.OrdinalIgnoreCase))) continue;
                    _careers.Add(new RawCareer
                    {
                        Id = role.Id, Name = role.Name, AlternateNames = role.AlternateNames,
                        DomainId = domain.Id, Domain = domain.Id,
                        SubDomainId = specialization.Id, SubDomain = specialization.Id,
                        Description = role.Description,
                        RequiredEducation = role.RequiredEducation ?? new List<string> { $"Relevant {domain.Name} qualification or demonstrated equivalent capability" },
                        ExperienceRequirements = new RawExperienceReq { EntryLevel = 0, MidLevel = 2, Senior = 5 },
                        Certifications = role.Certifications ?? new(),
                        CareerLevels = role.CareerLevels ?? new List<string> { "Entry", "Mid", "Senior" },
                        InterestAreas = role.InterestAreas ?? new List<string> { specialization.Name },
                        Source = role.Source ?? taxonomy.Source ?? "ESCO v1.2.1 / O*NET crosswalk-informed normalization",
                        LastUpdated = taxonomy.LastUpdated
                    });
                }
            }
        }
    }

    private string FindCareerIntelligenceDir()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "CareerIntelligence"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "SkillScape.Infrastructure", "CareerIntelligence"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SkillScape.Infrastructure", "CareerIntelligence"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "SkillScape.Infrastructure", "CareerIntelligence"),
        };

        foreach (var candidate in candidates)
        {
            var full = Path.GetFullPath(candidate);
            if (Directory.Exists(full))
            {
                _logger.LogInformation("CareerIntelligence directory found at: {Path}", full);
                return full;
            }
        }

        _logger.LogWarning("CareerIntelligence directory not found. Searched: {Paths}", string.Join(", ", candidates.Select(Path.GetFullPath)));
        return candidates[0];
    }

    private static T? LoadJson<T>(string path)
    {
        if (!File.Exists(path)) return default;
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, JsonOpts);
    }

    // ─── Interface implementation ───────────────────────────────────────────

    public Task<List<DomainDto>> GetDomainsAsync()
    {
        EnsureLoaded();
        var result = _domains.Select(d => new DomainDto
        {
            Id = d.Id,
            Name = d.Name,
            AlternateNames = d.AlternateNames ?? new()
        }).ToList();
        return Task.FromResult(result);
    }

    public Task<List<SubDomainDto>> GetSubDomainsAsync(string? domainId = null)
    {
        EnsureLoaded();
        var query = _subDomains.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(domainId))
        {
            query = query.Where(sd => sd.DomainId.Equals(domainId, StringComparison.OrdinalIgnoreCase));
        }
        var result = query
            .OrderBy(sd => sd.Order)
            .Select(sd => new SubDomainDto
            {
                Id = sd.Id,
                DomainId = sd.DomainId,
                Name = sd.Name,
                Description = sd.Description,
                Order = sd.Order
            }).ToList();
        return Task.FromResult(result);
    }

    public Task<List<CareerSummaryDto>> GetCareersBySubDomainAsync(string subDomainId)
    {
        EnsureLoaded();
        var result = _careers
            .Where(c => MatchesSubDomain(c, subDomainId))
            .Select(ToSummaryDto)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<List<CareerSummaryDto>> GetCareersByDomainAsync(string domainId)
    {
        EnsureLoaded();
        var result = _careers
            .Where(c => c.Domain.Equals(domainId, StringComparison.OrdinalIgnoreCase) ||
                        c.DomainId.Equals(domainId, StringComparison.OrdinalIgnoreCase))
            .Select(ToSummaryDto)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<List<CareerSummaryDto>> GetAllCareersAsync()
    {
        EnsureLoaded();
        return Task.FromResult(_careers.Select(ToSummaryDto).ToList());
    }

    public Task<List<CareerSummaryDto>> SearchCareersAsync(string query, string? domainId = null, string? subDomainId = null)
    {
        EnsureLoaded();
        if (string.IsNullOrWhiteSpace(query))
        {
            if (!string.IsNullOrWhiteSpace(subDomainId))
                return GetCareersBySubDomainAsync(subDomainId);
            if (!string.IsNullOrWhiteSpace(domainId))
                return GetCareersByDomainAsync(domainId);
            return GetAllCareersAsync();
        }

        var q = query.Trim().ToLower();
        var matches = _careers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(domainId))
            matches = matches.Where(c => c.Domain.Equals(domainId, StringComparison.OrdinalIgnoreCase) || c.DomainId.Equals(domainId, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(subDomainId))
            matches = matches.Where(c => MatchesSubDomain(c, subDomainId));

        var filtered = matches.Where(c =>
            c.Name.ToLower().Contains(q) ||
            c.Id.ToLower().Contains(q) ||
            (c.AlternateNames != null && c.AlternateNames.Any(a => a.ToLower().Contains(q))) ||
            (c.InterestAreas != null && c.InterestAreas.Any(i => i.ToLower().Contains(q))) ||
            q.Contains(c.Name.ToLower())
        )
        .Select(ToSummaryDto)
        .ToList();

        return Task.FromResult(filtered);
    }

    public Task<CareerDetailDto?> GetCareerByIdAsync(string careerId)
    {
        EnsureLoaded();
        var career = _careers.FirstOrDefault(c =>
            c.Id.Equals(careerId, StringComparison.OrdinalIgnoreCase));

        if (career == null) return Task.FromResult<CareerDetailDto?>(null);
        return Task.FromResult<CareerDetailDto?>(ToDetailDto(career));
    }

    public Task<bool> IsCareerInHierarchyAsync(string careerId, string? domainId, string? subDomainId)
    {
        EnsureLoaded();
        var career = _careers.FirstOrDefault(c => c.Id.Equals(careerId, StringComparison.OrdinalIgnoreCase));
        if (career == null) return Task.FromResult(false);
        var domainMatches = string.IsNullOrWhiteSpace(domainId) ||
            career.DomainId.Equals(domainId, StringComparison.OrdinalIgnoreCase) || career.Domain.Equals(domainId, StringComparison.OrdinalIgnoreCase);
        var specializationMatches = string.IsNullOrWhiteSpace(subDomainId) ||
            career.SubDomainId.Equals(subDomainId, StringComparison.OrdinalIgnoreCase) || career.SubDomain.Equals(subDomainId, StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(domainMatches && specializationMatches);
    }

    public Task<CareerDetailDto?> FindCareerByNameAsync(string name)
    {
        EnsureLoaded();
        if (string.IsNullOrWhiteSpace(name)) return Task.FromResult<CareerDetailDto?>(null);

        var norm = name.Trim();

        // Exact name match
        var career = _careers.FirstOrDefault(c =>
            c.Name.Equals(norm, StringComparison.OrdinalIgnoreCase));

        // Alternate name match
        career ??= _careers.FirstOrDefault(c =>
            c.AlternateNames?.Any(a => a.Equals(norm, StringComparison.OrdinalIgnoreCase)) == true);

        // Partial match (contains)
        career ??= _careers.FirstOrDefault(c =>
            c.Name.Contains(norm, StringComparison.OrdinalIgnoreCase) ||
            norm.Contains(c.Name, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(career == null ? null : (CareerDetailDto?)ToDetailDto(career));
    }

    public Task<List<string>> GetInterestAreasForCareerAsync(string careerId)
    {
        EnsureLoaded();
        var career = _careers.FirstOrDefault(c =>
            c.Id.Equals(careerId, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(career?.InterestAreas ?? new List<string>());
    }

    public Task<SkillDefinitionDto?> GetSkillByIdAsync(string skillId)
    {
        EnsureLoaded();
        var skill = _skills.FirstOrDefault(s => s.Id.Equals(skillId, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(skill == null ? null : (SkillDefinitionDto?)ToSkillDto(skill));
    }

    public Task<SkillDefinitionDto?> FindSkillByNameAsync(string skillName)
    {
        EnsureLoaded();
        if (string.IsNullOrWhiteSpace(skillName)) return Task.FromResult<SkillDefinitionDto?>(null);
        var norm = skillName.Trim();

        var skill = _skills.FirstOrDefault(s => s.Name.Equals(norm, StringComparison.OrdinalIgnoreCase))
            ?? _skills.FirstOrDefault(s => s.AlternateNames?.Any(a => a.Equals(norm, StringComparison.OrdinalIgnoreCase)) == true)
            ?? _skills.FirstOrDefault(s => s.Name.Contains(norm, StringComparison.OrdinalIgnoreCase) || norm.Contains(s.Name, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(skill == null ? null : (SkillDefinitionDto?)ToSkillDto(skill));
    }

    // ─── Internal: get skill requirements for a career ──────────────────────

    internal List<CareerSkillRequirementDto> GetSkillRequirements(string careerId)
    {
        EnsureLoaded();
        var mapping = _mappings.FirstOrDefault(m =>
            m.CareerId.Equals(careerId, StringComparison.OrdinalIgnoreCase));

        // Every role remains analyzable even when it has no bespoke mapping yet.
        // The domain baseline deliberately includes a full, eight-skill profile so
        // expanded taxonomy roles never degrade to a misleading four-skill comparison.
        if (mapping == null)
        {
            var career = _careers.FirstOrDefault(c => c.Id.Equals(careerId, StringComparison.OrdinalIgnoreCase));
            var roleSpecificRequirements = GetRoleSpecificRequirements(career);
            if (roleSpecificRequirements.Count > 0)
                return roleSpecificRequirements;

            return GetBaselineRequirements(career?.DomainId ?? career?.Domain ?? string.Empty);
        }

        return mapping.Skills.Select(s =>
        {
            var skill = _skills.FirstOrDefault(sk => sk.Id.Equals(s.SkillId, StringComparison.OrdinalIgnoreCase));
            return new CareerSkillRequirementDto
            {
                SkillId = s.SkillId,
                SkillName = skill?.Name ?? s.SkillId,
                RequiredLevel = s.RequiredLevel,
                Importance = s.Importance
            };
        }).ToList();
    }

    // The expanded taxonomy has many precise job titles while the curated mapping file
    // only contains mappings for its original careers. Do not collapse those titles
    // into one domain-wide checklist: use a specialization / role-family profile first.
    // Every taxonomy role gets an individual contextual profile as the final fallback;
    // the old domain baseline is reserved only for legacy records with no specialization.
    private static List<CareerSkillRequirementDto> GetRoleSpecificRequirements(RawCareer? career)
    {
        if (career == null) return new();

        var role = $"{career.Name} {career.Id} {career.SubDomain} {career.SubDomainId}".ToLowerInvariant();
        (string Id, string Name)[]? profile = role switch
        {
            // ── Design: each specialization has a distinct professional toolkit ──
            var r when r.Contains("des_uiux") || r.Contains("ui/ux") || r.Contains("ux designer") => new[]
            {
                ("skill_user_research", "User Research and Interview Synthesis"),
                ("skill_information_architecture", "Information Architecture and User Flows"),
                ("skill_interaction_design", "Interaction Design and Prototyping"),
                ("skill_figma", "Figma Design Systems and Handoff"),
                ("skill_usability_testing", "Usability Testing"),
                ("skill_accessibility", "Accessibility and Inclusive Design"),
                ("skill_ux_writing", "UX Writing and Content States"),
                ("skill_product_metrics", "Product Metrics and Iteration")
            },
            var r when r.Contains("des_product") || r.Contains("product designer") => new[]
            {
                ("skill_product_discovery", "Product Discovery and Problem Framing"),
                ("skill_user_research", "User Research and Journey Mapping"),
                ("skill_interaction_design", "Interaction Design and Prototyping"),
                ("skill_figma", "Figma Components and Design Systems"),
                ("skill_design_critique", "Design Critique and Decision Rationale"),
                ("skill_usability_testing", "Usability Testing"),
                ("skill_accessibility", "Accessible Product Design"),
                ("skill_product_metrics", "Post-launch Product Metrics")
            },
            var r when r.Contains("des_visual") || r.Contains("graphic designer") || r.Contains("brand designer") => new[]
            {
                ("skill_visual_design", "Visual Composition and Layout"),
                ("skill_brand_strategy", "Brand Strategy and Visual Identity"),
                ("skill_typography", "Typography and Hierarchy"),
                ("skill_color_theory", "Colour Systems and Art Direction"),
                ("skill_adobe_creative", "Adobe Creative Suite Production"),
                ("skill_brand_guidelines", "Brand Guidelines and Asset Systems"),
                ("skill_print_digital", "Print and Digital Production Specifications"),
                ("skill_design_presentation", "Design Presentation and Rationale")
            },
            var r when r.Contains("des_motion") || r.Contains("motion graphics") || r.Contains("3d artist") => new[]
            {
                ("skill_storyboarding", "Storyboarding and Visual Narrative"),
                ("skill_motion_design", "Motion Design Principles and Timing"),
                ("skill_3d_modeling", "3D Modelling and Scene Composition"),
                ("skill_animation_software", "After Effects / 3D Animation Tools"),
                ("skill_video_editing", "Video Editing and Post-production"),
                ("skill_sound_sync", "Sound Synchronisation and Audio Basics"),
                ("skill_rendering", "Rendering and Delivery Optimisation"),
                ("skill_asset_management", "Motion Asset Management")
            },
            var r when r.Contains("des_fashion") || r.Contains("fashion designer") || r.Contains("textile designer") => new[]
            {
                ("skill_fashion_illustration", "Fashion Illustration and Technical Flats"),
                ("skill_textile_materials", "Textile Materials and Fabric Selection"),
                ("skill_pattern_making", "Pattern Making and Garment Construction"),
                ("skill_trend_research", "Trend Research and Collection Development"),
                ("skill_cad", "Fashion CAD and Technical Specification"),
                ("skill_fit_testing", "Fit, Sampling and Quality Review"),
                ("skill_sourcing", "Material Sourcing and Production Coordination"),
                ("skill_visual_merchandising", "Collection Presentation and Merchandising")
            },
            var r when r.Contains("des_service") || r.Contains("service designer") || r.Contains("customer experience") => new[]
            {
                ("skill_service_research", "Service Research and Stakeholder Mapping"),
                ("skill_journey_mapping", "Customer Journey Mapping"),
                ("skill_service_blueprinting", "Service Blueprinting"),
                ("skill_facilitation", "Co-design Workshop Facilitation"),
                ("skill_prototyping", "Service Prototyping"),
                ("skill_process_design", "Operational Process Design"),
                ("skill_measurement", "Experience Measurement and Feedback"),
                ("skill_change_management", "Service Change Implementation")
            },

            // ── Software, data and infrastructure specializations ───────────
            var r when r.Contains("backend") || r.Contains("platform engineer") || r.Contains("systems engineer") => new[]
            {
                ("skill_backend_development", "Backend Application Development"),
                ("skill_api_design", "API Design and Integration"),
                ("skill_database_design", "Database Design and Query Optimisation"),
                ("skill_distributed_systems", "Distributed Systems and Reliability"),
                ("skill_testing", "Automated Testing"),
                ("skill_cloud", "Cloud Deployment"),
                ("skill_security", "Application Security"),
                ("skill_observability", "Logging, Monitoring and Debugging")
            },
            var r when r.Contains("frontend") || r.Contains("web developer") || r.Contains("full stack") => new[]
            {
                ("skill_javascript", "Modern JavaScript / TypeScript"),
                ("skill_frontend_frameworks", "Frontend Framework Development"),
                ("skill_html_css", "Responsive HTML, CSS and Accessibility"),
                ("skill_api_integration", "API Integration and State Management"),
                ("skill_testing", "Frontend Testing"),
                ("skill_performance", "Web Performance Optimisation"),
                ("skill_ui_implementation", "Design System Implementation"),
                ("skill_git", "Version Control and Code Review")
            },
            var r when r.Contains("data engineer") || r.Contains("data_engineering") || r.Contains("analytics engineering") => new[]
            {
                ("skill_sql", "Advanced SQL and Data Modelling"),
                ("skill_python", "Python Data Pipelines"),
                ("skill_data_modeling", "Warehouse and Data Model Design"),
                ("skill_etl", "ETL / ELT Pipeline Development"),
                ("skill_data_quality", "Data Quality and Validation"),
                ("skill_orchestration", "Workflow Orchestration"),
                ("skill_cloud", "Cloud Data Platforms"),
                ("skill_data_governance", "Data Governance and Documentation")
            },
            var r when r.Contains("data analyst") || r.Contains("business intelligence") || r.Contains("dashboard") => new[]
            {
                ("skill_sql", "SQL for Analysis"),
                ("skill_data_analysis", "Data Analysis and Insight Generation"),
                ("skill_data_visualization", "Dashboard and Data Visualisation Design"),
                ("skill_statistics", "Applied Statistics"),
                ("skill_excel", "Excel Analysis and Modelling"),
                ("skill_power_bi", "Business Intelligence Tools"),
                ("skill_data_storytelling", "Data Storytelling for Decisions"),
                ("skill_data_quality", "Data Validation")
            },
            var r when r.Contains("devops") || r.Contains("site reliability") || r.Contains("release engineer") => new[]
            {
                ("skill_linux", "Linux and Systems Administration"),
                ("skill_ci_cd", "CI/CD Pipeline Engineering"),
                ("skill_containers", "Containers and Orchestration"),
                ("skill_cloud", "Cloud Infrastructure"),
                ("skill_infrastructure_as_code", "Infrastructure as Code"),
                ("skill_observability", "Observability and Incident Response"),
                ("skill_security", "DevSecOps Practices"),
                ("skill_scripting", "Automation Scripting")
            },
            var r when r.Contains("computer_vision") || r.Contains("computer vision") => new[]
            {
                ("skill_python", "Python for Computer Vision"),
                ("skill_ml", "Machine Learning and Deep Learning"),
                ("skill_computer_vision", "Image Processing and Computer Vision"),
                ("skill_model_evaluation", "Model Evaluation and Error Analysis"),
                ("skill_data_annotation", "Image Data Preparation and Annotation"),
                ("skill_deployment", "Model Deployment and Inference"),
                ("skill_cloud", "Cloud Compute for ML Workloads"),
                ("skill_problem_solving", "Experiment Design and Debugging")
            },
            var r when r.Contains("nlp") || r.Contains("natural language") => new[]
            {
                ("skill_python", "Python for NLP"),
                ("skill_ml", "Machine Learning and Deep Learning"),
                ("skill_nlp", "Natural Language Processing"),
                ("skill_data_preprocessing", "Text Data Preparation and Evaluation"),
                ("skill_model_evaluation", "Model Evaluation and Error Analysis"),
                ("skill_deployment", "Model Deployment and Inference"),
                ("skill_cloud", "Cloud Compute for ML Workloads"),
                ("skill_problem_solving", "Experiment Design and Debugging")
            },
            var r when r.Contains("machine learning") || r.Contains("machine_learning") => new[]
            {
                ("skill_python", "Python for Machine Learning"),
                ("skill_ml", "Machine Learning Algorithms"),
                ("skill_statistics", "Statistics and Experiment Design"),
                ("skill_pandas", "Data Preparation with Pandas"),
                ("skill_model_evaluation", "Model Evaluation and Error Analysis"),
                ("skill_feature_engineering", "Feature Engineering"),
                ("skill_deployment", "Model Deployment and MLOps"),
                ("skill_sql", "SQL for Training Data"),
            },
            var r when r.Contains("ai engineer") || r.Contains("ai_engineer") => new[]
            {
                ("skill_python", "Python for AI Applications"),
                ("skill_ml", "Machine Learning and Deep Learning"),
                ("skill_ai_system_design", "AI System Design"),
                ("skill_data_preprocessing", "Data Preparation and Evaluation"),
                ("skill_api_integration", "Model and API Integration"),
                ("skill_deployment", "Production Deployment and Monitoring"),
                ("skill_cloud", "Cloud AI Services"),
                ("skill_ethics", "Responsible AI and Safety")
            },
            var r when r.Contains("geotech") || r.Contains("soil") || r.Contains("foundation") => new[]
            {
                ("skill_soil_mechanics", "Soil Mechanics & Ground Behaviour"),
                ("skill_site_investigation", "Geotechnical Site Investigation"),
                ("skill_foundation_design", "Foundation Design"),
                ("skill_slope_stability", "Slope Stability & Retaining Structures"),
                ("skill_geotechnical_testing", "Field and Laboratory Soil Testing"),
                ("skill_geotechnical_reporting", "Geotechnical Reporting"),
                ("skill_cad", "Geotechnical Drawings and Sections"),
                ("skill_site_safety", "Groundworks Safety and Method Statements")
            },
            var r when r.Contains("structural") => new[]
            {
                ("skill_structural_analysis", "Structural Analysis & Load Paths"),
                ("skill_rc_design", "Reinforced Concrete Design"),
                ("skill_steel_design", "Steel Structure Design"),
                ("skill_structural_software", "Structural Analysis Software (ETABS/STAAD)"),
                ("skill_detailing", "Structural Detailing and Drawings"),
                ("skill_building_codes", "Design Codes and Load Combinations"),
                ("skill_cad", "CAD"),
                ("skill_quality_control", "Construction Quality Control")
            },
            var r when r.Contains("civil") || r.Contains("construction") || r.Contains("site engineer") => new[]
            {
                ("skill_construction_drawings", "Construction Drawings and Specifications"),
                ("skill_setting_out", "Site Setting-out and Survey Coordination"),
                ("skill_concrete_quality", "Concrete and Reinforcement Quality Control"),
                ("skill_boq", "BOQ, Quantity Take-off and Estimation"),
                ("skill_method_statements", "Method Statements and Site Safety"),
                ("skill_progress_tracking", "Site Progress Tracking"),
                ("skill_cad", "CAD"),
                ("skill_project_management", "Project Coordination")
            },
            var r when r.Contains("mechanical") || r.Contains("hvac") || r.Contains("manufacturing") => new[]
            {
                ("skill_mechanical_design", "Mechanical Design Principles"),
                ("skill_thermodynamics", "Thermodynamics and Heat Transfer"),
                ("skill_cad", "CAD and Engineering Drawings"),
                ("skill_materials", "Materials and Manufacturing Processes"),
                ("skill_machine_elements", "Machine Elements and Tolerancing"),
                ("skill_maintenance", "Equipment Maintenance and Reliability"),
                ("skill_quality_control", "Quality Control"),
                ("skill_problem_solving", "Engineering Problem Solving")
            },
            var r when r.Contains("audit") => new[]
            {
                ("skill_auditing", "Auditing"),
                ("skill_accounting_standards", "Accounting Standards and Financial Reporting"),
                ("skill_internal_controls", "Internal Controls and Risk Assessment"),
                ("skill_excel", "Excel for Audit Analysis"),
                ("skill_working_papers", "Audit Working Papers and Documentation"),
                ("skill_ethics", "Professional Ethics and Independence"),
                ("skill_communication", "Client and Stakeholder Communication"),
                ("skill_data_analysis", "Data Analysis")
            },
            var r when r.Contains("tax") => new[]
            {
                ("skill_taxation", "Direct and Indirect Taxation"),
                ("skill_tax_compliance", "Tax Compliance and Return Preparation"),
                ("skill_tax_research", "Tax Research and Interpretation"),
                ("skill_accounting", "Accounting"),
                ("skill_excel", "Excel for Tax Working Papers"),
                ("skill_client_advisory", "Client Advisory and Documentation"),
                ("skill_ethics", "Professional Ethics"),
                ("skill_financial_analysis", "Financial Analysis")
            },
            var r when r.Contains("account") || r.Contains("bookkeep") || r.Contains("finance") => new[]
            {
                ("skill_accounting", "Accounting and Ledger Management"),
                ("skill_financial_reporting", "Financial Statement Preparation"),
                ("skill_reconciliation", "Bank and Account Reconciliation"),
                ("skill_excel", "Excel for Financial Analysis"),
                ("skill_financial_analysis", "Financial Analysis"),
                ("skill_taxation", "Taxation and Compliance"),
                ("skill_accounting_software", "Accounting Software"),
                ("skill_ethics", "Professional Ethics")
            },
            _ => null
        };

        if (profile == null)
        {
            // A specialization record is available for every expanded taxonomy role.
            // Give it a role-specific checklist rather than letting unrelated roles
            // inherit the exact same domain baseline.
            return string.IsNullOrWhiteSpace(career.SubDomainId)
                ? new()
                : CreateContextualRoleRequirements(career);
        }

        return profile.Select((skill, index) => new CareerSkillRequirementDto
        {
            SkillId = skill.Id,
            SkillName = skill.Name,
            RequiredLevel = index < 4 ? 80 : 65,
            Importance = index < 4 ? "Core" : "Supporting"
        }).ToList();
    }

    private static List<CareerSkillRequirementDto> CreateContextualRoleRequirements(RawCareer career)
    {
        var specialization = HumanizeIdentifier(career.SubDomainId);
        var role = career.Name;
        var requirements = new[]
        {
            ($"skill_{career.SubDomainId}_foundations", $"{specialization} Foundations"),
            ($"skill_{career.Id}_workflow", $"{role} Workflow and Deliverables"),
            ($"skill_{career.SubDomainId}_methods", $"{specialization} Methods and Tools"),
            ($"skill_{career.Id}_standards", $"{role} Standards, Quality and Compliance"),
            ($"skill_{career.SubDomainId}_analysis", $"{specialization} Analysis and Decision Making"),
            ($"skill_{career.Id}_documentation", $"{role} Documentation and Reporting"),
            ($"skill_{career.SubDomainId}_stakeholders", $"{specialization} Stakeholder Collaboration"),
            ($"skill_{career.Id}_practice", $"{role} Applied Practice")
        };

        return requirements.Select((item, index) => new CareerSkillRequirementDto
        {
            SkillId = item.Item1,
            SkillName = item.Item2,
            RequiredLevel = index < 4 ? 80 : 65,
            Importance = index < 4 ? "Core" : "Supporting"
        }).ToList();
    }

    private static string HumanizeIdentifier(string value) =>
        string.Join(" ", value.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Where(part => !part.Equals("des", StringComparison.OrdinalIgnoreCase)
                && !part.Equals("eng", StringComparison.OrdinalIgnoreCase)
                && !part.Equals("cs", StringComparison.OrdinalIgnoreCase)
                && !part.Equals("com", StringComparison.OrdinalIgnoreCase)
                && !part.Equals("fin", StringComparison.OrdinalIgnoreCase)
                && !part.Equals("acc", StringComparison.OrdinalIgnoreCase))
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    private List<CareerSkillRequirementDto> GetBaselineRequirements(string domainId)
    {
        var profiles = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["cs"] = new[] { "skill_problem_solving", "skill_oop", "skill_python", "skill_sql", "skill_communication", "skill_research", "skill_cloud", "skill_project_management" },
            ["it"] = new[] { "skill_problem_solving", "skill_cloud", "skill_communication", "skill_project_management", "skill_cybersecurity_fundamentals", "skill_research", "skill_sql", "skill_decision_making" },
            ["engineering"] = new[] { "skill_engineering_math", "skill_problem_solving", "skill_cad", "skill_project_management", "skill_communication", "skill_data_analysis", "skill_critical_thinking", "skill_ethics" },
            ["commerce"] = new[] { "skill_accounting", "skill_excel", "skill_communication", "skill_financial_analysis", "skill_taxation", "skill_data_analysis", "skill_ethics", "skill_problem_solving" },
            ["finance"] = new[] { "skill_financial_analysis", "skill_excel", "skill_statistics", "skill_communication", "skill_financial_modeling", "skill_data_analysis", "skill_risk_modeling", "skill_ethics" },
            ["accounting"] = new[] { "skill_accounting", "skill_excel", "skill_taxation", "skill_ethics", "skill_auditing", "skill_financial_analysis", "skill_communication", "skill_critical_thinking" },
            ["business"] = new[] { "skill_business_analysis", "skill_communication", "skill_problem_solving", "skill_data_analysis", "skill_strategy", "skill_project_management", "skill_leadership", "skill_critical_thinking" },
            ["management"] = new[] { "skill_project_management", "skill_leadership", "skill_communication", "skill_strategy", "skill_operations", "skill_decision_making", "skill_data_analysis", "skill_problem_solving" },
            ["data_science"] = new[] { "skill_statistics", "skill_data_analysis", "skill_python", "skill_data_visualization", "skill_sql", "skill_pandas", "skill_communication", "skill_problem_solving" },
            ["cybersecurity"] = new[] { "skill_cybersecurity_fundamentals", "skill_network_security", "skill_problem_solving", "skill_communication", "skill_python", "skill_cloud", "skill_research", "skill_critical_thinking" },
            ["law"] = new[] { "skill_legal_research", "skill_legal_reasoning", "skill_writing", "skill_ethics", "skill_contract_knowledge", "skill_communication", "skill_critical_thinking", "skill_research" },
            ["healthcare"] = new[] { "skill_clinical_assessment", "skill_patient_safety", "skill_healthcare_knowledge", "skill_communication", "skill_medical_documentation", "skill_ethics", "skill_critical_thinking", "skill_problem_solving" },
            ["design"] = new[] { "skill_visual_design", "skill_creative_thinking", "skill_communication", "skill_research", "skill_ux_design", "skill_figma", "skill_critical_thinking", "skill_project_management" },
        };
        var skills = profiles.TryGetValue(domainId, out var selected)
            ? selected : new[] { "skill_communication", "skill_research", "skill_problem_solving", "skill_critical_thinking", "skill_writing", "skill_data_analysis", "skill_ethics", "skill_project_management" };
        return skills.Select((id, index) =>
        {
            var skill = _skills.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return new CareerSkillRequirementDto { SkillId = id, SkillName = skill?.Name ?? id, RequiredLevel = index < 4 ? 75 : 60, Importance = index < 4 ? "Core" : "Supporting" };
        }).ToList();
    }

    // ─── Mapping helpers ────────────────────────────────────────────────────

    private CareerSummaryDto ToSummaryDto(RawCareer c)
    {
        var domainObj = _domains.FirstOrDefault(d => d.Id.Equals(c.Domain, StringComparison.OrdinalIgnoreCase) || d.Id.Equals(c.DomainId, StringComparison.OrdinalIgnoreCase));
        var subDomainObj = _subDomains.FirstOrDefault(sd => sd.Id.Equals(c.SubDomain, StringComparison.OrdinalIgnoreCase) || sd.Id.Equals(c.SubDomainId, StringComparison.OrdinalIgnoreCase));

        return new CareerSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            DomainId = domainObj?.Id ?? c.Domain,
            DomainName = domainObj?.Name ?? c.Domain,
            SubDomainId = subDomainObj?.Id ?? c.SubDomain,
            SubDomainName = subDomainObj?.Name ?? c.SubDomain,
            Domain = domainObj?.Name ?? c.Domain,
            SubDomain = subDomainObj?.Name ?? c.SubDomain,
            Description = c.Description,
            AlternateNames = c.AlternateNames ?? new(),
            InterestAreas = c.InterestAreas ?? new(),
            CareerLevels = c.CareerLevels ?? new(),
        };
    }

    // Target-role selection is always resolved by the canonical specialization id.
    // Do not use text similarity here: broad words such as "engineering" create
    // false matches (for example Mechanical Engineering under Civil Engineering).
    // Every UI specialization has explicit JSON role records keyed by this id.
    private bool MatchesSubDomain(RawCareer career, string subDomainId)
    {
        return career.SubDomainId.Equals(subDomainId, StringComparison.OrdinalIgnoreCase);
    }

    private CareerDetailDto ToDetailDto(RawCareer c)
    {
        var domainObj = _domains.FirstOrDefault(d => d.Id.Equals(c.Domain, StringComparison.OrdinalIgnoreCase) || d.Id.Equals(c.DomainId, StringComparison.OrdinalIgnoreCase));
        var subDomainObj = _subDomains.FirstOrDefault(sd => sd.Id.Equals(c.SubDomain, StringComparison.OrdinalIgnoreCase) || sd.Id.Equals(c.SubDomainId, StringComparison.OrdinalIgnoreCase));

        return new CareerDetailDto
        {
            Id = c.Id,
            Name = c.Name,
            DomainId = domainObj?.Id ?? c.Domain,
            DomainName = domainObj?.Name ?? c.Domain,
            SubDomainId = subDomainObj?.Id ?? c.SubDomain,
            SubDomainName = subDomainObj?.Name ?? c.SubDomain,
            Domain = domainObj?.Name ?? c.Domain,
            SubDomain = subDomainObj?.Name ?? c.SubDomain,
            Description = c.Description,
            AlternateNames = c.AlternateNames ?? new(),
            InterestAreas = c.InterestAreas ?? new(),
            CareerLevels = c.CareerLevels ?? new(),
            RequiredEducation = c.RequiredEducation ?? new(),
            ExperienceRequirements = c.ExperienceRequirements == null ? new() : new()
            {
                EntryLevel = c.ExperienceRequirements.EntryLevel,
                MidLevel = c.ExperienceRequirements.MidLevel,
                Senior = c.ExperienceRequirements.Senior,
            },
            Certifications = c.Certifications ?? new(),
            SkillRequirements = GetSkillRequirements(c.Id),
            Source = c.Source ?? "ESCO/O*NET",
            LastUpdated = c.LastUpdated ?? "2025-01-01",
        };
    }

    private static SkillDefinitionDto ToSkillDto(RawSkill s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        AlternateNames = s.AlternateNames ?? new(),
        Category = s.Category,
        Description = s.Description,
    };

    // ─── Raw JSON models ────────────────────────────────────────────────────

    private class RawDomain
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("alternateNames")] public List<string>? AlternateNames { get; set; }
    }

    private class RawSubDomain
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("domainId")] public string DomainId { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("order")] public int Order { get; set; }
    }

    private class RawCareer
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("alternateNames")] public List<string>? AlternateNames { get; set; }
        [JsonPropertyName("domain")] public string Domain { get; set; } = string.Empty;
        [JsonPropertyName("domainId")] public string DomainId { get; set; } = string.Empty;
        [JsonPropertyName("subDomain")] public string SubDomain { get; set; } = string.Empty;
        [JsonPropertyName("subDomainId")] public string SubDomainId { get; set; } = string.Empty;
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("requiredEducation")] public List<string>? RequiredEducation { get; set; }
        [JsonPropertyName("experienceRequirements")] public RawExperienceReq? ExperienceRequirements { get; set; }
        [JsonPropertyName("certifications")] public List<string>? Certifications { get; set; }
        [JsonPropertyName("careerLevels")] public List<string>? CareerLevels { get; set; }
        [JsonPropertyName("interestAreas")] public List<string>? InterestAreas { get; set; }
        [JsonPropertyName("source")] public string? Source { get; set; }
        [JsonPropertyName("lastUpdated")] public string? LastUpdated { get; set; }
    }

    private class RawExperienceReq
    {
        [JsonPropertyName("entryLevel")] public int EntryLevel { get; set; }
        [JsonPropertyName("midLevel")] public int MidLevel { get; set; }
        [JsonPropertyName("senior")] public int Senior { get; set; }
    }

    private class RawSkill
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("alternateNames")] public List<string>? AlternateNames { get; set; }
        [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("relatedSkills")] public List<string>? RelatedSkills { get; set; }
    }

    private class RawCareerSkillMapping
    {
        [JsonPropertyName("careerId")] public string CareerId { get; set; } = string.Empty;
        [JsonPropertyName("skills")] public List<RawSkillEntry> Skills { get; set; } = new();
    }

    private class RawTaxonomy
    {
        [JsonPropertyName("version")] public string? Version { get; set; }
        [JsonPropertyName("lastUpdated")] public string? LastUpdated { get; set; }
        [JsonPropertyName("source")] public string? Source { get; set; }
        [JsonPropertyName("domains")] public List<RawTaxonomyDomain>? Domains { get; set; }
    }

    private class RawTaxonomyDomain : RawDomain
    {
        [JsonPropertyName("specializations")] public List<RawTaxonomySpecialization>? Specializations { get; set; }
    }

    private class RawTaxonomySpecialization
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("order")] public int Order { get; set; }
        [JsonPropertyName("targetRoles")] public List<RawTaxonomyRole>? TargetRoles { get; set; }
    }

    private class RawTaxonomyRole
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("alternateNames")] public List<string>? AlternateNames { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("requiredEducation")] public List<string>? RequiredEducation { get; set; }
        [JsonPropertyName("certifications")] public List<string>? Certifications { get; set; }
        [JsonPropertyName("careerLevels")] public List<string>? CareerLevels { get; set; }
        [JsonPropertyName("interestAreas")] public List<string>? InterestAreas { get; set; }
        [JsonPropertyName("source")] public string? Source { get; set; }
    }

    private class RawSkillEntry
    {
        [JsonPropertyName("skillId")] public string SkillId { get; set; } = string.Empty;
        [JsonPropertyName("requiredLevel")] public int RequiredLevel { get; set; }
        [JsonPropertyName("importance")] public string Importance { get; set; } = string.Empty;
    }
}
