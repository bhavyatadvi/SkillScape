import json
import os

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_AfterPostGraduation.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

existing_ids = set(q['id'] for q in data)

new_questions = [
    {
        "id": "Q_EXP_SCI_PHD_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Science",
        "subdomain": "Research",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "When conducting pure science research, what type of laboratory work do you prefer?",
        "intent": "Science Lab Preference",
        "targetDiscriminatesJson": "[\"phd.bio\", \"phd.physics\", \"phd.math\"]",
        "options": [
            {"index": 0, "text": "Wet lab: Handling biological samples, chemicals, and cultures", "signals": [{"trait": "phd.bio", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Dry lab: Computational modeling, simulations, and data analysis", "signals": [{"trait": "phd.math", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Hardware lab: Building experimental physics apparatus or optics", "signals": [{"trait": "phd.physics", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 72,
        "isActive": True
    },
    {
        "id": "Q_EXP_ENG_PHD_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Engineering",
        "subdomain": "Research",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "For your Engineering PhD, what is the primary goal of your research?",
        "intent": "Engineering Research Goal",
        "targetDiscriminatesJson": "[\"afterpg.doctoral.phd\"]",
        "options": [
            {"index": 0, "text": "Inventing novel algorithms or software architectures (CS/AI)", "signals": [{"trait": "phd.ai", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Developing new physical materials or hardware (Materials/Mech)", "signals": [{"trait": "phd.materials", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Improving civil infrastructure and sustainable systems (Civil/Env)", "signals": [{"trait": "phd.civil", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 73,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_PHD_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Doctoral",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "WorkStyle",
        "competencyId": "",
        "text": "In a research environment, how do you approach publishing?",
        "intent": "Research Publishing Style",
        "targetDiscriminatesJson": "[\"afterpg.doctoral.phd\"]",
        "options": [
            {"index": 0, "text": "I focus on high-impact journal papers requiring years of deep theoretical proof", "signals": [{"trait": "phd.theoretical", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "I prefer rapid conference publications detailing applied engineering solutions", "signals": [{"trait": "phd.applied", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 74,
        "isActive": True
    },
    {
        "id": "Q_EXP_DOC_ACAD_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Academia",
        "subdomain": "Teaching",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Goals",
        "competencyId": "",
        "text": "Post-PhD, what is your ideal career trajectory?",
        "intent": "Post-PhD Trajectory",
        "targetDiscriminatesJson": "[\"afterpg.doctoral.phd\"]",
        "options": [
            {"index": 0, "text": "Tenure-track University Professor (teaching + research)", "signals": [{"trait": "phd.academia", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Industry R&D Scientist (e.g., at Google Brain, Bell Labs, Pfizer)", "signals": [{"trait": "phd.industry_rnd", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Policy advisor or government research lab scientist", "signals": [{"trait": "phd.government", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 75,
        "isActive": True
    },
    {
        "id": "Q_EXP_DOC_PROF_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Professional Doctorate",
        "subdomain": "DBA/EdD",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If you chose a Professional Doctorate (DBA, EdD, PsyD), what is your motivation?",
        "intent": "Professional Doctorate Focus",
        "targetDiscriminatesJson": "[\"afterpg.doctoral.prof\"]",
        "options": [
            {"index": 0, "text": "To solve high-level corporate strategy and management problems (DBA)", "signals": [{"trait": "prof.dba", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "To reform educational systems, administration, and pedagogy (EdD)", "signals": [{"trait": "prof.edd", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "To attain advanced clinical licensure and run a private practice (PsyD)", "signals": [{"trait": "prof.psyd", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 76,
        "isActive": True
    },
    {
        "id": "Q_EXP_ADVDIP_MGT_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Management",
        "subdomain": "Advanced Diploma",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "For a Post-PG Diploma in Business/Management, what is the target skill?",
        "intent": "Management Diploma Focus",
        "targetDiscriminatesJson": "[\"afterpg.adv_dip.mgmt\"]",
        "options": [
            {"index": 0, "text": "Executive Leadership and Organizational Strategy", "signals": [{"trait": "dip.mgmt.exec", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Business Analytics and Big Data strategy", "signals": [{"trait": "dip.mgmt.analytics", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Digital Marketing and E-Commerce strategy", "signals": [{"trait": "dip.mgmt.digital", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 77,
        "isActive": True
    },
    {
        "id": "Q_EXP_PROF_FIN_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Finance",
        "subdomain": "Professional Qualification",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within elite financial certifications (e.g., CFA, FRM), what is your ultimate career target?",
        "intent": "Finance Cert Target",
        "targetDiscriminatesJson": "[\"afterpg.prof_qual.finance\"]",
        "options": [
            {"index": 0, "text": "Hedge fund portfolio manager or equity research analyst (CFA)", "signals": [{"trait": "prof.fin.cfa", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Quantitative risk modeling for banks or insurance firms (FRM)", "signals": [{"trait": "prof.fin.frm", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Actuarial science and predicting financial impact of risk", "signals": [{"trait": "prof.fin.actuary", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 78,
        "isActive": True
    },
    {
        "id": "Q_EXP_PHD_MATH_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Science",
        "subdomain": "Mathematics",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If pursuing a PhD in Mathematics, which area calls to you?",
        "intent": "Math PhD Specialty",
        "targetDiscriminatesJson": "[\"phd.math\"]",
        "options": [
            {"index": 0, "text": "Pure Mathematics (Algebra, Topology, Number Theory)", "signals": [{"trait": "phd.math.pure", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Applied Mathematics and Mathematical Physics", "signals": [{"trait": "phd.math.applied", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Statistics, Probability Theory, and Stochastic Processes", "signals": [{"trait": "phd.math.stats", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 79,
        "isActive": True
    },
    {
        "id": "Q_EXP_PHD_PHYS_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Science",
        "subdomain": "Physics",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "For a PhD in Physics, what is your domain of choice?",
        "intent": "Physics PhD Specialty",
        "targetDiscriminatesJson": "[\"phd.physics\"]",
        "options": [
            {"index": 0, "text": "Quantum Computing and Condensed Matter Physics", "signals": [{"trait": "phd.physics.quantum", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Astrophysics, Cosmology, and Space Science", "signals": [{"trait": "phd.physics.astro", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "High Energy and Particle Physics (e.g., CERN)", "signals": [{"trait": "phd.physics.particle", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 80,
        "isActive": True
    },
    {
        "id": "Q_EXP_PHD_CHEM_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Science",
        "subdomain": "Chemistry",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "What specialized branch of Chemistry are you pursuing in your PhD?",
        "intent": "Chemistry PhD Specialty",
        "targetDiscriminatesJson": "[\"phd.physics\", \"phd.bio\"]",
        "options": [
            {"index": 0, "text": "Organic Chemistry and drug discovery/synthesis", "signals": [{"trait": "phd.chem.organic", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Physical Chemistry and spectroscopy", "signals": [{"trait": "phd.chem.physical", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Analytical Chemistry and materials characterization", "signals": [{"trait": "phd.chem.analytical", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 81,
        "isActive": True
    }
]

added_count = 0
for q in new_questions:
    if q['id'] not in existing_ids:
        data.append(q)
        added_count += 1

with open(file_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print(f'Successfully added {added_count} science/doctoral questions.')
