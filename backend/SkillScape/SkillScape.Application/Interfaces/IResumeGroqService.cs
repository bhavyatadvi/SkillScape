using System.Threading;
using System.Threading.Tasks;

namespace SkillScape.Application.Interfaces;

public interface IResumeGroqService
{
    /// <summary>
    /// Extract structured JSON profile data from resume text using dedicated resume Groq API key and model.
    /// </summary>
    Task<string> GetResumeProfileAsync(string resumeText, CancellationToken cancellationToken = default);
}
