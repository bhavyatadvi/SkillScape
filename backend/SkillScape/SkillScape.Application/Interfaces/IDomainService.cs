using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

/// <summary>
/// Career domain service interface
/// </summary>
public interface IDomainService
{
    Task<List<CareerDomainDto>> GetAllDomainsAsync();
    Task<CareerDomainDetailDto> GetDomainWithSkillsAsync(string domainId);
    Task<RoadmapDto> GetDomainRoadmapAsync(string domainId);
}
