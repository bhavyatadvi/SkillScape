using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SkillScape.Domain.Entities;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Deterministic, exhaustible fallback question banks organized by:
///   Field → Stage → Dimension (unique per dimension, no repeats)
///
/// These questions are ONLY used when LLM generation fails.
/// Each question has a unique ID, verified signals, and a dimension tag.
/// The bank selects the next unused question appropriate for the current field/stage.
/// </summary>
public class FieldSpecificQuestionBank
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public static bool HasDedicatedBank(string userMajor, string phaseKey)
    {
        var m = (userMajor ?? string.Empty).ToLowerInvariant();
        var hasBaBackground = QuizProfileClassifier.IsBachelorOfArts(userMajor, null);
        if (m.Contains("mbbs") || m.Contains("medicine") || m.Contains("medical")) return true;
        if (IsArchitectureProfile(m)) return true;
        if (phaseKey.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase))
            return hasBaBackground || m.Contains("arts") || m.Contains("humanit") || m.Contains("social science") || m.Contains("psychology") || m.Contains("sociology") || m.Contains("history") || m.Contains("english");
        return hasBaBackground || QuizProfileClassifier.IsBachelorOfScience(userMajor, null) ||
            m.Contains("humanit") || m.Contains("arts & humanities");
    }

    public static bool IsMedicalProfile(string userMajor)
    {
        var major = userMajor ?? string.Empty;
        return major.Contains("mbbs", StringComparison.OrdinalIgnoreCase) ||
            major.Contains("medicine", StringComparison.OrdinalIgnoreCase) ||
            major.Contains("medical", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Last-resort continuation for a medical assessment. The adaptive selector
    /// can exhaust a stage after profile and duplicate filters run; this keeps
    /// the quiz from declaring completion before its minimum question count.
    /// </summary>
    public static QuizQuestionV2? GetUnusedMedicalQuestion(string phaseKey, IEnumerable<string> excludedQuestionIds)
    {
        var excluded = new HashSet<string>(excludedQuestionIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var question = _mbbsQuestions.FirstOrDefault(item => !excluded.Contains(item.Id));
        return question == null ? null : MapToEntity(question, phaseKey);
    }

    public static QuizQuestionV2? GetQuestionById(string questionId, string phaseKey)
    {
        var allQuestions = _baQuestions.Concat(_baPostgraduateQuestions).Concat(_architectureQuestions)
            .Concat(_bscQuestions).Concat(_mbbsQuestions);
        var question = allQuestions.FirstOrDefault(item => item.Id.Equals(questionId, StringComparison.OrdinalIgnoreCase));
        return question == null ? null : MapToEntity(question, phaseKey);
    }

    public static QuizQuestionV2? GetQuestionById(string questionId, string phaseKey, List<string>? selectedFields)
    {
        if (questionId.StartsWith("FB_ARCH_SELECTED_", StringComparison.OrdinalIgnoreCase))
            return CreateArchitectureSelectedFieldQuestion(selectedFields, phaseKey, questionId);
        if (questionId.StartsWith("FB_BA_SELECTED_", StringComparison.OrdinalIgnoreCase))
            return CreateSelectedFieldQuestion(selectedFields, phaseKey, questionId);
        if (questionId.StartsWith("FB_SELECTED_", StringComparison.OrdinalIgnoreCase))
            return CreateGeneralSelectedFieldQuestion(selectedFields, phaseKey, questionId);
        return GetQuestionById(questionId, phaseKey);
    }

    public static HashSet<string> GetSelectedFieldTargetNodeIds(List<string>? selectedFields)
    {
        return (selectedFields ?? new List<string>())
            .Select(MapSelectedField)
            .Where(option => option != null)
            .SelectMany(option => option!.Signals.Select(signal => signal.NodeId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    // ── CSE / IT Field Questions ────────────────────────────────────────────

    private static readonly List<FallbackQuestion> _cseQuestions = new()
    {
        // --- BroadFieldDiscovery ---
        new("FB_CSE_BROAD_01", QuizStage.BroadFieldDiscovery, "problem_type_preference",
            "When you imagine your ideal workday as a technology professional, which of these resonates most?",
            new[] {
                new FallbackOption("Investigating why a system behaved unexpectedly — tracing logs, finding the breach",
                    new[]{ ("sci.eng.cs.sec",0.8), ("sci.eng.cs.soc",0.5) }),
                new FallbackOption("Building and optimizing software systems that serve millions of users",
                    new[]{ ("sci.eng.cs.backend",0.8), ("sci.eng.cs.fullstack",0.5) }),
                new FallbackOption("Training models on large datasets and evaluating their accuracy",
                    new[]{ ("sci.eng.cs.ai",0.8), ("sci.eng.cs.ml",0.6) }),
                new FallbackOption("Designing cloud infrastructure and automation pipelines",
                    new[]{ ("sci.eng.cs.devops",0.8), ("sci.eng.cs.cloud",0.6) }),
            }),

        new("FB_CSE_BROAD_02", QuizStage.BroadFieldDiscovery, "analytical_vs_creative",
            "Which of these technical challenges would you find most intellectually satisfying?",
            new[] {
                new FallbackOption("Reverse-engineering a piece of malware to understand how it evades detection",
                    new[]{ ("sci.eng.cs.malware",0.9), ("sci.eng.cs.forensics",0.6) }),
                new FallbackOption("Designing a machine learning architecture that predicts user behavior",
                    new[]{ ("sci.eng.cs.ml",0.9), ("sci.eng.cs.ai",0.6) }),
                new FallbackOption("Architecting a microservices system that scales to 10 million requests per day",
                    new[]{ ("sci.eng.cs.backend",0.9), ("sci.eng.cs.sre",0.5) }),
                new FallbackOption("Creating a data pipeline that transforms raw logs into business intelligence",
                    new[]{ ("sci.eng.cs.dataeng",0.9), ("sci.eng.cs.data_analysis",0.5) }),
            }),

        new("FB_CSE_BROAD_03", QuizStage.BroadFieldDiscovery, "working_style",
            "How would you describe your approach to technology problems?",
            new[] {
                new FallbackOption("Defensive — I want to protect systems and find hidden threats before attackers do",
                    new[]{ ("sci.eng.cs.sec",0.8), ("sci.eng.cs.threat",0.6) }),
                new FallbackOption("Analytical — I want to find patterns and extract insights from large volumes of data",
                    new[]{ ("sci.eng.cs.data",0.8), ("sci.eng.cs.data_analysis",0.6) }),
                new FallbackOption("Constructive — I want to build reliable, scalable software products",
                    new[]{ ("sci.eng.cs.backend",0.8), ("sci.eng.cs.fullstack",0.5) }),
                new FallbackOption("Experimental — I want to push the boundaries of what AI can do",
                    new[]{ ("sci.eng.cs.ai",0.8), ("sci.eng.cs.nlp",0.4) }),
            }),

        new("FB_CSE_BROAD_04", QuizStage.BroadFieldDiscovery, "risk_orientation",
            "When a critical production system alerts at 2 AM, which role would you most want to be playing?",
            new[] {
                new FallbackOption("Security on-call: investigate whether this is a breach, assess the threat",
                    new[]{ ("sci.eng.cs.soc",0.9), ("sci.eng.cs.ir",0.6) }),
                new FallbackOption("SRE on-call: diagnose the outage, roll back the deployment, restore service",
                    new[]{ ("sci.eng.cs.sre",0.9), ("sci.eng.cs.devops",0.6) }),
                new FallbackOption("Backend on-call: debug the service, find the code bug, push the fix",
                    new[]{ ("sci.eng.cs.backend",0.9) }),
                new FallbackOption("Data engineer on-call: fix the broken pipeline that caused bad metrics",
                    new[]{ ("sci.eng.cs.dataeng",0.9) }),
            }),

        // --- FieldConfirmation: Cybersecurity ---
        new("FB_CSE_CYBER_CONF_01", QuizStage.FieldConfirmation, "cyber_scenario_preference",
            "A company discovers that an attacker has been inside their network for 3 months. Which part of the investigation would you most want to lead?",
            new[] {
                new FallbackOption("Analyzing SIEM alerts and correlating logs to determine which systems were touched",
                    new[]{ ("sci.eng.cs.soc",0.9), ("sci.eng.cs.sec",0.5) }),
                new FallbackOption("Containing the active breach, blocking attacker movement, and recovering systems",
                    new[]{ ("sci.eng.cs.ir",0.9), ("sci.eng.cs.sec",0.5) }),
                new FallbackOption("Hunting proactively for the attacker's remaining footholds in the network",
                    new[]{ ("sci.eng.cs.threat",0.9), ("sci.eng.cs.sec",0.5) }),
                new FallbackOption("Collecting disk and memory forensics to reconstruct the attacker's full timeline",
                    new[]{ ("sci.eng.cs.forensics",0.9), ("sci.eng.cs.sec",0.5) }),
            }),

        new("FB_CSE_CYBER_CONF_02", QuizStage.FieldConfirmation, "cyber_motivation",
            "What drives you most in the cybersecurity domain?",
            new[] {
                new FallbackOption("Catching attackers — monitoring for anomalies and identifying malicious behavior",
                    new[]{ ("sci.eng.cs.soc",0.8), ("sci.eng.cs.sec",0.4) }),
                new FallbackOption("Defending systems — building controls, hardening infrastructure, reducing attack surface",
                    new[]{ ("sci.eng.cs.secarch",0.8), ("sci.eng.cs.netsec",0.5) }),
                new FallbackOption("Attacking to find weaknesses — testing defenses through penetration testing",
                    new[]{ ("sci.eng.cs.pentest",0.9) }),
                new FallbackOption("Understanding adversaries — researching threat actors, campaigns, and TTPs",
                    new[]{ ("sci.eng.cs.ti",0.9), ("sci.eng.cs.threat",0.5) }),
            }),

        // --- SubdomainDiscovery: Cybersecurity ---
        new("FB_CSE_CYBER_SUB_01", QuizStage.SubdomainDiscovery, "alert_vs_hunt",
            "During a suspected ransomware incident, which responsibility would you most want to handle?",
            new[] {
                new FallbackOption("Monitor security alerts and determine which events require immediate escalation",
                    new[]{ ("sci.eng.cs.soc",0.95), ("sci.eng.cs.ir",0.3) }),
                new FallbackOption("Contain the compromised systems and coordinate recovery with IT teams",
                    new[]{ ("sci.eng.cs.ir",0.95) }),
                new FallbackOption("Search proactively for additional attacker footholds beyond what alerts showed",
                    new[]{ ("sci.eng.cs.threat",0.95) }),
                new FallbackOption("Preserve and analyze digital evidence to reconstruct the attack for legal proceedings",
                    new[]{ ("sci.eng.cs.forensics",0.95) }),
            }),

        new("FB_CSE_CYBER_SUB_02", QuizStage.SubdomainDiscovery, "offensive_vs_defensive",
            "Which type of cybersecurity work most excites you?",
            new[] {
                new FallbackOption("Simulating real-world attacks to find vulnerabilities before real hackers do",
                    new[]{ ("sci.eng.cs.pentest",0.95) }),
                new FallbackOption("Analyzing SIEM dashboards, triaging alerts, and writing detection rules",
                    new[]{ ("sci.eng.cs.soc",0.95) }),
                new FallbackOption("Reviewing application code for security flaws and building secure APIs",
                    new[]{ ("sci.eng.cs.appsec",0.95) }),
                new FallbackOption("Ensuring cloud configurations are secure and monitoring cloud-native threats",
                    new[]{ ("sci.eng.cs.cloudsec",0.95) }),
            }),

        new("FB_CSE_CYBER_SUB_03", QuizStage.SubdomainDiscovery, "threat_intelligence_vs_compliance",
            "How would you prefer to spend a typical security research day?",
            new[] {
                new FallbackOption("Reading threat reports, tracking APT groups, and building IOC feeds",
                    new[]{ ("sci.eng.cs.ti",0.95) }),
                new FallbackOption("Developing risk frameworks, writing security policies, and conducting compliance audits",
                    new[]{ ("sci.eng.cs.grc",0.95) }),
                new FallbackOption("Analyzing malware samples in sandboxes to extract behavioral signatures",
                    new[]{ ("sci.eng.cs.malware",0.95) }),
                new FallbackOption("Designing and building security controls into enterprise network architecture",
                    new[]{ ("sci.eng.cs.secarch",0.95), ("sci.eng.cs.netsec",0.5) }),
            }),

        new("FB_CSE_CYBER_SUB_04", QuizStage.SubdomainDiscovery, "soc_vs_ir_discrimination",
            "A new alert fires: unusual outbound traffic from a finance server. What do you do first?",
            new[] {
                new FallbackOption("Check the alert against known threat signatures in the SIEM; triage its severity",
                    new[]{ ("sci.eng.cs.soc",0.95) }),
                new FallbackOption("Immediately isolate the server and start incident response procedures",
                    new[]{ ("sci.eng.cs.ir",0.95) }),
                new FallbackOption("Pivot to look for other affected systems — the server may not be the only one",
                    new[]{ ("sci.eng.cs.threat",0.9), ("sci.eng.cs.soc",0.4) }),
                new FallbackOption("Image the server''s disk and memory before doing anything else",
                    new[]{ ("sci.eng.cs.forensics",0.95) }),
            }),

        new("FB_CSE_CYBER_SUB_05", QuizStage.SubdomainDiscovery, "iam_vs_netsec",
            "Which security domain feels most natural to you?",
            new[] {
                new FallbackOption("Managing who can access what — identity, roles, permissions, authentication",
                    new[]{ ("sci.eng.cs.iam",0.95) }),
                new FallbackOption("Securing network perimeters, firewalls, segmentation, and traffic analysis",
                    new[]{ ("sci.eng.cs.netsec",0.95) }),
                new FallbackOption("Breaking into systems legally to demonstrate vulnerabilities to management",
                    new[]{ ("sci.eng.cs.pentest",0.95) }),
                new FallbackOption("Managing risk, writing compliance reports, and preparing for audits",
                    new[]{ ("sci.eng.cs.grc",0.95) }),
            }),

        // --- SubdomainDiscovery: AI/ML ---
        new("FB_CSE_AI_SUB_01", QuizStage.SubdomainDiscovery, "research_vs_applied",
            "When working with AI, do you prefer the research frontier or real-world application?",
            new[] {
                new FallbackOption("Publishing papers on novel architectures and pushing model accuracy records",
                    new[]{ ("sci.eng.cs.ai",0.9), ("edu.research",0.4) }),
                new FallbackOption("Deploying models in production and optimizing them for latency and cost",
                    new[]{ ("sci.eng.cs.ml",0.9), ("sci.eng.cs.devops",0.4) }),
                new FallbackOption("Building NLP systems that understand and generate human language",
                    new[]{ ("sci.eng.cs.nlp",0.95) }),
                new FallbackOption("Training computer vision systems for image recognition and detection",
                    new[]{ ("sci.eng.cs.cv",0.95) }),
            }),

        new("FB_CSE_AI_SUB_02", QuizStage.SubdomainDiscovery, "data_vs_model",
            "In an AI project, which phase do you find most exciting?",
            new[] {
                new FallbackOption("Collecting, cleaning, and engineering the features from raw data",
                    new[]{ ("sci.eng.cs.dataeng",0.8), ("sci.eng.cs.data",0.6) }),
                new FallbackOption("Experimenting with model architectures and hyperparameter tuning",
                    new[]{ ("sci.eng.cs.ml",0.9), ("sci.eng.cs.ai",0.5) }),
                new FallbackOption("Interpreting model outputs and communicating insights to business stakeholders",
                    new[]{ ("sci.eng.cs.data_analysis",0.9) }),
                new FallbackOption("Building the MLOps pipeline to automate training, testing, and deployment",
                    new[]{ ("sci.eng.cs.devops",0.8), ("sci.eng.cs.ml",0.5) }),
            }),
    };

    // ── MBBS / Medical Field Questions ───────────────────────────────────────

    private static readonly List<FallbackQuestion> _mbbsQuestions = new()
    {
        new("FB_MBBS_BROAD_01", QuizStage.BroadFieldDiscovery, "clinical_vs_diagnostic",
            "Imagine yourself 5 years into your medical career. Which setting feels most right?",
            new[] {
                new FallbackOption("A busy ward — directly managing and treating complex patients",
                    new[]{ ("sci.med.internal",0.8), ("sci.med.cardiology",0.4) }),
                new FallbackOption("An operating theater — performing surgical procedures",
                    new[]{ ("sci.med.surgery",0.9) }),
                new FallbackOption("A radiology reading room — interpreting imaging to guide diagnosis",
                    new[]{ ("sci.med.radiology",0.9) }),
                new FallbackOption("A laboratory or research institute — analyzing disease mechanisms",
                    new[]{ ("sci.med.pathology",0.7), ("sci.med.research",0.7) }),
            }),

        new("FB_MBBS_BROAD_02", QuizStage.BroadFieldDiscovery, "patient_interaction_preference",
            "Which type of patient relationship resonates with you most?",
            new[] {
                new FallbackOption("Long-term — knowing patients over years, managing chronic conditions",
                    new[]{ ("sci.med.family",0.9), ("sci.med.internal",0.5) }),
                new FallbackOption("High-acuity — brief, intense encounters in life-or-death situations",
                    new[]{ ("sci.med.emergency",0.9) }),
                new FallbackOption("Children and families — building trust with pediatric patients",
                    new[]{ ("sci.med.pediatrics",0.9) }),
                new FallbackOption("Minimal direct contact — results-driven diagnosis with limited bedside time",
                    new[]{ ("sci.med.radiology",0.8), ("sci.med.pathology",0.8) }),
            }),

        new("FB_MBBS_BROAD_03", QuizStage.BroadFieldDiscovery, "hands_on_vs_cognitive",
            "Which medical skill excites you most to develop?",
            new[] {
                new FallbackOption("Surgical dexterity — mastering complex operative techniques",
                    new[]{ ("sci.med.surgery",0.95) }),
                new FallbackOption("Clinical reasoning — diagnosing rare conditions from subtle clues",
                    new[]{ ("sci.med.internal",0.9), ("sci.med.neurology",0.5) }),
                new FallbackOption("Procedural skills in a high-stakes environment (intubation, central lines)",
                    new[]{ ("sci.med.emergency",0.9), ("sci.med.anesthesiology",0.6) }),
                new FallbackOption("Population-level thinking — designing programs to prevent disease at scale",
                    new[]{ ("sci.med.public_health",0.95) }),
            }),

        new("FB_MBBS_BROAD_04", QuizStage.BroadFieldDiscovery, "research_vs_practice",
            "Where do you see your biggest contribution to medicine?",
            new[] {
                new FallbackOption("Discovering new treatments through clinical trials and basic research",
                    new[]{ ("sci.med.research",0.9) }),
                new FallbackOption("Providing exceptional direct care to individual patients",
                    new[]{ ("sci.med.internal",0.7), ("sci.med.family",0.7) }),
                new FallbackOption("Teaching the next generation of doctors in academic medicine",
                    new[]{ ("edu.research",0.7), ("sci.med.research",0.5) }),
                new FallbackOption("Transforming healthcare delivery at hospital or policy level",
                    new[]{ ("sci.med.public_health",0.8) }),
            }),

        // --- FieldConfirmation: Surgery ---
        new("FB_MBBS_SURG_CONF_01", QuizStage.FieldConfirmation, "surgery_type_preference",
            "If you pursue surgery, which aspect of operative work excites you most?",
            new[] {
                new FallbackOption("General surgery — abdominal, GI, laparoscopic procedures",
                    new[]{ ("sci.med.surgery.general",0.9), ("sci.med.surgery",0.6) }),
                new FallbackOption("Orthopedic surgery — bones, joints, trauma reconstruction",
                    new[]{ ("sci.med.surgery.ortho",0.9), ("sci.med.surgery",0.6) }),
                new FallbackOption("Neurosurgery — brain and spinal cord procedures",
                    new[]{ ("sci.med.surgery.neuro",0.9), ("sci.med.surgery",0.6) }),
                new FallbackOption("Cardiac or vascular surgery — heart and major vessel operations",
                    new[]{ ("sci.med.cardiology",0.7), ("sci.med.surgery",0.7) }),
            }),

        // --- SubdomainDiscovery: Medical ---
        new("FB_MBBS_SUB_01", QuizStage.SubdomainDiscovery, "specialty_discrimination",
            "A 45-year-old presents with chest pain and shortness of breath. What is your immediate focus?",
            new[] {
                new FallbackOption("Rule out MI — check ECG, troponins, arrange emergency management",
                    new[]{ ("sci.med.cardiology",0.9), ("sci.med.emergency",0.5) }),
                new FallbackOption("Assess airway, breathing, circulation — stabilize first, diagnose second",
                    new[]{ ("sci.med.emergency",0.9), ("sci.med.anesthesiology",0.4) }),
                new FallbackOption("Order a CT chest and review imaging to rule out PE or aortic dissection",
                    new[]{ ("sci.med.radiology",0.8), ("sci.med.internal",0.5) }),
                new FallbackOption("Take a thorough history to identify a chronic underlying cause (COPD, HF)",
                    new[]{ ("sci.med.internal",0.9) }),
            }),

        new("FB_MBBS_SUB_02", QuizStage.SubdomainDiscovery, "mental_vs_physical",
            "Which of these specialized practices most aligns with your vision of medicine?",
            new[] {
                new FallbackOption("Psychiatry — understanding the mind, treating mental illness, psychotherapy",
                    new[]{ ("sci.med.psychiatry",0.95) }),
                new FallbackOption("Dermatology — skin disorders, aesthetic procedures, outpatient practice",
                    new[]{ ("sci.med.dermatology",0.95) }),
                new FallbackOption("Obstetrics & Gynecology — pregnancy care, labor, women''s health",
                    new[]{ ("sci.med.obg",0.95) }),
                new FallbackOption("Anesthesiology — pain management, sedation, perioperative care",
                    new[]{ ("sci.med.anesthesiology",0.95) }),
            }),

        new("FB_MBBS_SUB_03", QuizStage.SubdomainDiscovery, "primary_vs_specialist",
            "Do you prefer being the first point of contact or a referred specialist?",
            new[] {
                new FallbackOption("First contact — I want to know every patient and manage their full health",
                    new[]{ ("sci.med.family",0.9) }),
                new FallbackOption("Specialist — I want deep expertise in one organ system or condition",
                    new[]{ ("sci.med.neurology",0.6), ("sci.med.cardiology",0.6) }),
                new FallbackOption("Emergency — I want to see undifferentiated cases and stabilize them rapidly",
                    new[]{ ("sci.med.emergency",0.9) }),
                new FallbackOption("Academic specialist — see complex referrals AND conduct research",
                    new[]{ ("sci.med.research",0.7), ("sci.med.internal",0.5) }),
            }),

        new("FB_MBBS_CONFIRM_02", QuizStage.FieldConfirmation, "acute_vs_longitudinal_care",
            "Which responsibility would you most want to take on during a difficult shift?",
            new[] {
                new FallbackOption("Rapidly assess unstable patients and decide who needs immediate intervention",
                    new[]{ ("sci.med.emergency",0.95), ("sci.med.anesthesiology",0.35) }),
                new FallbackOption("Coordinate ongoing treatment for patients with several medical conditions",
                    new[]{ ("sci.med.internal",0.9), ("sci.med.cardiology",0.45) }),
                new FallbackOption("Perform an operation that directly addresses the problem",
                    new[]{ ("sci.med.surgery",0.95) }),
                new FallbackOption("Interpret tests and give the care team a precise diagnosis",
                    new[]{ ("sci.med.radiology",0.7), ("sci.med.pathology",0.7) }),
            }),

        new("FB_MBBS_CONFIRM_03", QuizStage.FieldConfirmation, "specialty_work_environment",
            "Which workday would you find most rewarding?",
            new[] {
                new FallbackOption("A fast-moving emergency unit with varied urgent cases",
                    new[]{ ("sci.med.emergency",0.95) }),
                new FallbackOption("An operating theatre focused on procedures and perioperative care",
                    new[]{ ("sci.med.surgery",0.75), ("sci.med.anesthesiology",0.75) }),
                new FallbackOption("A clinic building trust with patients over repeated visits",
                    new[]{ ("sci.med.family",0.8), ("sci.med.pediatrics",0.55) }),
                new FallbackOption("A diagnostic service using scans, specimens, and clinical evidence",
                    new[]{ ("sci.med.radiology",0.65), ("sci.med.pathology",0.65) }),
            }),

        new("FB_MBBS_SUB_04", QuizStage.SubdomainDiscovery, "organ_system_interest",
            "Which area of the body or health would you most like to specialize in?",
            new[] {
                new FallbackOption("Heart and circulation, including complex cardiovascular disease",
                    new[]{ ("sci.med.cardiology",0.95) }),
                new FallbackOption("Brain, nerves, and disorders of movement or sensation",
                    new[]{ ("sci.med.neurology",0.95) }),
                new FallbackOption("Pregnancy, childbirth, and reproductive health",
                    new[]{ ("sci.med.obg",0.95) }),
                new FallbackOption("Skin conditions and outpatient procedures",
                    new[]{ ("sci.med.dermatology",0.95) }),
            }),

        new("FB_MBBS_SUB_05", QuizStage.SubdomainDiscovery, "care_delivery_preference",
            "How would you prefer to make a difference in a patient's care?",
            new[] {
                new FallbackOption("Operate or perform procedures to treat the underlying condition",
                    new[]{ ("sci.med.surgery",0.9) }),
                new FallbackOption("Manage medicines and long-term care for complex medical illness",
                    new[]{ ("sci.med.internal",0.85), ("sci.med.cardiology",0.45) }),
                new FallbackOption("Keep patients safe through anaesthesia, pain control, and critical monitoring",
                    new[]{ ("sci.med.anesthesiology",0.95) }),
                new FallbackOption("Support children and families through illness and development",
                    new[]{ ("sci.med.pediatrics",0.95) }),
            }),

        new("FB_MBBS_SUB_06", QuizStage.SubdomainDiscovery, "medical_evidence_preference",
            "Which evidence would you most enjoy working with to solve a clinical problem?",
            new[] {
                new FallbackOption("Patient history, examination findings, and changes over time",
                    new[]{ ("sci.med.internal",0.8), ("sci.med.family",0.55) }),
                new FallbackOption("Imaging studies that reveal anatomy and disease patterns",
                    new[]{ ("sci.med.radiology",0.95) }),
                new FallbackOption("Microscopy, laboratory results, and tissue findings",
                    new[]{ ("sci.med.pathology",0.95) }),
                new FallbackOption("Mental-health assessment and a patient's lived experience",
                    new[]{ ("sci.med.psychiatry",0.95) }),
            }),

        new("FB_MBBS_FINAL_01", QuizStage.FinalValidation, "specialty_validation_acute",
            "A patient suddenly becomes critically unwell. Which role would you most want to fill?",
            new[] {
                new FallbackOption("Lead the initial assessment and stabilisation",
                    new[]{ ("sci.med.emergency",0.95) }),
                new FallbackOption("Manage the airway, anaesthesia, and vital support",
                    new[]{ ("sci.med.anesthesiology",0.95) }),
                new FallbackOption("Take the patient for urgent operative treatment",
                    new[]{ ("sci.med.surgery",0.95) }),
                new FallbackOption("Interpret urgent imaging to guide the next decision",
                    new[]{ ("sci.med.radiology",0.9) }),
            }),

        new("FB_MBBS_FINAL_02", QuizStage.FinalValidation, "specialty_validation_diagnostic",
            "Which difficult diagnostic task would you be happiest to own?",
            new[] {
                new FallbackOption("Identify the cause of a complex heart rhythm or circulation problem",
                    new[]{ ("sci.med.cardiology",0.95) }),
                new FallbackOption("Recognize disease patterns in tissue or blood samples",
                    new[]{ ("sci.med.pathology",0.95) }),
                new FallbackOption("Connect symptoms with detailed imaging findings",
                    new[]{ ("sci.med.radiology",0.95) }),
                new FallbackOption("Understand how mental health affects symptoms and recovery",
                    new[]{ ("sci.med.psychiatry",0.95) }),
            }),

        new("FB_MBBS_FINAL_03", QuizStage.FinalValidation, "specialty_validation_patient_group",
            "Which patient group would you most like to care for throughout your specialty training?",
            new[] {
                new FallbackOption("Infants, children, and adolescents",
                    new[]{ ("sci.med.pediatrics",0.95) }),
                new FallbackOption("People during pregnancy and reproductive-health care",
                    new[]{ ("sci.med.obg",0.95) }),
                new FallbackOption("People with long-term or multiple adult health conditions",
                    new[]{ ("sci.med.internal",0.85), ("sci.med.family",0.5) }),
                new FallbackOption("People needing immediate care after illness or injury",
                    new[]{ ("sci.med.emergency",0.95) }),
            }),

        new("FB_MBBS_FINAL_04", QuizStage.FinalValidation, "specialty_validation_work_style",
            "Which professional strength would you most want your medical specialty to use every day?",
            new[] {
                new FallbackOption("Calm, rapid decisions when time is critical",
                    new[]{ ("sci.med.emergency",0.9), ("sci.med.anesthesiology",0.45) }),
                new FallbackOption("Fine procedural skill and comfort with operating",
                    new[]{ ("sci.med.surgery",0.95) }),
                new FallbackOption("Careful interpretation of complex diagnostic evidence",
                    new[]{ ("sci.med.radiology",0.7), ("sci.med.pathology",0.7) }),
                new FallbackOption("Listening, communication, and sustained therapeutic relationships",
                    new[]{ ("sci.med.psychiatry",0.7), ("sci.med.family",0.65) }),
            }),

        new("FB_MBBS_FINAL_05", QuizStage.FinalValidation, "specialty_validation_future",
            "Which kind of impact would make you proud of your medical career?",
            new[] {
                new FallbackOption("Saving lives through timely emergency treatment",
                    new[]{ ("sci.med.emergency",0.95) }),
                new FallbackOption("Restoring health through a successful operation",
                    new[]{ ("sci.med.surgery",0.95) }),
                new FallbackOption("Helping patients manage a serious condition over many years",
                    new[]{ ("sci.med.internal",0.75), ("sci.med.cardiology",0.6) }),
                new FallbackOption("Improving diagnoses through research, imaging, or laboratory medicine",
                    new[]{ ("sci.med.research",0.6), ("sci.med.radiology",0.6), ("sci.med.pathology",0.6) }),
            }),
    };

    // ── B.Com / Finance Field Questions ─────────────────────────────────────

    private static readonly List<FallbackQuestion> _bcomQuestions = new()
    {
        new("FB_BCOM_BROAD_01", QuizStage.BroadFieldDiscovery, "numbers_vs_strategy",
            "In a business context, which role do you find most rewarding?",
            new[] {
                new FallbackOption("The numbers expert — ensuring financial accuracy and compliance",
                    new[]{ ("com.fin.ca",0.8), ("com.fin.audit",0.6) }),
                new FallbackOption("The strategist — using financial data to guide business decisions",
                    new[]{ ("com.fin.cfa",0.8), ("com.mgmt",0.5) }),
                new FallbackOption("The deal-maker — raising capital, M&A, investment banking",
                    new[]{ ("com.fin.banking",0.9) }),
                new FallbackOption("The tax specialist — optimizing tax strategy and ensuring legal compliance",
                    new[]{ ("com.fin.tax",0.9) }),
            }),

        new("FB_BCOM_BROAD_02", QuizStage.BroadFieldDiscovery, "client_vs_internal",
            "Do you prefer client-facing advisory work or internal financial operations?",
            new[] {
                new FallbackOption("Advisory — consulting businesses on financial strategy and risk",
                    new[]{ ("com.fin.cfa",0.8), ("com.mgmt",0.5) }),
                new FallbackOption("Operational — managing the books, reconciliations, and financial reporting",
                    new[]{ ("com.fin.ca",0.8), ("com.fin.audit",0.5) }),
                new FallbackOption("Client-facing banking — managing portfolios and relationships",
                    new[]{ ("com.fin.banking",0.9) }),
                new FallbackOption("Fintech — building digital financial products and services",
                    new[]{ ("com.fin.fintech",0.9) }),
            }),
    };

    // ── Mechanical Engineering Field Questions ───────────────────────────────

    private static readonly List<FallbackQuestion> _mechQuestions = new()
    {
        new("FB_MECH_BROAD_01", QuizStage.BroadFieldDiscovery, "design_vs_manufacturing",
            "Within mechanical engineering, which area excites you most?",
            new[] {
                new FallbackOption("Designing new products using CAD/CAE tools",
                    new[]{ ("sci.eng.mech.design",0.9) }),
                new FallbackOption("Optimizing manufacturing processes and production efficiency",
                    new[]{ ("sci.eng.mech",0.8) }),
                new FallbackOption("Automotive systems — engines, drivetrains, vehicle dynamics",
                    new[]{ ("sci.eng.mech.auto",0.9) }),
                new FallbackOption("Robotics and automation — mechatronics and intelligent machines",
                    new[]{ ("sci.eng.mech.robot",0.9) }),
            }),

        new("FB_MECH_BROAD_02", QuizStage.BroadFieldDiscovery, "aerospace_vs_thermal",
            "If you had to specialize right now, which area would you choose?",
            new[] {
                new FallbackOption("Aerospace — aircraft structures, propulsion, aerodynamics",
                    new[]{ ("sci.eng.mech.aero",0.95) }),
                new FallbackOption("Thermal/HVAC — heat transfer, energy systems, refrigeration",
                    new[]{ ("sci.eng.mech",0.7) }),
                new FallbackOption("R&D — materials science, simulation, research projects",
                    new[]{ ("sci.eng.mech",0.7), ("edu.research",0.4) }),
                new FallbackOption("Project management for large-scale manufacturing or infrastructure",
                    new[]{ ("com.mgmt",0.7), ("sci.eng.mech",0.4) }),
            }),
    };

    // BA / humanities questions cover common pathways that are otherwise absent
    // from the seeded degree-specific question pool.
    private static readonly List<FallbackQuestion> _baQuestions = new()
    {
        new("FB_BA_BROAD_01", QuizStage.BroadFieldDiscovery, "humanities_work_style",
            "Which kind of work would you most like to build on your BA studies?",
            new[] {
                new FallbackOption("Researching society and evaluating community programmes", new[]{ ("role_social_researcher",0.9), ("role_programme_officer",0.7) }),
                new FallbackOption("Writing, editing, or reporting stories for an audience", new[]{ ("role_journalist",0.9), ("role_editor",0.7) }),
                new FallbackOption("Supporting people through education or social services", new[]{ ("role_social_worker",0.8), ("role_career_counsellor",0.7) }),
                new FallbackOption("Studying policy and helping improve public programmes", new[]{ ("role_public_policy_analyst",0.9), ("role_development_officer",0.7) }),
            }),
        new("FB_BA_BROAD_02", QuizStage.BroadFieldDiscovery, "humanities_strengths",
            "Which strength from your BA would you most like to use at work?",
            new[] {
                new FallbackOption("Careful reading, evidence gathering, and clear writing", new[]{ ("role_research_analyst",0.8), ("role_editor",0.7) }),
                new FallbackOption("Understanding people, groups, and social behaviour", new[]{ ("role_sociologist",0.9), ("role_social_researcher",0.7) }),
                new FallbackOption("Explaining ideas and helping others learn", new[]{ ("role_curriculum_developer",0.8), ("role_career_counsellor",0.7) }),
                new FallbackOption("Making a case, interpreting rules, and advocating for change", new[]{ ("role_public_administration_officer",0.8), ("role_public_policy_analyst",0.7) }),
            }),
        new("FB_BA_BROAD_03", QuizStage.BroadFieldDiscovery, "humanities_subject_direction",
            "Which area from arts and humanities would you most like to explore further?",
            new[] {
                new FallbackOption("Psychology — understand behaviour and support people or research", new[]{ ("role_career_counsellor",0.8), ("role_social_researcher",0.65) }),
                new FallbackOption("English and languages — write, edit, teach, or translate", new[]{ ("role_editor",0.8), ("role_journalist",0.7), ("role_translator",0.6) }),
                new FallbackOption("History and heritage — research sources and make the past accessible", new[]{ ("role_archivist",0.8), ("role_museum_educator",0.7) }),
                new FallbackOption("Political science — study governance, policy, and public service", new[]{ ("role_public_policy_analyst",0.8), ("role_public_administration_officer",0.7) }),
            }),
        new("FB_BA_BROAD_04", QuizStage.BroadFieldDiscovery, "humanities_arts_and_social_sciences",
            "Which kind of arts or social-science question would you enjoy working on?",
            new[] {
                new FallbackOption("How communities change and which programmes improve people's lives", new[]{ ("role_social_researcher",0.8), ("role_community_development_officer",0.7) }),
                new FallbackOption("How economic trends affect households and public decisions", new[]{ ("role_economic_analyst",0.85), ("role_public_policy_analyst",0.6) }),
                new FallbackOption("How places, maps, and cities shape people's lives", new[]{ ("role_geospatial_analyst",0.8), ("role_urban_planner",0.7) }),
                new FallbackOption("How stories and media communicate ideas creatively", new[]{ ("role_journalist",0.75), ("role_content_strategist",0.75) }),
            }),
        new("FB_BA_BROAD_05", QuizStage.BroadFieldDiscovery, "humanities_education_and_creative_arts",
            "Which arts or education direction would you most like to try?",
            new[] {
                new FallbackOption("Education — teach, guide learners, or design useful lessons", new[]{ ("role_curriculum_developer",0.85), ("role_career_counsellor",0.65) }),
                new FallbackOption("Fine arts and heritage — create work or help people engage with collections", new[]{ ("role_museum_educator",0.8), ("role_content_strategist",0.65) }),
                new FallbackOption("Languages — translate, interpret, and communicate across cultures", new[]{ ("role_translator",0.85), ("role_editor",0.65) }),
                new FallbackOption("Philosophy and ethics — examine arguments and real-world dilemmas", new[]{ ("role_ethics_researcher",0.85), ("role_social_research_analyst",0.65) }),
            }),
        new("FB_BA_CONFIRM_01", QuizStage.FieldConfirmation, "humanities_pathway",
            "Which early-career project would you choose?",
            new[] {
                new FallbackOption("Interview people and analyze findings for a social research study", new[]{ ("role_social_researcher",0.95) }),
                new FallbackOption("Develop a reported feature from interviews and document research", new[]{ ("role_journalist",0.9), ("role_editor",0.6) }),
                new FallbackOption("Coordinate a local education or development programme", new[]{ ("role_programme_officer",0.9), ("role_development_officer",0.7) }),
                new FallbackOption("Compare policy options and write an evidence-based brief", new[]{ ("role_public_policy_analyst",0.95) }),
            }),
        new("FB_BA_CONFIRM_02", QuizStage.FieldConfirmation, "humanities_next_step",
            "Which next step sounds most motivating after your BA?",
            new[] {
                new FallbackOption("A master's degree and research in my subject", new[]{ ("role_social_researcher",0.8), ("role_sociologist",0.7) }),
                new FallbackOption("Journalism, communication, or a portfolio of published work", new[]{ ("role_journalist",0.9), ("role_content_strategist",0.6) }),
                new FallbackOption("Teacher training or further study in education", new[]{ ("role_curriculum_developer",0.8), ("role_instructional_designer",0.7) }),
                new FallbackOption("Preparation for public service, policy, or law-related study", new[]{ ("role_public_administration_officer",0.8), ("role_public_policy_analyst",0.7) }),
            }),
        new("FB_BA_CONFIRM_03", QuizStage.FieldConfirmation, "humanities_applied_task",
            "Which arts-related task would you be most interested in trying as a short project?",
            new[] {
                new FallbackOption("Interview people and write an accurate feature about their experiences", new[]{ ("role_journalist",0.85), ("role_editor",0.6) }),
                new FallbackOption("Review historical sources and create an exhibit or archive entry", new[]{ ("role_archivist",0.8), ("role_museum_educator",0.75) }),
                new FallbackOption("Study a local issue and present evidence-based recommendations", new[]{ ("role_public_policy_analyst",0.85), ("role_social_researcher",0.65) }),
                new FallbackOption("Design a learning activity that explains a difficult idea", new[]{ ("role_curriculum_developer",0.85), ("role_instructional_designer",0.7) }),
            }),
        new("FB_BA_SUB_01", QuizStage.SubdomainDiscovery, "humanities_subdomain",
            "Which BA-related specialization would you like to explore more deeply?",
            new[] {
                new FallbackOption("Social research, surveys, and programme evaluation", new[]{ ("role_social_researcher",0.95), ("role_sociologist",0.7) }),
                new FallbackOption("Reporting, editing, and public communication", new[]{ ("role_journalist",0.9), ("role_editor",0.8) }),
                new FallbackOption("Teaching, curriculum, and learner support", new[]{ ("role_curriculum_developer",0.9), ("role_career_counsellor",0.7) }),
                new FallbackOption("Governance, public policy, and development", new[]{ ("role_public_policy_analyst",0.9), ("role_development_officer",0.7) }),
            }),
        new("FB_BA_SUB_02", QuizStage.SubdomainDiscovery, "humanities_application",
            "Where would you prefer to apply your humanities or social-science knowledge?",
            new[] {
                new FallbackOption("Universities, research organizations, or think tanks", new[]{ ("role_social_researcher",0.9), ("role_public_policy_analyst",0.7) }),
                new FallbackOption("Newsrooms, publishers, or digital media teams", new[]{ ("role_journalist",0.9), ("role_editor",0.7) }),
                new FallbackOption("Schools, learning organizations, or student services", new[]{ ("role_curriculum_developer",0.9), ("role_career_counsellor",0.7) }),
                new FallbackOption("Government, NGOs, or community programmes", new[]{ ("role_public_administration_officer",0.8), ("role_programme_officer",0.8) }),
            }),
        new("FB_BA_SUB_03", QuizStage.SubdomainDiscovery, "humanities_discipline_focus",
            "Which subject direction would you want to build deeper skills in?",
            new[] {
                new FallbackOption("Psychology, counselling skills, and human behaviour", new[]{ ("role_career_counsellor",0.8), ("role_psychology_researcher",0.75) }),
                new FallbackOption("Economics, data interpretation, and social outcomes", new[]{ ("role_economic_analyst",0.85), ("role_social_researcher",0.6) }),
                new FallbackOption("Geography, GIS, and sustainable places", new[]{ ("role_geospatial_analyst",0.85), ("role_urban_planner",0.7) }),
                new FallbackOption("Languages, translation, and editorial communication", new[]{ ("role_translator",0.8), ("role_editor",0.75) }),
            }),
        new("FB_BA_SUB_04", QuizStage.SubdomainDiscovery, "humanities_education_and_creative_arts",
            "How would you prefer to use your learning in arts and humanities?",
            new[] {
                new FallbackOption("Teach a subject or create useful learning materials", new[]{ ("role_curriculum_developer",0.85), ("role_instructional_designer",0.7) }),
                new FallbackOption("Create visual or media work that informs an audience", new[]{ ("role_content_strategist",0.8), ("role_journalist",0.65) }),
                new FallbackOption("Preserve and explain cultural or historical collections", new[]{ ("role_archivist",0.8), ("role_museum_educator",0.75) }),
                new FallbackOption("Support people through guidance or community services", new[]{ ("role_career_counsellor",0.75), ("role_social_worker",0.75) }),
            }),
        new("FB_BA_FINAL_01", QuizStage.FinalValidation, "humanities_environment_fit",
            "Which work environment would keep you motivated over time?",
            new[] {
                new FallbackOption("Independent research with time to read, analyze, and write", new[]{ ("role_social_researcher",0.9) }),
                new FallbackOption("A fast-moving newsroom or communications team", new[]{ ("role_journalist",0.9), ("role_editor",0.7) }),
                new FallbackOption("A collaborative teaching or learner-support setting", new[]{ ("role_curriculum_developer",0.9), ("role_career_counsellor",0.7) }),
                new FallbackOption("A public-service or development team focused on measurable outcomes", new[]{ ("role_development_officer",0.9), ("role_public_policy_analyst",0.7) }),
            }),
        new("FB_BA_FINAL_02", QuizStage.FinalValidation, "humanities_outcome_fit",
            "Which outcome would make you feel your arts degree is being put to good use?",
            new[] {
                new FallbackOption("People understand a social issue better because of my research", new[]{ ("role_social_researcher",0.85), ("role_sociologist",0.7) }),
                new FallbackOption("A clear story helps an audience understand an issue", new[]{ ("role_journalist",0.8), ("role_editor",0.75) }),
                new FallbackOption("A public or community programme becomes more effective", new[]{ ("role_development_officer",0.8), ("role_public_policy_analyst",0.7) }),
                new FallbackOption("Learners gain confidence from better teaching or guidance", new[]{ ("role_curriculum_developer",0.8), ("role_career_counsellor",0.7) }),
            }),
    };

    private static readonly List<FallbackQuestion> _baPostgraduateQuestions = new()
    {
        new("FB_BA_PG_BROAD_01", QuizStage.BroadFieldDiscovery, "humanities_postgraduate_direction",
            "How would you most like to use your postgraduate study in an arts or social-science field?",
            new[] {
                new FallbackOption("Conduct original research and continue toward a PhD", new[]{ ("role_social_researcher",0.9), ("role_sociologist",0.7) }),
                new FallbackOption("Teach, design learning, or support students", new[]{ ("role_curriculum_developer",0.9), ("role_career_counsellor",0.7) }),
                new FallbackOption("Apply specialist knowledge in policy or development programmes", new[]{ ("role_public_policy_analyst",0.9), ("role_development_officer",0.7) }),
                new FallbackOption("Use advanced writing and subject expertise in media or publishing", new[]{ ("role_editor",0.9), ("role_journalist",0.7) }),
            }),
        new("FB_BA_PG_CONFIRM_01", QuizStage.FieldConfirmation, "humanities_postgraduate_work",
            "Which postgraduate-level responsibility would you most enjoy?",
            new[] {
                new FallbackOption("Design a study, analyze evidence, and publish findings", new[]{ ("role_social_researcher",0.95) }),
                new FallbackOption("Teach a subject and develop course materials", new[]{ ("role_curriculum_developer",0.9), ("role_instructional_designer",0.7) }),
                new FallbackOption("Evaluate a public programme and recommend policy changes", new[]{ ("role_public_policy_analyst",0.95) }),
                new FallbackOption("Lead editorial research and shape content for readers", new[]{ ("role_editor",0.9), ("role_content_strategist",0.7) }),
            }),
        new("FB_BA_PG_CONFIRM_02", QuizStage.FieldConfirmation, "humanities_postgraduate_next_step",
            "What would make further study feel worthwhile to you?",
            new[] {
                new FallbackOption("A clear route to doctoral research and an academic career", new[]{ ("role_social_researcher",0.9), ("role_sociologist",0.7) }),
                new FallbackOption("Advanced preparation for teaching or learning design", new[]{ ("role_curriculum_developer",0.9) }),
                new FallbackOption("Deeper expertise for public policy or development work", new[]{ ("role_public_policy_analyst",0.9), ("role_development_officer",0.7) }),
                new FallbackOption("A stronger portfolio for journalism, editing, or communications", new[]{ ("role_journalist",0.8), ("role_editor",0.8) }),
            }),
        new("FB_BA_PG_SUB_01", QuizStage.SubdomainDiscovery, "humanities_postgraduate_specialization",
            "Which area would you most like to become a recognized specialist in?",
            new[] {
                new FallbackOption("Research methods, social evidence, and programme evaluation", new[]{ ("role_social_researcher",0.95) }),
                new FallbackOption("Curriculum design, teaching, and learning outcomes", new[]{ ("role_curriculum_developer",0.95) }),
                new FallbackOption("Policy analysis, governance, and development", new[]{ ("role_public_policy_analyst",0.9), ("role_development_officer",0.7) }),
                new FallbackOption("Editorial judgment, reporting, and audience strategy", new[]{ ("role_editor",0.9), ("role_journalist",0.7) }),
            }),
        new("FB_BA_PG_SUB_02", QuizStage.SubdomainDiscovery, "humanities_postgraduate_setting",
            "Which setting best fits your postgraduate expertise and preferred impact?",
            new[] {
                new FallbackOption("A university, research center, or policy think tank", new[]{ ("role_social_researcher",0.9), ("role_public_policy_analyst",0.7) }),
                new FallbackOption("An education organization or academic department", new[]{ ("role_curriculum_developer",0.9), ("role_career_counsellor",0.6) }),
                new FallbackOption("A government or development programme", new[]{ ("role_development_officer",0.9), ("role_public_administration_officer",0.7) }),
                new FallbackOption("A publication, newsroom, or communications team", new[]{ ("role_editor",0.9), ("role_journalist",0.7) }),
            }),
        new("FB_BA_PG_FINAL_01", QuizStage.FinalValidation, "humanities_postgraduate_impact",
            "At the end of a successful year, which result would matter most to you?",
            new[] {
                new FallbackOption("A strong study or publication that adds useful knowledge", new[]{ ("role_social_researcher",0.95) }),
                new FallbackOption("Learners making progress because of teaching or better materials", new[]{ ("role_curriculum_developer",0.95) }),
                new FallbackOption("A policy or programme change that improves people's lives", new[]{ ("role_public_policy_analyst",0.9), ("role_development_officer",0.7) }),
                new FallbackOption("Clear, accurate coverage or content reaching the right audience", new[]{ ("role_journalist",0.9), ("role_editor",0.7) }),
            }),
    };

    private static readonly List<FallbackQuestion> _architectureQuestions = new()
    {
        new("FB_ARCH_CONFIRM_01", QuizStage.FieldConfirmation, "architecture_management_interest",
            "Within architecture, which management responsibility would you most like to take on?",
            new[] {
                new FallbackOption("Coordinate a design project, schedule, budget, and consultant team", new[]{ ("arch.bim",0.7), ("sci.eng.civil.pm",0.6) }),
                new FallbackOption("Guide a city or neighborhood plan with public agencies and communities", new[]{ ("arch.urban",0.9) }),
                new FallbackOption("Manage a building project from client brief through design decisions", new[]{ ("arch.residential",0.8), ("arch.bim",0.5) }),
                new FallbackOption("Lead sustainability targets across a building project", new[]{ ("arch.sustainable",0.9) }),
            }),
        new("FB_ARCH_SUB_01", QuizStage.SubdomainDiscovery, "architecture_project_delivery",
            "A building project is falling behind. Which part would you want to organize first?",
            new[] {
                new FallbackOption("Coordinate the design, engineering, and contractor teams through a shared model", new[]{ ("arch.bim",0.9) }),
                new FallbackOption("Rework the site and public-space plan with local stakeholders", new[]{ ("arch.urban",0.9) }),
                new FallbackOption("Align client needs, cost choices, and interior decisions", new[]{ ("arch.interior",0.8), ("arch.residential",0.5) }),
                new FallbackOption("Review materials and performance targets to protect sustainability goals", new[]{ ("arch.sustainable",0.9) }),
            }),
        new("FB_ARCH_SUB_02", QuizStage.SubdomainDiscovery, "architecture_leadership_style",
            "Which architecture team would you most enjoy leading?",
            new[] {
                new FallbackOption("A multidisciplinary team delivering a coordinated building design", new[]{ ("arch.bim",0.9) }),
                new FallbackOption("A planning team shaping streets, public spaces, and movement", new[]{ ("arch.urban",0.9) }),
                new FallbackOption("A design team improving how people experience interior spaces", new[]{ ("arch.interior",0.9) }),
                new FallbackOption("A team reducing a project's energy and resource use", new[]{ ("arch.sustainable",0.9) }),
            }),
        new("FB_ARCH_FINAL_01", QuizStage.FinalValidation, "architecture_management_outcome",
            "What result would make you proud of managing an architecture project?",
            new[] {
                new FallbackOption("A coordinated project delivered to a clear brief and schedule", new[]{ ("arch.bim",0.8), ("sci.eng.civil.pm",0.5) }),
                new FallbackOption("A public space that works well for its community", new[]{ ("arch.urban",0.9) }),
                new FallbackOption("A comfortable building shaped around its users", new[]{ ("arch.residential",0.9) }),
                new FallbackOption("A building that meets ambitious sustainability goals", new[]{ ("arch.sustainable",0.9) }),
            }),
    };

    private static readonly List<FallbackQuestion> _bscQuestions = new()
    {
        new("FB_BSC_BROAD_01", QuizStage.BroadFieldDiscovery, "bsc_science_direction",
            "Which B.Sc field would you most like to explore further?",
            new[] {
                new FallbackOption("Understand matter, energy, and physical systems", new[]{ ("sci.pure.physics",0.9) }),
                new FallbackOption("Study chemical materials, reactions, and laboratory analysis", new[]{ ("sci.pure.chem",0.9) }),
                new FallbackOption("Explore organisms, biotechnology, and life-science research", new[]{ ("sci.pure.biotech",0.9) }),
                new FallbackOption("Use mathematics and statistics to find patterns in data", new[]{ ("sci.pure.math",0.9) }),
            }),
        new("FB_BSC_CONFIRM_01", QuizStage.FieldConfirmation, "bsc_work_direction",
            "After your B.Sc, which kind of work would you like to try first?",
            new[] {
                new FallbackOption("Run experiments and interpret results in a research or testing lab", new[]{ ("sci.pure.chem",0.7), ("sci.pure.biotech",0.6) }),
                new FallbackOption("Analyze physical systems, instruments, or energy technologies", new[]{ ("sci.pure.physics",0.9) }),
                new FallbackOption("Work with ecology, conservation, or environmental evidence", new[]{ ("sci.pure.env_sci",0.9) }),
                new FallbackOption("Apply statistics and quantitative analysis to real decisions", new[]{ ("sci.pure.math",0.9) }),
            }),
        new("FB_BSC_SUB_01", QuizStage.SubdomainDiscovery, "bsc_method_preference",
            "Which scientific task sounds most engaging to you?",
            new[] {
                new FallbackOption("Design a controlled experiment and explain what the measurements show", new[]{ ("sci.pure.physics",0.7), ("sci.pure.chem",0.6) }),
                new FallbackOption("Study cells, organisms, or biological processes", new[]{ ("sci.pure.biotech",0.9) }),
                new FallbackOption("Collect field observations and assess environmental change", new[]{ ("sci.pure.env_sci",0.9) }),
                new FallbackOption("Model a problem with mathematics, probability, or statistics", new[]{ ("sci.pure.math",0.9) }),
            }),
        new("FB_BSC_SUB_02", QuizStage.SubdomainDiscovery, "bsc_application_preference",
            "Where would you most like to apply your science knowledge?",
            new[] {
                new FallbackOption("Research, education, or scientific instrumentation", new[]{ ("sci.pure.physics",0.8) }),
                new FallbackOption("Quality testing, materials, pharmaceuticals, or chemical analysis", new[]{ ("sci.pure.chem",0.9) }),
                new FallbackOption("Biotechnology, healthcare research, or life-science products", new[]{ ("sci.pure.biotech",0.9) }),
                new FallbackOption("Climate, sustainability, ecology, or environmental assessment", new[]{ ("sci.pure.env_sci",0.9) }),
            }),
        new("FB_BSC_FINAL_01", QuizStage.FinalValidation, "bsc_impact_preference",
            "Which outcome would make your next step after a B.Sc feel worthwhile?",
            new[] {
                new FallbackOption("A reliable scientific result that helps answer an important question", new[]{ ("sci.pure.physics",0.7), ("sci.pure.chem",0.6) }),
                new FallbackOption("A useful discovery or process developed from biological research", new[]{ ("sci.pure.biotech",0.9) }),
                new FallbackOption("Evidence that helps protect ecosystems or improve environmental decisions", new[]{ ("sci.pure.env_sci",0.9) }),
                new FallbackOption("A clear quantitative insight that improves a real-world decision", new[]{ ("sci.pure.math",0.9) }),
            }),
    };

    // ── Generic fallback (when field cannot be determined) ──────────────────

    private static readonly List<FallbackQuestion> _genericQuestions = new()
    {
        new("FB_GEN_01", QuizStage.BroadFieldDiscovery, "domain_preference",
            "Which broad professional domain feels most natural to you?",
            new[] {
                new FallbackOption("Technology — software, data, AI, or cybersecurity",
                    new[]{ ("sci.eng.cs",0.8) }),
                new FallbackOption("Healthcare — medicine, clinical practice, or medical research",
                    new[]{ ("sci.med",0.8) }),
                new FallbackOption("Commerce — finance, accounting, banking, or business strategy",
                    new[]{ ("com.fin",0.8), ("com.mgmt",0.5) }),
                new FallbackOption("Engineering — mechanical, civil, electrical, or aerospace",
                    new[]{ ("sci.eng.mech",0.6), ("sci.eng.civil",0.6) }),
            }),

        new("FB_GEN_02", QuizStage.BroadFieldDiscovery, "work_environment",
            "Which work environment suits your personality best?",
            new[] {
                new FallbackOption("High-intensity, fast-paced environments with critical decisions under pressure",
                    new[]{ ("sci.med.emergency",0.6), ("sci.eng.cs.soc",0.6) }),
                new FallbackOption("Structured, analytical environments where precision and accuracy matter most",
                    new[]{ ("com.fin.ca",0.7), ("sci.med.pathology",0.5) }),
                new FallbackOption("Creative, collaborative environments where you build and experiment",
                    new[]{ ("sci.eng.cs.backend",0.6), ("sci.eng.mech.design",0.5) }),
                new FallbackOption("Service-oriented environments where you directly help individuals",
                    new[]{ ("sci.med.family",0.7), ("edu",0.5) }),
            }),
    };

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Select the next appropriate fallback question for the given profile and stage.
    /// Returns null if all questions for this combination have been exhausted.
    /// </summary>
    public static QuizQuestionV2? GetNextQuestion(
        string userMajor,
        QuizStage stage,
        List<string> usedFallbackIds,
        Dictionary<string, double> prior,
        string phaseKey = "AfterGraduation",
        List<string>? selectedFields = null,
        bool selectedMode = false)
    {
        if (selectedMode && selectedFields?.Any() == true)
        {
            var selectedFieldQuestion = IsArchitectureProfile(userMajor)
                ? CreateArchitectureSelectedFieldQuestion(selectedFields, phaseKey, stage.ToString())
                : QuizProfileClassifier.IsBachelorOfArts(userMajor, null)
                    ? CreateSelectedFieldQuestion(selectedFields, phaseKey, stage.ToString())
                    : CreateGeneralSelectedFieldQuestion(selectedFields, phaseKey, stage.ToString());
            if (selectedFieldQuestion != null && !usedFallbackIds.Contains(selectedFieldQuestion.Id, StringComparer.OrdinalIgnoreCase))
                return selectedFieldQuestion;
        }

        var pool = SelectPool(userMajor, phaseKey);
        var stageQuestions = pool
            .Where(q => q.Stage == stage && !usedFallbackIds.Contains(q.Id))
            .ToList();

        if (!stageQuestions.Any())
        {
            // Try one stage lookahead
            var nextStage = stage == QuizStage.BroadFieldDiscovery ? QuizStage.FieldConfirmation
                          : stage == QuizStage.FieldConfirmation ? QuizStage.SubdomainDiscovery
                          : stage == QuizStage.SubdomainDiscovery ? QuizStage.FinalValidation
                          : QuizStage.Completed;

            if (nextStage == QuizStage.Completed) return null;

            stageQuestions = pool
                .Where(q => q.Stage == nextStage && !usedFallbackIds.Contains(q.Id))
                .ToList();
        }

        if (!stageQuestions.Any()) return null;

        // Prefer questions that discriminate between top competing candidates
        var topCandidates = prior.OrderByDescending(kv => kv.Value).Take(4).Select(kv => kv.Key).ToHashSet();
        var best = stageQuestions
            .OrderByDescending(q => q.Options.Sum(o =>
                o.Signals.Count(s => topCandidates.Contains(s.NodeId))))
            .First();

        return MapToEntity(best, phaseKey);
    }

    private static QuizQuestionV2? CreateGeneralSelectedFieldQuestion(List<string>? selectedFields, string phaseKey, string stageOrId)
    {
        if (selectedFields == null || selectedFields.Count == 0) return null;
        var fields = selectedFields.Select(value => value.Trim()).Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList();
        if (fields.Count == 0) return null;

        var stage = Enum.TryParse<QuizStage>(stageOrId, out var parsedStage) ? parsedStage : QuizStage.BroadFieldDiscovery;
        if (stageOrId.StartsWith("FB_SELECTED_", StringComparison.OrdinalIgnoreCase))
        {
            var knownStage = Enum.GetValues<QuizStage>().FirstOrDefault(value =>
                stageOrId.Contains($"_{value}_", StringComparison.OrdinalIgnoreCase));
            if (Enum.IsDefined(knownStage)) stage = knownStage;
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", fields).ToLowerInvariant())))[..10];
        var id = stageOrId.StartsWith("FB_SELECTED_", StringComparison.OrdinalIgnoreCase)
            ? stageOrId
            : $"FB_SELECTED_{stage}_{hash}";
        var selectedLabels = string.Join(", ", fields);
        var representative = fields[0].ToLowerInvariant();
        var targetNode = GetSelectedFieldCareerNode(fields[0]);

        string[] taskLabels;
        if (representative.Contains("electrical") || representative.Contains("power system") || representative.Contains("renewable") || representative.Contains("power electronic") || representative.Contains("control system") || representative.Contains("electrical machine"))
        {
            targetNode = representative.Contains("renewable") ? "sci.eng.elec.renew" :
                representative.Contains("power system") ? "sci.eng.elec.power" : "sci.eng.elec";
            taskLabels = stage switch
            {
                _ when representative.Contains("power electronic") => new[] { "Design a converter for an electric-drive or energy application", "Select switching devices and reduce conversion losses", "Control power flow between a source, storage, and load", "Validate converter performance under changing operating conditions" },
                _ when representative.Contains("electrical design") => new[] { "Size cables and protection for a building or industrial system", "Prepare a safe single-line diagram and equipment layout", "Estimate loads and select suitable electrical equipment", "Review a design for code compliance and energy efficiency" },
                _ when representative.Contains("electrical machine") => new[] { "Design an efficient motor or generator for a specific application", "Analyze machine performance under different operating conditions", "Select and control an electrical drive for an industrial process", "Improve machine reliability, cooling, or energy efficiency" },
                _ when representative.Contains("control system") => new[] { "Design a feedback controller for a changing process", "Use sensors and instrumentation to monitor a system", "Automate a process while maintaining safety and reliability", "Tune a control system using measured performance data" },
                QuizStage.FieldConfirmation => new[] { "Plan a reliable grid connection for new generation capacity", "Diagnose a protection or fault problem in a power network", "Improve the efficiency and stability of an electrical machine", "Integrate renewable generation and storage into an existing grid" },
                QuizStage.SubdomainDiscovery => new[] { "Model load flow and identify network constraints", "Coordinate relay protection and fault isolation", "Evaluate grid-scale storage and renewable integration", "Design control strategies for stable power delivery" },
                QuizStage.FinalValidation => new[] { "A more reliable transmission and distribution network", "A protection system that isolates faults quickly", "A renewable energy system that operates consistently", "A power system design that meets safety and efficiency needs" },
                _ => new[] { "Design and analyze electrical power networks", "Improve protection, relays, and fault response", "Connect renewable sources and storage to the grid", "Develop control strategies for stable power delivery" }
            };
        }
        else if (representative.Contains("electronics") || representative.Contains("vlsi") || representative.Contains("embedded") || representative.Contains("telecommunication") || representative.Contains("signal"))
        {
            targetNode = "sci.eng.ece";
            taskLabels = new[] { "Design and verify an electronic circuit", "Develop embedded firmware for a connected device", "Analyze signals or communication-system performance", "Design and validate a VLSI block" };
        }
        else if (representative.Contains("aerospace") || representative.Contains("aeronautical") || representative.Contains("aviation") || representative.Contains("avionics") || representative.Contains("aircraft") || representative.Contains("space system"))
        {
            targetNode = "sci.eng.aero";
            taskLabels = new[] { "Analyze aerodynamic performance and airflow", "Develop or validate a propulsion system", "Design avionics or flight-control functionality", "Evaluate aircraft or spacecraft structures and materials" };
        }
        else if (representative.Contains("automobile") || representative.Contains("automotive") || representative.Contains("vehicle") || representative.Contains("powertrain"))
        {
            targetNode = "sci.eng.auto";
            taskLabels = new[] { "Design vehicle components or powertrains", "Improve electric-vehicle range and battery integration", "Test vehicle safety, performance, and reliability", "Develop manufacturing methods for vehicle production" };
        }
        else if (representative.Contains("mechanical") || representative.Contains("mechatronic") || representative.Contains("robot") || representative.Contains("thermal") || representative.Contains("machine design") || representative.Contains("manufactur") || representative.Contains("industrial engineering"))
        {
            targetNode = representative.Contains("robot") || representative.Contains("mechatronic") ? "sci.eng.mech.auto" : "sci.eng.mech";
            taskLabels = new[] { "Design and test a mechanical component or machine", "Improve a manufacturing process and product quality", "Model thermal behavior and energy efficiency", "Integrate sensors, controls, and mechanical systems" };
        }
        else if (representative.Contains("civil") || representative.Contains("structural") || representative.Contains("geotechnical") || representative.Contains("transport") || representative.Contains("construction") || representative.Contains("environmental engineering") || representative.Contains("water") || representative.Contains("irrigation"))
        {
            targetNode = "sci.eng.civil";
            taskLabels = new[] { "Analyze structural safety and material performance", "Plan transport or urban infrastructure", "Evaluate soil, foundation, and site conditions", "Coordinate construction quality, cost, and delivery" };
        }
        else if (representative.Contains("chemical") || representative.Contains("process engineering") || representative.Contains("petroleum"))
        {
            targetNode = "sci.eng.chem";
            taskLabels = new[] { "Optimize a chemical production process", "Improve plant safety and process control", "Develop or scale a product formulation", "Reduce energy use, emissions, or process waste" };
        }
        else if (representative.Contains("computer") || representative.Contains("software") || representative.Contains("data") || representative.Contains("cyber") || representative.Contains("cloud") || representative.Contains("artificial intelligence") || representative.Contains("machine learning"))
        {
            targetNode = representative.Contains("cyber") ? "sci.eng.cs.cybersec" :
                representative.Contains("data") ? "sci.eng.cs.data" : "sci.eng.cs";
            taskLabels = new[] { "Build and improve a software or data system", "Investigate a security or reliability problem", "Use data or models to answer a practical question", "Design a service that is dependable at scale" };
        }
        else if (representative.Contains("finance") || representative.Contains("risk") || representative.Contains("investment") || representative.Contains("account") || representative.Contains("bank"))
        {
            targetNode = "com.fin";
            taskLabels = new[] { "Analyze financial statements and business performance", "Evaluate investment, credit, or market risk", "Build a financial model to compare decisions", "Support audit, compliance, or financial reporting" };
        }
        else if (representative.Contains("management") || representative.Contains("marketing") || representative.Contains("human resource") || representative.Contains("operation") || representative.Contains("consult") || representative.Contains("business") || representative.Contains("supply chain"))
        {
            targetNode = "com.mgmt";
            taskLabels = new[] { "Use customer and market research to shape a business decision", "Improve a process, service, or supply chain", "Plan a product, brand, or market-entry strategy", "Lead a team and track delivery against business goals" };
        }
        else if (representative.Contains("design") || representative.Contains("fashion") || representative.Contains("graphic") || representative.Contains("visual"))
        {
            targetNode = "design";
            taskLabels = new[] { "Research user needs and develop a design concept", "Create and refine visual or product prototypes", "Test a design with its intended audience", "Prepare a design system and communicate decisions" };
        }
        else if (representative.Contains("medicine") || representative.Contains("surgery") || representative.Contains("clinical") || representative.Contains("pharma") || representative.Contains("health") || representative.Contains("biomedical") || representative.Contains("medical device") || representative.Contains("biomechanics"))
        {
            targetNode = representative.Contains("pharma") ? "sci.med.pharma" : "sci.med";
            taskLabels = new[] { "Use evidence to support a safe, effective care decision", "Plan or evaluate a clinical or public-health study", "Improve the quality and coordination of healthcare", "Explain health information clearly to patients or teams" };
        }
        else if (representative.Contains("law") || representative.Contains("legal") || representative.Contains("litigation"))
        {
            targetNode = "arts.law";
            taskLabels = new[] { "Analyze case law and prepare a clear legal argument", "Draft or review agreements and legal documents", "Negotiate a fair resolution to a dispute", "Research compliance, policy, or regulatory requirements" };
        }
        else if (representative.Contains("agricultur") || representative.Contains("precision agriculture") || representative.Contains("farm machinery"))
        {
            targetNode = "sci.agri";
            taskLabels = new[] { "Develop technology that helps farms use inputs efficiently", "Improve irrigation, drainage, or soil and water management", "Design or evaluate agricultural machinery", "Use field data to improve crop and food production" };
        }
        else if (representative.Contains("psycholog") || representative.Contains("social") || representative.Contains("policy") || representative.Contains("journal") || representative.Contains("history") || representative.Contains("geograph") || representative.Contains("language") || representative.Contains("research"))
        {
            targetNode = representative.Contains("psycholog") ? "arts.psych" : "arts";
            taskLabels = new[] { "Research a question and evaluate evidence", "Communicate complex information to a specific audience", "Understand people's needs through interviews or fieldwork", "Develop a practical response to a social or organizational issue" };
        }
        else
        {
            targetNode = GetSelectedFieldCareerNode(fields[0]);
            taskLabels = new[] { $"Research a question within {fields[0]} and support your conclusions", $"Apply {fields[0]} knowledge to solve a practical problem", $"Create or improve a service, resource, or process in {fields[0]}", $"Plan and deliver a project related to {fields[0]}" };
        }

        var prompts = stage switch
        {
            QuizStage.FieldConfirmation => $"You selected {selectedLabels}. Which kind of work in this field would you most like to do?",
            QuizStage.SubdomainDiscovery => $"Within {selectedLabels}, which technical or professional task would you choose?",
            QuizStage.FinalValidation => $"Thinking about {selectedLabels}, which outcome would make this direction feel right for you?",
            _ => $"You selected {selectedLabels}. Which problem in this field would you most enjoy working on?"
        };
        var options = taskLabels.Select((label, index) => new FallbackOption(label,
            new[] { (targetNode, 0.8 + (index * 0.05)) })).ToArray();
        return MapToEntity(new FallbackQuestion(id, stage, "selected_field_fit", prompts, options), phaseKey);
    }

    private static string GetSelectedFieldCareerNode(string field)
    {
        var value = (field ?? string.Empty).ToLowerInvariant();
        if (value.Contains("electrical") || value.Contains("power system")) return "sci.eng.elec";
        if (value.Contains("electronics") || value.Contains("vlsi") || value.Contains("embedded") || value.Contains("telecommunication")) return "sci.eng.ece";
        if (value.Contains("aerospace") || value.Contains("aeronautical") || value.Contains("avionics") || value.Contains("aircraft")) return "sci.eng.aero";
        if (value.Contains("automobile") || value.Contains("automotive")) return "sci.eng.auto";
        if (value.Contains("civil") || value.Contains("structural") || value.Contains("geotechnical") || value.Contains("environmental engineering")) return "sci.eng.civil";
        if (value.Contains("chemical") || value.Contains("process engineering")) return "sci.eng.chem";
        if (value.Contains("computer") || value.Contains("software") || value.Contains("data") || value.Contains("cyber") || value.Contains("cloud")) return "sci.eng.cs";
        if (value.Contains("finance") || value.Contains("risk") || value.Contains("investment") || value.Contains("account")) return "com.fin";
        if (value.Contains("management") || value.Contains("operation") || value.Contains("consult")) return "com.mgmt";
        if (value.Contains("marketing") || value.Contains("human resource") || value.Contains("business")) return "com.mgmt";
        if (value.Contains("psycholog")) return "arts.psych";
        if (value.Contains("law")) return "arts.law";
        if (value.Contains("medicine") || value.Contains("clinical") || value.Contains("pharma")) return "sci.med";
        if (value.Contains("biomedical") || value.Contains("medical device")) return "sci.med";
        if (value.Contains("agricultur") || value.Contains("farm")) return "sci.agri";
        if (value.Contains("design") || value.Contains("fashion")) return "design";
        return "sci.eng";
    }

    private static QuizQuestionV2? CreateSelectedFieldQuestion(List<string>? selectedFields, string phaseKey, string stageOrId)
    {
        if (selectedFields == null || selectedFields.Count == 0) return null;
        var fields = selectedFields.Select(value => value.Trim()).Where(value => value.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList();
        if (fields.Count == 0) return null;

        var stage = Enum.TryParse<QuizStage>(stageOrId, out var parsedStage) ? parsedStage : QuizStage.BroadFieldDiscovery;
        var idSuffix = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", fields).ToLowerInvariant())))[..10];
        var id = $"FB_BA_SELECTED_{stage}_{idSuffix}";
        if (stageOrId.StartsWith("FB_BA_SELECTED_", StringComparison.OrdinalIgnoreCase))
        {
            var knownIdStage = Enum.GetValues<QuizStage>().FirstOrDefault(value => stageOrId.Contains($"_{value}_", StringComparison.OrdinalIgnoreCase));
            if (Enum.IsDefined(knownIdStage)) stage = knownIdStage;
            id = stageOrId;
        }

        var fieldOptions = fields.Select(field => MapSelectedField(field)).Where(option => option != null).Cast<FallbackOption>().ToList();
        if (fieldOptions.Count == 0) return null;
        var filler = new[]
        {
            new FallbackOption("Review evidence and write a clear, well-supported analysis", new[]{ ("role_social_researcher",0.7), ("role_public_policy_analyst",0.5) }),
            new FallbackOption("Explain a complex idea so learners or the public can use it", new[]{ ("role_curriculum_developer",0.7), ("role_editor",0.5) }),
            new FallbackOption("Interview people and communicate their experiences accurately", new[]{ ("role_journalist",0.7), ("role_social_worker",0.5) }),
            new FallbackOption("Use research to improve a community or public programme", new[]{ ("role_development_officer",0.7), ("role_programme_officer",0.5) }),
        };
        foreach (var option in filler)
        {
            if (fieldOptions.Count >= 4) break;
            if (!fieldOptions.Any(existing => existing.Text.Equals(option.Text, StringComparison.OrdinalIgnoreCase))) fieldOptions.Add(option);
        }

        var selectedNames = string.Join(", ", fields);
        var text = stage switch
        {
            QuizStage.FieldConfirmation => $"For your selected BA path(s) — {selectedNames} — which task best matches your current strengths?",
            QuizStage.SubdomainDiscovery => $"Within {selectedNames}, which skill would you most like to use in a real project?",
            QuizStage.FinalValidation => $"Thinking about {selectedNames}, which outcome would make this career direction feel right for you?",
            _ => $"You selected {selectedNames}. Which BA-related task would you most like to take on?"
        };

        var question = new FallbackQuestion(id, stage, "selected_ba_skill_fit", text, fieldOptions.Take(4).ToArray());
        return MapToEntity(question, phaseKey);
    }

    private static bool IsArchitectureProfile(string value) =>
        value.Contains("architecture", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("b.arch", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("barch", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("interior design", StringComparison.OrdinalIgnoreCase);

    private static QuizQuestionV2? CreateArchitectureSelectedFieldQuestion(List<string>? selectedFields, string phaseKey, string stageOrId)
    {
        if (selectedFields == null || selectedFields.Count == 0) return null;
        var fields = selectedFields.Select(value => value.Trim()).Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList();
        if (fields.Count == 0) return null;

        var stage = Enum.TryParse<QuizStage>(stageOrId, out var parsedStage) ? parsedStage : QuizStage.FieldConfirmation;
        var idSuffix = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", fields).ToLowerInvariant())))[..10];
        var id = $"FB_ARCH_SELECTED_{stage}_{idSuffix}";
        if (stageOrId.StartsWith("FB_ARCH_SELECTED_", StringComparison.OrdinalIgnoreCase)) id = stageOrId;
        var fieldNames = string.Join(", ", fields);
        var question = new FallbackQuestion(id, stage, "architecture_selected_field_fit",
            $"You selected {fieldNames} within architecture. Which project responsibility would you most like to own?",
            new[] {
                new FallbackOption("Coordinate the design team, consultant input, schedule, and project budget", new[]{ ("arch.bim",0.8), ("sci.eng.civil.pm",0.5) }),
                new FallbackOption("Manage client needs and guide design decisions from brief to delivery", new[]{ ("arch.residential",0.8), ("arch.interior",0.5) }),
                new FallbackOption("Organize a planning project with public agencies and the community", new[]{ ("arch.urban",0.9) }),
                new FallbackOption("Lead the project's sustainability goals and performance reviews", new[]{ ("arch.sustainable",0.9) }),
            });
        return MapToEntity(question, phaseKey);
    }

    private static FallbackOption? MapSelectedField(string field)
    {
        var normalized = field.ToLowerInvariant();
        if (normalized.Contains("counsell") || normalized.Contains("counsel"))
            return new FallbackOption($"Use attentive listening and structured conversation in {field}", new[]{ ("role_counselling_psychologist",0.95), ("role_mental_health_counsellor",0.65) });
        if (normalized.Contains("clinical psychology"))
            return new FallbackOption($"Apply evidence-based assessment and supervised practice in {field}", new[]{ ("role_clinical_psychologist",0.95), ("role_psychology_researcher",0.6) });
        if (normalized.Contains("organizational") || normalized.Contains("industrial psychology"))
            return new FallbackOption($"Use behavioral insight to improve people and teams in {field}", new[]{ ("role_industrial_organizational_psychologist",0.95), ("role_people_analytics_specialist",0.65) });
        if (normalized.Contains("psycholog") || normalized.Contains("behavioral research"))
            return new FallbackOption($"Design a behavioral study and interpret its findings for {field}", new[]{ ("role_psychology_researcher",0.9), ("role_psychometrist",0.65) });
        if (normalized.Contains("journal") || normalized.Contains("reporting"))
            return new FallbackOption($"Research, verify, and report a story for {field}", new[]{ ("role_journalist",0.95), ("role_editor",0.65) });
        if (normalized.Contains("edit") || normalized.Contains("publish"))
            return new FallbackOption($"Improve accuracy, structure, and clarity for {field}", new[]{ ("role_editor",0.95), ("role_content_strategist",0.65) });
        if (normalized.Contains("policy") || normalized.Contains("civil service") || normalized.Contains("international relation"))
            return new FallbackOption($"Analyze evidence and write recommendations for {field}", new[]{ ("role_public_policy_analyst",0.9), ("role_public_administration_officer",0.65) });
        if (normalized.Contains("social research") || normalized.Contains("sociolog"))
            return new FallbackOption($"Plan interviews or surveys and explain social patterns for {field}", new[]{ ("role_social_researcher",0.95), ("role_sociologist",0.65) });
        if (normalized.Contains("social work") || normalized.Contains("community") || normalized.Contains("ngo"))
            return new FallbackOption($"Coordinate support and community outcomes for {field}", new[]{ ("role_social_worker",0.8), ("role_community_development_officer",0.8) });
        if (normalized.Contains("teach") || normalized.Contains("academ") || normalized.Contains("curriculum") || normalized.Contains("education"))
            return new FallbackOption($"Teach, explain ideas, or develop learning resources for {field}", new[]{ ("role_curriculum_developer",0.9), ("role_instructional_designer",0.65) });
        if (normalized.Contains("fine art") || normalized.Contains("visual art") || normalized.Contains("creative art") || normalized.Contains("painting") || normalized.Contains("design"))
            return new FallbackOption($"Create visual work or make art and heritage accessible for {field}", new[]{ ("role_museum_educator",0.8), ("role_content_strategist",0.7) });
        if (normalized.Contains("translat") || normalized.Contains("language") || normalized.Contains("linguistic"))
            return new FallbackOption($"Translate or analyze language with accuracy for {field}", new[]{ ("role_translator",0.9), ("role_linguist",0.7) });
        if (normalized.Contains("geograph") || normalized.Contains("gis") || normalized.Contains("urban"))
            return new FallbackOption($"Interpret spatial evidence and explain place-based patterns for {field}", new[]{ ("role_geospatial_analyst",0.8), ("role_urban_planner",0.7) });
        if (normalized.Contains("econom"))
            return new FallbackOption($"Use economic data to explain trends and inform decisions for {field}", new[]{ ("role_economic_analyst",0.8), ("role_public_policy_analyst",0.65) });
        if (normalized.Contains("history") || normalized.Contains("heritage") || normalized.Contains("museum"))
            return new FallbackOption($"Evaluate sources and make historical knowledge accessible for {field}", new[]{ ("role_archivist",0.8), ("role_museum_educator",0.7) });
        if (normalized.Contains("philosophy") || normalized.Contains("ethic"))
            return new FallbackOption($"Compare arguments and apply ethical reasoning for {field}", new[]{ ("role_ethics_researcher",0.9), ("role_social_research_analyst",0.65) });
        if (normalized.Contains("media") || normalized.Contains("content") || normalized.Contains("communication"))
            return new FallbackOption($"Plan audience-focused stories and communication for {field}", new[]{ ("role_content_strategist",0.8), ("role_social_media_manager",0.7) });
        return new FallbackOption($"Research, analyze, and communicate findings in {field}", new[]{ ("role_social_researcher",0.7), ("role_editor",0.5) });
    }

    private static List<FallbackQuestion> SelectPool(string userMajor, string phaseKey)
    {
        var m = userMajor.ToLowerInvariant();
        var hasBaBackground = QuizProfileClassifier.IsBachelorOfArts(userMajor, null);
        if (IsArchitectureProfile(m)) return _architectureQuestions;
        if (phaseKey.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase) &&
            (hasBaBackground || m.Contains("arts") || m.Contains("humanit") || m.Contains("social science") || m.Contains("psychology") || m.Contains("sociology") || m.Contains("history") || m.Contains("english")))
            return _baPostgraduateQuestions;
        if (hasBaBackground || m.Contains("humanit") || m.Contains("arts & humanities"))
            return _baQuestions;
        if (m.Contains("computer") || m.Contains("cse") || m.Contains("software") ||
            m.Contains("it") || m.Contains("information technology"))
            return _cseQuestions;
        if (QuizProfileClassifier.IsBachelorOfScience(userMajor, null) ||
            m.Contains("physics") || m.Contains("chemistry") || m.Contains("biology") || m.Contains("biotechnology") ||
            m.Contains("environmental science") || m.Contains("mathematics") || m.Contains("statistics"))
            return _bscQuestions;
        if (m.Contains("mbbs") || m.Contains("medicine") || m.Contains("medical") ||
            m.Contains("bds") || m.Contains("pharmacy"))
            return _mbbsQuestions;
        if (m.Contains("commerce") || m.Contains("bcom") || m.Contains("finance") ||
            m.Contains("account") || m.Contains("mba"))
            return _bcomQuestions;
        if (m.Contains("mechanical") || m.Contains("mech"))
            return _mechQuestions;
        return _genericQuestions;
    }

    private static QuizQuestionV2 MapToEntity(FallbackQuestion q, string phaseKey)
    {
        var options = q.Options.Select((opt, i) =>
        {
            var signals = opt.Signals.Select(s =>
                new { nodeId = s.NodeId, weight = s.Weight }).ToList();
            return new { index = i, text = opt.Text, signals };
        }).ToList();

        return new QuizQuestionV2
        {
            Id          = q.Id,
            PhaseKey    = phaseKey,
            Domain      = q.Id.StartsWith("FB_MBBS_", StringComparison.OrdinalIgnoreCase) ? "Healthcare" : string.Empty,
            // Keep the fallback at the medical-domain level. A profile may
            // already have a narrower eligible subdomain after a few answers;
            // hard-coding "Medicine" here makes the validator reject every
            // specialty question and incorrectly complete the assessment.
            Subdomain   = string.Empty,
            QuestionType = "AdaptiveFallback",
            Text        = q.Question,
            Dimension   = q.Dimension,
            Intent      = $"Distinguish candidates at stage {q.Stage}",
            TargetDiscriminatesJson = JsonSerializer.Serialize(
                q.Options.SelectMany(o => o.Signals.Select(s => s.NodeId))
                    .Concat(q.Id.StartsWith("FB_MBBS_", StringComparison.OrdinalIgnoreCase)
                        ? new[] { "sci.med.mbbs" }
                        : Array.Empty<string>())
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToList()),
            OptionsJson = JsonSerializer.Serialize(options),
            DisplayOrder = 999,
            IsActive    = true
        };
    }
}

// ── Internal models (internal namespace-level to avoid file-local restrictions) ─
internal record FallbackQuestion(
    string Id,
    QuizStage Stage,
    string Dimension,
    string Question,
    FallbackOption[] Options);

internal record FallbackOption(
    string Text,
    (string NodeId, double Weight)[] Signals);
