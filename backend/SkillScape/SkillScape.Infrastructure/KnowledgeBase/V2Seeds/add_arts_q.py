import json
import os

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_AfterGraduation.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

existing_ids = set(q['id'] for q in data)

new_questions = [
    {
        "id": "Q_EXP_ARTS_DES_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Design",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within the Design field, what medium do you prefer working with?",
        "intent": "Design Specialization",
        "targetDiscriminatesJson": "[\"arts.design\"]",
        "options": [
            {"index": 0, "text": "Digital interfaces, UX/UI, and web design", "signals": [{"trait": "arts.design.ux", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Physical products, industrial design, and 3D modeling", "signals": [{"trait": "arts.design.industrial", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Graphic design, branding, and visual communication", "signals": [{"trait": "arts.design.graphic", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 3, "text": "Fashion, textiles, and apparel design", "signals": [{"trait": "arts.design.fashion", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 62,
        "isActive": True
    },
    {
        "id": "Q_EXP_ARTS_MEDIA_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Media & Journalism",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In Media and Journalism, what role suits your communication style best?",
        "intent": "Media Specialization",
        "targetDiscriminatesJson": "[\"arts.media\"]",
        "options": [
            {"index": 0, "text": "Investigative journalism and long-form writing", "signals": [{"trait": "arts.media.journalism", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Broadcast media, anchoring, and video production", "signals": [{"trait": "arts.media.broadcast", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Public Relations (PR) and Corporate Communications", "signals": [{"trait": "arts.media.pr", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 63,
        "isActive": True
    },
    {
        "id": "Q_EXP_ARTS_PSYCH_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Psychology",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If pursuing Psychology, which application of it is most fulfilling to you?",
        "intent": "Psychology Specialization",
        "targetDiscriminatesJson": "[\"arts.psychology\"]",
        "options": [
            {"index": 0, "text": "Clinical Psychology and treating mental health disorders", "signals": [{"trait": "arts.psych.clinical", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Counseling Psychology and helping individuals with life transitions", "signals": [{"trait": "arts.psych.counseling", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Industrial/Organizational (I/O) Psychology in corporate settings", "signals": [{"trait": "arts.psych.io", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 64,
        "isActive": True
    },
    {
        "id": "Q_EXP_ARTS_FILM_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Film & Animation",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In Film and Animation, where do you want to contribute?",
        "intent": "Film Specialization",
        "targetDiscriminatesJson": "[\"arts.film\"]",
        "options": [
            {"index": 0, "text": "Directing and Screenwriting", "signals": [{"trait": "arts.film.direction", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Cinematography and Camera Work", "signals": [{"trait": "arts.film.cinematography", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Post-production, VFX, and Animation", "signals": [{"trait": "arts.film.vfx", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 65,
        "isActive": True
    },
    {
        "id": "Q_EXP_ARTS_HUMAN_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Humanities",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "For a career in Humanities (History, Sociology, Lit), what path is your goal?",
        "intent": "Humanities Specialization",
        "targetDiscriminatesJson": "[\"arts.humanities\"]",
        "options": [
            {"index": 0, "text": "Academia, Research, and Teaching", "signals": [{"trait": "arts.humanities.academia", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Public Policy, Think Tanks, and NGOs", "signals": [{"trait": "arts.humanities.policy", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Museum Curation, Archiving, and Heritage Management", "signals": [{"trait": "arts.humanities.curation", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 66,
        "isActive": True
    },
    {
        "id": "Q_EXP_ARTS_LANG_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Languages",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If you specialize in Languages and Linguistics, what work do you prefer?",
        "intent": "Languages Specialization",
        "targetDiscriminatesJson": "[\"arts.languages\"]",
        "options": [
            {"index": 0, "text": "Translation and Interpretation for international organizations/businesses", "signals": [{"trait": "arts.lang.translation", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Computational Linguistics and working with AI/NLP", "signals": [{"trait": "arts.lang.computational", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Foreign language education and cultural exchange", "signals": [{"trait": "arts.lang.education", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 67,
        "isActive": True
    },
    {
        "id": "Q_EXP_ARTS_PERF_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Performing Arts",
        "questionType": "multiple_choice",
        "languageComplexity": "Intermediate",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "Within Performing Arts (Music, Dance, Theater), what is your primary focus?",
        "intent": "Performing Arts Focus",
        "targetDiscriminatesJson": "[\"arts.performing\"]",
        "options": [
            {"index": 0, "text": "Live Performance and Stage Acting/Dancing", "signals": [{"trait": "arts.perf.stage", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Choreography, Composition, or Directing", "signals": [{"trait": "arts.perf.composition", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Arts Administration, Producing, and Talent Management", "signals": [{"trait": "arts.perf.admin", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 68,
        "isActive": True
    },
    {
        "id": "Q_EXP_ARTS_SOC_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Social Work",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "In the field of Social Work, which population or issue do you want to serve?",
        "intent": "Social Work Specialization",
        "targetDiscriminatesJson": "[\"arts.social_work\"]",
        "options": [
            {"index": 0, "text": "Child and Family Welfare", "signals": [{"trait": "arts.sw.family", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "Substance Abuse and Mental Health Recovery", "signals": [{"trait": "arts.sw.mental", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Community Organizing and Policy Advocacy", "signals": [{"trait": "arts.sw.community", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 69,
        "isActive": True
    },
    {
        "id": "Q_EXP_ARTS_LAW_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Public Policy",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "Specialization",
        "competencyId": "",
        "text": "If you are interested in Public Policy or Public Administration, what role do you seek?",
        "intent": "Public Policy Focus",
        "targetDiscriminatesJson": "[\"arts.policy\"]",
        "options": [
            {"index": 0, "text": "Policy Analyst researching data to propose legislative changes", "signals": [{"trait": "arts.policy.analyst", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "City Management or Government Administrator", "signals": [{"trait": "arts.policy.admin", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Political Campaign Management and Strategy", "signals": [{"trait": "arts.policy.campaign", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 70,
        "isActive": True
    },
    {
        "id": "Q_EXP_ARTS_UX_001",
        "phaseKey": "AfterGraduation",
        "subPhaseKey": "",
        "domain": "Arts",
        "subdomain": "Design",
        "questionType": "multiple_choice",
        "languageComplexity": "Advanced",
        "dimension": "WorkStyle",
        "competencyId": "",
        "text": "In UX/UI Design, which phase of the design process do you excel at?",
        "intent": "UX vs UI Focus",
        "targetDiscriminatesJson": "[\"arts.design.ux\"]",
        "options": [
            {"index": 0, "text": "UX Research: Conducting user interviews, creating personas, and mapping journeys", "signals": [{"trait": "arts.design.ux_research", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 1, "text": "UI Design: Crafting high-fidelity mockups, typography, and visual systems", "signals": [{"trait": "arts.design.ui", "weight": 5.0, "isRejection": False}], "isRejection": False},
            {"index": 2, "text": "Interaction Design: Prototyping animations and micro-interactions", "signals": [{"trait": "arts.design.interaction", "weight": 5.0, "isRejection": False}], "isRejection": False}
        ],
        "displayOrder": 71,
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

print(f'Successfully added {added_count} arts questions.')
