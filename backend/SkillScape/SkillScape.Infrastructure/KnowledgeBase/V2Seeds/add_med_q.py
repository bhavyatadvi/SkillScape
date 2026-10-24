import json
import os
import uuid

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_AfterPostGraduation.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

existing_ids = set(q['id'] for q in data)

new_questions = [
    {
        "id": "Q_EXP_MED_DM_002",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "DM",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "When managing chronic, complex systemic diseases, which organ system fascinates you the most?",
        "intent": "DM Organ System Preference",
        "targetDiscriminatesJson": "[\"afterpg.medical_super.dm\"]",
        "options": [
            {
                "index": 0, "text": "The heart and vascular system (Cardiology)",
                "signals": [{"trait": "dm.cardio", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "The brain and nervous system (Neurology)",
                "signals": [{"trait": "dm.neuro", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "The digestive tract and liver (Gastroenterology)",
                "signals": [{"trait": "dm.gastro", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 3, "text": "Hormones and metabolic pathways (Endocrinology)",
                "signals": [{"trait": "dm.gastro", "weight": 4.0, "isRejection": False}], "isRejection": False
            }
        ],
        "displayOrder": 20,
        "isActive": True
    },
    {
        "id": "Q_EXP_MED_MCH_002",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "MCh",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In the operating theater, what type of surgical procedure gives you the most satisfaction?",
        "intent": "MCh Surgical Preference",
        "targetDiscriminatesJson": "[\"afterpg.medical_super.mch\"]",
        "options": [
            {
                "index": 0, "text": "Micro-neurosurgery and spinal operations",
                "signals": [{"trait": "mch.neuro", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "Open heart surgeries, bypasses, and valve replacements",
                "signals": [{"trait": "mch.cardio", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "Complex reconstructive procedures and microsurgery",
                "signals": [{"trait": "mch.plastic", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 3, "text": "Minimally invasive endourology and renal transplants",
                "signals": [{"trait": "mch.uro", "weight": 5.0, "isRejection": False}], "isRejection": False
            }
        ],
        "displayOrder": 21,
        "isActive": True
    },
    {
        "id": "Q_EXP_MED_ONCO_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "Oncology",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "When treating cancer patients, how do you prefer to manage their care?",
        "intent": "Medical vs Surgical Oncology",
        "targetDiscriminatesJson": "[\"afterpg.medical_super\"]",
        "options": [
            {
                "index": 0, "text": "Using targeted therapies, chemotherapy, and immunotherapy (Medical Oncology)",
                "signals": [{"trait": "dm.onco", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "Excising tumors and managing oncological reconstructions (Surgical Oncology)",
                "signals": [{"trait": "mch.onco", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "I am not interested in Oncology",
                "signals": [{"trait": "dm.onco", "weight": -5.0, "isRejection": True}, {"trait": "mch.onco", "weight": -5.0, "isRejection": True}], "isRejection": True
            }
        ],
        "displayOrder": 22,
        "isActive": True
    },
    {
        "id": "Q_EXP_MED_FNB_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "FNB",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If you are considering a Fellowship (FNB) instead of a DM/MCh, which area appeals most?",
        "intent": "FNB Fellowship Preference",
        "targetDiscriminatesJson": "[\"afterpg.medical_super.fnb\"]",
        "options": [
            {
                "index": 0, "text": "Intensive Care and Critical Care Medicine",
                "signals": [{"trait": "dm.critical", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "Reproductive Medicine and IVF",
                "signals": [{"trait": "fnb.repro", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "Neonatology and Pediatric Intensive Care",
                "signals": [{"trait": "fnb.neo", "weight": 5.0, "isRejection": False}], "isRejection": False
            }
        ],
        "displayOrder": 23,
        "isActive": True
    },
    {
        "id": "Q_EXP_MED_CRIT_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "Critical Care",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "What aspect of critical care medicine draws your interest?",
        "intent": "Critical Care Deep Dive",
        "targetDiscriminatesJson": "[\"dm.critical\"]",
        "options": [
            {
                "index": 0, "text": "Managing multi-organ failure and sepsis in the ICU",
                "signals": [{"trait": "dm.critical", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "Rapid resuscitation and trauma management in the ER",
                "signals": [{"trait": "dm.critical.er", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "I prefer predictable, scheduled patient care over high-stress environments",
                "signals": [{"trait": "dm.critical", "weight": -5.0, "isRejection": True}], "isRejection": True
            }
        ],
        "displayOrder": 24,
        "isActive": True
    },
    {
        "id": "Q_EXP_MED_GASTRO_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "DM",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within Gastroenterology, what procedural skills do you wish to master?",
        "intent": "Gastroenterology Interest",
        "targetDiscriminatesJson": "[\"dm.gastro\"]",
        "options": [
            {
                "index": 0, "text": "Advanced therapeutic endoscopy and ERCP",
                "signals": [{"trait": "dm.gastro.endo", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "Hepatology and managing chronic liver disease/transplants",
                "signals": [{"trait": "dm.gastro.hepato", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "I am not interested in GI procedures",
                "signals": [{"trait": "dm.gastro", "weight": -3.0, "isRejection": False}], "isRejection": False
            }
        ],
        "displayOrder": 25,
        "isActive": True
    },
    {
        "id": "Q_EXP_MED_NEURO_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "MCh",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If pursuing Neurosurgery, which sub-niche do you find most compelling?",
        "intent": "Neurosurgery Deep Dive",
        "targetDiscriminatesJson": "[\"mch.neuro\"]",
        "options": [
            {
                "index": 0, "text": "Vascular neurosurgery and aneurysm clipping",
                "signals": [{"trait": "mch.neuro.vascular", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "Neuro-oncology and complex brain tumor resections",
                "signals": [{"trait": "mch.neuro.onco", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "Spinal surgery and instrumentation",
                "signals": [{"trait": "mch.neuro.spine", "weight": 5.0, "isRejection": False}], "isRejection": False
            }
        ],
        "displayOrder": 26,
        "isActive": True
    },
    {
        "id": "Q_EXP_MED_URO_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "MCh",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "What aspects of Urology (MCh) align with your surgical style?",
        "intent": "Urology Deep Dive",
        "targetDiscriminatesJson": "[\"mch.uro\"]",
        "options": [
            {
                "index": 0, "text": "Robotic-assisted radical prostatectomies and pelvic oncology",
                "signals": [{"trait": "mch.uro.onco", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "Endourology and minimally invasive stone management",
                "signals": [{"trait": "mch.uro.endo", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "Renal transplant surgery",
                "signals": [{"trait": "mch.uro.transplant", "weight": 5.0, "isRejection": False}], "isRejection": False
            }
        ],
        "displayOrder": 27,
        "isActive": True
    },
    {
        "id": "Q_EXP_MED_PLASTIC_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "MCh",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within Plastic Surgery, where do you see your primary focus?",
        "intent": "Plastic Surgery Deep Dive",
        "targetDiscriminatesJson": "[\"mch.plastic\"]",
        "options": [
            {
                "index": 0, "text": "Reconstructive microsurgery (e.g., post-trauma or post-cancer)",
                "signals": [{"trait": "mch.plastic.recon", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "Aesthetic and cosmetic surgery",
                "signals": [{"trait": "mch.plastic.cosmetic", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "Craniofacial surgery and congenital defect repair",
                "signals": [{"trait": "mch.plastic.cranio", "weight": 5.0, "isRejection": False}], "isRejection": False
            }
        ],
        "displayOrder": 28,
        "isActive": True
    },
    {
        "id": "Q_EXP_MED_CARDIO_001",
        "phaseKey": "AfterPostGraduation",
        "subPhaseKey": "",
        "domain": "Medical",
        "subdomain": "DM",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "When practicing Cardiology, what interventions interest you the most?",
        "intent": "Cardiology Deep Dive",
        "targetDiscriminatesJson": "[\"dm.cardio\"]",
        "options": [
            {
                "index": 0, "text": "Interventional cardiology (angioplasty, stenting, TAVR)",
                "signals": [{"trait": "dm.cardio.interventional", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 1, "text": "Electrophysiology (mapping arrhythmias, pacemaker insertion)",
                "signals": [{"trait": "dm.cardio.ep", "weight": 5.0, "isRejection": False}], "isRejection": False
            },
            {
                "index": 2, "text": "Non-invasive echocardiography and heart failure management",
                "signals": [{"trait": "dm.cardio.noninvasive", "weight": 5.0, "isRejection": False}], "isRejection": False
            }
        ],
        "displayOrder": 29,
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

print(f'Successfully added {added_count} medical questions.')
