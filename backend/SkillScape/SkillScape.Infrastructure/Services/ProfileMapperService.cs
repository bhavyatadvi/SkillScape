using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SkillScape.Domain.Entities;
using SkillScape.Application.DTOs;

namespace SkillScape.Infrastructure.Services
{
    public interface IProfileMapperService
    {
        AssessmentContext MapProfile(PhaseProfileInputDto inputProfile);
    }

    public class ProfileMapperService : IProfileMapperService
    {
        public AssessmentContext MapProfile(PhaseProfileInputDto inputProfile)
        {
            var ctx = new AssessmentContext
            {
                EducationLevel = inputProfile.PhaseKey
            };

            // Safely extract from dictionary
            if (inputProfile.Fields.TryGetValue("currentStatus", out var status) || 
                inputProfile.Fields.TryGetValue("status", out status))
                ctx.CurrentStatus = status;

            if (inputProfile.Fields.TryGetValue("degree", out var degree))
                ctx.Degree = degree;

            if (inputProfile.Fields.TryGetValue("major", out var major))
                ctx.Major = major;
                
            if (inputProfile.Fields.TryGetValue("specialization", out var specialization))
                ctx.Specialization = specialization;

            if (inputProfile.Fields.TryGetValue("goalConfidence", out var goalMode))
                ctx.CareerGoalMode = goalMode;

            if (inputProfile.Fields.TryGetValue("primaryGoal", out var goal) && !string.IsNullOrWhiteSpace(goal))
            {
                ctx.TargetCareers = goal.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim())
                                        .ToList();
            }

