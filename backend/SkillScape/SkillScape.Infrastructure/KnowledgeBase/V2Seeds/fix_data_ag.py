import json

file_path = r'c:\Users\VIDHI TRIVEDI\Desktop\SkillScape-final\SkillNavigator\backend\SkillScape\SkillScape.Infrastructure\KnowledgeBase\V2Seeds\QuestionBankSeed_AfterGraduation.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

original_len = len(data)

# Filter out duplicate scenario variations
cleaned_data = [q for q in data if '(Scenario' not in q['text']]

# Fix empty discriminates for non-generic
for q in cleaned_data:
    if q.get('targetDiscriminatesJson') == '[]' and q.get('domain') != 'Generic':
        domain_str = q.get('domain', '').lower().replace(' ', '_')
        if domain_str:
            q['targetDiscriminatesJson'] = json.dumps([f"domain.{domain_str}"])

with open(file_path, 'w', encoding='utf-8') as f:
    json.dump(cleaned_data, f, indent=2)

print(f"Removed {original_len - len(cleaned_data)} garbage duplicates from AfterGraduation. Fixed target discriminates.")
