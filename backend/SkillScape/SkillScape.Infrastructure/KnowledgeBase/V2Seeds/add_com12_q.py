import json

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_After12_Commerce.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

existing_ids = set(q['id'] for q in data)

new_questions = [
    {
        "id": "Q_EXP_12COM_CA_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Professional Degrees",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If you are considering a professional qualification like CA, CS, or CMA, what is your primary strength?",
        "intent": "CA vs CS vs CMA",
        "targetDiscriminatesJson": "[\"comm.prof\"]",
        "options": [
            {"index": 0, "text": "Deep financial analysis, auditing, and taxation (Chartered Accountancy - CA)", "signals": [{"trait": "deg.ca", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Corporate governance, law, and regulatory compliance (Company Secretary - CS)", "signals": [{"trait": "deg.cs", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Cost management, budgeting, and strategic pricing (Cost & Management Accountancy - CMA)", "signals": [{"trait": "deg.cma", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 10,
        "isActive": True
    },
    {
        "id": "Q_EXP_12COM_BBA_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Management",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within Business Administration (BBA/BMS), which area of management appeals to you the most?",
        "intent": "BBA Specialization",
        "targetDiscriminatesJson": "[\"comm.management\"]",
        "options": [
            {"index": 0, "text": "Marketing, branding, and consumer behavior", "signals": [{"trait": "deg.bba.marketing", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Finance, investment analysis, and banking", "signals": [{"trait": "deg.bba.finance", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Human Resources and organizational behavior", "signals": [{"trait": "deg.bba.hr", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 3, "text": "Operations, logistics, and supply chain management", "signals": [{"trait": "deg.bba.operations", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 11,
        "isActive": True
    },
    {
        "id": "Q_EXP_12COM_BCOM_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Accounting",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "For a B.Com degree, which specialization path interests you more?",
        "intent": "B.Com Honors vs General",
        "targetDiscriminatesJson": "[\"comm.bcom\"]",
        "options": [
            {"index": 0, "text": "B.Com (Honors) with a heavy focus on advanced accounting and corporate finance", "signals": [{"trait": "deg.bcom.honors", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "B.Com (General) covering a broad mix of taxation, law, and economics", "signals": [{"trait": "deg.bcom.general", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "B.Com with an integrated professional program (e.g., ACCA, CIMA)", "signals": [{"trait": "deg.bcom.professional", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 12,
        "isActive": True
    },
    {
        "id": "Q_EXP_12COM_ECO_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Economics",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If you pursue a BA/BSc in Economics, what type of analysis do you prefer?",
        "intent": "Economics Degree Focus",
        "targetDiscriminatesJson": "[\"comm.economics\"]",
        "options": [
            {"index": 0, "text": "Macroeconomics: Analyzing national policies, inflation, and global trade", "signals": [{"trait": "deg.eco.macro", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Microeconomics: Understanding consumer choices and corporate pricing strategies", "signals": [{"trait": "deg.eco.micro", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Econometrics: Heavy use of mathematics and statistics to model economic data", "signals": [{"trait": "deg.eco.quant", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 13,
        "isActive": True
    },
    {
        "id": "Q_EXP_12COM_LAW_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Law",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "As a Commerce student looking into an integrated Law degree (B.Com LLB / BBA LLB), what is your primary career goal?",
        "intent": "Commerce Law Integrated",
        "targetDiscriminatesJson": "[\"comm.law\"]",
        "options": [
            {"index": 0, "text": "Practicing Corporate Law, dealing with mergers, acquisitions, and compliance", "signals": [{"trait": "deg.llb.corporate", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Focusing on Taxation Law and commercial litigation", "signals": [{"trait": "deg.llb.tax", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Intellectual Property and technology business laws", "signals": [{"trait": "deg.llb.ip", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 14,
        "isActive": True
    },
    {
        "id": "Q_EXP_12COM_ENT_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Entrepreneurship",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Goals",
        "competencyId": "",
        "text": "If you plan to start your own business after graduation, what kind of venture are you envisioning?",
        "intent": "Entrepreneurship Focus",
        "targetDiscriminatesJson": "[\"comm.entrepreneurship\"]",
        "options": [
            {"index": 0, "text": "A scalable tech-based startup seeking venture capital", "signals": [{"trait": "deg.ent.startup", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "A traditional product or service business (retail, manufacturing, agency)", "signals": [{"trait": "deg.ent.traditional", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Taking over or expanding an existing family business", "signals": [{"trait": "deg.ent.family", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 15,
        "isActive": True
    },
    {
        "id": "Q_EXP_12COM_TECH_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Technology",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Commerce is increasingly merging with Technology. Which tech-commerce intersection interests you?",
        "intent": "Commerce Tech Intersection",
        "targetDiscriminatesJson": "[\"comm.other\", \"comm.management\"]",
        "options": [
            {"index": 0, "text": "FinTech: Digital banking, cryptocurrencies, and payment gateways", "signals": [{"trait": "comm.tech.fintech", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Business Analytics: Using data science and SQL to drive business decisions", "signals": [{"trait": "comm.tech.analytics", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "E-Commerce: Managing online retail platforms and digital supply chains", "signals": [{"trait": "comm.tech.ecommerce", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 16,
        "isActive": True
    },
    {
        "id": "Q_EXP_12COM_MATH_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Quantitative",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "WorkStyle",
        "competencyId": "",
        "text": "How comfortable are you with advanced mathematics (Calculus, Statistics) as part of your Commerce degree?",
        "intent": "Math Comfort Level",
        "targetDiscriminatesJson": "[]",
        "options": [
            {"index": 0, "text": "Highly comfortable. I enjoy quantitative subjects like Actuarial Science or Econometrics.", "signals": [{"trait": "comm.math_high", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Moderately comfortable. Basic business math and statistics are fine.", "signals": [{"trait": "comm.math_med", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Not comfortable. I prefer theory, management, and law over heavy math.", "signals": [{"trait": "comm.math_low", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 17,
        "isActive": True
    },
    {
        "id": "Q_EXP_12COM_HOTEL_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Hospitality",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If considering a BBA in Hospitality or Tourism, what aspect of the industry draws you?",
        "intent": "Hospitality Focus",
        "targetDiscriminatesJson": "[\"comm.hospitality\"]",
        "options": [
            {"index": 0, "text": "Hotel Operations and luxury brand management", "signals": [{"trait": "deg.hm.operations", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Event Management and organizing large-scale corporate or entertainment events", "signals": [{"trait": "deg.hm.events", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Travel, Tourism, and Aviation management", "signals": [{"trait": "deg.hm.tourism", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 18,
        "isActive": True
    },
    {
        "id": "Q_EXP_12COM_WORK_001",
        "phaseKey": "After12",
        "subPhaseKey": "Commerce",
        "domain": "Commerce",
        "subdomain": "Work Environment",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "WorkStyle",
        "competencyId": "",
        "text": "When thinking about your post-graduation career, what type of organizational culture do you prefer?",
        "intent": "Corporate Culture Preference",
        "targetDiscriminatesJson": "[\"comm.management\", \"comm.other\"]",
        "options": [
            {"index": 0, "text": "A highly competitive, fast-paced corporate environment (e.g., Big 4, Investment Banks)", "signals": [{"trait": "culture.corporate_intense", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "A stable, structured environment with good work-life balance (e.g., PSU, Govt Banks)", "signals": [{"trait": "culture.stable", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "A dynamic, flexible startup culture with high autonomy", "signals": [{"trait": "culture.startup", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 19,
        "isActive": True
    }
]

added_count = 0
for q in new_questions:
    if q['id'] not in existing_ids:
        # Avoid empty discriminates bug for non-root questions
        if q['targetDiscriminatesJson'] == '[]' and q['intent'] != 'Math Comfort Level':
             q['targetDiscriminatesJson'] = '["domain.commerce"]'
        data.append(q)
        added_count += 1

with open(file_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print(f'Successfully added {added_count} commerce after 12th questions.')