            // Explicit Career Mode determination for AfterGraduation AND AfterPostGraduation
            bool isGradPhase = ctx.EducationLevel.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase);
            bool isPGPhase   = ctx.EducationLevel.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase);
            if (isGradPhase || isPGPhase)
            {
                if (ctx.TargetCareers.Count > 1)
                {
                    ctx.CareerGoalMode = "selected_options";
                }
                else if (ctx.TargetCareers.Count == 1 && !ctx.TargetCareers[0].Equals("explore", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.CareerGoalMode = "specific_career";
                }
                else
                {
                    ctx.CareerGoalMode = "exploration";
                }
            }

            // Interests (multi-select)
            if (inputProfile.Fields.TryGetValue("interests", out var interestsRaw))
            {
                try {
                    // Try to parse as JSON array if frontend sent it stringified
                    ctx.Interests = JsonSerializer.Deserialize<List<string>>(interestsRaw) ?? new List<string>();
                } catch {
                    ctx.Interests = interestsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                .Select(s => s.Trim())
                                                .ToList();
                }
            }
            if (inputProfile.Fields.TryGetValue("preferenceMode", out var prefMode))
                ctx.PreferenceMode = prefMode;

            if (inputProfile.Fields.TryGetValue("targetCareers", out var tCareers) && !string.IsNullOrWhiteSpace(tCareers))
            {
                ctx.SelectedCareers = tCareers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Select(s => s.Trim())
                                              .ToList();
            }

            if (inputProfile.Fields.TryGetValue("selectedFields", out var fieldsRaw))
            {
                // Field selections only apply to the explicit SELECTED flow.
                // Ignore stale client/session values for exploration so an old
                // field (for example Fashion Design) cannot leak into this quiz.
                if (ctx.PreferenceMode.Equals("SELECTED", StringComparison.OrdinalIgnoreCase) ||
                    ctx.PreferenceMode.Equals("CAREER_CHANGE", StringComparison.OrdinalIgnoreCase))
                {
                    try {
                        ctx.SelectedFields = JsonSerializer.Deserialize<List<string>>(fieldsRaw) ?? new List<string>();
                    } catch {
                        ctx.SelectedFields = fieldsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(s => s.Trim())
                                                      .ToList();
                    }
                }
            }

            if (inputProfile.Fields.TryGetValue("careerChange", out var careerChangeRaw) && bool.TryParse(careerChangeRaw, out var isChange))
            {
                ctx.CareerChange = isChange;
            }
            if (inputProfile.Fields.TryGetValue("degreeIntent", out var degreeIntent))
                ctx.DegreeIntent = degreeIntent;
                
            if (inputProfile.Fields.TryGetValue("engineeringPreferenceMode", out var engMode))
                ctx.EngineeringPreferenceMode = engMode;
                
            if (inputProfile.Fields.TryGetValue("preferredEngineeringFields", out var engFieldsRaw))
            {
                ctx.PreferredEngineeringFields = engFieldsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                             .Select(s => s.Trim())
                                                             .ToList();
            }

            if (inputProfile.Fields.TryGetValue("previousDegree", out var prevDegree))
                ctx.PreviousDegree = prevDegree;
                
            if (inputProfile.Fields.TryGetValue("postGraduationDegree", out var pgDegree))
                ctx.PostGraduationDegree = pgDegree;
                
            if (inputProfile.Fields.TryGetValue("postGraduationMajor", out var pgMajor))
                ctx.PostGraduationMajor = pgMajor;
                
            if (inputProfile.Fields.TryGetValue("postGraduationSpecialization", out var pgSpec))
                ctx.PostGraduationSpecialization = pgSpec;
                
            if (inputProfile.Fields.TryGetValue("workExperience", out var workExp))
                ctx.WorkExperience = workExp;

            // Deterministic Mapping logic for Candidate Domains based on major and degree
            ctx.EligibleDomains = DetermineEligibleDomains(ctx);
            ctx.EligibleSubdomains = DetermineEligibleSubdomains(ctx);

            // ── Domain Taxonomy Engine (v2) — for AfterPostGraduation ─────────────
            // Use the new DomainTaxonomyEngine for precise domain classification.
            // This replaces the broken keyword-chain approach for PG profiles.
            bool isAfterPG = ctx.EducationLevel.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase);
            bool isAfterGrad = ctx.EducationLevel.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase);
            if (isAfterPG || isAfterGrad)
            {
                string taxDeg  = isAfterPG ? ctx.PostGraduationDegree : ctx.Degree;
                string taxMaj  = isAfterPG ? ctx.PostGraduationMajor  : ctx.Major;
                string taxSpec = isAfterPG ? ctx.PostGraduationSpecialization : ctx.Specialization;
                string taxUgDeg = isAfterPG ? ctx.Degree : string.Empty;
                string taxUgMaj = isAfterPG ? ctx.Major  : string.Empty;

                var domainProfile = DomainTaxonomyEngine.Classify(
                    pgDegree: taxDeg,
                    pgMajor: taxMaj,
                    pgSpecialization: taxSpec,
                    ugDegree: taxUgDeg,
                    ugMajor: taxUgMaj,
                    broadField: string.Join(" ", ctx.Interests),
                    isCareerChange: ctx.CareerChange);

                ctx.DomainTaxonomyTag     = domainProfile.PrimaryDomain;
                ctx.KnownFacts            = domainProfile.KnownFacts;
                ctx.InferredCareerFamilies = domainProfile.CareerFamilies;

                // Also set EligibleDomains from taxonomy for backwards compat
                if (domainProfile.PrimaryDomain != "General" && !ctx.EligibleDomains.Contains(domainProfile.PrimaryDomain))
                    ctx.EligibleDomains.Insert(0, domainProfile.PrimaryDomain);

                // Set EligibleSubdomains from taxonomy sub-domains
                if (domainProfile.SubDomains.Any())
                    ctx.EligibleSubdomains = domainProfile.SubDomains;
            }

            // "Explore" for a BA is a career-discovery assessment, not a test
            // that assumes the learner must remain in their exact BA subject.
            // Keep the existing question selector and scoring model, while
            // giving their prior distribution a broad set of realistic adjacent
            // career domains to compare (law, business, design, and analytics).
            bool isBaExplore = isAfterGrad && ctx.PreferenceMode.Equals("EXPLORE", StringComparison.OrdinalIgnoreCase) &&
                QuizProfileClassifier.IsBachelorOfArts(ctx.Degree, ctx.Major);
            if (isBaExplore)
            {
                foreach (var domain in new[] { "Arts", "Humanities", "Law", "Commerce", "Design", "Technology", "Education", "Government" })
                {
                    if (!ctx.EligibleDomains.Contains(domain, StringComparer.OrdinalIgnoreCase))
                        ctx.EligibleDomains.Add(domain);
                }
                ctx.EligibleSubdomains.Clear();
            }


            return ctx;
        }

        // ── Legacy keyword-chain domain detection (kept for After 10th/12th) ──
        private List<string> DetermineEligibleDomains(AssessmentContext ctx)
        {
            var domains = new List<string>();
            var lowerMajor = ctx.Major.ToLowerInvariant();
            var lowerDegree = ctx.Degree.ToLowerInvariant();
            var interests = string.Join(" ", ctx.Interests).ToLowerInvariant();
            var targets = string.Join(" ", ctx.TargetCareers).ToLowerInvariant();
            var combined = $"{lowerDegree} {lowerMajor} {interests} {targets}";

            if (ctx.EducationLevel.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase) && !ctx.CareerChange)
            {
                // Strict mapping for After Graduation
                if (lowerMajor.Contains("computer") || lowerMajor.Contains("tech") || lowerDegree.Contains("b.tech") || lowerDegree.Contains("bca") || lowerDegree.Contains("mca"))
                {
                    domains.Add("Technology");
                }
                else if (lowerMajor.Contains("account") || lowerMajor.Contains("finance") || lowerDegree.Contains("com") || lowerDegree.Contains("bba"))
                {
                    domains.Add("Commerce");
                }
                else if (lowerDegree.Contains("ba ") || lowerDegree == "ba" || lowerMajor.Contains("psychology") || lowerMajor.Contains("history"))
                {
                    domains.Add("Humanities");
                }
                else if (lowerDegree.Contains("mbbs") || lowerDegree.Contains("bds") || lowerDegree.Contains("pharm") || lowerDegree.Contains("nursing") || lowerMajor.Contains("medical"))
                {
                    domains.Add("Healthcare");
                }
                else if (lowerDegree.Contains("b.des") || lowerDegree.Contains("design"))
                {
                    domains.Add("Design");
                }
                else if (lowerDegree.Contains("llb") || lowerMajor.Contains("law"))
                {
                    domains.Add("Law");
                }
                else if (lowerDegree.Contains("b.sc") || lowerDegree.Contains("bsc") || lowerMajor.Contains("science"))
                {
                    domains.Add("Science");
                }
            }
            else if (!string.IsNullOrEmpty(ctx.EducationLevel) && 
                     (ctx.EducationLevel.Equals("after_post_graduation", StringComparison.OrdinalIgnoreCase) || 
                      ctx.EducationLevel.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase)) && 
                     !ctx.CareerChange)
            {
                var pgMajor = (ctx.PostGraduationMajor ?? "").ToLowerInvariant();
                var pgDegree = (ctx.PostGraduationDegree ?? "").ToLowerInvariant();
                
                if (pgMajor.Contains("computer") || pgMajor.Contains("tech") || pgMajor.Contains("ai") || pgMajor.Contains("data") || pgDegree.Contains("m.tech") || pgDegree.Contains("mca"))
                {
                    domains.Add("Technology");
                }
                else if (pgMajor.Contains("finance") || pgMajor.Contains("account") || pgMajor.Contains("business") || pgDegree.Contains("m.com") || pgDegree.Contains("mba"))
                {
                    domains.Add("Commerce");
                }
                else if (pgMajor.Contains("psychology") || pgMajor.Contains("arts") || pgDegree.Contains("ma ") || pgDegree == "ma")
                {
                    domains.Add("Humanities");
                }
                else if (pgMajor.Contains("science") || pgDegree.Contains("m.sc") || pgDegree.Contains("msc"))
                {
                    domains.Add("Science");
                }
                else if (pgMajor.Contains("law") || pgDegree.Contains("llm"))
                {
                    domains.Add("Law");
                }
                else if (pgDegree.Contains("md") || pgDegree.Contains("ms") || pgMajor.Contains("medic"))
                {
                    domains.Add("Healthcare");
                }
                else if (pgMajor.Contains("design") || pgDegree.Contains("m.des"))
                {
                    domains.Add("Design");
                }
            }
            else
            {
                // Legacy / General mapping for others or if CareerChange is true
                if (combined.Contains("computer") || combined.Contains("tech") || combined.Contains("b.tech") || combined.Contains("bca") || combined.Contains("cyber"))
                    domains.Add("Technology");
                
                if (combined.Contains("account") || combined.Contains("finance") || combined.Contains("com") || combined.Contains("bba") || combined.Contains("business"))
                    domains.Add("Commerce");
                
                if (combined.Contains("law") || combined.Contains("llb"))
                    domains.Add("Law");
                    
                if (combined.Contains("psychology") || combined.Contains("ba"))
                    domains.Add("Humanities");
                    
                if (combined.Contains("medical") || combined.Contains("mbbs") || combined.Contains("healthcare"))
                    domains.Add("Healthcare");
            }

            return domains.Distinct().ToList();
        }
        
        private List<string> DetermineEligibleSubdomains(AssessmentContext ctx)
        {
            var subdomains = new List<string>();
            var lowerMajor = ctx.Major.ToLowerInvariant();
            var interests = string.Join(" ", ctx.Interests).ToLowerInvariant();
            var targets = string.Join(" ", ctx.TargetCareers).ToLowerInvariant();
            var combined = $"{lowerMajor} {interests} {targets}";

            if (ctx.EducationLevel.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase))
            {
                if (combined.Contains("cyber") || combined.Contains("security")) subdomains.Add("Cybersecurity");
                if (combined.Contains("data") || combined.Contains("ai") || combined.Contains("machine learning") || combined.Contains("tech.data")) {
                    subdomains.Add("Data Science");
                    subdomains.Add("Artificial Intelligence");
                }
                if (combined.Contains("software") || combined.Contains("developer") || combined.Contains("tech.sw") || combined.Contains("computer application") || combined.Contains("bca") || combined.Contains("mca") || combined.Contains("computer science") || combined.Contains("information tech")) {
                    subdomains.Add("Software Engineering");
                }

                if (combined.Contains("account") || combined.Contains("audit") || combined.Contains("tax")) subdomains.Add("Accounting");
                if (combined.Contains("finance") || combined.Contains("investment") || combined.Contains("banking") || combined.Contains("wealth")) subdomains.Add("Finance");
                
                if (combined.Contains("psychology")) subdomains.Add("Psychology");
                if (combined.Contains("corporate law")) subdomains.Add("Corporate Law");
            }
            else if (ctx.EducationLevel.Equals("after_post_graduation", StringComparison.OrdinalIgnoreCase))
            {
                var pgCombined = $"{ctx.PostGraduationMajor.ToLowerInvariant()} {ctx.PostGraduationSpecialization.ToLowerInvariant()} {interests} {targets}";
                
                if (pgCombined.Contains("cyber") || pgCombined.Contains("security")) subdomains.Add("Cybersecurity");
                if (pgCombined.Contains("data")) subdomains.Add("Data Science");
                if (pgCombined.Contains("ai") || pgCombined.Contains("machine learning") || pgCombined.Contains("deep learning") || pgCombined.Contains("nlp")) subdomains.Add("Artificial Intelligence");
                if (pgCombined.Contains("software") || pgCombined.Contains("developer") || pgCombined.Contains("systems")) subdomains.Add("Software Engineering");
                if (pgCombined.Contains("cloud") || pgCombined.Contains("devops")) subdomains.Add("Cloud Computing");
                
                if (pgCombined.Contains("account") || pgCombined.Contains("audit") || pgCombined.Contains("tax")) subdomains.Add("Accounting");
                if (pgCombined.Contains("finance") || pgCombined.Contains("investment") || pgCombined.Contains("banking") || pgCombined.Contains("wealth")) subdomains.Add("Finance");
                if (pgCombined.Contains("business analytics") || pgCombined.Contains("management")) subdomains.Add("Business Management");
                
                if (pgCombined.Contains("psychology")) subdomains.Add("Psychology");
                if (pgCombined.Contains("corporate law") || pgCombined.Contains("law")) subdomains.Add("Law");
            }
            else
            {
                if (lowerMajor.Contains("accounting")) subdomains.Add("Accounting");
                if (lowerMajor.Contains("finance")) subdomains.Add("Finance");
                if (lowerMajor.Contains("computer science")) subdomains.Add("Computer Science");
                if (lowerMajor.Contains("psychology")) subdomains.Add("Psychology");
                if (lowerMajor.Contains("corporate law")) subdomains.Add("Corporate Law");
            }
            
            return subdomains.Distinct().ToList();
        }
    }
}
