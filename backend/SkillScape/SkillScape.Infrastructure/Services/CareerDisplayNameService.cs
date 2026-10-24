using System;
using System.Collections.Generic;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Maps internal taxonomy node IDs to human-readable career display names.
/// NEVER expose raw IDs (like "sci.eng.cs.sec" or "Sec") to the user.
/// </summary>
public static class CareerDisplayNameService
{
    private static readonly Dictionary<string, CareerDisplayInfo> _map =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // -- CSE / Technology -------------------------------------------------
        { "sci.eng.cs",            new("Software & Computer Engineering",  "Technology",     "Software Engineering") },
        { "sci.eng.cs.backend",    new("Backend Software Engineer",         "Technology",     "Software Engineering") },
        { "sci.eng.cs.frontend",   new("Frontend Developer",                "Technology",     "Software Engineering") },
        { "sci.eng.cs.fullstack",  new("Full-Stack Developer",              "Technology",     "Software Engineering") },
        { "sci.eng.cs.mobile",     new("Mobile App Developer",              "Technology",     "Software Engineering") },
        { "sci.eng.cs.devops",     new("DevOps / Platform Engineer",        "Technology",     "Cloud & DevOps") },
        { "sci.eng.cs.sre",        new("Site Reliability Engineer (SRE)",   "Technology",     "Cloud & DevOps") },
        { "sci.eng.cs.cloud",      new("Cloud Engineer",                    "Technology",     "Cloud & DevOps") },
        { "sci.eng.cs.ai",         new("AI & Machine Learning Engineer",    "Technology",     "Artificial Intelligence") },
        { "sci.eng.cs.ml",         new("Machine Learning Engineer",         "Technology",     "Artificial Intelligence") },
        { "sci.eng.cs.nlp",        new("NLP / Language AI Engineer",        "Technology",     "Artificial Intelligence") },
        { "sci.eng.cs.cv",         new("Computer Vision Engineer",          "Technology",     "Artificial Intelligence") },
        { "sci.eng.cs.data",       new("Data Scientist",                    "Technology",     "Data & Analytics") },
        { "sci.eng.cs.data_analysis", new("Data Analyst",                   "Technology",     "Data & Analytics") },
        { "sci.eng.cs.dataeng",    new("Data Engineer",                     "Technology",     "Data & Analytics") },
        { "sci.eng.ai",            new("Artificial Intelligence Researcher","Technology",     "Artificial Intelligence") },

        // -- Cybersecurity -----------------------------------------------------
        { "sci.eng.cs.sec",        new("Cybersecurity Engineer",            "Technology",     "Cybersecurity") },
        { "sci.eng.cs.soc",        new("SOC Analyst",                       "Technology",     "Cybersecurity") },
        { "sci.eng.cs.ir",         new("Incident Responder",                "Technology",     "Cybersecurity") },
        { "sci.eng.cs.threat",     new("Threat Hunter",                     "Technology",     "Cybersecurity") },
        { "sci.eng.cs.pentest",    new("Penetration Tester / Ethical Hacker","Technology",    "Cybersecurity") },
        { "sci.eng.cs.forensics",  new("Digital Forensics Analyst",         "Technology",     "Cybersecurity") },
        { "sci.eng.cs.ti",         new("Threat Intelligence Analyst",       "Technology",     "Cybersecurity") },
        { "sci.eng.cs.appsec",     new("Application Security Engineer",     "Technology",     "Cybersecurity") },
        { "sci.eng.cs.cloudsec",   new("Cloud Security Engineer",           "Technology",     "Cybersecurity") },
        { "sci.eng.cs.netsec",     new("Network Security Engineer",         "Technology",     "Cybersecurity") },
        { "sci.eng.cs.grc",        new("GRC Analyst (Governance, Risk & Compliance)","Technology","Cybersecurity") },
        { "sci.eng.cs.iam",        new("Identity & Access Management (IAM) Specialist","Technology","Cybersecurity") },
        { "sci.eng.cs.secarch",    new("Security Architect",                "Technology",     "Cybersecurity") },
        { "sci.eng.cs.malware",    new("Malware Analyst",                   "Technology",     "Cybersecurity") },
        { "sci.eng.cs.vuln",       new("Vulnerability Analyst",             "Technology",     "Cybersecurity") },

