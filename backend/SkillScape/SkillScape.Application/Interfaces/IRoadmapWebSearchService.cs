using System.Threading.Tasks;

namespace SkillScape.Application.Interfaces;

public interface IRoadmapWebSearchService
{
    Task<string> SearchCareerKnowledgeAsync(string careerTitle);
}
