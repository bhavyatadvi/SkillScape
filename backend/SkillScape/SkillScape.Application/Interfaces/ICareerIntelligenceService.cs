using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

/// <summary>
/// Provides access to career intelligence data loaded from JSON files.
/// Implemented as a singleton for efficient JSON loading.
/// </summary>
public interface ICareerIntelligenceService
{
    /// <summary>Returns all available career domains.</summary>
    Task<List<DomainDto>> GetDomainsAsync();

    /// <summary>Returns sub-domains for a given domain, or all sub-domains if domainId is null.</summary>
    Task<List<SubDomainDto>> GetSubDomainsAsync(string? domainId = null);

    /// <summary>Returns careers belonging to a specific sub-domain.</summary>
    Task<List<CareerSummaryDto>> GetCareersBySubDomainAsync(string subDomainId);

    /// <summary>Returns all careers for a given domain id.</summary>
    Task<List<CareerSummaryDto>> GetCareersByDomainAsync(string domainId);

    /// <summary>Returns all careers (summary list).</summary>
    Task<List<CareerSummaryDto>> GetAllCareersAsync();

    /// <summary>Searches careers by title, alternate names, or alias matching.</summary>
    Task<List<CareerSummaryDto>> SearchCareersAsync(string query, string? domainId = null, string? subDomainId = null);

    /// <summary>Returns full career detail including skill requirements.</summary>
    Task<CareerDetailDto?> GetCareerByIdAsync(string careerId);

    /// <summary>Confirms that a canonical role belongs to the supplied field and specialization.</summary>
    Task<bool> IsCareerInHierarchyAsync(string careerId, string? domainId, string? subDomainId);

    /// <summary>Finds a career by exact name, alternate name, or fuzzy match.</summary>
    Task<CareerDetailDto?> FindCareerByNameAsync(string name);

    /// <summary>Returns the interest areas for a given career.</summary>
    Task<List<string>> GetInterestAreasForCareerAsync(string careerId);

    /// <summary>Returns the skill definition for a given skill id.</summary>
    Task<SkillDefinitionDto?> GetSkillByIdAsync(string skillId);

    /// <summary>Finds a skill by name or alternate name (for normalization).</summary>
    Task<SkillDefinitionDto?> FindSkillByNameAsync(string skillName);
}
