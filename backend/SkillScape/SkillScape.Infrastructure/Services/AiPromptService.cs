using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services
{
    public class AiPromptService : IAiPromptService
    {
        private static readonly Dictionary<string, CategoryMeta> CategoryDefinitions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["after_10th"] = new(
                "after_10th",
                "After 10th",
                20,
                new List<string> { "science_pcm", "science_pcb", "commerce", "arts_humanities", "vocational_iti", "polytechnic" },
                new List<string>
                {
                    "When you imagine a school project, which activity feels most exciting?",
                    "Which subject or activity do you enjoy the most after class?",
                    "What type of challenge makes you want to learn more?",
                    "How do you prefer to solve a practical problem?",
                    "Which learning situation would make you feel most confident?",
                    "If you could spend a week exploring one topic, which would you choose?",
                    "When you picture yourself at 25, what kind of work would make you proud?",
                    "Which area do you partner with friends in naturally?",
                    "What kind of hands-on activity do you like best?",
                    "Which environment helps you stay curious and motivated?",
                    "When you read about future careers, which option sounds most interesting?",
                    "What do you enjoy more: experiments, numbers, stories, or building things?",
                    "Which path feels most like a natural extension of your hobbies?",
                    "What kind of practical skill would you like to develop first?",
                    "Which area makes your attention feel most focused?",
                    "If you had to make a simple product for school, what would it involve?",
                    "What kind of example do you understand most quickly?",
                    "Which kind of real-world problem would you like to help solve?",
                    "What motivates you more: creativity, structure, helping others, or technical work?",
                    "Which theme makes you want to plan ahead for the next year?"
                }),
            ["after_12th"] = new(
                "after_12th",
                "After 12th",
                25,
                new List<string>
                {
                    "btech_cs", "btech_core", "mbbs_medical", "bpharm_nursing", "bcom_bba",
                    "ba_humanities", "ba_llb", "bdes_design", "bsc_pure", "bsc_cs_data"
                },
                new List<string>
                {
                    "Which type of work do you want to see yourself doing every day after college?",
                    "Which outcome feels most rewarding: creating, analyzing, teaching, or caring for people?",
                    "If you could learn one professional skill deeply, what would it be?",
                    "Which description fits the kinds of projects you enjoy most?",
                    "Which role would let you use your favorite strengths regularly?",
                    "If your future job needed one core talent, what would it rely on?",
                    "Which career path sounds most aligned with the way you think?",
                    "What kind of team would energize you: technical, creative, research, or service?",
                    "Which college program would make you feel excited to attend every day?",
                    "What kind of problem do you enjoy solving the most?",
                    "Which subject area makes you curious about how the world works?",
                    "Which kind of career impact feels most meaningful?",
                    "Which environment matches your ideal way of working: structured, logical, caring, or creative?",
                    "What type of learning do you prefer: hands-on, analytical, human-centered, or design-driven?",
                    "Which long-term goal matters most to you?",
                    "What kind of mentors would you like to have in college?",
                    "Which path would help you build confidence and tangible results?",
                    "What kind of decision would you make first when choosing your course?",
                    "Which daily habit aligns best with your personality?",
                    "What should your future work allow you to do?",
                    "Which path seems best for someone who likes both challenge and stability?",
                    "What kind of knowledge would you like to gain in the next three years?",
                    "Which field feels like the right blend of curiosity and career potential?",
                    "Which area would you want to specialize in after your first degree?",
                    "What quality do you most want to develop through your studies?"
                }),
            ["after_graduation"] = new(
                "after_graduation",
                "After Graduation",
                30,
                new List<string>
                {
                    "frontend", "backend", "fullstack", "data", "ml", "devops", "business", "design", "healthcare", "education"
                },
                new List<string>
                {
                    "Which career track would keep you focused and excited after graduation?",
                    "What kind of product or service do you want to help build?",
                    "Which type of role would you choose if you could start today?",
                    "What do you want to be known for professionally?",
                    "Which combination of skills feels most like your strength?",
                    "Where do you enjoy spending your time: code, data, people, or design?",
                    "What kind of impact do you want your work to create?",
                    "Which work pace suits you best: fast innovation, careful research, or structured delivery?",
                    "How do you want to contribute to a team or organization?",
                    "Which area makes you feel most capable of solving complex problems?",
                    "What motivates you more: building, improving, advising, or educating?",
                    "Which environment would help you grow quickly in your first role?",
                    "What kind of career path seems most sustainable for you?",
                    "Which industry feels like the best fit for your interests?",
                    "What kind of learning style do you want to continue after graduation?",
                    "What type of problem do you want to solve for customers or users?",
                    "Which combination of technical and strategic work appeals to you?",
                    "What kind of collaboration do you prefer: cross-functional, specialist, or client-facing?",
                    "Which title sounds most inspiring when you picture your future?",
                    "Which area do you enjoy reading about or experimenting with?",
                    "What kind of tools or workflows are you most drawn to?",
                    "Which daily routine feels like the best professional fit?",
                    "What kind of outcomes would make you proud of your first job?",
                    "Which skills do you want to keep building after degree completion?",
                    "What kind of feedback helps you feel successful?",
                    "Which problem domain makes you feel confident you can solve it?",
                    "What type of role gives you the most room for creativity?",
                    "Which field feels most connected to your personal strengths?",
                    "What kind of career transition would feel natural from your current degree?"
                }),
            ["after_post_grad"] = new(
                "after_post_grad",
                "After Post-Graduation",
                20,
                new List<string>
                {
                    "phd_research", "industry_expert", "consulting", "product_mgmt", "startup_founder", "govt_policy", "academia_teaching"
                },
                new List<string>
                {
                    "Which impact do you want to make with a post-graduation specialization?",
                    "What kind of strategic responsibility feels most energizing?",
                    "Where do you see your expertise making the biggest difference?",
                    "Which career path fits your long-term leadership goals?",
                    "What type of influence do you want through your work?",
                    "Which role would make the most of your advanced learning?",
                    "What kind of professional legacy do you want to build?",
                    "What type of mentorship or teaching do you enjoy most?",
                    "Which research or policy challenge feels most worth solving?",
                    "How do you prefer to combine theory and real-world impact?",
                    "Which organizational context would suit your strengths best?",
                    "What kind of career path gives you both independence and structure?",
                    "What type of team would you like to lead or support?",
                    "Which area do you want to become recognized as an expert in?",
                    "What kind of work allows you to ask the biggest questions?",
                    "Which path best reflects your desire to develop people or products?",
                    "How do you prefer to influence outcomes in a large system?",
                    "What kind of knowledge transfer feels most rewarding to you?",
                    "Which area would you like to shape through long-term commitment?",
                    "What kind of credibility or domain mastery are you aiming for?"
                }),
            ["career_switch"] = new(
                "career_switch",
                "Career Switch",
                20,
                new List<string>
                {
                    "frontend_switch", "backend_switch", "data_analyst", "ml_engineer", "devops_switch",
                    "product_manager", "ux_designer", "biz_analyst", "tech_sales", "edtech_creator"
                },
                new List<string>
                {
                    "Which type of new career feels most achievable based on your current strengths?",
                    "What part of your existing experience would you like to reuse?",
                    "Which area seems easiest for you to reskill into?",
                    "What kind of work would feel rewarding after a transition?",
                    "Which new role matches your interest in technology or business?",
                    "What learning path seems most realistic for your next move?",
                    "Which area would let you build confidence quickly in a new field?",
                    "What kind of impact do you want from a career change?",
                    "Which role would help you stay curious during the switch?",
                    "What kind of team or client work would you enjoy next?",
                    "Which area would let you combine your past knowledge with new skills?",
                    "What kind of project would help you show value in a new domain?",
                    "Which new career path makes you feel most energized?",
                    "What kind of support would you need for your transition?",
                    "Which future outcome feels most worth the effort of switching?",
                    "What kind of role would improve your day-to-day motivation?",
                    "Which career goal feels most practical and meaningful?",
                    "What type of work would make your background feel valuable again?",
                    "Which direction would allow you to grow without losing stability?",
                    "What new talent would you like to demonstrate first?"
                }),
            ["upskilling"] = new(
                "upskilling",
                "Upskilling",
                20,
                new List<string>
                {
                    "ai_ml_certs", "cloud_aws", "cloud_azure", "cloud_gcp", "cybersecurity",
                    "data_analytics", "fullstack_web", "devops_certs", "product_mgmt", "low_code"
                },
                new List<string>
                {
                    "Which capability do you want to strengthen most in the next 6 months?",
                    "What do you want to do with a new certification or upskill?",
                    "Which technology or discipline feels most relevant to your goals?",
                    "What career benefit matters most from your next training investment?",
                    "Which skill will help you work more confidently or efficiently?",
                    "What type of learning outcome would make you feel accomplished?",
                    "Which area do you want to add to your existing experience?",
                    "What kind of role do you want to prepare for next?",
                    "Which skill area would improve your career momentum the most?",
                    "What new credential would make your profile more attractive?",
                    "Which domain would help you work on higher-value problems?",
                    "What kind of hands-on project would motivate you to learn?",
                    "Which skill gap do you want to close first?",
                    "What kind of professional confidence are you trying to build?",
                    "Which area would feel the most natural for you to study now?",
                    "What kind of technology do you want to use daily?",
                    "Which emerging field excites you the most?",
                    "What do you want your resume to showcase next?",
                    "What type of problem-solving do you want to improve?",
                    "Which learning path feels the most strategic for your future?"
                }),
            ["entrepreneurship"] = new(
                "entrepreneurship",
                "Entrepreneurship",
                20,
                new List<string>
                {
                    "saas_b2b", "d2c_consumer", "deep_tech", "marketplace", "edtech_startup",
                    "fintech_startup", "social_enterprise", "consulting_firm", "content_creator", "franchise_retail"
                },
                new List<string>
                {
                    "Which business idea feels most inspiring for you to build?",
                    "What type of customer would you most enjoy serving?",
                    "Which model feels like the best fit for your strengths?",
                    "What kind of market problem do you want to solve?",
                    "Which resource or asset do you want to leverage as a founder?",
                    "Which type of startup would keep you motivated every day?",
                    "What kind of value would you most like to create?",
                    "Which path feels most natural for your existing interests?",
                    "What kind of business allows you to test fast and learn quickly?",
                    "Which type of revenue model makes the most sense to you?",
                    "What kind of team or partnership would excite you?",
                    "Which business challenge do you feel confident addressing?",
                    "What kind of customer relationship do you want to build?",
                    "Which field feels ready for innovation from you?",
                    "What kind of product or service would you enjoy launching?",
                    "Which startup role feels most aligned with your style?",
                    "What type of growth would you like to pursue first?",
                    "Which business story makes you want to take action?",
                    "What kind of intellectual strength would help your venture?",
                    "Which path feels most sustainable for a first venture?"
                }),
            ["govt_jobs"] = new(
                "govt_jobs",
                "Govt Jobs",
                20,
                new List<string>
                {
                    "ias_upsc", "banking_po", "defence_forces", "ssc_cgl", "state_psc",
                    "judiciary_law", "teaching_govt", "railways_rrb", "psu_gate", "police_services"
                },
                new List<string>
                {
                    "Which area of public service feels most meaningful to you?",
                    "Which kind of government role would make you feel proud?",
                    "What kind of challenge do you want to solve for society?",
                    "Which exam path feels best suited to your skills?",
                    "What kind of discipline or structure helps you thrive?",
                    "Which type of public service work feels most stable?",
                    "What kind of preparation style suits you best?",
                    "Which social outcome motivates you the most?",
                    "Which government field would make you feel most secure?",
                    "What kind of knowledge do you enjoy applying in an exam setting?",
                    "Which role would help you influence policy or public good?",
                    "What kind of training do you find most engaging?",
                    "Which service area matches your strongest interests?",
                    "What kind of professional rhythm do you want?",
                    "Which mission would you feel proud to support?",
                    "What kind of government system do you want to be part of?",
                    "Which public sector pathway feels most practical?",
                    "What type of community impact do you want to create?",
                    "Which role balances personal growth and public service?",
                    "What kind of legacy do you want your government career to leave?"
                }),
            ["international_edu"] = new(
                "international_edu",
                "International Education",
                20,
                new List<string>
                {
                    "ms_usa", "ms_canada", "ms_europe", "mba_us_top10", "mba_global",
                    "phd_funded", "mim_europe", "stem_australia", "uk_1year_ms", "online_global"
                },
                new List<string>
                {
                    "Which global study goal feels most exciting to you?",
                    "What kind of international academic experience do you want?",
                    "Which country or program feels most aligned with your strengths?",
                    "What motivates you more: research, business, or international exposure?",
                    "Which credential would best expand your professional opportunities?",
                    "What kind of academic environment helps you perform at your best?",
                    "Which timeline feels most realistic for your next degree?",
                    "What kind of cultural experience would you like to have?",
                    "Which global qualification would be most valuable for your career?",
                    "What kind of classroom or research setting would you prefer?",
                    "Which application goal would you feel most confident pursuing?",
                    "What kind of scholarship or funding path suits you?",
                    "Which region’s work and visa prospects appeal to you most?",
                    "Which learning style appeals to you: intensive, international, flexible, or immersive?",
                    "What kind of outcome do you want from studying abroad?",
                    "Which program length is easiest for you to commit to?",
                    "What kind of network do you want to build while studying?",
                    "Which academic specialization feels most promising globally?",
                    "What kind of support system do you want during your studies?",
                    "Which future role are you trying to unlock with an international degree?"
                })
        };

        public List<AiQuizQuestionDto> GenerateQuizQuestions(string category)
        {
            var meta = GetCategoryMeta(category);
            var questions = new List<AiQuizQuestionDto>();
            var combinations = BuildDomainCombinations(meta.DomainIds, meta.QuestionCount);
            for (var i = 0; i < meta.QuestionCount; i++)
            {
                var selectedDomains = combinations[i];
                questions.Add(new AiQuizQuestionDto
                {
                    Id = $"{category}_q{i + 1}",
                    Text = meta.QuestionTemplates[i % meta.QuestionTemplates.Count],
                    Category = meta.Name,
                    QualificationLevel = meta.Code,
                    Options = selectedDomains.Select((domain, index) => new AiQuizOptionDto
                    {
                        Id = $"{category}_q{i + 1}_o{index + 1}",
                        Text = GenerateOptionText(domain, i),
                        Domain = domain,
                        Weight = 10
                    }).ToList()
                });
            }

            return questions;
        }

        public AiCareerPredictionDto PredictCareer(string category, List<string> answers, Dictionary<string, string>? extraContext = null)
        {
            var meta = GetCategoryMeta(category);
            var scores = ScoreDomains(meta, answers);
            var normalizedScores = NormalizeScores(scores, meta.DomainIds);
            var ordered = normalizedScores.OrderByDescending(x => x.Value).ToList();
            var primary = ordered.First();
            var secondary = ordered.Count > 1 ? ordered[1] : new KeyValuePair<string, int>(string.Empty, 0);
            var profile = GetDomainProfile(primary.Key);
            var confidence = CalculateConfidence(primary.Value, secondary.Value);
            var explanation = BuildExplanation(primary.Key, secondary.Key, answers, extraContext, meta, profile);

            return new AiCareerPredictionDto
            {
                RecommendedDomain = primary.Key,
                RecommendedTitle = profile.Title,
                Confidence = confidence,
                AllScores = normalizedScores,
                SecondaryDomain = secondary.Key,
                Explanation = explanation,
                KeyStrengths = profile.KeyStrengths.ToList(),
                RecommendedSkills = profile.RecommendedSkills.ToList(),
                SalaryIndiaInr = profile.SalaryIndia,
                SalaryGlobalUsd = profile.SalaryGlobal,
                JobGrowth = profile.JobGrowth,
                FiveYearSalaryTrend = profile.FiveYearSalaryTrend,
                NextSteps = profile.NextSteps.ToList(),
                Certifications = profile.Certifications.ToList(),
                Timeline = profile.Timeline
            };
        }

        private static CategoryMeta GetCategoryMeta(string category)
        {
            if (!CategoryDefinitions.TryGetValue(category.Trim().ToLowerInvariant(), out var meta))
            {
                throw new ArgumentException($"Unknown category: {category}");
            }

            return meta;
        }

        private static List<List<string>> BuildDomainCombinations(List<string> domains, int count)
        {
            var combinations = new List<List<string>>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var step = Math.Max(1, domains.Count / 2);
            var cycle = 0;

            while (combinations.Count < count)
            {
                var start = (cycle * step) % domains.Count;
                var combo = new List<string>();
                for (var j = 0; j < 4; j++)
                {
                    combo.Add(domains[(start + j) % domains.Count]);
                }

                var key = string.Join(',', combo);
                if (!used.Contains(key))
                {
                    used.Add(key);
                    combinations.Add(combo);
                }
                else
                {
                    for (var offset = 1; offset < domains.Count && combinations.Count < count; offset++)
                    {
                        combo = new List<string>();
                        for (var j = 0; j < 4; j++)
                        {
                            combo.Add(domains[(start + j + offset) % domains.Count]);
                        }

                        key = string.Join(',', combo);
                        if (!used.Contains(key))
                        {
                            used.Add(key);
                            combinations.Add(combo);
                            break;
                        }
                    }
                }

                cycle++;
                if (cycle > domains.Count * 4 && combinations.Count < count)
                {
                    // Fall back to the easiest distinct combinations in case we still need more
                    var nextCombo = domains.Take(4).ToList();
                    while (used.Contains(string.Join(',', nextCombo)))
                    {
                        nextCombo = nextCombo.Skip(1).Concat(nextCombo.Take(1)).ToList();
                    }

                    used.Add(string.Join(',', nextCombo));
                    combinations.Add(nextCombo);
                }
            }

            return combinations;
        }

        private static string GenerateOptionText(string domainId, int questionIndex)
        {
            var templates = GetOptionTemplates(domainId);
            var template = templates[questionIndex % templates.Length];
            return template;
        }

        private static string[] GetOptionTemplates(string domainId)
        {
            return domainId switch
            {
                "science_pcm" => new[]
                {
                    "I enjoy solving numbers and building simple machines.",
                    "I like physics experiments and logical puzzles.",
                    "I am drawn to circuits, models, and engineering ideas."
                },
                "science_pcb" => new[]
                {
                    "I like studying living systems, health, and plants.",
                    "I enjoy biology experiments and understanding how the body works.",
                    "I am curious about chemistry, life sciences, and care-based learning."
                },
                "commerce" => new[]
                {
                    "I enjoy markets, money management, and business ideas.",
                    "I like planning budgets and understanding how companies grow.",
                    "I am drawn to sales, finance, and practical commerce skills."
                },
                "arts_humanities" => new[]
                {
                    "I enjoy stories, history, and people-centered topics.",
                    "I like writing, culture, and creative thinking.",
                    "I am drawn to ideas about society, expression, and communication."
                },
                "vocational_iti" => new[]
                {
                    "I enjoy hands-on work and building useful things with tools.",
                    "I like learning a trade through practice and real tasks.",
                    "I am drawn to quick skill training and reliable hands-on jobs."
                },
                "polytechnic" => new[]
                {
                    "I enjoy technical projects and applied engineering training.",
                    "I like solving practical problems with machines and designs.",
                    "I am drawn to learning by doing and working with technology."
                },
                "btech_cs" => new[]
                {
                    "I enjoy coding, software ideas, and solving logic problems.",
                    "I like building apps and learning computer science concepts.",
                    "I am drawn to programming, systems thinking, and innovation."
                },
                "btech_core" => new[]
                {
                    "I enjoy engineering fundamentals and building physical systems.",
                    "I like working with machines, structures, and technical analysis.",
                    "I am drawn to core engineering skills and applied science."
                },
                "mbbs_medical" => new[]
                {
                    "I enjoy caring for people and understanding how the body works.",
                    "I like medical science, diagnosis, and helping patients recover.",
                    "I am drawn to medicine, clinical learning, and health care."
                },
                "bpharm_nursing" => new[]
                {
                    "I enjoy medicine-related science and patient support roles.",
                    "I like pharmacy, nursing, and practical health care skills.",
                    "I am drawn to health science and helping people feel better."
                },
                "bcom_bba" => new[]
                {
                    "I enjoy business planning, finance, and organizational work.",
                    "I like marketing, accounting, and learning how companies operate.",
                    "I am drawn to commerce, management, and strategic decision making."
                },
                "ba_humanities" => new[]
                {
                    "I enjoy literature, society, and thoughtful communication.",
                    "I like exploring ideas in history, language, and culture.",
                    "I am drawn to writing, analysis, and human-centered thinking."
                },
                "ba_llb" => new[]
                {
                    "I enjoy law, debate, and interpreting rules fairly.",
                    "I like arguments, justice, and helping people with legal issues.",
                    "I am drawn to advocacy, research, and social fairness."
                },
                "bdes_design" => new[]
                {
                    "I enjoy visual creativity and designing products or services.",
                    "I like styling experiences, graphics, and thoughtful design work.",
                    "I am drawn to creative problem solving and user-focused design."
                },
                "bsc_pure" => new[]
                {
                    "I enjoy exploring science ideas and understanding how things work.",
                    "I like experiments, theory, and learning deep scientific concepts.",
                    "I am drawn to research, analysis, and scientific discovery."
                },
                "bsc_cs_data" => new[]
                {
                    "I enjoy working with data, computers, and analytical problems.",
                    "I like turning information into insights and actionable ideas.",
                    "I am drawn to statistics, coding, and data-driven learning."
                },
                "frontend" => new[]
                {
                    "I enjoy crafting user interfaces and making apps feel intuitive.",
                    "I like styling pages and solving interaction design challenges.",
                    "I am drawn to browser technologies and user-facing development."
                },
                "backend" => new[]
                {
                    "I enjoy building systems and making data flow reliably behind the scenes.",
                    "I like APIs, databases, and logical service architecture.",
                    "I am drawn to server-side work and technical problem solving."
                },
                "fullstack" => new[]
                {
                    "I enjoy working across the full product from UI to backend logic.",
                    "I like both design and systems thinking in a single role.",
                    "I am drawn to end-to-end product development and integration."
                },
                "data" => new[]
                {
                    "I enjoy finding meaning in data and turning it into decisions.",
                    "I like analytics, dashboards, and business insights from data.",
                    "I am drawn to data analysis, reporting, and problem solving."
                },
                "ml" => new[]
                {
                    "I enjoy teaching machines to recognize patterns and predict outcomes.",
                    "I like models, algorithms, and building intelligent systems.",
                    "I am drawn to advanced analytics, AI, and experimentation."
                },
                "devops" => new[]
                {
                    "I enjoy making software delivery fast, stable, and automated.",
                    "I like cloud platforms, monitoring, and deployment workflows.",
                    "I am drawn to infrastructure, reliability, and team efficiency."
                },
                "business" => new[]
                {
                    "I enjoy shaping strategy and helping teams grow with data.",
                    "I like business models, planning, and organizational insight.",
                    "I am drawn to consulting, operations, and growth planning."
                },
                "design" => new[]
                {
                    "I enjoy designing experiences that solve real user problems.",
                    "I like research, prototyping, and making products more usable.",
                    "I am drawn to creativity, empathy, and thoughtful design."
                },
                "healthcare" => new[]
                {
                    "I enjoy supporting healthcare teams and improving patient outcomes.",
                    "I like organizing care, operations, and health systems.",
                    "I am drawn to medical administration and healthcare strategy."
                },
                "education" => new[]
                {
                    "I enjoy creating learning experiences and improving education outcomes.",
                    "I like teaching, curriculum planning, and mentoring others.",
                    "I am drawn to education strategy, training, and learning design."
                },
                "phd_research" => new[]
                {
                    "I enjoy deep research and building expertise in a narrow field.",
                    "I like solving big academic questions and publishing new findings.",
                    "I am drawn to a long-term scholarly or research path."
                },
                "industry_expert" => new[]
                {
                    "I enjoy building domain expertise and advising teams with pragmatic insight.",
                    "I like turning advanced knowledge into real-world leadership.",
                    "I am drawn to senior technical or domain-based roles."
                },
                "consulting" => new[]
                {
                    "I enjoy helping organizations solve problems across functions.",
                    "I like analysis, recommendations, and working with multiple stakeholders.",
                    "I am drawn to advisory roles and business problem solving."
                },
                "product_mgmt" => new[]
                {
                    "I enjoy defining product strategy and coordinating cross-functional teams.",
                    "I like shaping features, prioritizing work, and delivering value.",
                    "I am drawn to product vision, user research, and execution."
                },
                "startup_founder" => new[]
                {
                    "I enjoy taking initiative and launching ideas with a tight team.",
                    "I like building a new product, testing it, and learning quickly.",
                    "I am drawn to entrepreneurship, ownership, and fast iteration."
                },
                "govt_policy" => new[]
                {
                    "I enjoy thinking about policy, regulations, and public impact.",
                    "I like designing programs that help communities or government systems.",
                    "I am drawn to shaping decisions that affect society at scale."
                },
                "academia_teaching" => new[]
                {
                    "I enjoy teaching, mentoring, and building academic programs.",
                    "I like supporting students and creating structured learning paths.",
                    "I am drawn to the classroom, research, and intellectual leadership."
                },
                "frontend_switch" => new[]
                {
                    "I enjoy building interfaces and learning new frontend tools quickly.",
                    "I like working with visuals, interactivity, and user experience.",
                    "I am drawn to a web-focused career change with tangible results."
                },
                "backend_switch" => new[]
                {
                    "I enjoy working with systems, databases, and service logic.",
                    "I like solving backend problems and making data work reliably.",
                    "I am drawn to a server-side career change with technical depth."
                },
                "data_analyst" => new[]
                {
                    "I enjoy analyzing trends and turning data into clear decisions.",
                    "I like making charts, reports, and business recommendations.",
                    "I am drawn to data-driven insight and practical analytics work."
                },
                "ml_engineer" => new[]
                {
                    "I enjoy building predictive models and applying AI to real problems.",
                    "I like experimenting with data and machine learning workflows.",
                    "I am drawn to combining coding, math, and intelligent systems."
                },
                "devops_switch" => new[]
                {
                    "I enjoy working on automation, deployment, and cloud operations.",
                    "I like ensuring systems are reliable and scalable.",
                    "I am drawn to the intersection of development and infrastructure."
                },
                "product_manager" => new[]
                {
                    "I enjoy coordinating teams and defining product direction.",
                    "I like translating customer needs into clear product goals.",
                    "I am drawn to leadership, communication, and strategic planning."
                },
                "ux_designer" => new[]
                {
                    "I enjoy designing experiences that are easy and delightful to use.",
                    "I like researching user behavior and creating polished interfaces.",
                    "I am drawn to empathy-driven design and customer experience."
                },
                "biz_analyst" => new[]
                {
                    "I enjoy analyzing business performance and improving processes.",
                    "I like transforming data into strategic recommendations.",
                    "I am drawn to commercial insight and operations optimization."
                },
                "tech_sales" => new[]
                {
                    "I enjoy connecting technology solutions with customer needs.",
                    "I like explaining products and building trusted relationships.",
                    "I am drawn to sales roles that bridge tech and business."
                },
                "edtech_creator" => new[]
                {
                    "I enjoy designing learning content and education experiences.",
                    "I like creating courses, videos, or tools for learners.",
                    "I am drawn to teaching through technology and content creation."
                },
                "ai_ml_certs" => new[]
                {
                    "I enjoy studying AI concepts and building machine learning skills.",
                    "I like exploring models, data, and intelligent automation.",
                    "I am drawn to certification paths that focus on AI capabilities."
                },
                "cloud_aws" => new[]
                {
                    "I enjoy working with cloud infrastructure and services.",
                    "I like learning AWS tools and deploying scalable systems.",
                    "I am drawn to cloud architecture and platform operations."
                },
                "cloud_azure" => new[]
                {
                    "I enjoy working with Azure services and enterprise cloud systems.",
                    "I like configuring managed infrastructure and security controls.",
                    "I am drawn to cloud platforms and enterprise tooling."
                },
                "cloud_gcp" => new[]
                {
                    "I enjoy learning Google Cloud tools and data platform services.",
                    "I like building serverless and analytics solutions in the cloud.",
                    "I am drawn to cloud-native engineering and innovation."
                },
                "cybersecurity" => new[]
                {
                    "I enjoy protecting systems and finding security weaknesses.",
                    "I like risk assessment, monitoring, and secure engineering.",
                    "I am drawn to defending networks and protecting data."
                },
                "data_analytics" => new[]
                {
                    "I enjoy interpreting data and delivering actionable insights.",
                    "I like building analytics reports and measuring outcomes.",
                    "I am drawn to data-driven decision making and business insight."
                },
                "fullstack_web" => new[]
                {
                    "I enjoy building both frontend and backend parts of applications.",
                    "I like working end-to-end on product features and delivery.",
                    "I am drawn to full-stack development and modern web engineering."
                },
                "devops_certs" => new[]
                {
                    "I enjoy automating deployments and improving system reliability.",
                    "I like building CI/CD pipelines and monitoring infrastructures.",
                    "I am drawn to DevOps practices and platform engineering."
                },
                "low_code" => new[]
                {
                    "I enjoy creating applications quickly with low-code tools.",
                    "I like designing workflows and building solutions without heavy coding.",
                    "I am drawn to practical digital automation and rapid prototyping."
                },
                "saas_b2b" => new[]
                {
                    "I enjoy building software products for business customers.",
                    "I like solving operational problems with recurring value.",
                    "I am drawn to B2B product strategy and customer success."
                },
                "d2c_consumer" => new[]
                {
                    "I enjoy creating products that connect directly with consumers.",
                    "I like building brands, marketing, and retail experiences.",
                    "I am drawn to consumer behavior and product storytelling."
                },
                "deep_tech" => new[]
                {
                    "I enjoy solving hard technical problems and innovating new technology.",
                    "I like research-driven products and long-term technical breakthroughs.",
                    "I am drawn to deep technology development and scientific ideas."
                },
                "marketplace" => new[]
                {
                    "I enjoy connecting buyers and sellers through a shared platform.",
                    "I like designing ecosystems and balancing two sides of a market.",
                    "I am drawn to network effects and platform growth."
                },
                "edtech_startup" => new[]
                {
                    "I enjoy building tools that make learning easier and more accessible.",
                    "I like designing education products for learners and teachers.",
                    "I am drawn to the intersection of technology and learning."
                },
                "fintech_startup" => new[]
                {
                    "I enjoy creating financial products that simplify money management.",
                    "I like combining finance with modern technology and user experience.",
                    "I am drawn to digital finance innovation and secure services."
                },
                "social_enterprise" => new[]
                {
                    "I enjoy building ventures that create social and environmental value.",
                    "I like solving community problems with sustainable business models.",
                    "I am drawn to purpose-driven entrepreneurship and impact."
                },
                "consulting_firm" => new[]
                {
                    "I enjoy advising organizations and solving business challenges.",
                    "I like delivering strategic recommendations and supporting execution.",
                    "I am drawn to consultancy, analysis, and client collaboration."
                },
                "content_creator" => new[]
                {
                    "I enjoy producing content that educates or inspires an audience.",
                    "I like storytelling, media creation, and building a following.",
                    "I am drawn to digital content, learning experiences, and community."
                },
                "franchise_retail" => new[]
                {
                    "I enjoy operating established retail models with a clear system.",
                    "I like managing customer experience and store-level performance.",
                    "I am drawn to retail operations and brand-consistent service."
                },
                "ias_upsc" => new[]
                {
                    "I enjoy preparing for administrative leadership and public service.",
                    "I like studying policy, governance, and national issues.",
                    "I am drawn to civil services and broad societal impact."
                },
                "banking_po" => new[]
                {
                    "I enjoy finance, banking operations, and structured professional exams.",
                    "I like customer-facing finance roles and disciplined preparation.",
                    "I am drawn to bank careers, economic problems, and service stability."
                },
                "defence_forces" => new[]
                {
                    "I enjoy disciplined service, teamwork, and defending the nation.",
                    "I like physical training, leadership, and a patriotic career.",
                    "I am drawn to defense roles and structured public service."
                },
                "ssc_cgl" => new[]
                {
                    "I enjoy generalist government roles that support public administration.",
                    "I like exam preparation and working in central government departments.",
                    "I am drawn to stable public sector work and organized service."
                },
                "state_psc" => new[]
                {
                    "I enjoy serving state-level communities through governance roles.",
                    "I like local administration, policy, and public impact.",
                    "I am drawn to state civil services and regional leadership."
                },
                "judiciary_law" => new[]
                {
                    "I enjoy law, justice, and serving the legal system.",
                    "I like careful reasoning, courtroom work, and legal research.",
                    "I am drawn to judicial service and rights-based work."
                },
                "teaching_govt" => new[]
                {
                    "I enjoy teaching students and serving through government schools.",
                    "I like shaping learning in a stable public education role.",
                    "I am drawn to classroom instruction and education service."
                },
                "railways_rrb" => new[]
                {
                    "I enjoy operating or supporting a large public transport system.",
                    "I like technical, commercial, or administrative roles in railways.",
                    "I am drawn to railway jobs and public infrastructure work."
                },
                "psu_gate" => new[]
                {
                    "I enjoy cracking technical exams for PSU or engineering roles.",
                    "I like disciplined preparation and working in public enterprises.",
                    "I am drawn to engineer-level government jobs and stability."
                },
                "police_services" => new[]
                {
                    "I enjoy protecting communities and enforcing the law.",
                    "I like disciplined public service and working in safety roles.",
                    "I am drawn to police or civil defense careers."
                },
                "ms_usa" => new[]
                {
                    "I enjoy pursuing advanced study in the USA and global research opportunities.",
                    "I like immersing myself in a robust academic environment abroad.",
                    "I am drawn to a U.S.-based graduate experience and career exposure."
                },
                "ms_canada" => new[]
                {
                    "I enjoy the idea of studying in Canada with practical work options.",
                    "I like a globally respected program and strong post-study pathways.",
                    "I am drawn to Canada for its balanced academic and career environment."
                },
                "ms_europe" => new[]
                {
                    "I enjoy studying in Europe with rich cultural and academic variety.",
                    "I like concise programs and international research exposure.",
                    "I am drawn to Europe for multidisciplinary learning and travel."
                },
                "mba_us_top10" => new[]
                {
                    "I enjoy applying for a top MBA program in the USA.",
                    "I like building strong business networks and leadership skills.",
                    "I am drawn to a high-impact global MBA experience."
                },
                "mba_global" => new[]
                {
                    "I enjoy pursuing an international MBA with diverse campus experiences.",
                    "I like learning business from a global and multicultural lens.",
                    "I am drawn to a flexible MBA program with broad career options."
                },
                "phd_funded" => new[]
                {
                    "I enjoy applying for a funded PhD and focusing on research deeply.",
                    "I like the idea of a scholar path with sponsorship and mentorship.",
                    "I am drawn to long-term research and academic excellence abroad."
                },
                "mim_europe" => new[]
                {
                    "I enjoy studying management in Europe with an international cohort.",
                    "I like developing leadership skills in a shorter, focused program.",
                    "I am drawn to a MiM pathway with strong global branding."
                },
                "stem_australia" => new[]
                {
                    "I enjoy STEM study in Australia with practical research or internship options.",
                    "I like working in a supportive environment with strong post-study choices.",
                    "I am drawn to Australian programs for quality science education."
                },
                "uk_1year_ms" => new[]
                {
                    "I enjoy a one-year UK master’s program for fast international entry.",
                    "I like concise study with global university recognition.",
                    "I am drawn to an efficient UK pathway that opens overseas options."
                },
                "online_global" => new[]
                {
                    "I enjoy flexible online learning from global institutions.",
                    "I like earning credentials while balancing work or commitments.",
                    "I am drawn to practical, self-paced skill development with global access."
                },
                _ => new[]
                {
                    $"I am interested in {GetDomainTitle(domainId)} and practical learning.",
                    $"I like exploring {GetDomainTitle(domainId)} in a hands-on way.",
                    $"I am drawn to the ideas behind {GetDomainTitle(domainId)} and want to grow there."
                }
            };
        }

        private static Dictionary<string, int> ScoreDomains(CategoryMeta meta, List<string> answers)
        {
            var scores = meta.DomainIds.ToDictionary(domain => domain, domain => 0, StringComparer.OrdinalIgnoreCase);
            var normalizedAnswers = answers.Select(answer => answer?.ToLowerInvariant() ?? string.Empty).ToList();

            foreach (var answer in normalizedAnswers)
            {
                foreach (var domainId in meta.DomainIds)
                {
                    foreach (var keyword in GetDomainKeywords(domainId))
                    {
                        if (!string.IsNullOrWhiteSpace(keyword) && answer.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            scores[domainId] += 1;
                        }
                    }
                }
            }

            return scores;
        }

        private static Dictionary<string, int> NormalizeScores(Dictionary<string, int> scores, List<string> domainIds)
        {
            var normalized = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var total = scores.Values.Sum();
            if (total == 0)
            {
                foreach (var domain in domainIds)
                {
                    normalized[domain] = 0;
                }

                normalized[domainIds.First()] = 100;
                return normalized;
            }

            foreach (var domain in domainIds)
            {
                normalized[domain] = (int)Math.Round(scores[domain] * 100.0 / total);
            }

            var diff = 100 - normalized.Values.Sum();
            if (diff != 0)
            {
                var highest = normalized.OrderByDescending(x => x.Value).First().Key;
                normalized[highest] += diff;
            }

            return normalized;
        }

        private static int CalculateConfidence(int primaryScore, int secondaryScore)
        {
            if (primaryScore >= 90 && secondaryScore <= 20)
            {
                return 100;
            }

            var delta = primaryScore - secondaryScore;
            var confidence = 60 + delta * 6;
            return Math.Clamp(confidence, 50, 100);
        }

        private static string BuildExplanation(string primaryDomain, string? secondaryDomain, List<string> answers, Dictionary<string, string>? extraContext, CategoryMeta meta, DomainProfile profile)
        {
            var explanation = new List<string>();
            explanation.Add($"Your answers are strongest for {profile.Title} based on the themes you mentioned.");

            if (answers.Count > 0)
            {
                var sample = answers.First();
                explanation.Add($"You emphasized: '{sample.Trim()}' which maps well to the skills and interests of this path.");
            }

            if (!string.IsNullOrEmpty(secondaryDomain))
            {
                explanation.Add($"The second-best fit is {GetDomainTitle(secondaryDomain)}, which shares related strengths but is slightly less aligned.");
            }

            if (extraContext != null && extraContext.TryGetValue("current role", out var currentRole) && !string.IsNullOrWhiteSpace(currentRole))
            {
                explanation.Add($"Your current role as '{currentRole}' also supports this direction by reinforcing relevant strengths.");
            }

            explanation.Add($"The recommendation is tailored for the {meta.Name} category and reflects both your preferred problem-solving style and career momentum.");
            return string.Join(" ", explanation);
        }

        private static string[] GetDomainKeywords(string domainId)
        {
            if (string.IsNullOrWhiteSpace(domainId))
            {
                return Array.Empty<string>();
            }

            return domainId switch
            {
                "science_pcm" => new[] { "math", "physics", "engineer", "circuits", "numbers", "machines" },
                "science_pcb" => new[] { "biology", "chemistry", "health", "plants", "body", "life" },
                "commerce" => new[] { "business", "finance", "money", "market", "accounting", "trade" },
                "arts_humanities" => new[] { "history", "literature", "writing", "culture", "society", "language" },
                "vocational_iti" => new[] { "trade", "hands-on", "workshop", "repair", "tool", "skill" },
                "polytechnic" => new[] { "technical", "applied", "engineering", "machines", "design", "project" },
                "btech_cs" => new[] { "software", "programming", "code", "computer", "developer", "apps" },
                "btech_core" => new[] { "engineering", "mechanical", "electrical", "civil", "technical", "design" },
                "mbbs_medical" => new[] { "doctor", "medical", "patient", "health", "clinic", "surgery" },
                "bpharm_nursing" => new[] { "pharmacy", "nursing", "medicine", "care", "health", "clinical" },
                "bcom_bba" => new[] { "business", "management", "marketing", "finance", "accounts", "commerce" },
                "ba_humanities" => new[] { "arts", "society", "culture", "writing", "history", "communication" },
                "ba_llb" => new[] { "law", "justice", "legal", "rights", "court", "advocate" },
                "bdes_design" => new[] { "design", "creative", "visual", "product", "ux", "brand" },
                "bsc_pure" => new[] { "science", "research", "lab", "theory", "experiment", "analysis" },
                "bsc_cs_data" => new[] { "data", "analysis", "computing", "statistics", "models", "software" },
                "frontend" => new[] { "frontend", "ui", "design", "user interface", "web", "html" },
                "backend" => new[] { "backend", "server", "database", "api", "logic", "architecture" },
                "fullstack" => new[] { "fullstack", "frontend", "backend", "product", "application", "integration" },
                "data" => new[] { "data", "analytics", "report", "dashboard", "insight", "metrics" },
                "ml" => new[] { "machine learning", "ai", "model", "prediction", "algorithm", "training" },
                "devops" => new[] { "devops", "cloud", "deployment", "automation", "infrastructure", "pipeline" },
                "business" => new[] { "business", "strategy", "growth", "planning", "operations", "market" },
                "design" => new[] { "design", "ux", "ui", "experience", "prototype", "creative" },
                "healthcare" => new[] { "healthcare", "hospital", "patient", "system", "medical", "care" },
                "education" => new[] { "education", "teaching", "learning", "training", "students", "curriculum" },
                "phd_research" => new[] { "research", "phd", "study", "academic", "theory", "publication" },
                "industry_expert" => new[] { "expert", "domain", "industry", "consult", "practical", "leadership" },
                "consulting" => new[] { "consulting", "advisor", "strategy", "recommendation", "client", "problem" },
                "product_mgmt" => new[] { "product", "roadmap", "user", "feature", "prioritization", "strategy" },
                "startup_founder" => new[] { "startup", "founder", "build", "launch", "venture", "growth" },
                "govt_policy" => new[] { "policy", "government", "regulation", "public", "program", "impact" },
                "academia_teaching" => new[] { "teaching", "academia", "lecture", "students", "research", "university" },
                "frontend_switch" => new[] { "frontend", "ui", "react", "css", "javascript", "ux" },
                "backend_switch" => new[] { "backend", "api", "server", "database", "service", "architecture" },
                "data_analyst" => new[] { "data", "analysis", "excel", "report", "dashboard", "insights" },
                "ml_engineer" => new[] { "machine learning", "ai", "model", "python", "scikit", "prediction" },
                "devops_switch" => new[] { "devops", "cloud", "docker", "ci/cd", "infrastructure", "automation" },
                "product_manager" => new[] { "product", "customer", "roadmap", "strategy", "feature", "stakeholder" },
                "ux_designer" => new[] { "ux", "design", "user", "prototype", "research", "interface" },
                "biz_analyst" => new[] { "business", "analysis", "process", "data", "strategy", "requirements" },
                "tech_sales" => new[] { "sales", "customer", "product", "technology", "pitch", "relationship" },
                "edtech_creator" => new[] { "education", "content", "learning", "course", "teaching", "digital" },
                "ai_ml_certs" => new[] { "ai", "machine learning", "certification", "model", "data", "python" },
                "cloud_aws" => new[] { "aws", "cloud", "deployment", "serverless", "infrastructure", "automation" },
                "cloud_azure" => new[] { "azure", "cloud", "microsoft", "deployment", "infrastructure", "services" },
                "cloud_gcp" => new[] { "gcp", "cloud", "google", "compute", "serverless", "data" },
                "cybersecurity" => new[] { "security", "cyber", "network", "risk", "threat", "compliance" },
                "data_analytics" => new[] { "data", "analytics", "statistics", "report", "dashboard", "insight" },
                "fullstack_web" => new[] { "fullstack", "frontend", "backend", "web", "application", "stack" },
                "devops_certs" => new[] { "devops", "certification", "automation", "ci/cd", "infrastructure", "cloud" },
                "low_code" => new[] { "low-code", "automation", "workflow", "tools", "platform", "business" },
                "saas_b2b" => new[] { "saas", "business", "software", "b2b", "subscription", "enterprise" },
                "d2c_consumer" => new[] { "consumer", "d2c", "retail", "brand", "product", "marketing" },
                "deep_tech" => new[] { "deep tech", "research", "innovation", "technology", "science", "patent" },
                "marketplace" => new[] { "marketplace", "platform", "buyers", "sellers", "network", "transactions" },
                "edtech_startup" => new[] { "edtech", "education", "learning", "platform", "students", "teaching" },
                "fintech_startup" => new[] { "fintech", "finance", "payments", "banking", "digital", "security" },
                "social_enterprise" => new[] { "social", "impact", "community", "sustainability", "purpose", "nonprofit" },
                "consulting_firm" => new[] { "consulting", "strategy", "clients", "advice", "business", "analysis" },
                "content_creator" => new[] { "content", "creator", "media", "video", "audience", "story" },
                "franchise_retail" => new[] { "franchise", "retail", "store", "business", "customer", "operations" },
                "ias_upsc" => new[] { "ias", "upsc", "civil services", "administration", "policy", "governance" },
                "banking_po" => new[] { "banking", "po", "finance", "accounts", "customer", "exam" },
                "defence_forces" => new[] { "defence", "army", "navy", "air force", "service", "training" },
                "ssc_cgl" => new[] { "ssc", "cgl", "government", "exam", "administration", "public" },
                "state_psc" => new[] { "psc", "state", "government", "administration", "policy", "public" },
                "judiciary_law" => new[] { "judiciary", "law", "court", "justice", "legal", "advocate" },
                "teaching_govt" => new[] { "teaching", "school", "education", "students", "government", "class" },
                "railways_rrb" => new[] { "railways", "rrb", "transport", "public", "infrastructure", "service" },
                "psu_gate" => new[] { "psu", "gate", "engineering", "exam", "public", "sector" },
                "police_services" => new[] { "police", "security", "law", "public", "force", "service" },
                "ms_usa" => new[] { "usa", "study", "graduate", "international", "research", "visa" },
                "ms_canada" => new[] { "canada", "study", "pr", "work permit", "graduate", "international" },
                "ms_europe" => new[] { "europe", "study", "masters", "international", "research", "program" },
                "mba_us_top10" => new[] { "mba", "usa", "business school", "leadership", "network", "top" },
                "mba_global" => new[] { "mba", "global", "business", "leadership", "international", "strategy" },
                "phd_funded" => new[] { "phd", "funded", "research", "scholarship", "academic", "study" },
                "mim_europe" => new[] { "mim", "europe", "management", "international", "program", "business" },
                "stem_australia" => new[] { "stem", "australia", "science", "technology", "engineering", "study" },
                "uk_1year_ms" => new[] { "uk", "one-year", "masters", "ms", "study", "international" },
                "online_global" => new[] { "online", "global", "course", "credential", "flexible", "learning" },
                _ => new[] { "interest", "career", "skills", "goals", "learning", "growth" }
            };
        }

        private static DomainProfile GetDomainProfile(string domainId)
        {
            var title = GetDomainTitle(domainId);
            return domainId switch
            {
                "science_pcm" => new DomainProfile(
                    title,
                    new[] { "Logical reasoning", "Problem solving", "Technical curiosity" },
                    new[] { "Math fundamentals", "Physics concepts", "Circuit basics", "Analytical thinking", "Project modeling" },
                    "₹5,00,000 - ₹12,00,000 per year",
                    "$7,500 - $16,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(5, 7, 9, 12, 17),
                    new[] { "Start with engineering foundation courses", "Build small coding or robotics projects", "Explore PCM-based internships or extra labs" },
                    new[] { "NPTEL Engineering Foundations", "Basic Electronics courses", "Math reasoning certifications" },
                    "1-3 years"
                ),
                "science_pcb" => new DomainProfile(
                    title,
                    new[] { "Curiosity for life sciences", "Detail orientation", "Helping mindset" },
                    new[] { "Biology fundamentals", "Chemistry labs", "Health sciences", "Research methods", "Medical ethics" },
                    "₹6,00,000 - ₹14,00,000 per year",
                    "$9,000 - $18,000 per year",
                    "+20% demand by 2030",
                    BuildSalaryTrend(6, 8, 10, 13, 16),
                    new[] { "Join biology or health science projects", "Attend medical inquiry workshops", "Complete foundation health certifications" },
                    new[] { "Health science certificates", "Clinical internships", "Biology research programs" },
                    "1-4 years"
                ),
                "commerce" => new DomainProfile(
                    title,
                    new[] { "Financial awareness", "Organization", "Business acumen" },
                    new[] { "Accounting basics", "Marketing principles", "Finance modeling", "Market research", "Entrepreneurial thinking" },
                    "₹4,00,000 - ₹9,00,000 per year",
                    "$6,000 - $12,000 per year",
                    "+15% demand by 2030",
                    BuildSalaryTrend(4, 5, 6, 8, 11),
                    new[] { "Learn business fundamentals", "Practice budgeting and marketing plans", "Participate in entrepreneurship clubs" },
                    new[] { "Fundamentals of Finance", "Digital Marketing Basics", "Accounting certificates" },
                    "1-2 years"
                ),
                "arts_humanities" => new DomainProfile(
                    title,
                    new[] { "Communication", "Empathy", "Critical thinking" },
                    new[] { "Writing skills", "Cultural literacy", "Research", "Storytelling", "Society analysis" },
                    "₹3,00,000 - ₹7,00,000 per year",
                    "$5,000 - $10,000 per year",
                    "+12% demand by 2030",
                    BuildSalaryTrend(3, 4, 5, 6, 8),
                    new[] { "Build writing and communication practice", "Engage with social projects", "Explore humanities workshops" },
                    new[] { "Communication courses", "Creative writing", "Cultural studies programs" },
                    "1-2 years"
                ),
                "vocational_iti" => new DomainProfile(
                    title,
                    new[] { "Practical skills", "Hands-on focus", "Reliability" },
                    new[] { "Technical workshops", "Repair skills", "Machine handling", "Skilled craftsmanship", "Applied training" },
                    "₹2,50,000 - ₹5,00,000 per year",
                    "$4,000 - $8,000 per year",
                    "+14% demand by 2030",
                    BuildSalaryTrend(3, 3, 4, 4, 5),
                    new[] { "Join a trade or skill training program", "Practice with real equipment", "Build a portfolio of practical work" },
                    new[] { "Industrial training certificates", "Apprenticeship programs", "Technical practice courses" },
                    "1-2 years"
                ),
                "polytechnic" => new DomainProfile(
                    title,
                    new[] { "Applied engineering", "Project focus", "Technical design" },
                    new[] { "Technical drawing", "Systems projects", "Workshop practice", "Mechatronics basics", "Practical problem solving" },
                    "₹3,00,000 - ₹7,00,000 per year",
                    "$5,000 - $11,000 per year",
                    "+15% demand by 2030",
                    BuildSalaryTrend(3, 4, 5, 6, 9),
                    new[] { "Explore polytechnic diploma courses", "Build technical prototypes", "Intern at a manufacturing or service unit" },
                    new[] { "Diploma workshops", "Applied engineering courses", "Project practice certifications" },
                    "1-3 years"
                ),
                "btech_cs" => new DomainProfile(
                    title,
                    new[] { "Software development", "Analytical thinking", "Problem solving" },
                    new[] { "Programming", "Algorithms", "Data structures", "System design", "Software architecture" },
                    "₹6,00,000 - ₹16,00,000 per year",
                    "$10,000 - $22,000 per year",
                    "+30% demand by 2030",
                    BuildSalaryTrend(6, 8, 11, 13, 16),
                    new[] { "Practice coding projects", "Prepare computer science foundations", "Build a strong programming portfolio" },
                    new[] { "CS fundamentals", "Full-stack project work", "Cloud computing basics" },
                    "2-4 years"
                ),
                "btech_core" => new DomainProfile(
                    title,
                    new[] { "Technical rigor", "Engineering fundamentals", "System thinking" },
                    new[] { "Mechanical systems", "Electrical principles", "Civil design", "Engineering math", "Technical analysis" },
                    "₹5,00,000 - ₹12,00,000 per year",
                    "$8,000 - $18,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(5, 6, 8, 10, 12),
                    new[] { "Study core engineering subjects", "Join lab or design projects", "Explore internships in manufacturing or infrastructure" },
                    new[] { "Engineering fundamentals", "Project engineering courses", "Technical training certificates" },
                    "2-4 years"
                ),
                "mbbs_medical" => new DomainProfile(
                    title,
                    new[] { "Care", "Scientific precision", "Resilience" },
                    new[] { "Medical knowledge", "Clinical skills", "Patient care", "Research awareness", "Ethics" },
                    "₹7,00,000 - ₹18,00,000 per year",
                    "$12,000 - $25,000 per year",
                    "+22% demand by 2030",
                    BuildSalaryTrend(7, 9, 11, 14, 18),
                    new[] { "Focus on medical science preparation", "Join relevant internships or volunteering", "Practice human biology and patient communication" },
                    new[] { "Medical entrance coaching", "Clinical exposure programs", "Health science workshops" },
                    "5-8 years"
                ),
                "bpharm_nursing" => new DomainProfile(
                    title,
                    new[] { "Care orientation", "Health science knowledge", "Teamwork" },
                    new[] { "Pharmacy principles", "Nursing care", "Clinical coordination", "Health compliance", "Patient support" },
                    "₹4,00,000 - ₹10,00,000 per year",
                    "$7,000 - $14,000 per year",
                    "+20% demand by 2030",
                    BuildSalaryTrend(4, 5, 7, 9, 11),
                    new[] { "Explore allied health programs", "Gain practical clinical exposure", "Complete healthcare certifications" },
                    new[] { "Clinical certificates", "Hospital training", "Pharmacy basics" },
                    "3-5 years"
                ),
                "bcom_bba" => new DomainProfile(
                    title,
                    new[] { "Business understanding", "Financial literacy", "Organization" },
                    new[] { "Business fundamentals", "Accounting", "Marketing", "Management", "Strategy" },
                    "₹4,00,000 - ₹10,00,000 per year",
                    "$7,000 - $14,000 per year",
                    "+15% demand by 2030",
                    BuildSalaryTrend(4, 5, 6, 8, 10),
                    new[] { "Join commerce workshops", "Build business planning skills", "Track financial news and case studies" },
                    new[] { "Business administration", "Finance certificates", "Marketing programs" },
                    "1-3 years"
                ),
                "ba_humanities" => new DomainProfile(
                    title,
                    new[] { "Communication", "Analysis", "Empathy" },
                    new[] { "Writing", "Research", "Cultural insight", "Critical thinking", "Presentation" },
                    "₹3,00,000 - ₹8,00,000 per year",
                    "$5,000 - $11,000 per year",
                    "+12% demand by 2030",
                    BuildSalaryTrend(3, 4, 5, 6, 9),
                    new[] { "Practice writing and research", "Engage with social issues", "Build a portfolio of communication work" },
                    new[] { "Communication workshops", "Creative writing", "Public policy courses" },
                    "1-3 years"
                ),
                "ba_llb" => new DomainProfile(
                    title,
                    new[] { "Logical analysis", "Advocacy", "Attention to detail" },
                    new[] { "Legal research", "Argumentation", "Case study analysis", "Communication", "Ethics" },
                    "₹5,00,000 - ₹13,00,000 per year",
                    "$8,000 - $19,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(5, 6, 8, 10, 13),
                    new[] { "Prepare for legal entrance exams", "Study law fundamentals", "Participate in debate or moot courts" },
                    new[] { "Law prep programs", "Legal writing courses", "Advocacy workshops" },
                    "3-5 years"
                ),
                "bdes_design" => new DomainProfile(
                    title,
                    new[] { "Creativity", "Visual thinking", "User empathy" },
                    new[] { "Design thinking", "Visual communication", "Prototyping", "User research", "Creative systems" },
                    "₹4,00,000 - ₹12,00,000 per year",
                    "$7,000 - $16,000 per year",
                    "+19% demand by 2030",
                    BuildSalaryTrend(4, 5, 7, 10, 14),
                    new[] { "Build a design portfolio", "Practice prototypes", "Study user experience principles" },
                    new[] { "Design thinking", "UI/UX certificates", "Creative tools training" },
                    "1-3 years"
                ),
                "bsc_pure" => new DomainProfile(
                    title,
                    new[] { "Scientific curiosity", "Precision", "Research mindset" },
                    new[] { "Theory", "Lab work", "Experimental design", "Scientific analysis", "Data interpretation" },
                    "₹4,00,000 - ₹11,00,000 per year",
                    "$7,000 - $15,000 per year",
                    "+16% demand by 2030",
                    BuildSalaryTrend(4, 5, 6, 8, 11),
                    new[] { "Engage with laboratory research", "Join science projects", "Study advanced concepts deeply" },
                    new[] { "Research methods", "Technical science certificates", "Lab internships" },
                    "1-4 years"
                ),
                "bsc_cs_data" => new DomainProfile(
                    title,
                    new[] { "Analytical thinking", "Data fluency", "Coding interest" },
                    new[] { "Statistics", "Programming", "Data visualization", "Machine learning basics", "Database skills" },
                    "₹5,00,000 - ₹14,00,000 per year",
                    "$9,000 - $20,000 per year",
                    "+28% demand by 2030",
                    BuildSalaryTrend(5, 7, 9, 11, 14),
                    new[] { "Build data projects", "Learn statistical tools", "Practice coding for analytics" },
                    new[] { "Analytics courses", "Data science certificates", "SQL and Python training" },
                    "1-3 years"
                ),
                "frontend" => new DomainProfile(
                    title,
                    new[] { "Visual craftsmanship", "User empathy", "Iterative design" },
                    new[] { "HTML/CSS", "JavaScript", "UX principles", "Responsive design", "Interaction patterns" },
                    "₹6,00,000 - ₹18,00,000 per year",
                    "$10,000 - $25,000 per year",
                    "+25% demand by 2030",
                    BuildSalaryTrend(6, 8, 11, 14, 18),
                    new[] { "Build interface prototypes", "Learn modern frontend frameworks", "Create live web experiences" },
                    new[] { "Frontend bootcamps", "UI/UX certificates", "JavaScript frameworks training" },
                    "1-2 years"
                ),
                "backend" => new DomainProfile(
                    title,
                    new[] { "System thinking", "Reliability", "Logical design" },
                    new[] { "APIs", "Databases", "Server architecture", "Security", "Performance" },
                    "₹7,00,000 - ₹20,00,000 per year",
                    "$11,000 - $28,000 per year",
                    "+24% demand by 2030",
                    BuildSalaryTrend(7, 9, 12, 15, 20),
                    new[] { "Learn backend frameworks", "Build API-driven projects", "Study database design" },
                    new[] { "Backend development", "Database courses", "Cloud computing training" },
                    "1-3 years"
                ),
                "fullstack" => new DomainProfile(
                    title,
                    new[] { "Versatility", "Coordination", "Product focus" },
                    new[] { "Frontend", "Backend", "APIs", "UX", "Deployment" },
                    "₹8,00,000 - ₹22,00,000 per year",
                    "$12,000 - $30,000 per year",
                    "+26% demand by 2030",
                    BuildSalaryTrend(8, 10, 13, 17, 22),
                    new[] { "Build end-to-end applications", "Practice both backend and frontend skills", "Focus on product delivery" },
                    new[] { "Full-stack programs", "Web development certificates", "Cloud integration courses" },
                    "1-3 years"
                ),
                "data" => new DomainProfile(
                    title,
                    new[] { "Insight generation", "Curiosity", "Storytelling" },
                    new[] { "Data analysis", "Dashboards", "Business intelligence", "SQL", "Presentation" },
                    "₹8,00,000 - ₹22,00,000 per year",
                    "$12,000 - $30,000 per year",
                    "+30% demand by 2030",
                    BuildSalaryTrend(8, 10, 14, 18, 22),
                    new[] { "Build analytics reports", "Practice visualization tools", "Learn business intelligence systems" },
                    new[] { "Data analytics courses", "BI tool certifications", "SQL training" },
                    "1-3 years"
                ),
                "ml" => new DomainProfile(
                    title,
                    new[] { "Analytical creativity", "Math fluency", "Experimentation" },
                    new[] { "Machine learning", "Python", "Statistics", "Model training", "AI systems" },
                    "₹10,00,000 - ₹28,00,000 per year",
                    "$15,000 - $40,000 per year",
                    "+34% demand by 2030",
                    BuildSalaryTrend(10, 12, 16, 21, 28),
                    new[] { "Practice ML projects", "Study AI models", "Build a portfolio with machine learning examples" },
                    new[] { "AI/ML certifications", "Python for data science", "Model deployment training" },
                    "1-4 years"
                ),
                "devops" => new DomainProfile(
                    title,
                    new[] { "Automation", "Reliability", "Operational thinking" },
                    new[] { "CI/CD", "Cloud", "Monitoring", "Containers", "Infrastructure" },
                    "₹8,00,000 - ₹22,00,000 per year",
                    "$12,000 - $32,000 per year",
                    "+27% demand by 2030",
                    BuildSalaryTrend(8, 10, 14, 18, 22),
                    new[] { "Build deployment pipelines", "Learn cloud automation", "Practice infrastructure-as-code" },
                    new[] { "DevOps certifications", "Cloud platform training", "Automation courses" },
                    "1-3 years"
                ),
                "business" => new DomainProfile(
                    title,
                    new[] { "Strategic thinking", "Commercial insight", "Team coordination" },
                    new[] { "Business strategy", "Operations", "Market analysis", "Stakeholder communication", "Planning" },
                    "₹7,00,000 - ₹18,00,000 per year",
                    "$11,000 - $24,000 per year",
                    "+20% demand by 2030",
                    BuildSalaryTrend(7, 9, 12, 15, 18),
                    new[] { "Practice business analysis", "Study strategy frameworks", "Work with cross-functional teams" },
                    new[] { "Business strategy courses", "MBA prep certificates", "Operations training" },
                    "1-3 years"
                ),
                "design" => new DomainProfile(
                    title,
                    new[] { "Empathy", "Visual communication", "Creative problem solving" },
                    new[] { "UX research", "Prototyping", "Interaction design", "Visual systems", "User journeys" },
                    "₹6,00,000 - ₹16,00,000 per year",
                    "$10,000 - $22,000 per year",
                    "+19% demand by 2030",
                    BuildSalaryTrend(6, 8, 10, 13, 16),
                    new[] { "Create a design portfolio", "Practice prototyping tools", "Study user research methods" },
                    new[] { "UX/UI courses", "Design thinking certificates", "Interaction design programs" },
                    "1-3 years"
                ),
                "healthcare" => new DomainProfile(
                    title,
                    new[] { "Care coordination", "System awareness", "Compassion" },
                    new[] { "Healthcare operations", "Patient experience", "Data management", "Compliance", "Process improvement" },
                    "₹6,00,000 - ₹15,00,000 per year",
                    "$9,000 - $20,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(6, 8, 10, 12, 15),
                    new[] { "Learn healthcare workflows", "Study hospital systems", "Build patient-first planning skills" },
                    new[] { "Healthcare management courses", "Quality improvement certificates", "Medical administration training" },
                    "1-3 years"
                ),
                "education" => new DomainProfile(
                    title,
                    new[] { "Instructional design", "Mentorship", "Learning strategy" },
                    new[] { "Curriculum design", "Teaching methods", "Educational technology", "Assessment", "Learner engagement" },
                    "₹4,00,000 - ₹12,00,000 per year",
                    "$6,000 - $18,000 per year",
                    "+14% demand by 2030",
                    BuildSalaryTrend(4, 6, 8, 10, 12),
                    new[] { "Practice curriculum design", "Work on tutoring or training programs", "Study learning science" },
                    new[] { "Instructional design certifications", "Education technology courses", "Teaching skill programs" },
                    "1-3 years"
                ),
                "phd_research" => new DomainProfile(
                    title,
                    new[] { "Deep research", "Persistence", "Expertise" },
                    new[] { "Research methods", "Academic writing", "Grant proposals", "Data analysis", "Critical thinking" },
                    "₹9,00,000 - ₹25,00,000 per year",
                    "$13,000 - $35,000 per year",
                    "+12% demand by 2030",
                    BuildSalaryTrend(9, 11, 14, 18, 25),
                    new[] { "Apply for funded research programs", "Publish technical work", "Partner with academic advisors" },
                    new[] { "Research internships", "PhD prep workshops", "Academic writing courses" },
                    "3-6 years"
                ),
                "industry_expert" => new DomainProfile(
                    title,
                    new[] { "Expert judgment", "Business insight", "Consultative influence" },
                    new[] { "Industry trends", "Cross-functional guidance", "Decision support", "Technical leadership", "Mentoring" },
                    "₹10,00,000 - ₹30,00,000 per year",
                    "$15,000 - $45,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(10, 13, 17, 22, 30),
                    new[] { "Build deep domain knowledge", "Work on high-impact projects", "Develop a reputation for expertise" },
                    new[] { "Leadership programs", "Industry certifications", "Advanced domain training" },
                    "2-5 years"
                ),
                "consulting" => new DomainProfile(
                    title,
                    new[] { "Problem solving", "Communication", "Client focus" },
                    new[] { "Issue diagnosis", "Strategy recommendation", "Stakeholder alignment", "Presentation", "Process design" },
                    "₹10,00,000 - ₹26,00,000 per year",
                    "$15,000 - $40,000 per year",
                    "+22% demand by 2030",
                    BuildSalaryTrend(10, 13, 16, 20, 26),
                    new[] { "Practice structured problem solving", "Learn consulting frameworks", "Work on client-like projects" },
                    new[] { "Consulting courses", "Strategy certifications", "Business analysis programs" },
                    "1-4 years"
                ),
                "product_mgmt" => new DomainProfile(
                    title,
                    new[] { "Prioritization", "Customer empathy", "Execution" },
                    new[] { "Roadmap planning", "Customer research", "Metric tracking", "Stakeholder communication", "Feature definition" },
                    "₹9,00,000 - ₹24,00,000 per year",
                    "$13,000 - $35,000 per year",
                    "+21% demand by 2030",
                    BuildSalaryTrend(9, 12, 15, 19, 24),
                    new[] { "Build product specs", "Learn user research methods", "Practice stakeholder alignment" },
                    new[] { "Product management certificates", "Customer research courses", "Strategy workshops" },
                    "1-3 years"
                ),
                "startup_founder" => new DomainProfile(
                    title,
                    new[] { "Initiative", "Resilience", "Strategic risk taking" },
                    new[] { "Business modeling", "Customer discovery", "Launch execution", "Fundraising", "Growth strategy" },
                    "₹8,00,000 - ₹22,00,000 per year",
                    "$12,000 - $34,000 per year",
                    "+25% demand by 2030",
                    BuildSalaryTrend(8, 11, 15, 19, 22),
                    new[] { "Validate an idea with customers", "Build a minimum viable product", "Learn startup business models" },
                    new[] { "Entrepreneurship courses", "Lean startup workshops", "Market validation training" },
                    "1-3 years"
                ),
                "govt_policy" => new DomainProfile(
                    title,
                    new[] { "Policy awareness", "Systems thinking", "Public impact" },
                    new[] { "Regulatory analysis", "Program design", "Stakeholder engagement", "Public communication", "Ethics" },
                    "₹7,00,000 - ₹18,00,000 per year",
                    "$10,000 - $24,000 per year",
                    "+16% demand by 2030",
                    BuildSalaryTrend(7, 9, 12, 15, 18),
                    new[] { "Study public policy fundamentals", "Follow government programs", "Attend policy workshops" },
                    new[] { "Public policy courses", "Governance programs", "Social impact training" },
                    "1-4 years"
                ),
                "academia_teaching" => new DomainProfile(
                    title,
                    new[] { "Teaching ability", "Research support", "Mentorship" },
                    new[] { "Curriculum development", "Instructional methods", "Academic coordination", "Mentoring", "Research assistance" },
                    "₹5,00,000 - ₹14,00,000 per year",
                    "$8,000 - $20,000 per year",
                    "+13% demand by 2030",
                    BuildSalaryTrend(5, 7, 9, 11, 14),
                    new[] { "Teach or tutor learners", "Develop lesson plans", "Study pedagogy best practices" },
                    new[] { "Teaching certifications", "Educational design courses", "Academic support programs" },
                    "1-3 years"
                ),
                "frontend_switch" => new DomainProfile(
                    title,
                    new[] { "Visual thinking", "Hands-on coding", "User focus" },
                    new[] { "HTML/CSS", "JavaScript", "UI patterns", "Responsive design", "Design systems" },
                    "₹8,00,000 - ₹20,00,000 per year",
                    "$12,000 - $28,000 per year",
                    "+24% demand by 2030",
                    BuildSalaryTrend(8, 10, 13, 16, 20),
                    new[] { "Build frontend projects", "Learn a modern UI framework", "Practice responsive design" },
                    new[] { "Frontend bootcamps", "UI/UX training", "JavaScript certification" },
                    "1-2 years"
                ),
                "backend_switch" => new DomainProfile(
                    title,
                    new[] { "Logical depth", "Systems thinking", "Reliability" },
                    new[] { "APIs", "Databases", "Server code", "Security", "Scalability" },
                    "₹9,00,000 - ₹22,00,000 per year",
                    "$13,000 - $30,000 per year",
                    "+23% demand by 2030",
                    BuildSalaryTrend(9, 11, 14, 18, 22),
                    new[] { "Build backend services", "Learn database design", "Practice API development" },
                    new[] { "Backend courses", "Database certificates", "Cloud fundamentals" },
                    "1-3 years"
                ),
                "data_analyst" => new DomainProfile(
                    title,
                    new[] { "Insight focus", "Curiosity", "Storytelling" },
                    new[] { "SQL", "Excel", "Visualization", "Analytics", "Reporting" },
                    "₹8,00,000 - ₹20,00,000 per year",
                    "$12,000 - $28,000 per year",
                    "+28% demand by 2030",
                    BuildSalaryTrend(8, 10, 13, 17, 20),
                    new[] { "Build dashboards", "Analyze datasets", "Present findings clearly" },
                    new[] { "Data analytics certifications", "SQL and BI courses", "Visualization training" },
                    "1-2 years"
                ),
                "ml_engineer" => new DomainProfile(
                    title,
                    new[] { "Model thinking", "Experimentation", "Technical depth" },
                    new[] { "Machine learning", "Python", "Data pipelines", "Model deployment", "AI systems" },
                    "₹10,00,000 - ₹26,00,000 per year",
                    "$15,000 - $38,000 per year",
                    "+32% demand by 2030",
                    BuildSalaryTrend(10, 12, 15, 20, 26),
                    new[] { "Build ML prototypes", "Study model deployment", "Practice data science workflows" },
                    new[] { "AI certifications", "Machine learning programs", "Model deployment courses" },
                    "1-3 years"
                ),
                "devops_switch" => new DomainProfile(
                    title,
                    new[] { "Automation", "Reliability", "Cross-team collaboration" },
                    new[] { "CI/CD", "Infrastructure", "Monitoring", "Containers", "Cloud" },
                    "₹9,00,000 - ₹23,00,000 per year",
                    "$13,000 - $32,000 per year",
                    "+26% demand by 2030",
                    BuildSalaryTrend(9, 11, 15, 19, 23),
                    new[] { "Build deployment pipelines", "Learn cloud tools", "Automate workflows" },
                    new[] { "DevOps certificates", "Cloud certifications", "Automation courses" },
                    "1-3 years"
                ),
                "product_manager" => new DomainProfile(
                    title,
                    new[] { "Leadership", "Customer focus", "Decision making" },
                    new[] { "Roadmap planning", "Customer research", "Product metrics", "Stakeholder communication", "Prioritization" },
                    "₹9,00,000 - ₹24,00,000 per year",
                    "$13,000 - $35,000 per year",
                    "+22% demand by 2030",
                    BuildSalaryTrend(9, 12, 16, 20, 24),
                    new[] { "Practice product discovery", "Learn customer research", "Build a product case study" },
                    new[] { "Product management courses", "Leadership certifications", "User research training" },
                    "1-3 years"
                ),
                "ux_designer" => new DomainProfile(
                    title,
                    new[] { "Empathy", "User research", "Creative execution" },
                    new[] { "UX research", "Wireframing", "Prototyping", "Usability testing", "Interaction design" },
                    "₹7,00,000 - ₹18,00,000 per year",
                    "$10,000 - $24,000 per year",
                    "+20% demand by 2030",
                    BuildSalaryTrend(7, 9, 12, 15, 18),
                    new[] { "Build UX case studies", "Practice research interviews", "Design polished interfaces" },
                    new[] { "UX/UI certificates", "Design tools courses", "User research training" },
                    "1-3 years"
                ),
                "biz_analyst" => new DomainProfile(
                    title,
                    new[] { "Business sense", "Data awareness", "Problem solving" },
                    new[] { "Process analysis", "Requirements gathering", "Data interpretation", "Stakeholder communication", "Strategy" },
                    "₹8,00,000 - ₹20,00,000 per year",
                    "$12,000 - $28,000 per year",
                    "+21% demand by 2030",
                    BuildSalaryTrend(8, 10, 13, 17, 20),
                    new[] { "Work on business cases", "Learn analytics tools", "Practice stakeholder interviews" },
                    new[] { "Business analysis programs", "Data and process courses", "Requirements workshops" },
                    "1-3 years"
                ),
                "tech_sales" => new DomainProfile(
                    title,
                    new[] { "Communication", "Persuasion", "Product knowledge" },
                    new[] { "Product demos", "Customer relationships", "Sales strategy", "Market understanding", "Negotiation" },
                    "₹6,00,000 - ₹16,00,000 per year",
                    "$9,000 - $22,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(6, 8, 11, 14, 16),
                    new[] { "Learn product positioning", "Practice sales conversations", "Study technology benefits" },
                    new[] { "Sales training", "Technical sales programs", "Customer engagement courses" },
                    "1-2 years"
                ),
                "edtech_creator" => new DomainProfile(
                    title,
                    new[] { "Teaching creativity", "Content planning", "Digital communication" },
                    new[] { "Course design", "Video creation", "Instructional content", "Learning platforms", "Community building" },
                    "₹5,00,000 - ₹14,00,000 per year",
                    "$8,000 - $20,000 per year",
                    "+19% demand by 2030",
                    BuildSalaryTrend(5, 7, 10, 13, 14),
                    new[] { "Create learning content", "Publish short courses", "Build an audience" },
                    new[] { "Instructional design courses", "Content creation training", "EdTech workshops" },
                    "1-2 years"
                ),
                "ai_ml_certs" => new DomainProfile(
                    title,
                    new[] { "Analytical thinking", "Machine learning fundamentals", "Experimentation" },
                    new[] { "ML models", "Python", "Data pipelines", "Model evaluation", "AI concepts" },
                    "₹9,00,000 - ₹24,00,000 per year",
                    "$13,000 - $35,000 per year",
                    "+32% demand by 2030",
                    BuildSalaryTrend(9, 11, 14, 18, 24),
                    new[] { "Complete AI certification programs", "Work on ML case studies", "Practice applying models" },
                    new[] { "AI/ML certificates", "Data science courses", "Model deployment training" },
                    "1-2 years"
                ),
                "cloud_aws" => new DomainProfile(
                    title,
                    new[] { "Cloud infrastructure", "Automation", "Scalability" },
                    new[] { "AWS services", "Cloud deployment", "IaC", "Monitoring", "Security" },
                    "₹8,00,000 - ₹22,00,000 per year",
                    "$12,000 - $30,000 per year",
                    "+28% demand by 2030",
                    BuildSalaryTrend(8, 10, 14, 18, 22),
                    new[] { "Earn AWS certifications", "Practice cloud deployments", "Automate infrastructure" },
                    new[] { "AWS Certified Cloud Practitioner", "AWS Solutions Architect", "Cloud operations courses" },
                    "1-3 years"
                ),
                "cloud_azure" => new DomainProfile(
                    title,
                    new[] { "Enterprise cloud", "Platform services", "Governance" },
                    new[] { "Azure services", "Cloud management", "Security", "IaC", "DevOps" },
                    "₹8,00,000 - ₹22,00,000 per year",
                    "$12,000 - $30,000 per year",
                    "+27% demand by 2030",
                    BuildSalaryTrend(8, 10, 14, 18, 22),
                    new[] { "Earn Azure certifications", "Practice enterprise cloud projects", "Learn governance and security" },
                    new[] { "Azure Fundamentals", "Azure Administrator", "Cloud operations training" },
                    "1-3 years"
                ),
                "cloud_gcp" => new DomainProfile(
                    title,
                    new[] { "Cloud analytics", "Platform innovation", "Automation" },
                    new[] { "GCP services", "Data pipelines", "Serverless architecture", "Monitoring", "APIs" },
                    "₹8,00,000 - ₹22,00,000 per year",
                    "$12,000 - $30,000 per year",
                    "+27% demand by 2030",
                    BuildSalaryTrend(8, 10, 14, 18, 22),
                    new[] { "Earn GCP certifications", "Build cloud analytics projects", "Practice serverless architecture" },
                    new[] { "Google Cloud certificates", "Cloud data engineering courses", "Infrastructure training" },
                    "1-3 years"
                ),
                "cybersecurity" => new DomainProfile(
                    title,
                    new[] { "Security awareness", "Risk management", "Attention to detail" },
                    new[] { "Network security", "Threat detection", "Incident response", "Compliance", "Encryption" },
                    "₹8,00,000 - ₹20,00,000 per year",
                    "$12,000 - $28,000 per year",
                    "+30% demand by 2030",
                    BuildSalaryTrend(8, 10, 14, 18, 20),
                    new[] { "Study cybersecurity fundamentals", "Practice vulnerability analysis", "Complete security labs" },
                    new[] { "Security certifications", "Ethical hacking courses", "Security operations training" },
                    "1-3 years"
                ),
                "data_analytics" => new DomainProfile(
                    title,
                    new[] { "Data literacy", "Insight generation", "Communication" },
                    new[] { "Analytics tools", "Data storytelling", "Visualization", "SQL", "Business metrics" },
                    "₹8,00,000 - ₹21,00,000 per year",
                    "$12,000 - $29,000 per year",
                    "+29% demand by 2030",
                    BuildSalaryTrend(8, 10, 14, 18, 21),
                    new[] { "Complete analytics certifications", "Build reporting dashboards", "Practice story-driven data work" },
                    new[] { "Data analytics certificates", "BI training", "Visualization courses" },
                    "1-2 years"
                ),
                "fullstack_web" => new DomainProfile(
                    title,
                    new[] { "Versatility", "Delivery orientation", "Product focus" },
                    new[] { "Frontend", "Backend", "APIs", "Databases", "Deployment" },
                    "₹8,00,000 - ₹23,00,000 per year",
                    "$12,000 - $31,000 per year",
                    "+26% demand by 2030",
                    BuildSalaryTrend(8, 11, 15, 19, 23),
                    new[] { "Build full-stack applications", "Learn both user-facing and server-side skills", "Use cloud deployment tools" },
                    new[] { "Full-stack certificates", "Web development programs", "Cloud integration courses" },
                    "1-3 years"
                ),
                "devops_certs" => new DomainProfile(
                    title,
                    new[] { "Automation expertise", "Operational maturity", "Reliability" },
                    new[] { "CI/CD", "Infrastructure as code", "Monitoring", "Cloud tooling", "Release management" },
                    "₹9,00,000 - ₹23,00,000 per year",
                    "$13,000 - $32,000 per year",
                    "+27% demand by 2030",
                    BuildSalaryTrend(9, 11, 15, 19, 23),
                    new[] { "Earn DevOps certifications", "Build deployment automation", "Monitor production systems" },
                    new[] { "DevOps certificates", "Cloud automation courses", "Infrastructure training" },
                    "1-3 years"
                ),
                "low_code" => new DomainProfile(
                    title,
                    new[] { "Rapid delivery", "Business automation", "Creativity" },
                    new[] { "Low-code tools", "Workflow design", "Integration", "Automation", "Business requirements" },
                    "₹5,00,000 - ₹14,00,000 per year",
                    "$8,000 - $20,000 per year",
                    "+15% demand by 2030",
                    BuildSalaryTrend(5, 7, 10, 12, 14),
                    new[] { "Learn low-code platforms", "Build automation workflows", "Create business applications" },
                    new[] { "Low-code training", "Automation certificates", "Business app courses" },
                    "1-2 years"
                ),
                "saas_b2b" => new DomainProfile(
                    title,
                    new[] { "Product thinking", "Customer focus", "Scalability" },
                    new[] { "B2B sales", "Product development", "Subscription economics", "Market research", "Customer success" },
                    "₹10,00,000 - ₹30,00,000 per year",
                    "$15,000 - $45,000 per year",
                    "+28% demand by 2030",
                    BuildSalaryTrend(10, 13, 18, 24, 30),
                    new[] { "Validate a business problem with customers", "Design a subscription product", "Build early B2B traction" },
                    new[] { "SaaS founding courses", "B2B startup training", "Business model workshops" },
                    "1-3 years"
                ),
                "d2c_consumer" => new DomainProfile(
                    title,
                    new[] { "Consumer insight", "Brand building", "Marketing" },
                    new[] { "Product design", "Customer experience", "Digital marketing", "Supply chain", "Retail strategy" },
                    "₹7,00,000 - ₹20,00,000 per year",
                    "$10,000 - $28,000 per year",
                    "+24% demand by 2030",
                    BuildSalaryTrend(7, 10, 14, 18, 20),
                    new[] { "Research consumer preferences", "Build a marketing plan", "Test product-market fit" },
                    new[] { "Brand building courses", "E-commerce programs", "Digital marketing certificates" },
                    "1-3 years"
                ),
                "deep_tech" => new DomainProfile(
                    title,
                    new[] { "Technical innovation", "Research rigor", "Long-term vision" },
                    new[] { "Advanced engineering", "R&D", "Product validation", "Science commercialization", "IP strategy" },
                    "₹9,00,000 - ₹28,00,000 per year",
                    "$13,000 - $40,000 per year",
                    "+30% demand by 2030",
                    BuildSalaryTrend(9, 13, 18, 24, 28),
                    new[] { "Validate a breakthrough idea", "Partner with research experts", "Prototype a technical solution" },
                    new[] { "Deep tech workshops", "Innovation programs", "Technical entrepreneurship courses" },
                    "1-4 years"
                ),
                "marketplace" => new DomainProfile(
                    title,
                    new[] { "Platform design", "Network thinking", "Growth" },
                    new[] { "Marketplace modeling", "User acquisition", "Trust building", "Transaction design", "Analytics" },
                    "₹8,00,000 - ₹24,00,000 per year",
                    "$12,000 - $34,000 per year",
                    "+26% demand by 2030",
                    BuildSalaryTrend(8, 12, 16, 20, 24),
                    new[] { "Understand supply-demand dynamics", "Design a marketplace flow", "Test network incentives" },
                    new[] { "Platform strategy courses", "Marketplace training", "Growth programs" },
                    "1-3 years"
                ),
                "edtech_startup" => new DomainProfile(
                    title,
                    new[] { "Learning experience", "Impact focus", "Product thinking" },
                    new[] { "Education product design", "Content development", "Customer research", "Learning outcomes", "Engagement" },
                    "₹7,00,000 - ₹20,00,000 per year",
                    "$10,000 - $28,000 per year",
                    "+25% demand by 2030",
                    BuildSalaryTrend(7, 10, 14, 18, 20),
                    new[] { "Build learning content with validation", "Study education technology trends", "Pilot a learning product" },
                    new[] { "EdTech incubator programs", "Instructional design courses", "Education startup workshops" },
                    "1-3 years"
                ),
                "fintech_startup" => new DomainProfile(
                    title,
                    new[] { "Financial innovation", "Compliance awareness", "Customer trust" },
                    new[] { "Payments", "Regtech", "Personal finance", "Digital banking", "Scaling" },
                    "₹9,00,000 - ₹26,00,000 per year",
                    "$13,000 - $38,000 per year",
                    "+27% demand by 2030",
                    BuildSalaryTrend(9, 12, 17, 22, 26),
                    new[] { "Validate a fintech use case", "Learn compliance basics", "Build a secure MVP" },
                    new[] { "FinTech courses", "Payments training", "Regtech programs" },
                    "1-3 years"
                ),
                "social_enterprise" => new DomainProfile(
                    title,
                    new[] { "Impact focus", "Sustainability", "Community orientation" },
                    new[] { "Social mission", "Impact measurement", "Community outreach", "Sustainable models", "Partnerships" },
                    "₹6,00,000 - ₹16,00,000 per year",
                    "$9,000 - $22,000 per year",
                    "+19% demand by 2030",
                    BuildSalaryTrend(6, 8, 11, 14, 16),
                    new[] { "Define a social mission clearly", "Test a sustainable model", "Engage with beneficiaries" },
                    new[] { "Social entrepreneurship courses", "Impact strategy programs", "Sustainability certificates" },
                    "1-3 years"
                ),
                "consulting_firm" => new DomainProfile(
                    title,
                    new[] { "Advisory mindset", "Analytical rigor", "Client service" },
                    new[] { "Strategy", "Operations", "Stakeholder engagement", "Problem solving", "Deliverables" },
                    "₹8,00,000 - ₹24,00,000 per year",
                    "$12,000 - $34,000 per year",
                    "+23% demand by 2030",
                    BuildSalaryTrend(8, 11, 15, 19, 24),
                    new[] { "Learn consulting frameworks", "Practice client communication", "Work on problem statements" },
                    new[] { "Consulting training", "Strategy workshops", "Business analysis programs" },
                    "1-3 years"
                ),
                "content_creator" => new DomainProfile(
                    title,
                    new[] { "Storytelling", "Creativity", "Audience focus" },
                    new[] { "Video production", "Content planning", "Community building", "Digital marketing", "Platform strategy" },
                    "₹5,00,000 - ₹14,00,000 per year",
                    "$8,000 - $20,000 per year",
                    "+20% demand by 2030",
                    BuildSalaryTrend(5, 8, 11, 14, 16),
                    new[] { "Create original content consistently", "Build a niche audience", "Learn creator monetization" },
                    new[] { "Content creation courses", "Media strategy training", "Digital storytelling programs" },
                    "1-2 years"
                ),
                "franchise_retail" => new DomainProfile(
                    title,
                    new[] { "Operational discipline", "Customer service", "Business consistency" },
                    new[] { "Retail operations", "Brand standards", "Customer experience", "Inventory management", "Team leadership" },
                    "₹6,00,000 - ₹16,00,000 per year",
                    "$9,000 - $22,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(6, 8, 11, 14, 16),
                    new[] { "Study franchise models", "Practice store operations", "Focus on service standards" },
                    new[] { "Retail management courses", "Franchise programs", "Customer service training" },
                    "1-3 years"
                ),
                "ias_upsc" => new DomainProfile(
                    title,
                    new[] { "Public service", "Policy understanding", "Leadership" },
                    new[] { "Governance", "Constitution", "Current affairs", "Ethics", "Administration" },
                    "₹9,00,000 - ₹22,00,000 per year",
                    "$13,000 - $32,000 per year",
                    "+14% demand by 2030",
                    BuildSalaryTrend(9, 11, 14, 18, 22),
                    new[] { "Prepare for UPSC syllabus", "Practice essay and interview skills", "Follow public policy closely" },
                    new[] { "Civil services coaching", "Public administration courses", "Current affairs programs" },
                    "2-5 years"
                ),
                "banking_po" => new DomainProfile(
                    title,
                    new[] { "Financial service", "Customer focus", "Discipline" },
                    new[] { "Banking operations", "Credit analysis", "Accounts", "Customer service", "Risk awareness" },
                    "₹6,00,000 - ₹16,00,000 per year",
                    "$9,000 - $22,000 per year",
                    "+12% demand by 2030",
                    BuildSalaryTrend(6, 8, 11, 14, 16),
                    new[] { "Prepare for banking exams", "Learn banking concepts", "Build customer service skills" },
                    new[] { "Banking certifications", "Finance courses", "PO exam coaching" },
                    "1-2 years"
                ),
                "defence_forces" => new DomainProfile(
                    title,
                    new[] { "Discipline", "Leadership", "Service" },
                    new[] { "Physical fitness", "Teamwork", "Security", "Tactical planning", "National service" },
                    "₹5,00,000 - ₹14,00,000 per year",
                    "$8,000 - $18,000 per year",
                    "+10% demand by 2030",
                    BuildSalaryTrend(5, 7, 9, 12, 14),
                    new[] { "Prepare fitness and service requirements", "Learn defense protocols", "Build leadership discipline" },
                    new[] { "Defense preparation programs", "Physical training", "Leadership courses" },
                    "1-3 years"
                ),
                "ssc_cgl" => new DomainProfile(
                    title,
                    new[] { "Government service", "Analytical aptitude", "Stability" },
                    new[] { "General awareness", "Quantitative aptitude", "Reasoning", "English", "Administration" },
                    "₹5,00,000 - ₹13,00,000 per year",
                    "$8,000 - $16,000 per year",
                    "+11% demand by 2030",
                    BuildSalaryTrend(5, 6, 8, 10, 13),
                    new[] { "Study SSC subjects", "Practice mock exams", "Build disciplined preparation habits" },
                    new[] { "SSC coaching", "General studies programs", "Aptitude training" },
                    "1-2 years"
                ),
                "state_psc" => new DomainProfile(
                    title,
                    new[] { "Regional service", "Public administration", "Policy insight" },
                    new[] { "State governance", "Current affairs", "Law", "Administration", "Ethics" },
                    "₹5,00,000 - ₹14,00,000 per year",
                    "$8,000 - $18,000 per year",
                    "+12% demand by 2030",
                    BuildSalaryTrend(5, 7, 9, 12, 14),
                    new[] { "Prepare state PSC syllabus", "Study governance issues", "Practice essay and interview skills" },
                    new[] { "PSC coaching", "Public administration courses", "Regional current affairs programs" },
                    "1-3 years"
                ),
                "judiciary_law" => new DomainProfile(
                    title,
                    new[] { "Legal reasoning", "Justice focus", "Debate" },
                    new[] { "Law", "Court procedure", "Case analysis", "Ethics", "Legal drafting" },
                    "₹6,00,000 - ₹18,00,000 per year",
                    "$9,000 - $24,000 per year",
                    "+13% demand by 2030",
                    BuildSalaryTrend(6, 8, 11, 14, 18),
                    new[] { "Study legal concepts", "Practice courtroom reasoning", "Review precedent cases" },
                    new[] { "Law exam coaching", "Judicial preparation courses", "Legal research training" },
                    "2-4 years"
                ),
                "teaching_govt" => new DomainProfile(
                    title,
                    new[] { "Instruction", "Patience", "Organization" },
                    new[] { "Teaching methods", "Curriculum planning", "Student engagement", "Assessment", "Education policy" },
                    "₹4,00,000 - ₹12,00,000 per year",
                    "$6,000 - $18,000 per year",
                    "+10% demand by 2030",
                    BuildSalaryTrend(4, 6, 8, 10, 12),
                    new[] { "Prepare teaching exams", "Gain teaching practice", "Study education standards" },
                    new[] { "TEACHER eligibility programs", "Education certificates", "Classroom management courses" },
                    "1-2 years"
                ),
                "railways_rrb" => new DomainProfile(
                    title,
                    new[] { "Operations", "Public service", "Technical awareness" },
                    new[] { "Railway systems", "Logistics", "Safety", "Customer service", "Administration" },
                    "₹5,00,000 - ₹13,00,000 per year",
                    "$8,000 - $16,000 per year",
                    "+11% demand by 2030",
                    BuildSalaryTrend(5, 6, 8, 10, 13),
                    new[] { "Study RRB topics", "Practice technical and reasoning tests", "Build a routine around exam prep" },
                    new[] { "RRB coaching", "Railway exam courses", "Public sector training" },
                    "1-2 years"
                ),
                "psu_gate" => new DomainProfile(
                    title,
                    new[] { "Technical strength", "Stability", "Government employment" },
                    new[] { "GATE subjects", "Engineering theory", "Problem solving", "Aptitude", "Public sector roles" },
                    "₹6,00,000 - ₹15,00,000 per year",
                    "$9,000 - $20,000 per year",
                    "+13% demand by 2030",
                    BuildSalaryTrend(6, 8, 10, 12, 15),
                    new[] { "Prepare GATE subjects", "Practice engineering tests", "Explore PSU application paths" },
                    new[] { "GATE coaching", "PSU exam programs", "Engineering aptitude training" },
                    "1-2 years"
                ),
                "police_services" => new DomainProfile(
                    title,
                    new[] { "Protection", "Discipline", "Service" },
                    new[] { "Law enforcement", "Public safety", "Fitness", "Community work", "Ethics" },
                    "₹4,00,000 - ₹12,00,000 per year",
                    "$6,000 - $16,000 per year",
                    "+10% demand by 2030",
                    BuildSalaryTrend(4, 6, 8, 10, 12),
                    new[] { "Prepare physical and knowledge tests", "Understand policing systems", "Build leadership and discipline" },
                    new[] { "Police exam coaching", "Safety training", "Public service courses" },
                    "1-2 years"
                ),
                "ms_usa" => new DomainProfile(
                    title,
                    new[] { "Global exposure", "Research ambition", "Adaptability" },
                    new[] { "Graduate research", "International study", "Networking", "Academic writing", "Visa planning" },
                    "₹9,00,000 - ₹25,00,000 per year",
                    "$13,000 - $40,000 per year",
                    "+22% demand by 2030",
                    BuildSalaryTrend(9, 11, 14, 18, 25),
                    new[] { "Prepare strong applications", "Plan for standardized tests", "Research universities and funding" },
                    new[] { "GRE/TOEFL prep", "Study abroad counseling", "Research proposal workshops" },
                    "1-2 years"
                ),
                "ms_canada" => new DomainProfile(
                    title,
                    new[] { "Work-study balance", "International mobility", "Practical focus" },
                    new[] { "Graduate programs", "Co-op opportunities", "Visa planning", "Research", "Work permits" },
                    "₹8,00,000 - ₹22,00,000 per year",
                    "$12,000 - $34,000 per year",
                    "+20% demand by 2030",
                    BuildSalaryTrend(8, 10, 13, 17, 22),
                    new[] { "Explore Canadian universities", "Check co-op and PR paths", "Prepare application documents" },
                    new[] { "Study abroad programs", "Canadian admissions coaching", "Visa guidance" },
                    "1-2 years"
                ),
                "ms_europe" => new DomainProfile(
                    title,
                    new[] { "Cultural adaptability", "Research interest", "International credentials" },
                    new[] { "Masters study", "Research collaboration", "Scholarship planning", "Language readiness", "Study environment" },
                    "₹7,00,000 - ₹20,00,000 per year",
                    "$10,000 - $30,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(7, 9, 12, 15, 20),
                    new[] { "Identify European programs", "Check funding and visas", "Prepare academic applications" },
                    new[] { "Study in Europe courses", "International admissions training", "Research funding guidance" },
                    "1-2 years"
                ),
                "mba_us_top10" => new DomainProfile(
                    title,
                    new[] { "Leadership potential", "Network building", "Strategic thinking" },
                    new[] { "Business cases", "Leadership development", "Networking", "Strategy", "Global exposure" },
                    "₹12,00,000 - ₹35,00,000 per year",
                    "$18,000 - $55,000 per year",
                    "+21% demand by 2030",
                    BuildSalaryTrend(12, 15, 20, 28, 35),
                    new[] { "Build a strong MBA application", "Show leadership impact", "Prepare GMAT/TOEFL and essays" },
                    new[] { "MBA prep courses", "Leadership programs", "Business school guidance" },
                    "1-2 years"
                ),
                "mba_global" => new DomainProfile(
                    title,
                    new[] { "International perspective", "Business acumen", "Cross-cultural skills" },
                    new[] { "Global management", "Networking", "Business strategy", "Leadership", "Cultural agility" },
                    "₹10,00,000 - ₹30,00,000 per year",
                    "$15,000 - $45,000 per year",
                    "+20% demand by 2030",
                    BuildSalaryTrend(10, 13, 18, 24, 30),
                    new[] { "Prepare a global application", "Build business impact stories", "Research international programs" },
                    new[] { "Global MBA coaching", "Admissions counseling", "Leadership training" },
                    "1-2 years"
                ),
                "phd_funded" => new DomainProfile(
                    title,
                    new[] { "Research focus", "Academic persistence", "Specialization" },
                    new[] { "Research proposal", "Funding strategy", "Academic collaboration", "Publication planning", "Thesis work" },
                    "₹8,00,000 - ₹24,00,000 per year",
                    "$12,000 - $35,000 per year",
                    "+17% demand by 2030",
                    BuildSalaryTrend(8, 11, 15, 19, 24),
                    new[] { "Identify funded advisors", "Prepare research proposals", "Build an academic application" },
                    new[] { "PhD funding guidance", "Research methodology programs", "Academic writing courses" },
                    "3-5 years"
                ),
                "mim_europe" => new DomainProfile(
                    title,
                    new[] { "Management insight", "International exposure", "Business fundamentals" },
                    new[] { "Management study", "Global business", "Networking", "Leadership", "Analytics" },
                    "₹8,00,000 - ₹21,00,000 per year",
                    "$12,000 - $32,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(8, 10, 14, 18, 21),
                    new[] { "Research MiM programs", "Prepare your application", "Build international readiness" },
                    new[] { "MiM counseling", "Management courses", "Study abroad workshops" },
                    "1-2 years"
                ),
                "stem_australia" => new DomainProfile(
                    title,
                    new[] { "Research capability", "STEM focus", "Global mobility" },
                    new[] { "STEM study", "Research projects", "Internships", "Industry connections", "Visa planning" },
                    "₹7,00,000 - ₹19,00,000 per year",
                    "$10,000 - $28,000 per year",
                    "+19% demand by 2030",
                    BuildSalaryTrend(7, 9, 13, 16, 19),
                    new[] { "Identify Australian STEM programs", "Prepare scholarships", "Plan post-study pathways" },
                    new[] { "Study abroad support", "STEM admissions courses", "Visa guidance" },
                    "1-2 years"
                ),
                "uk_1year_ms" => new DomainProfile(
                    title,
                    new[] { "Efficiency", "Global recognition", "Focus" },
                    new[] { "One-year program", "International exposure", "Business or technical focus", "Academic writing", "Career planning" },
                    "₹8,00,000 - ₹22,00,000 per year",
                    "$11,000 - $32,000 per year",
                    "+18% demand by 2030",
                    BuildSalaryTrend(8, 10, 14, 17, 22),
                    new[] { "Prepare a concise study plan", "Choose the right UK program", "Gather application materials early" },
                    new[] { "UK masters application training", "Admissions guidance", "IELTS/GRE prep" },
                    "1-2 years"
                ),
                "online_global" => new DomainProfile(
                    title,
                    new[] { "Flexibility", "Self-learning", "Practical credentials" },
                    new[] { "Online credentialing", "Skill building", "Project showcases", "Remote collaboration", "Career readiness" },
                    "₹4,00,000 - ₹12,00,000 per year",
                    "$6,000 - $18,000 per year",
                    "+16% demand by 2030",
                    BuildSalaryTrend(4, 6, 9, 11, 12),
                    new[] { "Choose a high-quality online credential", "Complete projects with a portfolio", "Balance learning with work" },
                    new[] { "Online certification programs", "Micro-credential courses", "Skill-focused training" },
                    "6-12 months"
                ),
                _ => new DomainProfile(
                    GetDomainTitle(domainId),
                    new[] { "Adaptability", "Learning mindset", "Practical focus" },
                    new[] { "Core skills", "Project experience", "Communication", "Problem solving", "Growth mindset" },
                    "₹4,00,000 - ₹10,00,000 per year",
                    "$6,000 - $18,000 per year",
                    "+15% demand by 2030",
                    BuildSalaryTrend(4, 6, 8, 10, 12),
                    new[] { "Build relevant experience", "Practice active learning", "Identify growth opportunities" },
                    new[] { "Professional certificates", "Skill programs", "Project-based learning" },
                    "1-3 years"
                )
            };
        }

        private static string GetDomainTitle(string domainId)
        {
            return domainId switch
            {
                "science_pcm" => "Engineering stream with Physics, Chemistry, and Math",
                "science_pcb" => "Medical and biology stream with Physics, Chemistry, and Biology",
                "commerce" => "Commerce stream with finance, business, and economics",
                "arts_humanities" => "Arts and humanities stream focused on culture and communication",
                "vocational_iti" => "Vocational training through ITI and skilled trades",
                "polytechnic" => "Polytechnic technical education with applied engineering",
                "btech_cs" => "B.Tech in Computer Science",
                "btech_core" => "B.Tech in core engineering branches",
                "mbbs_medical" => "MBBS and medical doctor training",
                "bpharm_nursing" => "Pharmacy or nursing in healthcare sciences",
                "bcom_bba" => "Commerce and business administration",
                "ba_humanities" => "BA in humanities and liberal arts",
                "ba_llb" => "BA LLB in law and legal studies",
                "bdes_design" => "BDes in design and creative industries",
                "bsc_pure" => "BSc in pure sciences and research",
                "bsc_cs_data" => "BSc in computer science or data science",
                "frontend" => "Frontend development and user experience",
                "backend" => "Backend engineering and server systems",
                "fullstack" => "Full-stack software development",
                "data" => "Data analytics and business intelligence",
                "ml" => "Machine learning and AI engineering",
                "devops" => "DevOps, cloud, and infrastructure automation",
                "business" => "Business strategy and operations",
                "design" => "Product, UX, and service design",
                "healthcare" => "Healthcare administration and operations",
                "education" => "Education strategy and learning design",
                "phd_research" => "PhD research and academic expertise",
                "industry_expert" => "Senior industry expert or specialist",
                "consulting" => "Management and strategy consulting",
                "product_mgmt" => "Product management and delivery leadership",
                "startup_founder" => "Startup founding and entrepreneurship",
                "govt_policy" => "Government policy and public sector strategy",
                "academia_teaching" => "Academia, teaching, and instructional leadership",
                "frontend_switch" => "Frontend career switch into UI development",
                "backend_switch" => "Backend career switch into server-side engineering",
                "data_analyst" => "Career switch into data analytics",
                "ml_engineer" => "Career switch into machine learning engineering",
                "devops_switch" => "Career switch into DevOps and cloud operations",
                "product_manager" => "Career switch into product management",
                "ux_designer" => "Career switch into UX and interaction design",
                "biz_analyst" => "Career switch into business analysis",
                "tech_sales" => "Career switch into technology sales",
                "edtech_creator" => "Career switch into educational content creation",
                "ai_ml_certs" => "AI and machine learning certification path",
                "cloud_aws" => "AWS cloud upskilling",
                "cloud_azure" => "Azure cloud upskilling",
                "cloud_gcp" => "GCP cloud upskilling",
                "cybersecurity" => "Cybersecurity and information security upskilling",
                "data_analytics" => "Data analytics upskilling",
                "fullstack_web" => "Full-stack web development upskilling",
                "devops_certs" => "DevOps certification path",
                "low_code" => "Low-code and citizen development upskilling",
                "saas_b2b" => "B2B SaaS entrepreneurship",
                "d2c_consumer" => "Direct-to-consumer startup entrepreneurship",
                "deep_tech" => "Deep technology entrepreneurship",
                "marketplace" => "Marketplace and platform entrepreneurship",
                "edtech_startup" => "EdTech startup entrepreneurship",
                "fintech_startup" => "FinTech startup entrepreneurship",
                "social_enterprise" => "Social enterprise and impact entrepreneurship",
                "consulting_firm" => "Consulting firm entrepreneurship",
                "content_creator" => "Content creation and creator economy entrepreneurship",
                "franchise_retail" => "Franchise and retail business entrepreneurship",
                "ias_upsc" => "UPSC civil services preparation",
                "banking_po" => "Banking PO and financial services preparation",
                "defence_forces" => "Defence forces and military service preparation",
                "ssc_cgl" => "SSC CGL and central government exams preparation",
                "state_psc" => "State PSC and regional public service preparation",
                "judiciary_law" => "Judiciary and legal services preparation",
                "teaching_govt" => "Government teaching and education service preparation",
                "railways_rrb" => "Railways RRB and transport sector preparation",
                "psu_gate" => "PSU and GATE engineering job preparation",
                "police_services" => "Police services and homeland security preparation",
                "ms_usa" => "MS study in the USA",
                "ms_canada" => "MS study in Canada",
                "ms_europe" => "MS study in Europe",
                "mba_us_top10" => "MBA from a top US business school",
                "mba_global" => "Global MBA program",
                "phd_funded" => "Funded international PhD program",
                "mim_europe" => "MiM study program in Europe",
                "stem_australia" => "STEM study in Australia",
                "uk_1year_ms" => "One-year MS program in the UK",
                "online_global" => "Flexible online global credential",
                _ => domainId.Replace('_', ' ')
            };
        }

        private static Dictionary<string, string> BuildSalaryTrend(int year1, int year2, int year3, int year4, int year5)
        {
            return new Dictionary<string, string>
            {
                ["year1"] = $"₹{year1}L",
                ["year2"] = $"₹{year2}L",
                ["year3"] = $"₹{year3}L",
                ["year4"] = $"₹{year4}L",
                ["year5"] = $"₹{year5}L"
            };
        }

        private sealed record CategoryMeta(string Code, string Name, int QuestionCount, List<string> DomainIds, List<string> QuestionTemplates);

        private sealed record DomainProfile(
            string Title,
            IEnumerable<string> KeyStrengths,
            IEnumerable<string> RecommendedSkills,
            string SalaryIndia,
            string SalaryGlobal,
            string JobGrowth,
            Dictionary<string, string> FiveYearSalaryTrend,
            IEnumerable<string> NextSteps,
            IEnumerable<string> Certifications,
            string Timeline);
    }
}
