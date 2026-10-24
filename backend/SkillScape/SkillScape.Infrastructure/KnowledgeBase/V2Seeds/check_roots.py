import json

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_AfterPostGraduation.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

for q in data:
    if q.get('targetDiscriminatesJson') == '[]' and q.get('domain') != 'Generic':
        print(f"ID: {q['id']} | Domain: {q['domain']} | Text: {q['text'][:50]}")
