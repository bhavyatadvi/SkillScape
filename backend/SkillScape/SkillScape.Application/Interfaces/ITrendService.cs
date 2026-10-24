using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkillScape.Application.Interfaces;

public interface ITrendService
{
    Task<Dictionary<int, int>> GetFiveYearTrendAsync(string domainId);
}
