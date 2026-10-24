using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

public class DynamicRoadmapEngine : IDynamicRoadmapEngine
{
    public Task<DynamicRoadmapDto> GenerateRoadmapAsync(ParsedProfileDto profile, string targetNodeId, string targetRoleTitle)
    {
        string degree = string.IsNullOrWhiteSpace(profile.Education.Degree) ? "Graduate" : profile.Education.Degree;
        string spec = string.IsNullOrWhiteSpace(profile.Education.Specialization) ? "" : profile.Education.Specialization;
        string startingPoint = $"{degree} {(string.IsNullOrEmpty(spec) ? "" : "in " + spec)} ({profile.CurrentStatus.Role})".Trim();

        var roadmap = new DynamicRoadmapDto
        {
            TargetRole = string.IsNullOrWhiteSpace(targetRoleTitle) ? "Target Career Role" : targetRoleTitle,
            StartingPoint = startingPoint,
            Steps = BuildSteps(profile, targetNodeId, targetRoleTitle)
        };

        return Task.FromResult(roadmap);
    }

    private static List<RoadmapStepDto> BuildSteps(ParsedProfileDto profile, string targetNodeId, string targetRoleTitle)
    {
        var steps = new List<RoadmapStepDto>();

        // Step 1: Core Fundamentals & Bridge Knowledge
        steps.Add(new RoadmapStepDto
        {
            StepNumber = 1,
            Title = "Master Core Domain Fundamentals",
            Category = "Education",
            Description = $"Strengthen foundational principles required for {targetRoleTitle}. Focus on core concepts matching your baseline from {profile.Education.Degree}.",
            EstimatedDuration = "1–2 Months"
        });

        // Step 2: Key Skill Gap Development
        steps.Add(new RoadmapStepDto
        {
            StepNumber = 2,
            Title = "Bridge Key Skill Gaps",
            Category = "SkillGap",
            Description = "Learn industry-standard tools and frameworks. Build hands-on proficiency through online certifications and guided labs.",
            EstimatedDuration = "2–3 Months"
        });

        // Step 3: Practical Portfolio Projects
        steps.Add(new RoadmapStepDto
        {
            StepNumber = 3,
            Title = "Build Hands-On Capstone Projects",
            Category = "Project",
            Description = "Complete 2–3 end-to-end real-world projects demonstrating your practical skills to recruiters.",
            EstimatedDuration = "1–2 Months"
        });

        // Step 4: Certifications & Professional Proof
        steps.Add(new RoadmapStepDto
        {
            StepNumber = 4,
            Title = "Target Industry Certifications",
            Category = "Certification",
            Description = "Obtain recognized professional certifications to validate your specialized skill set.",
            EstimatedDuration = "1 Month"
        });

        // Step 5: Resume & Job Applications
        steps.Add(new RoadmapStepDto
        {
            StepNumber = 5,
            Title = "Apply for Targeted Roles & Internships",
            Category = "Application",
            Description = $"Prepare your tailored resume, GitHub/portfolio, and LinkedIn profile. Apply for entry-level {targetRoleTitle} positions.",
            EstimatedDuration = "Ongoing"
        });

        return steps;
    }
}
