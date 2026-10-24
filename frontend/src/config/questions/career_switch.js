/**
 * CAREER SWITCH QUIZ
 * Purpose: Help working professionals identify which new career domain they should
 * transition into based on their transferable skills, values, and aspirations.
 *
 * Weight dimensions:
 *   tech         → IT / Data Science / Product Management
 *   finance      → Banking / Finance / Investment
 *   consulting   → Strategy / Management Consulting
 *   marketing    → Marketing / Brand / Digital
 *   hr           → HR / L&D / People & Culture
 *   operations   → Operations / Supply Chain / Logistics
 *   creative     → Design / Content / Creative Industries
 *   entrepreneurship → Own Business / Freelancing
 *   healthcare   → Healthcare / Wellness / MedTech
 *   social       → NGO / CSR / Social Impact
 */

export const careerSwitchQuestions = [
  {
    id: "cs_q1",
    text: "What is the main reason you want to switch careers?",
    options: [
      { text: "My current work feels repetitive — I want intellectually stimulating challenges.", weights: { tech: 4, consulting: 4, finance: 3 } },
      { text: "I want higher financial rewards and faster growth.", weights: { finance: 5, tech: 4, consulting: 3 } },
      { text: "I want to do work that feels meaningful and socially impactful.", weights: { social: 5, healthcare: 4 } },
      { text: "I want creative freedom and autonomy over my own work.", weights: { creative: 5, entrepreneurship: 5 } }
    ]
  },
  {
    id: "cs_q2",
    text: "Which skill from your current job do you most want to carry into your new career?",
    options: [
      { text: "Data analysis, technical problem solving, and logical thinking.", weights: { tech: 5, finance: 3, consulting: 3 } },
      { text: "Communication, persuasion, and stakeholder management.", weights: { marketing: 5, consulting: 4, hr: 3 } },
      { text: "Process management, team coordination, and execution.", weights: { operations: 5, hr: 3, consulting: 2 } },
      { text: "Creativity, storytelling, and design thinking.", weights: { creative: 5, marketing: 3 } }
    ]
  },
  {
    id: "cs_q3",
    text: "Which of these job descriptions excites you most?",
    options: [
      { text: "\"Analyze large datasets to drive product decisions at a tech startup.\"", weights: { tech: 5, consulting: 2 } },
      { text: "\"Build and manage brand campaigns that reach 10 million users.\"", weights: { marketing: 5, creative: 3 } },
      { text: "\"Advise Fortune 500 companies on their 5-year growth strategy.\"", weights: { consulting: 5, finance: 3 } },
      { text: "\"Build a financial model to evaluate an acquisition worth ₹200 crore.\"", weights: { finance: 5, consulting: 3 } }
    ]
  },
  {
    id: "cs_q4",
    text: "How comfortable are you with learning new technical skills from scratch?",
    options: [
      { text: "Very comfortable — I enjoy coding, tools, and technical learning.", weights: { tech: 5 } },
      { text: "Comfortable — I can learn financial tools and market analysis.", weights: { finance: 4, consulting: 3 } },
      { text: "Moderate — I prefer applying skills, not learning deeply technical ones.", weights: { marketing: 4, hr: 4, operations: 3 } },
      { text: "I prefer creative or human-centric skills — tools are secondary.", weights: { creative: 4, hr: 4, social: 3 } }
    ]
  },
  {
    id: "cs_q5",
    text: "Which of these work styles suits you best?",
    options: [
      { text: "Independent deep work — solving complex problems alone.", weights: { tech: 5, finance: 3 } },
      { text: "Collaborative — constantly working with people, teams, and clients.", weights: { consulting: 5, marketing: 4, hr: 4 } },
      { text: "Entrepreneurial — building things, taking risks, experimenting fast.", weights: { entrepreneurship: 5, creative: 3 } },
      { text: "Community-driven — working with beneficiaries, communities, or patients.", weights: { social: 5, healthcare: 5 } }
    ]
  },
  {
    id: "cs_q6",
    text: "What is your biggest fear about switching careers?",
    options: [
      { text: "Losing the technical depth I've built over years.", weights: { tech: 3, consulting: 2 } },
      { text: "Taking a salary cut during the transition period.", weights: { finance: 4, marketing: 3 } },
      { text: "Starting over and being seen as a junior again.", weights: { hr: 3, operations: 3, social: 2 } },
      { text: "Losing creative freedom in a structured corporate environment.", weights: { creative: 4, entrepreneurship: 4 } }
    ]
  },
  {
    id: "cs_q7",
    text: "In 5 years, what does your ideal work life look like?",
    options: [
      { text: "Working on cutting-edge AI, product, or cloud projects.", weights: { tech: 5 } },
      { text: "Running my own consultancy with high-value corporate clients.", weights: { consulting: 5, entrepreneurship: 3 } },
      { text: "Leading a marketing or brand team at a company I admire.", weights: { marketing: 5, creative: 2 } },
      { text: "Running my own business or working as a full-time freelancer.", weights: { entrepreneurship: 5, creative: 3 } }
    ]
  },
  {
    id: "cs_q8",
    text: "Which of these fields fascinates you most even though you haven't worked in it yet?",
    options: [
      { text: "Artificial Intelligence, Cybersecurity, or Cloud Computing.", weights: { tech: 5 } },
      { text: "Investment Banking, Private Equity, or Fintech.", weights: { finance: 5 } },
      { text: "UX/UI Design, Motion Graphics, or Content Creation.", weights: { creative: 5, marketing: 3 } },
      { text: "Healthcare delivery, MedTech innovation, or mental health services.", weights: { healthcare: 5 } }
    ]
  },
  {
    id: "cs_q9",
    text: "How willing are you to take a short-term pay cut for a better long-term outcome?",
    options: [
      { text: "Fully willing — I'll upskill and take an entry-level role in tech.", weights: { tech: 5 } },
      { text: "Willing — but I need to see a clear ROI within 12 months.", weights: { consulting: 4, finance: 4 } },
      { text: "Minimally — I need to freelance or consult while transitioning.", weights: { entrepreneurship: 4, creative: 3, marketing: 3 } },
      { text: "Not willing — I want a lateral move that keeps my current salary.", weights: { operations: 4, hr: 4 } }
    ]
  },
  {
    id: "cs_q10",
    text: "Which describes how you make your best contribution at work?",
    options: [
      { text: "By building systems, automating processes, and writing code.", weights: { tech: 5 } },
      { text: "By persuading stakeholders and crafting compelling narratives.", weights: { marketing: 5, consulting: 4 } },
      { text: "By developing people, resolving conflicts, and building culture.", weights: { hr: 5, social: 3 } },
      { text: "By creating visually powerful experiences and original content.", weights: { creative: 5, marketing: 3 } }
    ]
  }
];
