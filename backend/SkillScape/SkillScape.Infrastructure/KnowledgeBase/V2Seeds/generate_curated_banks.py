import json
import os

seeds_dir = r"c:\Users\VIDHI TRIVEDI\Desktop\SkillScape\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds"

# ══════════════════════════════════════════════════════════════════════════════
# 1. MEDICAL & HEALTH SCIENCES QUESTION BANK
# ══════════════════════════════════════════════════════════════════════════════
medical_questions = [
    {
        "id": "MED_DISCRIM_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "medical",
        "educationLevelsJson": json.dumps(["Graduation", "Postgraduation", "Recently Graduated"]),
        "domain": "Healthcare",
        "subdomain": "Clinical & Surgical Medicine",
        "specialization": "",
        "questionType": "ClinicalScenario",
        "languageComplexity": "Advanced",
        "text": "A patient presents with non-specific chronic symptoms across multiple systems with inconclusive initial tests. Which part of the medical management would you find most intellectually fulfilling?",
        "intent": "Differentiate Diagnostic Synthesis vs Immediate Procedural Action vs Pediatric/Family Focus",
        "dimension": "diagnostic_reasoning",
        "fieldId": "sci.med.mbbs",
        "domainId": "med",
        "conceptId": "diagnostic_synthesis",
        "competencyId": "clinical_reasoning",
        "subdomainIdsJson": json.dumps(["med.clinical", "med.surg", "med.diag"]),
        "targetDiscriminatesJson": json.dumps(["med.clinical.gen_med", "med.surg.gen_surg", "med.clinical.peds", "med.diag.radiology"]),
        "optionsJson": json.dumps([
            {
                "index": 0,
                "text": "Reviewing longitudinal medical history, lab biomarkers, and pharmacological interactions to methodically narrow down a complex differential diagnosis.",
                "dimensionScores": { "diagnostic_reasoning": 5, "differential_diagnosis": 5, "long_term_patient_management": 4 },
                "signals": [{ "nodeId": "med.clinical.gen_med", "weight": 1.0 }, { "nodeId": "med.clinical", "weight": 0.8 }]
            },
            {
                "index": 1,
                "text": "Performing an immediate interventional procedure or exploratory surgery to directly inspect and repair the anatomical pathology.",
                "dimensionScores": { "procedural_interest": 5, "manual_precision": 5, "acute_decision_making": 4 },
                "signals": [{ "nodeId": "med.surg.gen_surg", "weight": 1.0 }, { "nodeId": "med.surg", "weight": 0.8 }]
            },
            {
                "index": 2,
                "text": "Gathering subtle clues through indirect behavioral observation and working patiently with the patient's family to uncover environmental and developmental triggers.",
                "dimensionScores": { "child_interaction": 5, "family_communication": 5, "empathy_and_patience": 5 },
                "signals": [{ "nodeId": "med.clinical.peds", "weight": 1.0 }, { "nodeId": "med.clinical", "weight": 0.8 }]
            },
            {
                "index": 3,
                "text": "Analyzing cross-sectional imaging (CT/MRI) and multi-planar reconstructions to detect occult structural abnormalities without direct bedside management.",
                "dimensionScores": { "image_interpretation": 5, "spatial_reasoning": 5, "technology_adaptation": 4 },
                "signals": [{ "nodeId": "med.diag.radiology", "weight": 1.0 }, { "nodeId": "med.diag", "weight": 0.8 }]
            }
        ])
    },
    {
        "id": "MED_DISCRIM_002",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "medical",
        "educationLevelsJson": json.dumps(["Graduation", "Postgraduation", "Recently Graduated"]),
        "domain": "Healthcare",
        "subdomain": "Procedural vs Longitudinal",
        "specialization": "",
        "questionType": "WorkPreference",
        "languageComplexity": "Advanced",
        "text": "Which daily working environment and professional rhythm aligns closest with your clinical strengths?",
        "intent": "Differentiate Operating Theater vs Outpatient/Ward Rounds vs Pediatric Clinic",
        "dimension": "work_environment",
        "fieldId": "sci.med.mbbs",
        "domainId": "med",
        "conceptId": "clinical_rhythm",
        "competencyId": "work_style",
        "subdomainIdsJson": json.dumps(["med.clinical", "med.surg"]),
        "targetDiscriminatesJson": json.dumps(["med.clinical.gen_med", "med.surg.gen_surg", "med.clinical.peds"]),
        "optionsJson": json.dumps([
            {
                "index": 0,
                "text": "High-intensity operating theater performing delicate, hands-on mechanical tissue repair with rapid, tangible post-op resolution.",
                "dimensionScores": { "procedural_interest": 5, "manual_precision": 5, "high_pressure_theater": 5 },
                "signals": [{ "nodeId": "med.surg.gen_surg", "weight": 1.0 }]
            },
            {
                "index": 1,
                "text": "Inpatient medical wards and outpatient clinics managing chronic multi-system illnesses (diabetes, cardiovascular, autoimmune) over years.",
                "dimensionScores": { "long_term_patient_management": 5, "patient_communication": 5, "diagnostic_reasoning": 4 },
                "signals": [{ "nodeId": "med.clinical.gen_med", "weight": 1.0 }]
            },
            {
                "index": 2,
                "text": "Nurturing pediatric clinic monitoring developmental milestones, vaccination schedules, and treating acute childhood infections in a family-friendly space.",
                "dimensionScores": { "child_interaction": 5, "family_communication": 5, "growth_development_tracking": 5 },
                "signals": [{ "nodeId": "med.clinical.peds", "weight": 1.0 }]
            }
        ])
    },
    {
        "id": "MED_DISCRIM_003",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "medical",
        "educationLevelsJson": json.dumps(["Graduation", "Postgraduation", "Recently Graduated"]),
        "domain": "Healthcare",
        "subdomain": "Acute Emergency vs Methodical Investigation",
        "specialization": "",
        "questionType": "SituationalScenario",
        "languageComplexity": "Advanced",
        "text": "In a hospital setting, which type of critical challenge tests your highest capability?",
        "intent": "Differentiate Acute Decision Making vs Diagnostic Depth vs Interpersonal Rapport",
        "dimension": "stress_tolerance",
        "fieldId": "sci.med.mbbs",
        "domainId": "med",
        "conceptId": "crisis_handling",
        "competencyId": "decision_making",
        "subdomainIdsJson": json.dumps(["med.clinical", "med.surg"]),
        "targetDiscriminatesJson": json.dumps(["med.clinical.gen_med", "med.surg.gen_surg", "med.clinical.peds"]),
        "optionsJson": json.dumps([
            {
                "index": 0,
                "text": "Making high-stakes, instantaneous decisions during active intraoperative hemorrhage where fine-motor steadiness under extreme pressure is mandatory.",
                "dimensionScores": { "acute_decision_making": 5, "stress_tolerance": 5, "manual_precision": 5 },
                "signals": [{ "nodeId": "med.surg.gen_surg", "weight": 1.0 }]
            },
            {
                "index": 1,
                "text": "Solving a diagnostic puzzle where multiple drug interactions and atypical laboratory values obscure the underlying autoimmune or infectious pathology.",
                "dimensionScores": { "diagnostic_reasoning": 5, "differential_diagnosis": 5, "analytical_reasoning": 5 },
                "signals": [{ "nodeId": "med.clinical.gen_med", "weight": 1.0 }]
            },
            {
                "index": 2,
                "text": "Calming an anxious, non-verbal distressed infant and distressed parents while accurately deducing a pediatric illness from subtle clinical signs.",
                "dimensionScores": { "child_interaction": 5, "empathy_and_patience": 5, "indirect_symptom_inference": 5 },
                "signals": [{ "nodeId": "med.clinical.peds", "weight": 1.0 }]
            }
        ])
    },
    {
        "id": "MED_DISCRIM_004",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "medical",
        "educationLevelsJson": json.dumps(["Graduation", "Postgraduation", "Recently Graduated"]),
        "domain": "Healthcare",
        "subdomain": "Clinical Communication & Rapport",
        "specialization": "",
        "questionType": "CommunicationPreference",
        "languageComplexity": "Intermediate",
        "text": "How do you prefer structuring your patient and professional communication?",
        "intent": "Differentiate Patient Counseling vs Child/Guardian Rapport vs Technical Case Discussion",
        "dimension": "patient_communication",
        "fieldId": "sci.med.mbbs",
        "domainId": "med",
        "conceptId": "communication_style",
        "competencyId": "patient_communication",
        "subdomainIdsJson": json.dumps(["med.clinical", "med.surg", "med.diag"]),
        "targetDiscriminatesJson": json.dumps(["med.clinical.gen_med", "med.clinical.peds", "med.surg.gen_surg", "med.diag.pathology"]),
        "optionsJson": json.dumps([
            {
                "index": 0,
                "text": "Empathetic, in-depth counseling educating adult patients on lifestyle modification, disease prognosis, and medication adherence.",
                "dimensionScores": { "patient_communication": 5, "long_term_patient_management": 5 },
                "signals": [{ "nodeId": "med.clinical.gen_med", "weight": 1.0 }]
            },
            {
                "index": 1,
                "text": "Playful, comforting communication with children while explaining pediatric care plans clearly and reassuringly to parents.",
                "dimensionScores": { "child_interaction": 5, "family_communication": 5 },
                "signals": [{ "nodeId": "med.clinical.peds", "weight": 1.0 }]
            },
            {
                "index": 2,
                "text": "Direct, concise pre-operative briefings explaining risks/benefits followed by focused post-operative wound and recovery check-ins.",
                "dimensionScores": { "procedural_interest": 4, "acute_decision_making": 3 },
                "signals": [{ "nodeId": "med.surg.gen_surg", "weight": 1.0 }]
            },
            {
                "index": 3,
                "text": "Inter-physician consultations delivering precise, objective histological and laboratory diagnostic reports without patient-facing counseling.",
                "dimensionScores": { "laboratory_precision": 5, "microscopic_analysis": 5 },
                "signals": [{ "nodeId": "med.diag.pathology", "weight": 1.0 }]
            }
        ])
    }
]