        // -- Medical / MBBS ----------------------------------------------------
        { "sci.med",               new("Medical Professional",              "Healthcare",     "General Medicine") },
        { "sci.med.internal",      new("Internal Medicine Specialist",      "Healthcare",     "Clinical Medicine") },
        { "sci.med.surgery",       new("Surgeon",                           "Healthcare",     "Surgery") },
        { "sci.med.surgery.general", new("General Surgeon",                 "Healthcare",     "Surgery") },
        { "sci.med.surgery.ortho", new("Orthopedic Surgeon",               "Healthcare",     "Surgery") },
        { "sci.med.surgery.neuro", new("Neurosurgeon",                     "Healthcare",     "Surgery") },
        { "sci.med.emergency",     new("Emergency Medicine Physician",      "Healthcare",     "Emergency Medicine") },
        { "sci.med.pediatrics",    new("Pediatrician",                      "Healthcare",     "Pediatrics") },
        { "sci.med.radiology",     new("Radiologist",                       "Healthcare",     "Diagnostic Medicine") },
        { "sci.med.pathology",     new("Pathologist",                       "Healthcare",     "Diagnostic Medicine") },
        { "sci.med.cardiology",    new("Cardiologist",                      "Healthcare",     "Clinical Medicine") },
        { "sci.med.neurology",     new("Neurologist",                       "Healthcare",     "Clinical Medicine") },
        { "sci.med.psychiatry",    new("Psychiatrist",                      "Healthcare",     "Mental Health") },
        { "sci.med.dermatology",   new("Dermatologist",                     "Healthcare",     "Clinical Medicine") },
        { "sci.med.obg",           new("Obstetrician & Gynecologist",       "Healthcare",     "Clinical Medicine") },
        { "sci.med.anesthesiology",new("Anesthesiologist",                  "Healthcare",     "Surgery Support") },
        { "sci.med.family",        new("Family Medicine / General Practitioner","Healthcare", "Primary Care") },
        { "sci.med.research",      new("Medical Researcher",                "Healthcare",     "Research") },
        { "sci.med.public_health", new("Public Health Specialist",          "Healthcare",     "Public Health") },

        // -- Commerce / Finance ------------------------------------------------
        { "com.fin",               new("Finance Professional",              "Commerce",       "Finance") },
        { "com.fin.ca",            new("Chartered Accountant (CA)",         "Commerce",       "Accounting") },
        { "com.fin.cfa",           new("CFA / Investment Analyst",          "Commerce",       "Investment") },
        { "com.fin.banking",       new("Banking Professional",              "Commerce",       "Banking") },
        { "com.fin.audit",         new("Auditor",                           "Commerce",       "Audit") },
        { "com.fin.tax",           new("Tax Consultant",                    "Commerce",       "Taxation") },
        { "com.mgmt",              new("Management Professional",           "Commerce",       "Management") },
        { "com.mgmt.mba",          new("Master of Business Administration (MBA)", "Commerce",   "Management") },
        { "com.mktg",              new("Marketing Professional",             "Commerce",       "Marketing") },
        { "com.fin.fintech",       new("FinTech Analyst",                   "Commerce",       "Finance Technology") },

        // -- Engineering (Non-CS) ----------------------------------------------
        { "sci.eng.civil",         new("Civil Engineer",                    "Engineering",    "Civil Engineering") },
        { "sci.eng.civil.struct",  new("Structural Engineer",               "Engineering",    "Civil Engineering") },
        { "sci.eng.civil.env",     new("Environmental Engineer",            "Engineering",    "Civil Engineering") },
        { "sci.eng.civil.pm",      new("Construction Project Manager",      "Engineering",    "Civil Engineering") },
        { "arch",                   new("Architecture & Built Environment", "Design",         "Architecture") },
        { "arch.urban",             new("Urban Design & Planning",           "Design",         "Urban Design") },
        { "arch.residential",       new("Residential Architecture",          "Design",         "Architecture") },
        { "arch.sustainable",       new("Sustainable Architecture",          "Design",         "Sustainable Design") },
        { "arch.interior",          new("Interior Architecture",             "Design",         "Interior Design") },
        { "arch.landscape",         new("Landscape Architecture",            "Design",         "Landscape Design") },
        { "arch.conservation",      new("Architectural Conservation",        "Design",         "Heritage Conservation") },
        { "arch.bim",                new("BIM & Computational Design",        "Design",         "Digital Design") },
        { "sci.eng.mech",          new("Mechanical Engineer",               "Engineering",    "Mechanical Engineering") },
        { "sci.eng.mech.design",   new("Product Design Engineer",           "Engineering",    "Mechanical Engineering") },
        { "sci.eng.mech.auto",     new("Automotive Engineer",               "Engineering",    "Mechanical Engineering") },
        { "sci.eng.mech.aero",     new("Aerospace Engineer",                "Engineering",    "Mechanical Engineering") },
        { "sci.eng.mech.robot",    new("Robotics Engineer",                 "Engineering",    "Mechanical Engineering") },
        { "sci.eng.elec",          new("Electrical Engineer",               "Engineering",    "Electrical Engineering") },

