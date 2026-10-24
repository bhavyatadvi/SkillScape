/**
 * AFTER GRADUATION - CAREER DIRECTION QUIZ
 * Purpose: Help graduates find the most suitable career path / postgraduate direction
 * based on their interests, work-style preferences, and long-term goals.
 *
 * Weight dimensions map to career clusters:
 *   mba          → MBA / Business Management
 *   mtech        → M.Tech / MS (Technical Postgrad)
 *   research     → Pure Research / PhD / Academia
 *   law          → LLB / Legal career
 *   upsc         → UPSC / Government / Civil Services
 *   finance      → CFA / CA / Finance specialisation
 *   healthcare   → Clinical / Medical / Healthcare roles
 *   creative     → Design / Media / Arts / Content
 *   entrepreneurship → Startup / Self-employment
 *   social       → NGO / Social Work / Development sector
 */

export const afterGraduationQuestions = [
  {
    id: "ag_q1",
    text: "Now that you have your degree, what feels missing the most?",
    options: [
      { text: "I need a broader view — management, strategy, and leadership.", weights: { mba: 5, entrepreneurship: 2 } },
      { text: "I want to go deeper into my technical field through advanced study.", weights: { mtech: 5, research: 3 } },
      { text: "I want to serve my country through a government role.", weights: { upsc: 5, law: 1 } },
      { text: "I want to start building something of my own right now.", weights: { entrepreneurship: 5, mba: 2 } }
    ]
  },
  {
    id: "ag_q2",
    text: "What kind of problem gets you most energized?",
    options: [
      { text: "Organizational inefficiency — teams that underperform or strategies that fail.", weights: { mba: 5, entrepreneurship: 2 } },
      { text: "Technical puzzles — unsolved engineering or computing challenges.", weights: { mtech: 5, research: 3 } },
      { text: "Social injustice — corruption, inequality, or policy failures.", weights: { upsc: 4, law: 4, social: 3 } },
      { text: "Financial complexity — markets, risk, valuations, or tax structures.", weights: { finance: 5, mba: 2 } }
    ]
  },
  {
    id: "ag_q3",
    text: "If you could shadow a professional for one month, who would it be?",
    options: [
      { text: "A CEO or Strategy Consultant at a top firm.", weights: { mba: 5, entrepreneurship: 2 } },
      { text: "A Senior Engineer at ISRO, Google DeepMind, or a top R&D lab.", weights: { mtech: 5, research: 4 } },
      { text: "An IAS / IPS officer managing a district.", weights: { upsc: 5 } },
      { text: "A Supreme Court advocate or High Court judge.", weights: { law: 5 } }
    ]
  },
  {
    id: "ag_q4",
    text: "How do you feel about spending 2–3 more years in studying?",
    options: [
      { text: "Fine — but I want an MBA that has direct ROI in salary and promotions.", weights: { mba: 5, finance: 2 } },
      { text: "I love studying — I want a M.Tech or MS that sharpens my technical edge.", weights: { mtech: 5, research: 3 } },
      { text: "I'm okay with years of preparation if it leads to a prestigious government role.", weights: { upsc: 5 } },
      { text: "I'd rather work and learn on the job — I want to start earning now.", weights: { entrepreneurship: 4, social: 2 } }
    ]
  },
  {
    id: "ag_q5",
    text: "Where do you see yourself working in 5 years?",
    options: [
      { text: "As a manager or team lead in a multinational company or consulting firm.", weights: { mba: 5 } },
      { text: "As a specialist engineer, data scientist, or researcher at a top tech company.", weights: { mtech: 5 } },
      { text: "As a government officer (IAS, IPS, IFS) or in public sector leadership.", weights: { upsc: 5 } },
      { text: "Running my own startup or freelancing independently.", weights: { entrepreneurship: 5 } }
    ]
  },
  {
    id: "ag_q6",
    text: "Which of these tasks do you enjoy most during your graduation?",
    options: [
      { text: "Group projects, presentations, leading teams, and strategy discussions.", weights: { mba: 5, entrepreneurship: 2 } },
      { text: "Coding assignments, lab experiments, and thesis writing.", weights: { mtech: 5, research: 4 } },
      { text: "Reading case studies, writing essays on policy or law.", weights: { law: 4, upsc: 3, social: 2 } },
      { text: "Financial modelling, case competitions, and market analysis.", weights: { finance: 5, mba: 2 } }
    ]
  },
  {
    id: "ag_q7",
    text: "Which magazine or news section do you enjoy reading most?",
    options: [
      { text: "Business Today, Forbes, Harvard Business Review.", weights: { mba: 5, finance: 2, entrepreneurship: 2 } },
      { text: "IEEE Spectrum, Wired, Science Daily, Nature.", weights: { mtech: 5, research: 5 } },
      { text: "The Hindu's opinion, Frontline, or policy briefs.", weights: { upsc: 5, law: 2, social: 2 } },
      { text: "Economic Times, Mint, or CFA Institute publications.", weights: { finance: 5, mba: 2 } }
    ]
  },
  {
    id: "ag_q8",
    text: "What is your biggest strength that you want a career to reward?",
    options: [
      { text: "People management, negotiation, and strategic thinking.", weights: { mba: 5, entrepreneurship: 3 } },
      { text: "Deep domain expertise, precision, and analytical problem-solving.", weights: { mtech: 5, research: 4, finance: 3 } },
      { text: "Communication, advocacy, and the ability to influence others.", weights: { law: 5, upsc: 3, creative: 3 } },
      { text: "Empathy, community orientation, and grassroots impact.", weights: { social: 5, upsc: 2, healthcare: 2 } }
    ]
  },
  {
    id: "ag_q9",
    text: "How important is financial earning to you right now?",
    options: [
      { text: "Very important — I want a top salary package within 2–3 years.", weights: { mba: 5, finance: 4 } },
      { text: "Important — I want a good salary but am willing to invest 3 years in M.Tech / MS.", weights: { mtech: 4, research: 2 } },
      { text: "Moderate — government jobs offer stability which matters more than salary.", weights: { upsc: 5 } },
      { text: "I'm willing to earn less now if I'm building something meaningful.", weights: { entrepreneurship: 4, social: 3, creative: 3 } }
    ]
  },
  {
    id: "ag_q10",
    text: "If you were to write a paper or report, what topic would you choose?",
    options: [
      { text: "Market entry strategies for a D2C startup in Tier-2 Indian cities.", weights: { mba: 5, entrepreneurship: 3 } },
      { text: "Optimizing deep learning models for edge computing devices.", weights: { mtech: 5, research: 5 } },
      { text: "Reforming India's judicial backlog and access to legal aid.", weights: { law: 5, upsc: 3 } },
      { text: "How microfinance can empower women in rural Rajasthan.", weights: { social: 5, finance: 2 } }
    ]
  },
  {
    id: "ag_q11",
    text: "What kind of feedback energizes you most?",
    options: [
      { text: "\"Your strategy was brilliant and your team executed perfectly.\"", weights: { mba: 5, entrepreneurship: 2 } },
      { text: "\"Your algorithm is 40% more efficient than anything published before.\"", weights: { mtech: 5, research: 5 } },
      { text: "\"Your argument won the case / your policy report was cited.\"", weights: { law: 5, upsc: 3 } },
      { text: "\"Your work changed the lives of 500 families in that village.\"", weights: { social: 5, upsc: 2 } }
    ]
  },
  {
    id: "ag_q12",
    text: "Which statement best describes your relationship with risk?",
    options: [
      { text: "I take calculated risks with strong data and planning to back me up.", weights: { mba: 4, finance: 4, entrepreneurship: 3 } },
      { text: "I prefer methodical research before taking any steps.", weights: { research: 5, mtech: 3 } },
      { text: "I prefer the structured, predictable path of government or law.", weights: { upsc: 5, law: 4 } },
      { text: "I thrive on uncertainty — building from scratch is exciting to me.", weights: { entrepreneurship: 5, creative: 3 } }
    ]
  },
  {
    id: "ag_q13",
    text: "How do you feel about relocating abroad for work or study?",
    options: [
      { text: "Yes — I want an MBA from a global B-school and an international career.", weights: { mba: 5 } },
      { text: "Yes — I want an MS from MIT, Stanford, or a top European university.", weights: { mtech: 5, research: 4 } },
      { text: "No — I want to serve India; relocation abroad doesn't appeal to me.", weights: { upsc: 5, law: 2, social: 2 } },
      { text: "Open to it — but only if my business or creative work demands it.", weights: { entrepreneurship: 3, creative: 3 } }
    ]
  },
  {
    id: "ag_q14",
    text: "Which certification or exam would you most seriously consider?",
    options: [
      { text: "CAT / GMAT for MBA admission.", weights: { mba: 5 } },
      { text: "GATE for M.Tech / PSU jobs.", weights: { mtech: 5 } },
      { text: "UPSC Civil Services / State PSC.", weights: { upsc: 5 } },
      { text: "CFA / FRM / CA Final for finance.", weights: { finance: 5 } }
    ]
  },
  {
    id: "ag_q15",
    text: "If all careers paid equally well, which would you genuinely pursue?",
    options: [
      { text: "Building and leading organizations — a CEO or management consultant.", weights: { mba: 5, entrepreneurship: 3 } },
      { text: "Inventing the next breakthrough in science or technology.", weights: { mtech: 5, research: 5 } },
      { text: "Serving in IAS/IPS or practicing constitutional law.", weights: { upsc: 5, law: 4 } },
      { text: "Creating art, media, or social change that impacts millions.", weights: { creative: 5, social: 4 } }
    ]
  }
];