# ══════════════════════════════════════════════════════════════════════════════
# 2. CYBERSECURITY QUESTION BANK
# ══════════════════════════════════════════════════════════════════════════════
cyber_questions = [
    {
        "id": "CYBER_DISCRIM_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "tech",
        "educationLevelsJson": json.dumps(["Graduation", "Postgraduation", "CareerSwitch", "Recently Graduated"]),
        "domain": "Technology",
        "subdomain": "Cybersecurity",
        "specialization": "",
        "questionType": "TechnicalScenario",
        "languageComplexity": "Intermediate",
        "text": "A corporate alert indicates that an executive's credentials were used to authenticate from two distant continents within 10 minutes. Which part of the security workflow do you find most compelling?",
        "intent": "Differentiate SOC Monitoring vs Threat Intel vs Digital Forensics vs Pentest Validation vs Cloud IAM",
        "dimension": "threat_detection",
        "fieldId": "sci.eng.cs",
        "domainId": "tech.cyber",
        "conceptId": "credential_anomaly",
        "competencyId": "incident_triage",
        "subdomainIdsJson": json.dumps(["tech.cyber.soc", "tech.cyber.pentest", "tech.cyber.forensics", "tech.cyber.threat_intel", "tech.cyber.cloud_sec"]),
        "targetDiscriminatesJson": json.dumps(["tech.cyber.soc", "tech.cyber.pentest", "tech.cyber.forensics", "tech.cyber.threat_intel", "tech.cyber.cloud_sec"]),
        "optionsJson": json.dumps([
            {
                "index": 0,
                "text": "Triage the live alert in the SIEM, verify IP reputation, isolate the affected session, and initiate containment playbooks.",
                "dimensionScores": { "threat_detection": 5, "log_analysis": 5, "incident_response": 4, "siem_operations": 5 },
                "signals": [{ "nodeId": "tech.cyber.soc", "weight": 1.0 }]
            },
            {
                "index": 1,
                "text": "Acquire volatile memory and disk artifacts from the host to extract adversary session tokens, persistence mechanisms, and reconstruct the timeline.",
                "dimensionScores": { "digital_forensics": 5, "evidence_reconstruction": 5, "artifact_analysis": 5 },
                "signals": [{ "nodeId": "tech.cyber.forensics", "weight": 1.0 }]
            },
            {
                "index": 2,
                "text": "Research the attacking infrastructure and TTPs against MITRE ATT&CK to attribute the activity to known APT or credential-stuffing threat groups.",
                "dimensionScores": { "threat_intelligence": 5, "adversary_attribution": 5, "mitre_attack_mapping": 5 },
                "signals": [{ "nodeId": "tech.cyber.threat_intel", "weight": 1.0 }]
            },
            {
                "index": 3,
                "text": "Attempt to exploit the authentication portal and session tokens yourself in a staging environment to discover how MFA bypass was achieved.",
                "dimensionScores": { "offensive_exploitation": 5, "vulnerability_assessment": 5, "web_app_security": 5 },
                "signals": [{ "nodeId": "tech.cyber.pentest", "weight": 1.0 }]
            },
            {
                "index": 4,
                "text": "Audit cloud IAM conditional access policies, SSO federation tokens, and enforce Zero-Trust access rules to prevent future bypasses.",
                "dimensionScores": { "cloud_security": 5, "iam_zero_trust": 5, "security_architecture": 5 },
                "signals": [{ "nodeId": "tech.cyber.cloud_sec", "weight": 1.0 }]
            }
        ])
    },
    {
        "id": "CYBER_DISCRIM_002",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "tech",
        "educationLevelsJson": json.dumps(["Graduation", "Postgraduation", "CareerSwitch", "Recently Graduated"]),
        "domain": "Technology",
        "subdomain": "Cybersecurity",
        "specialization": "",
        "questionType": "ProblemSolving",
        "languageComplexity": "Advanced",
        "text": "When assessing a company's web application and infrastructure, which objective provides you with the greatest satisfaction?",
        "intent": "Differentiate Ethical Hacking vs Forensic Root Cause vs Continuous Monitoring vs Secure Cloud Architecture",
        "dimension": "offensive_exploitation",
        "fieldId": "sci.eng.cs",
        "domainId": "tech.cyber",
        "conceptId": "security_assessment_style",
        "competencyId": "security_methodology",
        "subdomainIdsJson": json.dumps(["tech.cyber.pentest", "tech.cyber.soc", "tech.cyber.forensics", "tech.cyber.cloud_sec"]),
        "targetDiscriminatesJson": json.dumps(["tech.cyber.pentest", "tech.cyber.soc", "tech.cyber.forensics", "tech.cyber.cloud_sec"]),
        "optionsJson": json.dumps([
            {
                "index": 0,
                "text": "Bypassing defenses, finding SQLi/SSRF vulnerabilities, and chaining multiple weak points to gain unauthorized administrative access.",
                "dimensionScores": { "offensive_exploitation": 5, "vulnerability_assessment": 5, "exploit_development": 5 },
                "signals": [{ "nodeId": "tech.cyber.pentest", "weight": 1.0 }]
            },
            {
                "index": 1,
                "text": "Configuring detection engineering rules, custom Sigma/YARA queries, and fine-tuning alerts to minimize false positives.",
                "dimensionScores": { "threat_detection": 5, "siem_operations": 5, "log_analysis": 5 },
                "signals": [{ "nodeId": "tech.cyber.soc", "weight": 1.0 }]
            },
            {
                "index": 2,
                "text": "Performing deep post-mortem analysis on compromised servers to reconstruct step-by-step adversary behavior for a legal and forensic report.",
                "dimensionScores": { "digital_forensics": 5, "evidence_reconstruction": 5, "chain_of_custody": 5 },
                "signals": [{ "nodeId": "tech.cyber.forensics", "weight": 1.0 }]
            },
            {
                "index": 3,
                "text": "Designing hardened Kubernetes clusters, automated Terraform security gates, and Zero-Trust network segmentation across AWS/Azure.",
                "dimensionScores": { "cloud_security": 5, "iam_zero_trust": 5, "infrastructure_as_code_security": 5 },
                "signals": [{ "nodeId": "tech.cyber.cloud_sec", "weight": 1.0 }]
            }
        ])
    },
    {
        "id": "CYBER_DISCRIM_003",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "tech",
        "educationLevelsJson": json.dumps(["Graduation", "Postgraduation", "CareerSwitch"]),
        "domain": "Technology",
        "subdomain": "Cybersecurity",
        "specialization": "",
        "questionType": "WorkPreference",
        "languageComplexity": "Intermediate",
        "text": "Which category of cybersecurity tooling and analytical methodology excites you most?",
        "intent": "Differentiate Burp/Metasploit vs Volatility/Autopsy vs Splunk/Sentinel vs MISP/ThreatConnect",
        "dimension": "technical_curiosity",
        "fieldId": "sci.eng.cs",
        "domainId": "tech.cyber",
        "conceptId": "tooling_preference",
        "competencyId": "technical_depth",
        "subdomainIdsJson": json.dumps(["tech.cyber.pentest", "tech.cyber.forensics", "tech.cyber.soc", "tech.cyber.threat_intel"]),
        "targetDiscriminatesJson": json.dumps(["tech.cyber.pentest", "tech.cyber.forensics", "tech.cyber.soc", "tech.cyber.threat_intel"]),
        "optionsJson": json.dumps([
            {
                "index": 0,
                "text": "Offensive toolkits: Burp Suite Professional, Metasploit, Nmap, custom Python exploit scripts, and reverse engineering tools (Ghidra).",
                "dimensionScores": { "offensive_exploitation": 5, "vulnerability_assessment": 5 },
                "signals": [{ "nodeId": "tech.cyber.pentest", "weight": 1.0 }]
            },
            {
                "index": 1,
                "text": "Forensic suites: Volatility memory forensics, Autopsy/FTK Imager, Plaso log2timeline, and analyzing NTFS Master File Table ($MFT) records.",
                "dimensionScores": { "digital_forensics": 5, "artifact_analysis": 5, "memory_analysis": 5 },
                "signals": [{ "nodeId": "tech.cyber.forensics", "weight": 1.0 }]
            },
            {
                "index": 2,
                "text": "Defensive SOC suites: Splunk Enterprise Security, Microsoft Sentinel, CrowdStrike Falcon EDR, and SOAR workflow automation.",
                "dimensionScores": { "threat_detection": 5, "siem_operations": 5, "log_analysis": 5 },
                "signals": [{ "nodeId": "tech.cyber.soc", "weight": 1.0 }]
            },
            {
                "index": 3,
                "text": "Threat Intel platforms: MISP, OpenCTI, tracking dark web forums, analyzing APT malware hashes, and creating threat actor dossiers.",
                "dimensionScores": { "threat_intelligence": 5, "adversary_attribution": 5, "mitre_attack_mapping": 5 },
                "signals": [{ "nodeId": "tech.cyber.threat_intel", "weight": 1.0 }]
            }
        ])
    }
]

