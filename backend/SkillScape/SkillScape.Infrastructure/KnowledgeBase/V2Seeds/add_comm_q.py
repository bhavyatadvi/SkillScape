import json
import os

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_AfterGraduation.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

existing_ids = set(q['id'] for q in data)

new_questions = [
    {
        "id": "Q_EXP_COMM_FIN_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Finance",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within the broad field of Finance, what specific role do you want to pursue?",
        "intent": "Finance Specialization",
        "targetDiscriminatesJson": "[\"comm.finance\"]",
        "options": [
            {"index": 0, "text": "Investment Banking and Mergers & Acquisitions", "signals": [{"trait": "comm.fin.ib", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Corporate Finance and Financial Planning/Analysis (FP&A)", "signals": [{"trait": "comm.fin.corporate", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Asset Management and Portfolio Management", "signals": [{"trait": "comm.fin.asset", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 52,
        "isActive": True
    },
    {
        "id": "Q_EXP_COMM_ACCT_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Accounting",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In Accounting, which practice area is most aligned with your interests?",
        "intent": "Accounting Specialization",
        "targetDiscriminatesJson": "[\"comm.accounting\"]",
        "options": [
            {"index": 0, "text": "Statutory Audit and Assurance Services", "signals": [{"trait": "comm.acct.audit", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Direct and Indirect Taxation", "signals": [{"trait": "comm.acct.tax", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Management Accounting and Cost Control", "signals": [{"trait": "comm.acct.cma", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 53,
        "isActive": True
    },
    {
        "id": "Q_EXP_COMM_MKT_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Marketing",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "When marketing a product, what is your preferred approach?",
        "intent": "Marketing Specialization",
        "targetDiscriminatesJson": "[\"comm.marketing\"]",
        "options": [
            {"index": 0, "text": "Digital Marketing, SEO, and Performance Marketing", "signals": [{"trait": "comm.mkt.digital", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Brand Management and Creative Strategy", "signals": [{"trait": "comm.mkt.brand", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Market Research and Consumer Analytics", "signals": [{"trait": "comm.mkt.research", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 54,
        "isActive": True
    },
    {
        "id": "Q_EXP_COMM_HR_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Human Resources",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within Human Resources (HR), which function do you want to specialize in?",
        "intent": "HR Specialization",
        "targetDiscriminatesJson": "[\"comm.hr\"]",
        "options": [
            {"index": 0, "text": "Talent Acquisition and Recruitment", "signals": [{"trait": "comm.hr.talent", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Compensation, Benefits, and HR Analytics", "signals": [{"trait": "comm.hr.comp", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Organization Development and Employee Relations", "signals": [{"trait": "comm.hr.od", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 55,
        "isActive": True
    },
    {
        "id": "Q_EXP_COMM_SCM_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Supply Chain",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "What aspect of Supply Chain Management and Logistics appeals to you?",
        "intent": "Supply Chain Specialization",
        "targetDiscriminatesJson": "[\"comm.supply_chain\"]",
        "options": [
            {"index": 0, "text": "Procurement and Vendor Management", "signals": [{"trait": "comm.scm.procurement", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Inventory Planning and Demand Forecasting", "signals": [{"trait": "comm.scm.planning", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Global Logistics and Freight Forwarding", "signals": [{"trait": "comm.scm.logistics", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 56,
        "isActive": True
    },
    {
        "id": "Q_EXP_COMM_ENT_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Entrepreneurship",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Goals",
        "competencyId": "",
        "text": "If you are starting a business, what kind of venture are you most likely to build?",
        "intent": "Entrepreneurship Focus",
        "targetDiscriminatesJson": "[\"comm.entrepreneurship\"]",
        "options": [
            {"index": 0, "text": "A high-growth, venture-backed tech startup", "signals": [{"trait": "comm.ent.startup", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "A traditional, cash-flowing SME (e.g., retail, manufacturing)", "signals": [{"trait": "comm.ent.sme", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "A social enterprise focused on impact rather than purely profit", "signals": [{"trait": "comm.ent.social", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 57,
        "isActive": True
    },
    {
        "id": "Q_EXP_COMM_CONS_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Consulting",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In Management Consulting, which practice area do you prefer?",
        "intent": "Consulting Focus",
        "targetDiscriminatesJson": "[\"comm.consulting\"]",
        "options": [
            {"index": 0, "text": "Strategy Consulting (e.g., Market Entry, M&A Strategy)", "signals": [{"trait": "comm.cons.strategy", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Operations Consulting (e.g., Cost Reduction, Process Optimization)", "signals": [{"trait": "comm.cons.operations", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Technology/Digital Transformation Consulting", "signals": [{"trait": "comm.cons.tech", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 58,
        "isActive": True
    },
    {
        "id": "Q_EXP_COMM_IB_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Finance",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "WorkStyle",
        "competencyId": "",
        "text": "Investment Banking is notoriously high-pressure. How do you handle extreme hours and tight deadlines?",
        "intent": "Investment Banking Stress Check",
        "targetDiscriminatesJson": "[\"comm.fin.ib\"]",
        "options": [
            {"index": 0, "text": "I thrive under pressure and am willing to sacrifice work-life balance for high rewards", "signals": [{"trait": "comm.fin.ib", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "I prefer a predictable 9-to-5 schedule even if it means lower compensation", "signals": [{"trait": "comm.fin.ib", "weight": -5.0, "isRejection": True}], "isRejection": True}
        ],
        "displayOrder": 59,
        "isActive": True
    },
    {
        "id": "Q_EXP_COMM_DATA_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Business Analytics",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "For a career in Business Analytics, what toolset are you most comfortable deeply mastering?",
        "intent": "Business Analytics Tools",
        "targetDiscriminatesJson": "[\"comm.analytics\"]",
        "options": [
            {"index": 0, "text": "Advanced Excel, VBA, and Financial Modeling", "signals": [{"trait": "comm.analytics.excel", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "SQL, Python/R, and statistical modeling", "signals": [{"trait": "comm.analytics.ds", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Tableau, PowerBI, and data visualization", "signals": [{"trait": "comm.analytics.viz", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 60,
        "isActive": True
    },
    {
        "id": "Q_EXP_COMM_ECO_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Commerce",
        "subdomain": "Economics",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If you are pursuing Economics, what application of it excites you the most?",
        "intent": "Economics Deep Dive",
        "targetDiscriminatesJson": "[\"comm.economics\"]",
        "options": [
            {"index": 0, "text": "Macroeconomic policy, central banking, and public finance", "signals": [{"trait": "comm.eco.macro", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Microeconomics, behavioral economics, and pricing strategy", "signals": [{"trait": "comm.eco.micro", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Econometrics and quantitative forecasting", "signals": [{"trait": "comm.eco.quant", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 61,
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

print(f'Successfully added {added_count} commerce questions.')
