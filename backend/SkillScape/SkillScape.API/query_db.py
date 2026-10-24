import sqlite3
import json

conn = sqlite3.connect('skillscape.db')
cur = conn.cursor()
cur.execute("SELECT Id, Domain, Subdomain, TargetDiscriminatesJson FROM QuizQuestionsV2 WHERE PhaseKey = 'AfterGraduation'")
rows = cur.fetchall()

print(f"Total AfterGraduation Questions: {len(rows)}")
for r in rows:
    print(f"{r[0]} | Domain: {r[1]} | Subdomain: {r[2]} | Discriminates: {r[3]}")