# ══════════════════════════════════════════════════════════════════════════════
# 3. DATA & AI QUESTION BANK
# ══════════════════════════════════════════════════════════════════════════════
data_questions = [
    {
        "id": "DATA_DISCRIM_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "tech",
        "educationLevelsJson": json.dumps(["Graduation", "Postgraduation", "CareerSwitch"]),
        "domain": "Technology",
        "subdomain": "Data & AI",
        "specialization": "",
        "questionType": "TechnicalScenario",
        "languageComplexity": "Intermediate",
        "text": "A high-growth company wants to leverage their customer interaction logs. Which project would you lead with the highest enthusiasm?",
        "intent": "Differentiate Data Analytics vs Predictive Data Science vs Production ML Engineering",
        "dimension": "data_focus",
        "fieldId": "sci.eng.cs",
        "domainId": "sci.eng.cs",
        "conceptId": "data_project_type",
        "competencyId": "data_analysis",
        "subdomainIdsJson": json.dumps(["tech.data.analytics", "tech.data.ds", "tech.data.ml_eng"]),
        "targetDiscriminatesJson": json.dumps(["tech.data.analytics", "tech.data.ds", "tech.data.ml_eng"]),
        "optionsJson": json.dumps([
            {
                "index": 0,
                "text": "Authoring complex SQL aggregations and building interactive PowerBI/Tableau executive dashboards showing conversion trends.",
                "dimensionScores": { "sql_querying": 5, "bi_dashboards": 5, "business_metrics": 5 },
                "signals": [{ "nodeId": "tech.data.analytics", "weight": 1.0 }]
            },
            {
                "index": 1,
                "text": "Formulating statistical hypotheses, performing feature engineering, and training predictive churn/recommendation models in Python (scikit-learn/XGBoost).",
                "dimensionScores": { "statistical_analysis": 5, "predictive_modeling": 5, "machine_learning": 5 },
                "signals": [{ "nodeId": "tech.data.ds", "weight": 1.0 }]
            },
            {
                "index": 2,
                "text": "Engineering real-time MLOps deployment pipelines, optimizing model latency on GPU clusters, and managing automated model retraining via Kubernetes.",
                "dimensionScores": { "deep_learning": 5, "mlops_pipelines": 5, "model_optimization": 5 },
                "signals": [{ "nodeId": "tech.data.ml_eng", "weight": 1.0 }]
            }
        ])
    }
]

# Write to seed files
with open(os.path.join(seeds_dir, "QuestionBankSeed_Medical.json"), "w", encoding="utf-8") as f:
    json.dump(medical_questions, f, indent=2)

with open(os.path.join(seeds_dir, "QuestionBankSeed_Cybersecurity.json"), "w", encoding="utf-8") as f:
    json.dump(cyber_questions, f, indent=2)

with open(os.path.join(seeds_dir, "QuestionBankSeed_DataScience.json"), "w", encoding="utf-8") as f:
    json.dump(data_questions, f, indent=2)

print("Created QuestionBankSeed_Medical.json, QuestionBankSeed_Cybersecurity.json, and QuestionBankSeed_DataScience.json successfully.")
