import json
import os

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_AfterGraduation.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

existing_ids = set(q['id'] for q in data)

new_questions = [
    {
        "id": "Q_EXP_LAW_CORP_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "Corporate Law",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "When dealing with corporate entities, which aspect of their legal operations interests you most?",
        "intent": "Corporate Law Deep Dive",
        "targetDiscriminatesJson": "[\"law.corporate\"]",
        "options": [
            {"index": 0, "text": "Mergers, Acquisitions, and large-scale corporate restructuring", "signals": [{"trait": "law.corporate.ma", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Drafting and negotiating complex commercial contracts", "signals": [{"trait": "law.corporate.contracts", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Ensuring regulatory compliance and corporate governance", "signals": [{"trait": "law.corporate.compliance", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 30,
        "isActive": True
    },
    {
        "id": "Q_EXP_LAW_CRIM_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "Criminal Law",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In the realm of Criminal Law, what is your preferred role?",
        "intent": "Criminal Law Deep Dive",
        "targetDiscriminatesJson": "[\"law.criminal\"]",
        "options": [
            {"index": 0, "text": "Defending individuals accused of crimes (Defense Attorney)", "signals": [{"trait": "law.criminal.defense", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Representing the state in criminal proceedings (Prosecutor)", "signals": [{"trait": "law.criminal.prosecution", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Appellate litigation and challenging convictions", "signals": [{"trait": "law.criminal.appeals", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 31,
        "isActive": True
    },
    {
        "id": "Q_EXP_LAW_IPR_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "IPR",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within Intellectual Property Rights (IPR), which asset class do you want to specialize in protecting?",
        "intent": "IPR Deep Dive",
        "targetDiscriminatesJson": "[\"law.ipr\"]",
        "options": [
            {"index": 0, "text": "Patents and technological innovations", "signals": [{"trait": "law.ipr.patents", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Copyrights for creative works, media, and software", "signals": [{"trait": "law.ipr.copyrights", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Trademarks and brand identity protection", "signals": [{"trait": "law.ipr.trademarks", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 32,
        "isActive": True
    },
    {
        "id": "Q_EXP_LAW_CYBER_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "Cyber Law",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Cyber Law covers many digital domains. Which area is most critical to your career interests?",
        "intent": "Cyber Law Deep Dive",
        "targetDiscriminatesJson": "[\"law.cyber\"]",
        "options": [
            {"index": 0, "text": "Data Privacy and GDPR compliance", "signals": [{"trait": "law.cyber.privacy", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Handling cybercrime and digital forensics cases", "signals": [{"trait": "law.cyber.crime", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Tech policy and regulating artificial intelligence", "signals": [{"trait": "law.cyber.policy", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 33,
        "isActive": True
    },
    {
        "id": "Q_EXP_LAW_HUMAN_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "Human Rights",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In the field of Human Rights and Public International Law, what is your primary focus?",
        "intent": "Human Rights Deep Dive",
        "targetDiscriminatesJson": "[\"law.human_rights\"]",
        "options": [
            {"index": 0, "text": "Advocating for marginalized communities in domestic courts (PIL)", "signals": [{"trait": "law.human_rights.domestic", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Working with international bodies like the UN or Amnesty International", "signals": [{"trait": "law.human_rights.intl", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Refugee, immigration, and asylum law", "signals": [{"trait": "law.human_rights.refugee", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 34,
        "isActive": True
    },
    {
        "id": "Q_EXP_LAW_ENV_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "Environmental Law",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "How do you wish to practice Environmental Law?",
        "intent": "Environmental Law Deep Dive",
        "targetDiscriminatesJson": "[\"law.environmental\"]",
        "options": [
            {"index": 0, "text": "Helping corporations navigate environmental compliance and ESG standards", "signals": [{"trait": "law.environmental.corporate", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Litigating against polluters on behalf of NGOs and communities", "signals": [{"trait": "law.environmental.litigation", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Drafting policy and advising government agencies on climate regulations", "signals": [{"trait": "law.environmental.policy", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 35,
        "isActive": True
    },
    {
        "id": "Q_EXP_LAW_TAX_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "Tax Law",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "What type of Tax Law is most appealing to your analytical skills?",
        "intent": "Tax Law Deep Dive",
        "targetDiscriminatesJson": "[\"law.tax\"]",
        "options": [
            {"index": 0, "text": "International tax structuring and transfer pricing", "signals": [{"trait": "law.tax.international", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Corporate taxation and transaction advisory", "signals": [{"trait": "law.tax.corporate", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Resolving tax disputes and litigation with authorities", "signals": [{"trait": "law.tax.litigation", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 36,
        "isActive": True
    },
    {
        "id": "Q_EXP_LAW_LIT_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "Litigation",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If you choose to be a litigator arguing in court, what is your preferred court setting?",
        "intent": "Litigation Setting Preference",
        "targetDiscriminatesJson": "[\"law.litigation\"]",
        "options": [
            {"index": 0, "text": "Trial courts, cross-examining witnesses and presenting evidence to a judge/jury", "signals": [{"trait": "law.litigation.trial", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Appellate courts, arguing complex points of law and statutory interpretation", "signals": [{"trait": "law.litigation.appellate", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Alternative Dispute Resolution (Arbitration/Mediation)", "signals": [{"trait": "law.litigation.adr", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 37,
        "isActive": True
    },
    {
        "id": "Q_EXP_LAW_FAMILY_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "Family Law",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "When handling Family Law cases, what approach do you prefer?",
        "intent": "Family Law Style",
        "targetDiscriminatesJson": "[\"law.family\"]",
        "options": [
            {"index": 0, "text": "Aggressive representation in contested divorces and custody battles", "signals": [{"trait": "law.family.litigation", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Collaborative divorce and mediation to reach amicable settlements", "signals": [{"trait": "law.family.mediation", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Estate planning, wills, and trusts", "signals": [{"trait": "law.family.estate", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 38,
        "isActive": True
    },
    {
        "id": "Q_EXP_LAW_CORP_002",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Law",
        "subdomain": "Corporate Law",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "WorkStyle",
        "competencyId": "",
        "text": "As a corporate lawyer, would you prefer working in a law firm or in-house?",
        "intent": "Corporate Law Environment Preference",
        "targetDiscriminatesJson": "[\"law.corporate\"]",
        "options": [
            {"index": 0, "text": "In a fast-paced top-tier law firm advising multiple diverse corporate clients", "signals": [{"trait": "law.firm_practice", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "As In-House Counsel, deeply integrated into the business strategy of a single company", "signals": [{"trait": "law.inhouse", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 39,
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

print(f'Successfully added {added_count} law questions.')