        // -- Government / Civil Services ---------------------------------------
        { "gov",                   new("Government Service",                "Government",     "Civil Services") },
        { "gov.civil",             new("IAS / Civil Services Officer",      "Government",     "Civil Services") },
        { "gov.eng",               new("Government Engineer (PSU)",         "Government",     "Technical Services") },

        // -- Education ---------------------------------------------------------
        { "edu",                   new("Education / Teaching",              "Education",      "Academia") },
        { "edu.research",          new("Academic Researcher / PhD",         "Education",      "Research") },

        // -- Degrees / Professional --------------------------------------------
        { "deg.btech",             new("B.Tech / B.E. Degree",              "Education",      "Undergraduate") },
        { "deg.btech.cse",         new("B.Tech in Computer Science and Engineering", "Education", "Undergraduate") },
        { "deg.btech.mech",        new("B.Tech in Mechanical Engineering",  "Education",      "Undergraduate") },
        { "deg.be.cse",            new("B.E. in Computer Science and Engineering", "Education", "Undergraduate") },
        { "deg.be.mech",           new("B.E. in Mechanical Engineering",    "Education",      "Undergraduate") },
        { "deg.bsc",               new("B.Sc. Degree",                      "Education",      "Undergraduate") },
        { "deg.bcom",              new("Bachelor of Commerce (B.Com.)",     "Education",      "Undergraduate") },
        { "deg.ba",                new("B.A. Degree",                       "Education",      "Undergraduate") },
        { "deg.bba",               new("BBA Degree",                        "Education",      "Undergraduate") },
        { "deg.bca",               new("Bachelor of Computer Applications (BCA)", "Education", "Undergraduate") },
        { "deg.mbbs",              new("MBBS Degree",                       "Education",      "Undergraduate") },
        { "deg.llb",               new("Bachelor of Laws (LL.B.)",          "Education",      "Undergraduate") },
        { "deg.mca",               new("Master of Computer Applications (MCA)", "Education",  "Postgraduate") },
        { "deg.mba",               new("Master of Business Administration (MBA)", "Education", "Postgraduate") },
        { "prof.ca",               new("Chartered Accountancy",             "Education",      "Professional Qualification") },
        { "prof.cs",               new("Company Secretary",                 "Education",      "Professional Qualification") },
        { "prof.cma",              new("Cost and Management Accountant",    "Education",      "Professional Qualification") },
    };

    /// <summary>Get display info for a taxonomy node ID.</summary>
    public static CareerDisplayInfo GetDisplayInfo(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return new CareerDisplayInfo("Unknown Career", "Unknown", "Unknown");

        if (_map.TryGetValue(nodeId, out var info))
            return info;

        // Try parent path
        var parts = nodeId.Split('.');
        for (int len = parts.Length - 1; len >= 1; len--)
        {
            var parentId = string.Join(".", parts.Take(len));
            if (_map.TryGetValue(parentId, out var parentInfo))
                return parentInfo with { DisplayName = FormatFallbackName(nodeId) };
        }

        return new CareerDisplayInfo(FormatFallbackName(nodeId), "Career", "Specialization");
    }

    /// <summary>Get just the display name for a taxonomy node ID.</summary>
    public static string GetDisplayName(string nodeId)
        => GetDisplayInfo(nodeId).DisplayName;

    private static string FormatFallbackName(string nodeId)
    {
        var lowerId = nodeId.ToLowerInvariant();
        if (lowerId == "prof") return "Professional Qualification";
        if (lowerId.StartsWith("prof.")) return "Professional Qualification";
        if (lowerId == "deg") return "Academic Degree";
        if (lowerId.StartsWith("deg.")) return "Academic Degree";
        if (lowerId == "sci") return "Science";
        if (lowerId == "eng") return "Engineering";
        if (lowerId == "cs") return "Computer Science";
        if (lowerId == "cybersec") return "Cybersecurity";

        var last = nodeId.Split('.').Last();
        var words = last.Split('_', '-');
        var name = string.Join(" ", words.Select(w =>
            w.Length > 0 ? char.ToUpper(w[0]) + w[1..].ToLower() : w));

        return name
            .Replace("Ai", "AI").Replace("Ml", "ML")
            .Replace("Nlp", "NLP").Replace("Cv", "CV")
            .Replace("Cs", "CS").Replace("Soc", "SOC Analyst")
            .Replace("Ir", "Incident Responder").Replace("Grc", "GRC Analyst")
            .Replace("Iam", "IAM Specialist").Replace("Sre", "SRE")
            .Replace("Mktg", "Marketing").Replace("Mgmt", "Management").Replace("Mgt", "Management")
            .Replace("Mbbs", "MBBS").Replace("Obg", "OBG");
    }
}

/// <summary>Structured display info for a career node.</summary>
public record CareerDisplayInfo(string DisplayName, string Field, string Subdomain);
