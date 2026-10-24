import json

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_After12_Commerce.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)
print(f"Total questions: {len(data)}")
for i, q in enumerate(data[:3]):
    print(f"Q{i}: {q['id']} | {q['text'][:60]}")
