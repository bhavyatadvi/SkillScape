/**
 * AFTER 12th - STREAM SELECTOR QUIZ
 * Purpose: Help students who haven't chosen a stream yet decide between
 * Science (PCM), Science (PCB), Commerce, Arts & Humanities, ITI, or Diploma.
 * 
 * Weight dimensions:
 *   pcm       → Physics-Chemistry-Math (Engineering, CS, Architecture)
 *   pcb       → Physics-Chemistry-Biology (Medicine, Pharmacy, Biotech)
 *   commerce  → Commerce (CA, BBA, Economics, Finance)
 *   arts      → Arts & Humanities (Law, Psychology, Journalism, Civil Services)
 *   iti       → ITI / Polytechnic Diploma (Hands-on technical trades)
 */

export const after12Questions = [
  {
    id: "a12_q1",
    text: "Which school subject do you look forward to studying the most?",
    options: [
      { text: "Mathematics and Physics — equations, forces, and logical proofs.", weights: { pcm: 5, pcb: 1 } },
      { text: "Biology and Chemistry — living cells, reactions, and the human body.", weights: { pcb: 5, pcm: 1 } },
      { text: "Accountancy and Business Studies — balance sheets and market strategies.", weights: { commerce: 5 } },
      { text: "History, Political Science, or Literature — ideas, societies, and stories.", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q2",
    text: "If you had to pick a weekend hobby right now, which would it be?",
    options: [
      { text: "Building a small circuit or coding a mini project.", weights: { pcm: 5, iti: 3 } },
      { text: "Watching documentaries on the human body, diseases, or nature.", weights: { pcb: 5 } },
      { text: "Studying stock markets or planning a small business idea.", weights: { commerce: 5 } },
      { text: "Writing, debating, painting, or reading books on world events.", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q3",
    text: "Imagine your dream college. What kind of campus do you picture?",
    options: [
      { text: "An IIT or NIT with labs full of machines and computers.", weights: { pcm: 5 } },
      { text: "A medical college with a hospital attached, or a biotech research centre.", weights: { pcb: 5 } },
      { text: "A business school (IIM/BBA) with trading rooms and corporate ties.", weights: { commerce: 5 } },
      { text: "A university with law, journalism, or social science departments.", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q4",
    text: "Which type of problem excites you most?",
    options: [
      { text: "How do we design a bridge that can bear 10 times its own weight?", weights: { pcm: 5 } },
      { text: "How does a virus mutate and what drug can stop it?", weights: { pcb: 5 } },
      { text: "How can a company cut costs by 30% while growing revenue?", weights: { commerce: 5 } },
      { text: "Why do societies repeat the same historical mistakes, and how can we stop that?", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q5",
    text: "What does your ideal job environment look like 10 years from now?",
    options: [
      { text: "An engineering firm, IT company, research lab, or space agency.", weights: { pcm: 5 } },
      { text: "A hospital, clinic, pharmaceutical lab, or biotech startup.", weights: { pcb: 5 } },
      { text: "A bank, CA firm, corporate office, or my own business.", weights: { commerce: 5 } },
      { text: "A court, newsroom, NGO, government ministry, or university.", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q6",
    text: "How do you feel about working with your hands and physical tools?",
    options: [
      { text: "I love it — I want to fix, build, or operate machinery and equipment.", weights: { iti: 5, pcm: 1 } },
      { text: "I prefer lab tools and medical instruments — precision instruments only.", weights: { pcb: 3 } },
      { text: "I prefer using a computer, spreadsheets, or business tools.", weights: { commerce: 3 } },
      { text: "I prefer books, discussions, and creative or social activities.", weights: { arts: 3 } }
    ]
  },
  {
    id: "a12_q7",
    text: "Which entrance exam would you rather prepare for?",
    options: [
      { text: "JEE Main / JEE Advanced (Engineering).", weights: { pcm: 5 } },
      { text: "NEET (Medical / Dental / Pharmacy).", weights: { pcb: 5 } },
      { text: "CAT / IPMAT / CA Foundation (Business / Accounts).", weights: { commerce: 5 } },
      { text: "CLAT / DU JAT / CUET for Law, History, Journalism.", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q8",
    text: "If you got a ₹10,000 scholarship to learn any skill, what would you choose?",
    options: [
      { text: "Advanced Programming, Robotics, or CAD Design.", weights: { pcm: 5 } },
      { text: "First Aid, Medical Terminology, or Biotechnology basics.", weights: { pcb: 5 } },
      { text: "Stock Trading, Tally ERP, or Digital Marketing.", weights: { commerce: 5 } },
      { text: "Public Speaking, Creative Writing, or a Foreign Language.", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q9",
    text: "How do you feel about the idea of getting a job right after a 2-year trade course?",
    options: [
      { text: "That sounds perfect — I want job-ready skills quickly (electrician, mechanic, welder, etc.).", weights: { iti: 5 } },
      { text: "I'd prefer a 3-year diploma in Engineering before working.", weights: { iti: 4, pcm: 2 } },
      { text: "No — I need a 3–5 year degree (B.Tech / MBBS / BBA / BA) first.", weights: { pcm: 2, pcb: 2, commerce: 2, arts: 2 } },
      { text: "I want a full 5-year professional degree (CA / MBBS / LLB).", weights: { commerce: 3, pcb: 3, arts: 3 } }
    ]
  },
  {
    id: "a12_q10",
    text: "Which real-world challenge would you most want to solve in your career?",
    options: [
      { text: "Clean energy, faster internet, smarter cities — technology challenges.", weights: { pcm: 5 } },
      { text: "Curing cancer, fighting pandemics, or improving mental health access.", weights: { pcb: 5 } },
      { text: "Reducing poverty, growing businesses, or fixing financial systems.", weights: { commerce: 5 } },
      { text: "Upholding justice, protecting free speech, or preserving cultural heritage.", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q11",
    text: "How comfortable are you with heavy theory and abstract concepts in textbooks?",
    options: [
      { text: "Very comfortable — I enjoy Calculus, Thermodynamics, and complex equations.", weights: { pcm: 5 } },
      { text: "Comfortable — I enjoy detailed Biology, Organic Chemistry, and Physiology.", weights: { pcb: 5 } },
      { text: "Moderate — I prefer practical concepts like profit, tax, and business law.", weights: { commerce: 4 } },
      { text: "I prefer essays, case studies, and open-ended conceptual thinking.", weights: { arts: 4 } }
    ]
  },
  {
    id: "a12_q12",
    text: "Which of these famous professionals inspires you the most?",
    options: [
      { text: "Elon Musk (Tesla, SpaceX) or Sundar Pichai (Google).", weights: { pcm: 5 } },
      { text: "Dr. APJ Abdul Kalam (Scientist) or Dr. Devi Shetty (Cardiac Surgeon).", weights: { pcb: 5 } },
      { text: "Rakesh Jhunjhunwala (Investor) or Narayana Murthy (Infosys Founder).", weights: { commerce: 5 } },
      { text: "Arundhati Roy (Author) or Dr. B.R. Ambedkar (Jurist, Social Reformer).", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q13",
    text: "How do you feel when asked to memorize long lists of facts for an exam?",
    options: [
      { text: "I dislike it — I much prefer solving equations and applying formulas.", weights: { pcm: 5 } },
      { text: "I don't mind — Biology and Anatomy require a lot of it, and I'm fine with that.", weights: { pcb: 5 } },
      { text: "I'm okay — Commerce facts, GST rates, and accounting rules are manageable.", weights: { commerce: 4 } },
      { text: "I love it — history dates, court cases, literary themes — I enjoy all of it.", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q14",
    text: "What is your biggest concern about choosing the wrong stream?",
    options: [
      { text: "Missing out on a tech/engineering career — that would devastate me.", weights: { pcm: 5 } },
      { text: "Not getting into medicine — healing people is my life's purpose.", weights: { pcb: 5 } },
      { text: "Missing a business career — money and entrepreneurship matter to me.", weights: { commerce: 5 } },
      { text: "Being stuck in a career that doesn't contribute to society or culture.", weights: { arts: 5 } }
    ]
  },
  {
    id: "a12_q15",
    text: "If money and parental expectations were not factors, which path would you pick?",
    options: [
      { text: "Study engineering or computer science and build something that changes the world.", weights: { pcm: 5 } },
      { text: "Become a doctor, pharmacist, or biomedical researcher.", weights: { pcb: 5 } },
      { text: "Run my own business or become a financial expert.", weights: { commerce: 5 } },
      { text: "Become a lawyer, civil servant, journalist, or artist.", weights: { arts: 5 } }
    ]
  }
];
