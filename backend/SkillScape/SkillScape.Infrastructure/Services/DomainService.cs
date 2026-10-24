using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class DomainService : IDomainService
{
    private readonly ApplicationDbContext _context;

    public DomainService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CareerDomainDto>> GetAllDomainsAsync()
    {
        return await _context.CareerDomains
            .Where(d => d.IsActive)
            .OrderBy(d => d.DisplayOrder)
            .Select(d => new CareerDomainDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                IconUrl = d.IconUrl,
                Color = d.Color,
                DisplayOrder = d.DisplayOrder,
                IsActive = d.IsActive
            })
            .ToListAsync();
    }

    public async Task<CareerDomainDetailDto> GetDomainWithSkillsAsync(string domainId)
    {
        var domain = await _context.CareerDomains
            .Include(d => d.Skills.Where(s => s.IsActive))
            .FirstOrDefaultAsync(d => d.Id == domainId && d.IsActive)
            ?? throw new InvalidOperationException("Domain not found");

        return new CareerDomainDetailDto
        {
            Id = domain.Id,
            Name = domain.Name,
            Description = domain.Description,
            IconUrl = domain.IconUrl,
            Color = domain.Color,
            Skills = domain.Skills
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    DifficultyLevel = s.DifficultyLevel,
                    XPReward = s.XPReward,
                    ResourceUrl = s.ResourceUrl,
                    DisplayOrder = s.DisplayOrder
                })
                .ToList()
        };
    }

    public async Task<RoadmapDto> GetDomainRoadmapAsync(string domainId)
    {
        var domain = await _context.CareerDomains
            .Include(d => d.Skills.Where(s => s.IsActive))
            .Include(d => d.RoadmapSteps.Where(r => r.IsActive))
            .ThenInclude(r => r.Topics)
            .FirstOrDefaultAsync(d => d.Id == domainId && d.IsActive)
            ?? throw new InvalidOperationException("Domain not found");

        var roadmapDto = new RoadmapDto
        {
            DomainId = domain.Id,
            DomainName = domain.Name,
            ProgressPercentage = 0,
            Skills = new List<SkillRoadmapDto>()
        };

        foreach (var skill in domain.Skills.OrderBy(s => s.DisplayOrder))
        {
            var skillDto = new SkillRoadmapDto
            {
                SkillId = skill.Id,
                SkillName = skill.Name,
                SkillDescription = skill.Description,
                IsCompleted = false,
                Modules = new List<RoadmapModuleDto>()
            };

            var skillModules = domain.RoadmapSteps.Where(m => m.SkillId == skill.Id).OrderBy(m => m.StepNumber);

            foreach (var mod in skillModules)
            {
                skillDto.Modules.Add(new RoadmapModuleDto
                {
                    ModuleId = mod.Id,
                    Title = mod.Title,
                    Description = mod.Description,
                    StepNumber = mod.StepNumber,
                    EstimatedHours = mod.EstimatedHours,
                    IsCompleted = false,
                    Topics = mod.Topics?.OrderBy(t => t.DisplayOrder).Select(t => new RoadmapTopicDto
                    {
                        TopicId = t.Id,
                        Title = t.Title,
                        ResourceUrl = t.ResourceUrl
                    }).ToList() ?? new List<RoadmapTopicDto>()
                });
            }

            roadmapDto.Skills.Add(skillDto);
        }

        return roadmapDto;
    }
}
