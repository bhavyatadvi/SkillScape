using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SkillScape.Domain.Entities;

namespace SkillScape.Infrastructure.Services
{
    public interface IProfileValidationEngine
    {
        bool IsQuestionAllowed(QuizQuestionV2 question, AssessmentContext context, List<string> answeredQuestionIds, out string reason);
    }

    public class ProfileValidationEngine : IProfileValidationEngine
    {
        private readonly ILogger<ProfileValidationEngine> _logger;

        public ProfileValidationEngine(ILogger<ProfileValidationEngine> logger)
        {
            _logger = logger;
        }

        public bool IsQuestionAllowed(QuizQuestionV2 question, AssessmentContext context, List<string> answeredQuestionIds, out string reason)
        {
            reason = "Valid";

            // If context is null, allow it (for After 10th / After 12th fallback)
            if (context == null) return true;

            // Normalize phase key checks — handle both PascalCase and snake_case
            bool isAfterGrad = !string.IsNullOrEmpty(context.EducationLevel) &&
                (context.EducationLevel.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase) ||
                 context.EducationLevel.Equals("after_graduation", StringComparison.OrdinalIgnoreCase));

            // BUG FIX: old code used "after_post_graduation" — sessions store "AfterPostGraduation"
            bool isAfterPG = !string.IsNullOrEmpty(context.EducationLevel) &&
                (context.EducationLevel.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase) ||
                 context.EducationLevel.Equals("after_post_graduation", StringComparison.OrdinalIgnoreCase));

            bool isGovJobs = !string.IsNullOrEmpty(context.EducationLevel) &&
                context.EducationLevel.Equals("GovernmentJobs", StringComparison.OrdinalIgnoreCase);

            bool isCareerSwitch = !string.IsNullOrEmpty(context.EducationLevel) &&
                context.EducationLevel.Equals("CareerSwitch", StringComparison.OrdinalIgnoreCase);

            // The BA question bank is intentionally degree-specific. Humanities
            // is also a valid classification for some B.Sc subjects (for example
            // Psychology), but that must not make BA-only questions eligible.
            bool isBaOnlyQuestion = question.Id.StartsWith("Q_AFTERGRAD_BA_", StringComparison.OrdinalIgnoreCase)
                || question.Id.StartsWith("FB_BA_", StringComparison.OrdinalIgnoreCase);
            if ((isAfterGrad || isAfterPG) && isBaOnlyQuestion &&
                !QuizProfileClassifier.IsBachelorOfArts(context.Degree, context.Major) &&
                !QuizProfileClassifier.IsBachelorOfArts(context.PreviousDegree, null))
            {
                reason = "BA-specific question is not allowed for a non-BA profile.";
                return false;
            }

            // CHECK 1 - PHASE KEY
            if (isAfterGrad || isAfterPG || isGovJobs || isCareerSwitch)
            {
                string expectedPhase = isAfterPG ? "AfterPostGraduation" : (isGovJobs ? "GovernmentJobs" : (isCareerSwitch ? "CareerSwitch" : "AfterGraduation"));
                if (!question.PhaseKey.Equals(expectedPhase, StringComparison.OrdinalIgnoreCase))
                {
                    reason = $"Phase Mismatch: Question '{question.PhaseKey}' != '{expectedPhase}'";
                    return false;
                }
            }

            // CHECK 1B - GOV JOBS DOMAIN FILTER
            if (isGovJobs)
            {
                bool isUniversal = !string.IsNullOrEmpty(question.Domain) && question.Domain.Equals("Universal", StringComparison.OrdinalIgnoreCase);
                if (context.EligibleDomains.Any() && !string.IsNullOrEmpty(question.Domain) && !isUniversal)
                {
                    if (!context.EligibleDomains.Any(domain => AreEquivalentCareerDomains(question.Domain, domain)))
                    {
                        reason = $"GovJobs Domain Mismatch: '{question.Domain}' not in EligibleDomains [{string.Join(", ", context.EligibleDomains)}]";
                        return false;
                    }
                }
            }

            // CHECK 2 - LOCKED DOMAIN (mid-quiz domain lock, e.g. After12th PCM engineering)
            if (!string.IsNullOrEmpty(context.LockedDomain))
            {
                if (string.IsNullOrEmpty(question.Domain) || !question.Domain.Equals(context.LockedDomain, StringComparison.OrdinalIgnoreCase))
                {
                    reason = $"Locked Domain Mismatch: '{question.Domain}' != '{context.LockedDomain}'";
                    return false;
                }
            }

