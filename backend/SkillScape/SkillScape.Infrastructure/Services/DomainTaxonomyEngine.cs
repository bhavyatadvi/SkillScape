using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillScape.Infrastructure.Services
{
    /// <summary>
    /// Classifies any combination of UG degree, PG degree, broad field, and major
    /// into a canonical DomainProfile — without hardcoded degree-specific if-chains.
    /// Uses a scored keyword rule table so any future degree works automatically.
    /// </summary>
    public static class DomainTaxonomyEngine
    {
        private static readonly List<(string Keyword, string Domain, double Weight)> DomainRules = new()
        {
            // Healthcare / Medicine
            ("mbbs",             "Healthcare",   3.0),
            ("md ",              "Healthcare",   2.5),
            ("ms ",              "Healthcare",   1.5),
            ("surgery",          "Healthcare",   2.5),
            ("medicine",         "Healthcare",   2.5),
            ("medical",          "Healthcare",   2.5),
            ("physician",        "Healthcare",   2.5),
            ("clinical",         "Healthcare",   2.0),
            ("cardiology",       "Healthcare",   2.5),
            ("neurology",        "Healthcare",   2.5),
            ("orthopedics",      "Healthcare",   2.5),
            ("pediatrics",       "Healthcare",   2.5),
            ("psychiatry",       "Healthcare",   2.5),
            ("radiology",        "Healthcare",   2.5),
            ("pathology",        "Healthcare",   2.5),
            ("ophthalmology",    "Healthcare",   2.5),
            ("dermatology",      "Healthcare",   2.5),
            ("anesthesiology",   "Healthcare",   2.5),
            ("oncology",         "Healthcare",   2.5),
            ("gastroenterology", "Healthcare",   2.5),
            ("nephrology",       "Healthcare",   2.5),
            ("pulmonology",      "Healthcare",   2.5),
            ("endocrinology",    "Healthcare",   2.5),
            ("emergency",        "Healthcare",   1.5),
            ("obstetrics",       "Healthcare",   2.5),
            ("gynecology",       "Healthcare",   2.5),
            ("nursing",          "Healthcare",   2.0),
            ("healthcare",       "Healthcare",   2.5),
            ("health science",   "Healthcare",   2.0),
            ("public health",    "Healthcare",   2.0),
            ("bds",              "Healthcare",   2.5),
            ("dentist",          "Healthcare",   2.5),
            ("physiotherapy",    "Healthcare",   2.0),
            ("mpt",              "Healthcare",   2.0),
            ("bpt",              "Healthcare",   2.0),
            // Pharmacy
            ("pharm",            "Pharmacy",     3.0),
            ("pharmacy",         "Pharmacy",     3.0),
            ("pharmaceut",       "Pharmacy",     3.0),
            ("pharmacology",     "Pharmacy",     3.0),
            // Technology / Engineering
            ("b.tech",           "Technology",   2.5),
            ("btech",            "Technology",   2.5),
            ("b.e.",             "Technology",   2.5),
            ("m.tech",           "Technology",   3.0),
            ("mtech",            "Technology",   3.0),
            ("m.e.",             "Technology",   2.5),
            ("mca",              "Technology",   2.5),
            ("bca",              "Technology",   2.0),
            ("computer science", "Technology",   3.0),
            ("computer engineering","Technology",3.0),
            ("information technology","Technology",2.5),
            ("software",         "Technology",   2.5),
            ("cybersecurity",    "Technology",   3.0),
            ("cyber security",   "Technology",   3.0),
            ("network",          "Technology",   2.0),
            ("artificial intelligence","Technology",3.0),
            ("machine learning", "Technology",   3.0),
            ("deep learning",    "Technology",   3.0),
            ("data science",     "Technology",   3.0),
            ("data engineering", "Technology",   2.5),
            ("cloud",            "Technology",   2.5),
            ("devops",           "Technology",   2.5),
            ("electronics",      "Technology",   2.0),
            ("electrical",       "Technology",   2.0),
            ("mechanical",       "Technology",   2.0),
            ("civil engineering","Technology",   2.0),
            ("chemical engineering","Technology",2.0),
            ("aerospace",        "Technology",   2.5),
            ("robotics",         "Technology",   2.5),
            ("embedded",         "Technology",   2.5),
            ("vlsi",             "Technology",   2.5),
            ("iot",              "Technology",   2.5),
            ("information systems","Technology", 2.5),
            ("nlp",              "Technology",   2.5),
            ("computer vision",  "Technology",   2.5),
            ("blockchain",       "Technology",   2.0),
            // Commerce / Business
            ("mba",              "Commerce",     3.0),
            ("b.com",            "Commerce",     2.5),
            ("bcom",             "Commerce",     2.5),
            ("m.com",            "Commerce",     2.5),
            ("mcom",             "Commerce",     2.5),
            ("bba",              "Commerce",     2.5),
            ("finance",          "Commerce",     3.0),
            ("accounting",       "Commerce",     2.5),
            ("economics",        "Commerce",     2.5),
            ("commerce",         "Commerce",     2.5),
            ("marketing",        "Commerce",     2.5),
            ("human resource",   "Commerce",     2.5),
            ("operations management","Commerce", 2.5),
            ("supply chain",     "Commerce",     2.5),
            ("business",         "Commerce",     2.0),
            ("investment banking","Commerce",    3.0),
            ("financial analysis","Commerce",    3.0),
            ("chartered accountant","Commerce",  2.5),
            ("audit",            "Commerce",     2.0),
            ("taxation",         "Commerce",     2.5),
            ("risk management",  "Commerce",     2.0),
            ("banking",          "Commerce",     2.5),
            ("consulting",       "Commerce",     2.0),
            // Law
            ("llb",              "Law",          3.0),
            ("llm",              "Law",          3.0),
            ("law",              "Law",          3.0),
            ("legal",            "Law",          3.0),
            ("advocate",         "Law",          2.5),
            ("litigation",       "Law",          2.5),
            ("corporate law",    "Law",          3.0),
            ("criminal law",     "Law",          2.5),
            ("intellectual property","Law",      2.5),
            ("human rights",     "Law",          2.0),
            ("international law","Law",          2.5),
            ("cyber law",        "Law",          2.5),
            ("environmental law","Law",          2.0),
            // Architecture / Design
            ("b.arch",           "Architecture", 3.0),
            ("m.arch",           "Architecture", 3.0),
            ("architecture",     "Architecture", 3.0),
            ("urban design",     "Architecture", 3.0),
            ("urban planning",   "Architecture", 3.0),
            ("landscape",        "Architecture", 2.5),
            ("interior design",  "Architecture", 2.5),
            ("b.des",            "Design",       2.5),
            ("m.des",            "Design",       2.5),
            ("graphic design",   "Design",       2.5),
            ("product design",   "Design",       2.5),
            ("fashion",          "Design",       2.0),
            // Psychology
            ("psychology",       "Psychology",   3.0),
            ("psycholog",        "Psychology",   3.0),
            ("counseling",       "Psychology",   2.5),
            ("counselling",      "Psychology",   2.5),
            ("psychotherapy",    "Psychology",   2.5),
            ("clinical psychology","Psychology", 3.0),
            ("organizational behavior","Psychology",2.5),
            ("neuropsychology",  "Psychology",   2.5),
            ("forensic psychology","Psychology", 2.5),
            ("behavioral science","Psychology",  2.5),
            ("social work",      "Psychology",   2.0),
            // Pure Sciences
            ("m.sc",             "Science",      2.0),
            ("msc",              "Science",      2.0),
            ("b.sc",             "Science",      1.5),
            ("bsc",              "Science",      1.5),
            ("physics",          "Science",      2.5),
            ("chemistry",        "Science",      2.5),
            ("mathematics",      "Science",      2.5),
            ("statistics",       "Science",      2.5),
            ("microbiology",     "Science",      2.5),
            ("biochemistry",     "Science",      2.5),
            ("environmental science","Science",  2.5),
            ("geology",          "Science",      2.5),
            ("zoology",          "Science",      2.5),
            ("botany",           "Science",      2.5),
            // Life Sciences / Biotechnology
            ("biotechnology",    "LifeSciences", 3.0),
            ("biomedical",       "LifeSciences", 3.0),
            ("life science",     "LifeSciences", 2.5),
            ("bioinformatics",   "LifeSciences", 3.0),
            ("genetics",         "LifeSciences", 2.5),
            ("genomics",         "LifeSciences", 2.5),
            ("molecular biology","LifeSciences", 2.5),
            ("neuroscience",     "LifeSciences", 2.5),
            // Humanities / Arts
            ("history",          "Humanities",   2.5),
            ("political science","Humanities",   2.5),
            ("philosophy",       "Humanities",   2.5),
            ("sociology",        "Humanities",   2.5),
            ("anthropology",     "Humanities",   2.5),
            ("journalism",       "Humanities",   2.5),
            ("mass communication","Humanities",  2.5),
            ("public administration","Humanities",2.5),
            ("social science",   "Humanities",   2.5),
            ("international relations","Humanities",2.5),
            ("linguistics",      "Humanities",   2.0),
            ("fine arts",        "Humanities",   2.0),
        };

        private static readonly List<(string Domain, string MajorKeyword, string SubDomain)> SubDomainRules = new()
        {
            ("Healthcare", "cardiology",     "Cardiology"),
            ("Healthcare", "neurology",      "Neurology"),
            ("Healthcare", "surgery",        "Surgery"),
            ("Healthcare", "pediatrics",     "Pediatrics"),
            ("Healthcare", "psychiatry",     "Psychiatry"),
            ("Healthcare", "radiology",      "Radiology"),
            ("Healthcare", "pathology",      "Pathology"),
            ("Healthcare", "oncology",       "Oncology"),
            ("Healthcare", "orthopedics",    "Orthopedics"),
            ("Healthcare", "dermatology",    "Dermatology"),
            ("Healthcare", "obstetrics",     "Obstetrics & Gynecology"),
            ("Healthcare", "gynecology",     "Obstetrics & Gynecology"),
            ("Healthcare", "anesthesiology", "Anesthesiology"),
            ("Healthcare", "ophthalmology",  "Ophthalmology"),
            ("Healthcare", "research",       "Medical Research"),
            ("Healthcare", "public health",  "Public Health"),
            ("Healthcare", "administration", "Healthcare Administration"),
            ("Healthcare", "emergency",      "Emergency Medicine"),
            ("Technology", "artificial intelligence", "AI & Machine Learning"),
            ("Technology", "machine learning",        "AI & Machine Learning"),
            ("Technology", "deep learning",           "AI & Machine Learning"),
            ("Technology", "data science",            "Data Science"),
            ("Technology", "data engineering",        "Data Engineering"),
            ("Technology", "cybersecurity",           "Cybersecurity"),
            ("Technology", "cyber security",          "Cybersecurity"),
            ("Technology", "cloud",                   "Cloud Computing"),
            ("Technology", "devops",                  "DevOps & SRE"),
            ("Technology", "software",                "Software Engineering"),
            ("Technology", "computer science",        "Software Engineering"),
            ("Technology", "computer application",    "Software Engineering"),
            ("Technology", "bca",                     "Software Engineering"),
            ("Technology", "mca",                     "Software Engineering"),
            ("Technology", "electronics",             "Electronics & VLSI"),
            ("Technology", "vlsi",                    "Electronics & VLSI"),
            ("Technology", "embedded",                "Embedded Systems"),
            ("Technology", "robotics",                "Robotics & Automation"),
            ("Technology", "mechanical",              "Mechanical Engineering"),
            ("Technology", "civil",                   "Civil Engineering"),
            ("Technology", "electrical",              "Electrical Engineering"),
            ("Technology", "aerospace",               "Aerospace Engineering"),
            ("Technology", "nlp",                     "AI & Machine Learning"),
            ("Technology", "computer vision",         "AI & Machine Learning"),
            ("Commerce", "finance",           "Finance"),
            ("Commerce", "investment",        "Investment Banking & Finance"),
            ("Commerce", "banking",           "Banking & Financial Services"),
            ("Commerce", "accounting",        "Accounting & Audit"),
            ("Commerce", "marketing",         "Marketing & Brand Management"),
            ("Commerce", "human resource",    "Human Resources"),
            ("Commerce", "supply chain",      "Supply Chain & Operations"),
            ("Commerce", "operations",        "Operations Management"),
            ("Commerce", "strategy",          "Strategy & Consulting"),
            ("Commerce", "consulting",        "Strategy & Consulting"),
            ("Commerce", "risk",              "Risk Management"),
            ("Commerce", "taxation",          "Taxation & Compliance"),
            ("Commerce", "analytics",         "Business Analytics"),
            ("Law", "corporate",       "Corporate Law"),
            ("Law", "criminal",        "Criminal Law"),
            ("Law", "intellectual",    "Intellectual Property Law"),
            ("Law", "international",   "International Law"),
            ("Law", "cyber",           "Cyber Law"),
            ("Law", "taxation",        "Taxation Law"),
            ("Law", "constitutional",  "Constitutional Law"),
            ("Law", "human rights",    "Human Rights Law"),
            ("Law", "environmental",   "Environmental Law"),
            ("Architecture", "urban",      "Urban Design & Planning"),
            ("Architecture", "landscape",  "Landscape Architecture"),
            ("Architecture", "interior",   "Interior Design"),
            ("Architecture", "structural", "Structural Engineering"),
            ("Architecture", "sustainable","Sustainable Design"),
            ("Psychology", "clinical",       "Clinical Psychology"),
            ("Psychology", "organizational", "Organizational Psychology"),
            ("Psychology", "neuropsych",     "Neuropsychology"),
            ("Psychology", "forensic",       "Forensic Psychology"),
            ("Psychology", "counseling",     "Counseling Psychology"),
            ("Psychology", "research",       "Research Psychology"),
            ("Science", "physics",       "Physics"),
            ("Science", "chemistry",     "Chemistry"),
            ("Science", "mathematics",   "Mathematics"),
            ("Science", "statistics",    "Statistics"),
            ("Science", "microbiology",  "Microbiology"),
            ("Science", "biochemistry",  "Biochemistry"),
            ("LifeSciences", "biotechnology", "Biotechnology"),
            ("LifeSciences", "biomedical",    "Biomedical Research"),
            ("LifeSciences", "bioinformatics","Bioinformatics"),
            ("LifeSciences", "genetics",      "Genetics & Genomics"),
            ("LifeSciences", "neuroscience",  "Neuroscience"),
            ("LifeSciences", "molecular",     "Molecular Biology"),
        };

        private static readonly Dictionary<string, List<string>> CareerFamilyMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Healthcare"]   = new() { "Clinical Practice", "Medical Research", "Healthcare Academia", "Healthcare Administration", "Public Health", "Medical Technology", "Healthcare Consulting" },
            ["Pharmacy"]     = new() { "Clinical Pharmacy", "Industrial Pharmacy", "Pharmaceutical Research", "Drug Regulatory Affairs", "Pharmacovigilance", "Hospital Pharmacy" },
            ["Technology"]   = new() { "Software Engineering", "AI & Machine Learning", "Data Science", "Cybersecurity", "Cloud & DevOps", "Product Management", "Research & Academia", "Technical Leadership" },
            ["Commerce"]     = new() { "Corporate Finance", "Investment Banking", "Consulting & Strategy", "Marketing & Brand", "Human Resources", "Operations & Supply Chain", "Business Analytics", "Entrepreneurship" },
            ["Law"]          = new() { "Litigation & Advocacy", "Corporate Law Practice", "In-House Counsel", "Legal Research & Academia", "Public Policy & Law", "Alternative Dispute Resolution" },
            ["Architecture"] = new() { "Architectural Practice", "Urban Design & Planning", "Interior Design", "Landscape Architecture", "Real Estate Development", "Heritage Conservation" },
            ["Design"]       = new() { "UX/Product Design", "Graphic & Visual Design", "Motion & Animation", "Fashion Design", "Design Research" },
            ["Psychology"]   = new() { "Clinical Psychology Practice", "Organizational Psychology", "Research Psychology", "Educational Psychology", "Forensic Psychology", "Counseling" },
            ["Science"]      = new() { "Academic Research", "Industry R&D", "Data & Analytics", "Science Communication", "Government Science", "Teaching & Academia" },
            ["LifeSciences"] = new() { "Biotechnology R&D", "Pharmaceutical Research", "Bioinformatics", "Clinical Research", "Science Writing", "Regulatory Affairs" },
            ["Humanities"]   = new() { "Journalism & Media", "Policy & Public Administration", "Academia & Research", "International Relations", "Social Work", "Content & Communications" },
        };

        public static DomainProfile Classify(
            string pgDegree,
            string pgMajor,
            string pgSpecialization,
            string ugDegree = "",
            string ugMajor = "",
            string broadField = "",
            bool isCareerChange = false)
        {
            var combined = $"{(pgDegree ?? "").ToLowerInvariant()} {(pgMajor ?? "").ToLowerInvariant()} {(pgSpecialization ?? "").ToLowerInvariant()} {(broadField ?? "").ToLowerInvariant()} {(ugDegree ?? "").ToLowerInvariant()} {(ugMajor ?? "").ToLowerInvariant()}";

            var scores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var (keyword, domain, weight) in DomainRules)
            {
                if (combined.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    scores[domain] = scores.GetValueOrDefault(domain, 0) + weight;
            }

            // PG degree is authoritative — double-weight its contribution
            string pgLower = (pgDegree ?? "").ToLowerInvariant();
            string pgMajorLower = (pgMajor ?? "").ToLowerInvariant();
            foreach (var (keyword, domain, weight) in DomainRules)
            {
                if (pgLower.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    pgMajorLower.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    scores[domain] = scores.GetValueOrDefault(domain, 0) + weight;
            }

            string primaryDomain = "General";
            double topScore = 0;
            foreach (var kv in scores)
            {
                if (kv.Value > topScore) { topScore = kv.Value; primaryDomain = kv.Key; }
            }

            var subDomains = new List<string>();
            string majorText = $"{pgMajorLower} {(pgSpecialization ?? "").ToLowerInvariant()}";
            foreach (var (domain, keyword, sub) in SubDomainRules)
            {
                if (domain.Equals(primaryDomain, StringComparison.OrdinalIgnoreCase) &&
                    majorText.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    !subDomains.Contains(sub))
                    subDomains.Add(sub);
            }
            // fallback disabled

            var careerFamilies = CareerFamilyMap.TryGetValue(primaryDomain, out var cf) ? cf.ToList() : new List<string> { "General Career", "Research & Academia" };

            var knownFacts = new List<string> { "broad_field_preference" };
            if (!string.IsNullOrWhiteSpace(pgMajor)) knownFacts.Add("pg_major_field");
            if (!string.IsNullOrWhiteSpace(pgSpecialization)) knownFacts.Add("specialization_area");

            string taxonomyTag = subDomains.Any()
                ? $"{primaryDomain}.{subDomains[0].Replace(" ", "_").Replace("&", "and")}"
                : primaryDomain;

            var secondaryDomains = scores
                .Where(kv => kv.Key != primaryDomain && kv.Value >= 2.0)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(2)
                .ToList();

            return new DomainProfile
            {
                PrimaryDomain    = primaryDomain,
                SubDomains       = subDomains,
                CareerFamilies   = careerFamilies,
                KnownFacts       = knownFacts,
                TaxonomyTag      = taxonomyTag,
                SecondaryDomains = isCareerChange ? secondaryDomains : new(),
                DomainScore      = Math.Round(topScore, 2),
            };
        }

        public static string GetQuestionBankDomain(string primaryDomain) => primaryDomain switch
        {
            "Healthcare"   => "Healthcare",
            "Pharmacy"     => "Healthcare",
            "Technology"   => "Technology",
            "Commerce"     => "Commerce",
            "Law"          => "Law",
            "Architecture" => "Architecture",
            "Design"       => "Design",
            "Psychology"   => "Psychology",
            "Science"      => "Science",
            "LifeSciences" => "Science",
            "Humanities"   => "Humanities",
            _              => ""
        };

        private static string CapitalizeWords(string s) =>
            string.IsNullOrWhiteSpace(s) ? s :
            string.Join(" ", s.Split(' ').Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w.Substring(1).ToLower() : w));
    }

    public class DomainProfile
    {
        public string PrimaryDomain    { get; set; } = "General";
        public List<string> SubDomains       { get; set; } = new();
        public List<string> CareerFamilies   { get; set; } = new();
        public List<string> KnownFacts       { get; set; } = new();
        public string TaxonomyTag      { get; set; } = string.Empty;
        public List<string> SecondaryDomains { get; set; } = new();
        public double DomainScore      { get; set; }
    }
}

