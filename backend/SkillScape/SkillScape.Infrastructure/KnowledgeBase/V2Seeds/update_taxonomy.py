import json
import os

seeds_dir = r"c:\Users\VIDHI TRIVEDI\Desktop\SkillScape\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds"
taxonomy_file = os.path.join(seeds_dir, "TaxonomySeed.json")

with open(taxonomy_file, "r", encoding="utf-8") as f:
    taxonomy = json.load(f)

existing_ids = {node["id"] for node in taxonomy if "id" in node}

new_nodes = [
    # ── Healthcare & Medicine ──
    {
        "id": "med",
        "parentId": "",
        "level": 0,
        "name": "Medical & Health Sciences",
        "shortCode": "MEDICINE",
        "description": "Diagnosis, treatment, surgical intervention, and prevention of human disease and promotion of health",
        "phasesApplicable": ["After12th", "AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Healthcare",
        "subdomain": "",
        "specialization": "",
        "eligibleEducationLevelsJson": json.dumps(["After12th", "Graduation", "Postgraduation"]),
        "icon": "activity",
        "colorHex": "#EF4444"
    },
    {
        "id": "med.clinical",
        "parentId": "med",
        "level": 1,
        "name": "Clinical Medicine",
        "shortCode": "CLIN_MED",
        "description": "Direct non-surgical patient diagnosis, long-term disease management, and specialized therapies",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Healthcare",
        "subdomain": "Clinical Medicine",
        "specialization": "",
        "icon": "heart",
        "colorHex": "#DC2626"
    },
    {
        "id": "med.clinical.gen_med",
        "parentId": "med.clinical",
        "level": 2,
        "name": "General Medicine",
        "shortCode": "GEN_MED",
        "description": "Comprehensive diagnostic evaluation, differential diagnosis, and longitudinal treatment of multisystem conditions",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Healthcare",
        "subdomain": "Clinical Medicine",
        "specialization": "Internal Medicine",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation", "Recently Graduated"]),
        "traitWeightsJson": json.dumps({
            "diagnostic_reasoning": 0.95,
            "patient_communication": 0.88,
            "long_term_patient_management": 0.92,
            "differential_diagnosis": 0.96,
            "clinical_decision_making": 0.90,
            "procedural_interest": 0.35,
            "manual_precision": 0.40,
            "acute_interventions": 0.50
        }),
        "aptitudeDimensionsJson": json.dumps({
            "analytical_reasoning": 0.95,
            "deductive_logic": 0.92,
            "complex_synthesis": 0.94
        }),
        "workStyleDimensionsJson": json.dumps({
            "investigative": 0.95,
            "patient_facing": 0.90,
            "structured_review": 0.85
        }),
        "typicalRoles": ["Physician", "Internist", "Consultant Physician", "Medical Officer"],
        "requiredDegrees": ["MBBS", "MD General Medicine", "DNB Internal Medicine"],
        "icon": "stethoscope",
        "colorHex": "#B91C1C"
    },
    {
        "id": "med.surg.gen_surg",
        "parentId": "med",
        "level": 2,
        "name": "General Surgery",
        "shortCode": "GEN_SURG",
        "description": "Operative management, hands-on surgical interventions, acute trauma care, and perioperative treatment",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Healthcare",
        "subdomain": "Surgical Medicine",
        "specialization": "General Surgery",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation", "Recently Graduated"]),
        "traitWeightsJson": json.dumps({
            "procedural_interest": 0.98,
            "manual_precision": 0.96,
            "acute_decision_making": 0.95,
            "stress_tolerance": 0.92,
            "spatial_reasoning": 0.88,
            "diagnostic_reasoning": 0.75,
            "long_term_patient_management": 0.45,
            "patient_communication": 0.65
        }),
        "aptitudeDimensionsJson": json.dumps({
            "fine_motor_control": 0.96,
            "spatial_orientation": 0.90,
            "rapid_decision_under_pressure": 0.95
        }),
        "workStyleDimensionsJson": json.dumps({
            "hands_on_procedural": 0.98,
            "high_pressure_theater": 0.95,
            "fast_paced_intervention": 0.92
        }),
        "typicalRoles": ["General Surgeon", "Surgical Consultant", "Trauma Surgeon"],
        "requiredDegrees": ["MBBS", "MS General Surgery", "DNB Surgery", "FRCS"],
        "icon": "scissors",
        "colorHex": "#991B1B"
    },
    {
        "id": "med.clinical.peds",
        "parentId": "med.clinical",
        "level": 2,
        "name": "Pediatrics",
        "shortCode": "PEDIATRICS",
        "description": "Medical care, growth monitoring, developmental assessment, and disease management for infants, children, and adolescents",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Healthcare",
        "subdomain": "Clinical Medicine",
        "specialization": "Pediatric Medicine",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation", "Recently Graduated"]),
        "traitWeightsJson": json.dumps({
            "child_interaction": 0.98,
            "family_communication": 0.95,
            "empathy_and_patience": 0.96,
            "diagnostic_reasoning": 0.88,
            "growth_development_tracking": 0.92,
            "long_term_patient_management": 0.90,
            "procedural_interest": 0.40,
            "manual_precision": 0.45
        }),
        "aptitudeDimensionsJson": json.dumps({
            "observational_acuity": 0.95,
            "indirect_symptom_inference": 0.92,
            "emotional_intelligence": 0.96
        }),
        "workStyleDimensionsJson": json.dumps({
            "collaborative_family_care": 0.98,
            "longitudinal_nurturing": 0.94,
            "gentle_clinical_environment": 0.90
        }),
        "typicalRoles": ["Pediatrician", "Child Health Specialist", "Pediatric Consultant"],
        "requiredDegrees": ["MBBS", "MD Pediatrics", "DCH", "DNB Pediatrics"],
        "icon": "smile",
        "colorHex": "#F87171"
    },
    {
        "id": "med.diag.radiology",
        "parentId": "med",
        "level": 2,
        "name": "Radiology",
        "shortCode": "RADIOLOGY",
        "description": "Diagnostic imaging interpretation including MRI, CT, Ultrasound, and image-guided interventional procedures",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Healthcare",
        "subdomain": "Diagnostics",
        "specialization": "Diagnostic Radiology",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation"]),
        "traitWeightsJson": json.dumps({
            "image_interpretation": 0.98,
            "spatial_reasoning": 0.94,
            "technology_adaptation": 0.90,
            "diagnostic_reasoning": 0.92,
            "patient_communication": 0.35,
            "procedural_interest": 0.55
        }),
        "typicalRoles": ["Radiologist", "Interventional Radiologist", "Sonologist"],
        "requiredDegrees": ["MBBS", "MD Radio-Diagnosis", "DMRD"],
        "icon": "film",
        "colorHex": "#7F1D1D"
    },
    {
        "id": "med.diag.pathology",
        "parentId": "med",
        "level": 2,
        "name": "Pathology",
        "shortCode": "PATHOLOGY",
        "description": "Laboratory analysis of tissue, blood, and body fluids to diagnose disease mechanisms and oncology staging",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Healthcare",
        "subdomain": "Diagnostics",
        "specialization": "Histopathology & Hematology",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation"]),
        "traitWeightsJson": json.dumps({
            "microscopic_analysis": 0.98,
            "laboratory_precision": 0.95,
            "analytical_reasoning": 0.92,
            "oncology_correlation": 0.88,
            "patient_communication": 0.20
        }),
        "typicalRoles": ["Pathologist", "Histopathologist", "Clinical Biochemist"],
        "requiredDegrees": ["MBBS", "MD Pathology", "DCP"],
        "icon": "search",
        "colorHex": "#991B1B"
    },

    # ── Cybersecurity ──
    {
        "id": "tech.cyber",
        "parentId": "sci",
        "level": 1,
        "name": "Cybersecurity",
        "shortCode": "CYBERSEC",
        "description": "Protecting systems, networks, programs, devices and data from cyber attacks, unauthorized access, and exploitation",
        "phasesApplicable": ["After12th", "AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation", "CareerSwitch"],
        "domain": "Technology",
        "subdomain": "Cybersecurity",
        "specialization": "",
        "icon": "shield",
        "colorHex": "#059669"
    },
    {
        "id": "tech.cyber.soc",
        "parentId": "tech.cyber",
        "level": 2,
        "name": "Cybersecurity Analyst / SOC Analyst",
        "shortCode": "SOC_ANALYST",
        "description": "Real-time threat monitoring, SIEM log analysis, security alert triage, and frontline incident response",
        "phasesApplicable": ["After12th", "AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation", "CareerSwitch"],
        "domain": "Technology",
        "subdomain": "Security Operations",
        "specialization": "Defensive Security",
        "eligibleEducationLevelsJson": json.dumps(["After12th", "Graduation", "Postgraduation", "CareerSwitch"]),
        "traitWeightsJson": json.dumps({
            "threat_detection": 0.96,
            "log_analysis": 0.94,
            "incident_response": 0.92,
            "siem_operations": 0.95,
            "alert_triage": 0.90,
            "offensive_exploitation": 0.40,
            "malware_reverse_engineering": 0.50,
            "security_governance": 0.45
        }),
        "aptitudeDimensionsJson": json.dumps({
            "pattern_recognition": 0.94,
            "rapid_triage": 0.92,
            "system_monitoring": 0.95
        }),
        "workStyleDimensionsJson": json.dumps({
            "soc_operations": 0.96,
            "incident_handling": 0.92,
            "structured_playbooks": 0.90
        }),
        "typicalRoles": ["SOC Analyst (L1/L2)", "Security Operations Engineer", "Cyber Incident Responder"],
        "requiredDegrees": ["B.Tech CS/IT", "BCA", "B.Sc Cyber Security", "Certifications (CompTIA CySA+, CEH)"],
        "icon": "eye",
        "colorHex": "#10B981"
    },
    {
        "id": "tech.cyber.pentest",
        "parentId": "tech.cyber",
        "level": 2,
        "name": "Penetration Tester",
        "shortCode": "PENTESTER",
        "description": "Ethical hacking, vulnerability discovery, simulated adversary attacks, network and web application penetration testing",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation", "CareerSwitch"],
        "domain": "Technology",
        "subdomain": "Offensive Security",
        "specialization": "Offensive Security",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation", "CareerSwitch"]),
        "traitWeightsJson": json.dumps({
            "offensive_exploitation": 0.98,
            "vulnerability_assessment": 0.95,
            "exploit_development": 0.90,
            "web_app_security": 0.92,
            "creative_problem_solving": 0.94,
            "threat_detection": 0.50,
            "log_analysis": 0.45,
            "grc_compliance": 0.25
        }),
        "aptitudeDimensionsJson": json.dumps({
            "out_of_the_box_thinking": 0.96,
            "adversarial_modeling": 0.95,
            "deep_technical_probing": 0.92
        }),
        "workStyleDimensionsJson": json.dumps({
            "offensive_testing": 0.98,
            "autonomous_research": 0.92,
            "technical_reporting": 0.88
        }),
        "typicalRoles": ["Penetration Tester", "Ethical Hacker", "Red Team Specialist", "Security Researcher"],
        "requiredDegrees": ["B.Tech CS", "Certifications (OSCP, CEH, PNPT, GPEN)"],
        "icon": "target",
        "colorHex": "#047857"
    },
    {
        "id": "tech.cyber.forensics",
        "parentId": "tech.cyber",
        "level": 2,
        "name": "Digital Forensics Analyst",
        "shortCode": "FORENSICS",
        "description": "Post-incident digital evidence recovery, chain of custody preservation, memory & disk artifact reconstruction, and root cause analysis",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation", "CareerSwitch"],
        "domain": "Technology",
        "subdomain": "Digital Forensics",
        "specialization": "Forensics & Incident Investigation",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation", "CareerSwitch"]),
        "traitWeightsJson": json.dumps({
            "digital_forensics": 0.98,
            "evidence_reconstruction": 0.96,
            "artifact_analysis": 0.94,
            "memory_analysis": 0.90,
            "chain_of_custody": 0.95,
            "incident_response": 0.85,
            "offensive_exploitation": 0.35,
            "cloud_architecture": 0.45
        }),
        "aptitudeDimensionsJson": json.dumps({
            "meticulous_investigation": 0.98,
            "timeline_reconstruction": 0.95,
            "forensic_synthesis": 0.92
        }),
        "workStyleDimensionsJson": json.dumps({
            "deep_investigative_inquiry": 0.96,
            "methodical_evidence_logging": 0.95,
            "post_mortem_analysis": 0.90
        }),
        "typicalRoles": ["Digital Forensics Analyst", "Incident Forensics Specialist", "Cyber Crime Investigator"],
        "requiredDegrees": ["B.Tech CS", "M.Sc Digital Forensics", "Certifications (CHFI, GCFA, GCFE)"],
        "icon": "database",
        "colorHex": "#065F46"
    },
    {
        "id": "tech.cyber.threat_intel",
        "parentId": "tech.cyber",
        "level": 2,
        "name": "Threat Intelligence Analyst",
        "shortCode": "THREAT_INTEL",
        "description": "Adversary profiling, mapping attacker TTPs (MITRE ATT&CK), dark web tracking, and converting raw intelligence into defense strategies",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Technology",
        "subdomain": "Threat Intelligence",
        "specialization": "Strategic & Technical Intelligence",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation"]),
        "traitWeightsJson": json.dumps({
            "threat_intelligence": 0.98,
            "adversary_attribution": 0.95,
            "mitre_attack_mapping": 0.94,
            "tactical_reporting": 0.90,
            "analytical_synthesis": 0.92,
            "threat_detection": 0.70,
            "offensive_exploitation": 0.50
        }),
        "typicalRoles": ["Cyber Threat Intelligence (CTI) Analyst", "Threat Researcher", "Adversary Hunter"],
        "requiredDegrees": ["B.Tech CS", "M.Tech Cybersecurity", "Certifications (GCTI, CTIA)"],
        "icon": "globe",
        "colorHex": "#047857"
    },
    {
        "id": "tech.cyber.cloud_sec",
        "parentId": "tech.cyber",
        "level": 2,
        "name": "Cloud Security Engineer",
        "shortCode": "CLOUD_SEC",
        "description": "Architecting secure multi-cloud environments (AWS, Azure, GCP), IAM zero-trust policies, container security, and CSPM posture management",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation", "CareerSwitch"],
        "domain": "Technology",
        "subdomain": "Cloud Security",
        "specialization": "Cloud & Infrastructure Security",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation", "CareerSwitch"]),
        "traitWeightsJson": json.dumps({
            "cloud_security": 0.98,
            "iam_zero_trust": 0.95,
            "infrastructure_as_code_security": 0.92,
            "container_kubernetes_sec": 0.90,
            "security_architecture": 0.92,
            "log_analysis": 0.65,
            "offensive_exploitation": 0.45
        }),
        "typicalRoles": ["Cloud Security Engineer", "DevSecOps Engineer", "Cloud Security Architect"],
        "requiredDegrees": ["B.Tech CS/IT", "Certifications (CCSP, AWS Certified Security, Azure Security AZ-500)"],
        "icon": "cloud",
        "colorHex": "#059669"
    },

    # ── Data & AI ──
    {
        "id": "tech.data.ds",
        "parentId": "sci.eng.cs",
        "level": 2,
        "name": "Data Scientist",
        "shortCode": "DATA_SCI",
        "description": "Statistical modeling, predictive machine learning, exploratory data analysis, and deriving business insights from complex datasets",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Technology",
        "subdomain": "Data Science",
        "specialization": "Applied Statistics & Predictive Modeling",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation"]),
        "traitWeightsJson": json.dumps({
            "statistical_analysis": 0.96,
            "predictive_modeling": 0.95,
            "exploratory_data_analysis": 0.92,
            "python_r_programming": 0.90,
            "machine_learning": 0.92,
            "business_communication": 0.80,
            "software_architecture": 0.50
        }),
        "typicalRoles": ["Data Scientist", "Lead Data Scientist", "Analytics Specialist"],
        "requiredDegrees": ["B.Tech CS/Data", "B.Sc/M.Sc Statistics/Math", "M.Tech Data Science"],
        "icon": "bar-chart-2",
        "colorHex": "#6366F1"
    },
    {
        "id": "tech.data.ml_eng",
        "parentId": "sci.eng.cs",
        "level": 2,
        "name": "Machine Learning Engineer",
        "shortCode": "ML_ENG",
        "description": "Production ML model deployment, MLOps pipelines, deep learning architectures, latency optimization, and distributed model training",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation"],
        "domain": "Technology",
        "subdomain": "AI & Machine Learning",
        "specialization": "MLOps & Deep Learning Engineering",
        "eligibleEducationLevelsJson": json.dumps(["Graduation", "Postgraduation"]),
        "traitWeightsJson": json.dumps({
            "deep_learning": 0.96,
            "mlops_pipelines": 0.95,
            "distributed_training": 0.90,
            "software_engineering": 0.92,
            "model_optimization": 0.94,
            "statistical_analysis": 0.85
        }),
        "typicalRoles": ["ML Engineer", "AI Platform Engineer", "Deep Learning Scientist"],
        "requiredDegrees": ["B.Tech CS", "M.Tech AI/ML", "MS Computer Science"],
        "icon": "cpu",
        "colorHex": "#4F46E5"
    },
    {
        "id": "tech.data.analytics",
        "parentId": "sci.eng.cs",
        "level": 2,
        "name": "Data Analyst",
        "shortCode": "DATA_ANALYST",
        "description": "SQL data querying, dashboard development (PowerBI/Tableau), trend metrics extraction, and stakeholder reporting",
        "phasesApplicable": ["AfterGraduation", "AfterPostGraduation", "Graduation", "Postgraduation", "CareerSwitch"],
        "domain": "Technology",
        "subdomain": "Data Analytics",
        "specialization": "Business Intelligence & SQL Analytics",
        "eligibleEducationLevelsJson": json.dumps(["After12th", "Graduation", "Postgraduation", "CareerSwitch"]),
        "traitWeightsJson": json.dumps({
            "sql_querying": 0.96,
            "bi_dashboards": 0.95,
            "business_metrics": 0.90,
            "stakeholder_reporting": 0.92,
            "exploratory_analysis": 0.88,
            "deep_learning": 0.20
        }),
        "typicalRoles": ["Data Analyst", "BI Developer", "Operations Analyst"],
        "requiredDegrees": ["B.Tech", "BCA", "B.Com", "B.Sc", "Any Graduate with SQL/BI"],
        "icon": "pie-chart",
        "colorHex": "#4338CA"
    }
]

for node in new_nodes:
    if node["id"] not in existing_ids:
        taxonomy.append(node)
        existing_ids.add(node["id"])
    else:
        # Update existing node with rich data
        for i, existing in enumerate(taxonomy):
            if existing.get("id") == node["id"]:
                taxonomy[i].update(node)
                break

with open(taxonomy_file, "w", encoding="utf-8") as f:
    json.dump(taxonomy, f, indent=2)

print(f"Updated TaxonomySeed.json with {len(taxonomy)} total nodes.")