            // CHECK 2B - DOMAIN TAXONOMY FILTER (AfterPostGraduation)
            // Uses DomainTaxonomyEngine to prevent cross-domain leakage:
            // e.g. MBBS+MD user must never see Technology questions.
            // Domain-level (not subdomain-level) to avoid "zero questions pass" problem.
            if (isAfterPG && !context.CareerChange && !string.IsNullOrEmpty(context.DomainTaxonomyTag))
            {
                string questionBankDomain = DomainTaxonomyEngine.GetQuestionBankDomain(context.DomainTaxonomyTag);
                if (!string.IsNullOrEmpty(questionBankDomain) && !string.IsNullOrEmpty(question.Domain))
                {
                    bool domainMatch = IsPostGraduationDomainMatch(question.Domain, questionBankDomain);

                    // Generic, research, and education questions are valid cross-domain
                    // postgraduate pathway questions. The seed bank also uses legacy
                    // labels such as Engineering/Medical/Nursing that map to the
                    // canonical Technology/Healthcare domains.
                    bool isUniversal = question.Domain.Equals("Universal", StringComparison.OrdinalIgnoreCase)
                        || question.Domain.Equals("Generic", StringComparison.OrdinalIgnoreCase)
                        || question.Domain.Equals("Research", StringComparison.OrdinalIgnoreCase)
                        || question.Domain.Equals("Education", StringComparison.OrdinalIgnoreCase);

                    if (!domainMatch && !isUniversal)
                    {
                        reason = $"Domain Taxonomy Mismatch: '{question.Domain}' not valid for profile domain '{context.DomainTaxonomyTag}' (expected: '{questionBankDomain}')";
                        return false;
                    }
                }
            }

            // CHECK 3 - KNOWN FACTS FILTER
            // Reject questions whose Intent was already answered in the profile form.
            // e.g. Intent="broad_field_preference" is known if user already stated their major.
            if ((isAfterPG || isAfterGrad || isGovJobs) && context.KnownFacts.Any() && !string.IsNullOrEmpty(question.Intent))
            {
                if (context.KnownFacts.Contains(question.Intent, StringComparer.OrdinalIgnoreCase))
                {
                    reason = $"Known Fact: Intent '{question.Intent}' is already captured in the profile.";
                    return false;
                }
            }

            // DEEP CHECKS — only for AfterGrad/AfterPG
            if (isAfterGrad || isAfterPG)
            {
                bool isTargetedMode = context.CareerGoalMode == "selected_options" || context.CareerGoalMode == "specific_career";

                // CHECK 4 - TARGET CAREER EVIDENCE (targeted mode)
                if (isTargetedMode && context.TargetCareers.Any())
                {
                    if (!string.IsNullOrEmpty(question.TargetDiscriminatesJson) && question.TargetDiscriminatesJson != "[]")
                    {
                        try
                        {
                            var targets = JsonSerializer.Deserialize<List<string>>(question.TargetDiscriminatesJson) ?? new();
                            if (targets.Any() && !targets.Intersect(context.TargetCareers, StringComparer.OrdinalIgnoreCase).Any())
                            {
                                reason = $"Target Career Mismatch: [{string.Join(", ", targets)}] vs [{string.Join(", ", context.TargetCareers)}]";
                                return false;
                            }
                        }
                        catch { }
                    }
                }

                // CHECK 5 - EXPLORATION DOMAIN & SUBDOMAIN BOUNDS (AfterGraduation only)
                // STRICT FIELD ISOLATION: Must enforce both Domain and Subdomain (if present in profile).
                // For AfterPostGraduation, CHECK 2B already handles domain filtering.
                if (isAfterGrad && !context.CareerChange)
                {
                    bool hasExplicitFieldSelection = context.PreferenceMode.Equals("SELECTED", StringComparison.OrdinalIgnoreCase)
                        && context.SelectedFields.Any();
                    if (hasExplicitFieldSelection)
                    {
                        bool matchesSelectedField = !string.IsNullOrWhiteSpace(question.Subdomain) &&
                            context.SelectedFields.Any(field => FieldMatches(field, question.Subdomain));
                        if (!matchesSelectedField)
                        {
                            reason = $"Selected-field mismatch: '{question.Subdomain}' does not match [{string.Join(", ", context.SelectedFields)}].";
                            return false;
                        }
                    }

                    bool isUniversal = !string.IsNullOrEmpty(question.Domain) && question.Domain.Equals("Universal", StringComparison.OrdinalIgnoreCase);

                    // 5A: Validate Domain
                    if (!hasExplicitFieldSelection && context.EligibleDomains.Any() && !string.IsNullOrEmpty(question.Domain) && !isUniversal)
                    {
                        if (!context.EligibleDomains.Any(domain => AreEquivalentCareerDomains(question.Domain, domain)))
                        {
                            reason = $"AfterGrad Exploration Domain Mismatch: '{question.Domain}' not in EligibleDomains [{string.Join(", ", context.EligibleDomains)}]";
                            return false;
                        }
                    }

                    // 5B: Validate Subdomain (STRICT ISOLATION)
                    // If the user's profile resolved to a specific subdomain (e.g., "Civil Engineering"),
                    // the question MUST either match that subdomain or be a broad question for the same
                    // domain. The latter is needed for shared domain starter questions such as the
                    // Commerce question used to start a Finance assessment.
                    if (!hasExplicitFieldSelection && context.EligibleSubdomains.Any() && !string.IsNullOrEmpty(question.Subdomain) && !isUniversal)
                    {
                        bool isBroadQuestionForEligibleDomain = context.EligibleDomains
                            .Any(domain => question.Subdomain.Equals(domain, StringComparison.OrdinalIgnoreCase));

                        if (!isBroadQuestionForEligibleDomain &&
                            !context.EligibleSubdomains.Contains(question.Subdomain, StringComparer.OrdinalIgnoreCase))
                        {
                            reason = $"AfterGrad Exploration Subdomain Mismatch: '{question.Subdomain}' not in EligibleSubdomains [{string.Join(", ", context.EligibleSubdomains)}]";
                            return false;
                        }
                    }
                }

                // Exploration questions can be marked Universal even when their
                // answer signals are tied to one profession (for example surgery).
                // Keep those broad-stage prompts inside the learner's degree family.
                // This is a profile eligibility check only; question ranking and
                // information-gain selection remain unchanged.
                if ((isAfterGrad || isAfterPG) &&
                    context.PreferenceMode.Equals("EXPLORE", StringComparison.OrdinalIgnoreCase) &&
                    !context.CareerChange &&
                    !IsQuestionWithinExploreField(question, context))
                {
                    reason = $"Explore question targets a field outside the learner's degree family: '{question.Id}'.";
                    return false;
                }
            }

