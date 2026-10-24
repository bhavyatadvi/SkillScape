import json
import os

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_AfterGraduation.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

existing_ids = set(q['id'] for q in data)

new_questions = [
    {
        "id": "Q_EXP_TECH_CS_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Computer Science",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within Computer Science, what layer of the technology stack do you most enjoy working on?",
        "intent": "CS Stack Preference",
        "targetDiscriminatesJson": "[\"tech.cs\"]",
        "options": [
            {"index": 0, "text": "Front-end and user interface design (React, Vue, UX)", "signals": [{"trait": "tech.cs.frontend", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Back-end server logic, APIs, and databases (Node, .NET, Java)", "signals": [{"trait": "tech.cs.backend", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Low-level systems programming (C/C++, Rust, OS Kernels)", "signals": [{"trait": "tech.cs.systems", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 3, "text": "Cloud infrastructure and DevOps (AWS, Kubernetes, CI/CD)", "signals": [{"trait": "tech.cs.cloud", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 40,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_AI_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Artificial Intelligence",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "What aspect of Artificial Intelligence and Machine Learning do you find most compelling?",
        "intent": "AI Specialization",
        "targetDiscriminatesJson": "[\"tech.ai\"]",
        "options": [
            {"index": 0, "text": "Natural Language Processing (NLP) and Large Language Models", "signals": [{"trait": "tech.ai.nlp", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Computer Vision and Image Recognition", "signals": [{"trait": "tech.ai.cv", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Reinforcement Learning and Autonomous Agents", "signals": [{"trait": "tech.ai.rl", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 41,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_CYBER_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Cybersecurity",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In Cybersecurity, which approach matches your problem-solving style?",
        "intent": "Cybersecurity Red vs Blue",
        "targetDiscriminatesJson": "[\"tech.cyber\"]",
        "options": [
            {"index": 0, "text": "Offensive Security: Penetration testing and finding vulnerabilities (Red Team)", "signals": [{"trait": "tech.cyber.offensive", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Defensive Security: Incident response, threat hunting, and SOC (Blue Team)", "signals": [{"trait": "tech.cyber.defensive", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Security Architecture and designing Zero Trust networks", "signals": [{"trait": "tech.cyber.architecture", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 42,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_DATA_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Data Science",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "When working with big data, what is your primary interest?",
        "intent": "Data Science vs Data Engineering",
        "targetDiscriminatesJson": "[\"tech.data\"]",
        "options": [
            {"index": 0, "text": "Building ETL pipelines and managing data warehouses (Data Engineering)", "signals": [{"trait": "tech.data.engineering", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Creating dashboards and extracting business insights (Data Analytics)", "signals": [{"trait": "tech.data.analytics", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Developing predictive statistical models and algorithms (Data Science)", "signals": [{"trait": "tech.data.science", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 43,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_CIVIL_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Civil Engineering",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within Civil Engineering, what type of infrastructure project excites you?",
        "intent": "Civil Engineering Deep Dive",
        "targetDiscriminatesJson": "[\"tech.civil\"]",
        "options": [
            {"index": 0, "text": "Designing high-rise buildings and structural frameworks", "signals": [{"trait": "tech.civil.structural", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Transportation networks, highways, and bridges", "signals": [{"trait": "tech.civil.transportation", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Water resources, dams, and environmental engineering", "signals": [{"trait": "tech.civil.water", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 44,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_MECH_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Mechanical Engineering",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "What sub-field of Mechanical Engineering aligns with your career goals?",
        "intent": "Mechanical Engineering Deep Dive",
        "targetDiscriminatesJson": "[\"tech.mechanical\"]",
        "options": [
            {"index": 0, "text": "Thermodynamics, HVAC, and energy systems", "signals": [{"trait": "tech.mech.thermal", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Robotics, mechatronics, and automation", "signals": [{"trait": "tech.mech.robotics", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Automotive design and aerodynamics", "signals": [{"trait": "tech.mech.auto", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 45,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_ELEC_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Electrical Engineering",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In Electrical Engineering, do you prefer working with macro or micro systems?",
        "intent": "Electrical Engineering Power vs VLSI",
        "targetDiscriminatesJson": "[\"tech.electrical\"]",
        "options": [
            {"index": 0, "text": "Macro: Power generation, smart grids, and renewable energy", "signals": [{"trait": "tech.electrical.power", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Micro: VLSI, semiconductor design, and embedded systems", "signals": [{"trait": "tech.electrical.vlsi", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Telecommunications and signal processing", "signals": [{"trait": "tech.electrical.telecom", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 46,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_PROD_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Product Management",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "WorkStyle",
        "competencyId": "",
        "text": "If you transition into Tech Product Management, what part of the lifecycle is most exciting?",
        "intent": "Product Management Focus",
        "targetDiscriminatesJson": "[\"tech.product\"]",
        "options": [
            {"index": 0, "text": "User research, wireframing, and product discovery", "signals": [{"trait": "tech.product.discovery", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Writing technical PRDs, managing sprints, and unblocking engineers", "signals": [{"trait": "tech.product.execution", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Go-to-market strategy, pricing, and growth metrics", "signals": [{"trait": "tech.product.growth", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 47,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_AERO_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Aerospace Engineering",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within Aerospace, what specialized engineering discipline do you want to pursue?",
        "intent": "Aerospace Deep Dive",
        "targetDiscriminatesJson": "[\"tech.aerospace\"]",
        "options": [
            {"index": 0, "text": "Aerodynamics and computational fluid dynamics (CFD)", "signals": [{"trait": "tech.aero.aerodynamics", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Avionics, flight control systems, and navigation", "signals": [{"trait": "tech.aero.avionics", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Propulsion systems and rocket engine design", "signals": [{"trait": "tech.aero.propulsion", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 48,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_BIOTECH_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Biotechnology",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "For a career in Biotechnology, which application area is your primary focus?",
        "intent": "Biotech Deep Dive",
        "targetDiscriminatesJson": "[\"tech.biotech\"]",
        "options": [
            {"index": 0, "text": "Medical and Pharmaceutical Biotechnology (Red Biotech)", "signals": [{"trait": "tech.biotech.medical", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Agricultural Biotechnology and genetically modified crops (Green Biotech)", "signals": [{"trait": "tech.biotech.agri", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Industrial Biotechnology and biofuels (White Biotech)", "signals": [{"trait": "tech.biotech.industrial", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 49,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_CLOUD_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Cloud Computing",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In the Cloud Computing domain, what type of architecture are you most interested in mastering?",
        "intent": "Cloud Engineering Specialty",
        "targetDiscriminatesJson": "[\"tech.cs.cloud\"]",
        "options": [
            {"index": 0, "text": "Serverless architectures (e.g., AWS Lambda, EventBridge)", "signals": [{"trait": "tech.cloud.serverless", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Container orchestration and microservices (Kubernetes, Docker)", "signals": [{"trait": "tech.cloud.kubernetes", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Multi-cloud strategy and infrastructure as code (Terraform)", "signals": [{"trait": "tech.cloud.multi", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 50,
        "isActive": True
    },
    {
        "id": "Q_EXP_TECH_QA_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Technology",
        "subdomain": "Quality Assurance",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "WorkStyle",
        "competencyId": "",
        "text": "If working in Software Quality Assurance (QA), what is your preferred methodology?",
        "intent": "QA Specialization",
        "targetDiscriminatesJson": "[\"tech.qa\"]",
        "options": [
            {"index": 0, "text": "Writing automated test scripts using Selenium or Playwright", "signals": [{"trait": "tech.qa.automation", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Manual exploratory testing and uncovering edge cases", "signals": [{"trait": "tech.qa.manual", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Performance and load testing (e.g., JMeter)", "signals": [{"trait": "tech.qa.performance", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 51,
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

print(f'Successfully added {added_count} technology questions.')
