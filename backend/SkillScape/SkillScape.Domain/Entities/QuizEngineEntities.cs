using System;
using System.Collections.Generic;

namespace SkillScape.Domain.Entities;

public class QuizPhase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty; 
    public string Key { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class CareerTaxonomyNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Level { get; set; } = string.Empty; // Stream, Domain, Degree, Specialization, Role
    public string Name { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty;
}

public class QuizQuestionSignal
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string QuizOptionId { get; set; } = string.Empty;
    public string TargetDomain { get; set; } = string.Empty; 
    public double Weight { get; set; } 
    public QuizOption? QuizOption { get; set; }
}

public class EligibilityRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TargetDomain { get; set; } = string.Empty; 
    public string RequiredSubject { get; set; } = string.Empty; 
    public string RequiredLevel { get; set; } = string.Empty; 
}