            // CHECK 9 - ALREADY ANSWERED
            if (answeredQuestionIds != null && answeredQuestionIds.Contains(question.Id))
            {
                reason = "Question Already Asked";
                return false;
            }

            return true;
        }

        private static bool IsPostGraduationDomainMatch(string questionDomain, string canonicalDomain)
        {
            if (questionDomain.Equals(canonicalDomain, StringComparison.OrdinalIgnoreCase)) return true;

            return canonicalDomain switch
            {
                "Technology" => questionDomain.Equals("Engineering", StringComparison.OrdinalIgnoreCase),
                "Healthcare" => questionDomain.Equals("Medical", StringComparison.OrdinalIgnoreCase)
                    || questionDomain.Equals("Nursing", StringComparison.OrdinalIgnoreCase),
                "Science" => questionDomain.Equals("Agriculture", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        private static bool FieldMatches(string selectedField, string questionField)
        {
            if (string.IsNullOrWhiteSpace(selectedField) || string.IsNullOrWhiteSpace(questionField)) return false;
            static string Normalize(string value) => new(value.Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant).ToArray());
            var selected = Normalize(selectedField).Replace("engineering", string.Empty, StringComparison.Ordinal);
            var question = Normalize(questionField).Replace("engineering", string.Empty, StringComparison.Ordinal);
            return selected.Length > 0 && question.Length > 0 &&
                (selected == question || selected.Contains(question, StringComparison.Ordinal) || question.Contains(selected, StringComparison.Ordinal));
        }

        private static bool IsQuestionWithinExploreField(QuizQuestionV2 question, AssessmentContext context)
        {
            var allowedDomains = context.EligibleDomains
                .Select(NormalizeExploreDomain)
                .Where(domain => !string.IsNullOrEmpty(domain))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // BA exploration intentionally includes adjacent career directions;
            // retain that scope while excluding unrelated healthcare pathways.
            if (QuizProfileClassifier.IsBachelorOfArts(context.Degree, context.Major) ||
                QuizProfileClassifier.IsBachelorOfArts(context.PreviousDegree, null))
            {
                foreach (var domain in new[] { "Arts", "Humanities", "Law", "Commerce", "Design", "Technology", "Education", "Government" })
                    allowedDomains.Add(NormalizeExploreDomain(domain));
            }
            else if (IsLawProfile(context))
            {
                allowedDomains.Add("Law");
                allowedDomains.Add("Humanities");
            }
            else if (IsEngineeringProfile(context))
            {
                allowedDomains.Add("Technology");
                allowedDomains.Add("Engineering");
            }
            else if (QuizProfileClassifier.IsBachelorOfScience(context.Degree, context.Major) ||
                QuizProfileClassifier.IsBachelorOfScience(context.PreviousDegree, null))
            {
                allowedDomains.Add("Science");
                allowedDomains.Add("LifeSciences");
            }

            // Without a degree/domain signal, preserve the prior broad behavior.
            if (allowedDomains.Count == 0) return true;

            if (!string.IsNullOrWhiteSpace(question.Domain) &&
                !question.Domain.Equals("Universal", StringComparison.OrdinalIgnoreCase) &&
                !question.Domain.Equals("Generic", StringComparison.OrdinalIgnoreCase))
            {
                if (!allowedDomains.Contains(NormalizeExploreDomain(question.Domain))) return false;
            }

            var targetIds = ReadTargetIds(question.TargetDiscriminatesJson);
            if (targetIds.Count == 0) return true;

            var targetDomains = targetIds
                .Select(GetExploreDomainForTarget)
                .Where(domain => !string.IsNullOrEmpty(domain))
                .ToList();

            // IDs outside the known taxonomy are neutral. Every classified target
            // on a Universal question must fit the degree family to keep, for
            // example, surgeon signals out of an LLB exploration quiz.
            return targetDomains.Count == 0 || targetDomains.All(allowedDomains.Contains);
        }

        private static bool IsLawProfile(AssessmentContext context)
        {
            var profile = $"{context.Degree} {context.Major} {context.Specialization} {context.PreviousDegree}";
            return profile.Contains("llb", StringComparison.OrdinalIgnoreCase) ||
                profile.Contains("law", StringComparison.OrdinalIgnoreCase) ||
                profile.Contains("legal", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEngineeringProfile(AssessmentContext context)
        {
            var profile = $"{context.Degree} {context.Major} {context.Specialization} {context.PreviousDegree}";
            return profile.Contains("b.e", StringComparison.OrdinalIgnoreCase) ||
                profile.Contains("btech", StringComparison.OrdinalIgnoreCase) ||
                profile.Contains("b.tech", StringComparison.OrdinalIgnoreCase) ||
                profile.Contains("engineering", StringComparison.OrdinalIgnoreCase) ||
                profile.Contains("computer science", StringComparison.OrdinalIgnoreCase) ||
                profile.Contains("cse", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeExploreDomain(string domain) => domain.Trim().ToLowerInvariant() switch
        {
            "arts" or "humanities" or "liberal arts" or "arts & humanities" => "Humanities",
            "eng" or "engineering" or "technology" or "tech" => "Technology",
            "pure science" or "science" or "lifesciences" or "life sciences" => "Science",
            "healthcare" or "medical" or "medicine" or "nursing" => "Healthcare",
            "commerce" or "business" or "management" => "Commerce",
            "law" or "legal" => "Law",
            "design" or "creative arts" or "architecture" => "Design",
            "government" or "public service" => "Government",
            _ => domain.Trim()
        };

        private static List<string> ReadTargetIds(string? json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "[]") return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string GetExploreDomainForTarget(string targetId)
        {
            var id = targetId.Trim().ToLowerInvariant();
            // Selected-field fallback questions can use a broad taxonomy root
            // (for example "design" or "sci.med") instead of a dotted leaf.
            // Treat those roots as classified too; otherwise a persisted design
            // question has no recognized target and can leak into any Explore quiz.
            if (id == "sci.med" || id.StartsWith("sci.med.") || id.StartsWith("role_surgeon") || id.Contains("physician") || id.Contains("clinical")) return "Healthcare";
            if (id == "sci.eng" || id.StartsWith("sci.eng.") || id == "sci.tech" || id.StartsWith("sci.tech.")) return "Technology";
            if (id == "sci.pure" || id.StartsWith("sci.pure.") || id == "sci.life" || id.StartsWith("sci.life.") || id == "sci.bio" || id.StartsWith("sci.bio.")) return "Science";
            if (id == "com" || id.StartsWith("com.")) return "Commerce";
            if (id == "law" || id.StartsWith("law.")) return "Law";
            if (id == "arts" || id.StartsWith("arts.")) return "Humanities";
            if (id == "design" || id.StartsWith("design.")) return "Design";
            if (id == "govt" || id.StartsWith("govt.")) return "Government";
            if (id == "edu" || id.StartsWith("edu.")) return "Education";
            if (id == "arch" || id.StartsWith("arch.")) return "Design";
            if (id == "psych" || id.StartsWith("psych.")) return "Psychology";
            if (id.StartsWith("role_") && id.Contains("surgeon")) return "Healthcare";
            return string.Empty;
        }

        private static bool AreEquivalentCareerDomains(string first, string second)
        {
            static string Canonicalize(string value) => value.Trim().ToLowerInvariant() switch
            {
                "humanities" or "arts" or "liberal arts" or "arts & humanities" => "humanities",
                "engineering" or "eng" or "technology" or "tech" => "technology",
                "commerce" or "business" or "management" => "commerce",
                "law" or "legal" => "law",
                "design" or "creative arts" => "design",
                "education" or "teaching" => "education",
                "government" or "public service" => "government",
                _ => value.Trim().ToLowerInvariant()
            };

            return Canonicalize(first).Equals(Canonicalize(second), StringComparison.OrdinalIgnoreCase);
        }
    }
}
