using System.IO;
using System.Threading.Tasks;

namespace SkillScape.Application.Interfaces;

public interface IResumeTextExtractor
{
    Task<string> ExtractTextAsync(Stream stream, string fileName);
}
