/**
 * AFTER POST-GRADUATION - CAREER DIRECTION QUIZ
 * Purpose: Help postgraduates (Masters/MBA/LLM holders) identify the next best step —
 * senior industry roles, PhD/research, teaching, consulting, or entrepreneurship.
 *
 * Weight dimensions:
 *   phd          → PhD / Academic Research
 *   senior_role  → Senior Industry / Corporate Role
 *   consulting   → Management / Strategy Consulting
 *   teaching     → Teaching / Training / EdTech
 *   entrepreneurship → Startup / Own Venture
 *   policy       → Public Policy / Think Tank / Government Advisory
 *   freelance    → Independent Consultant / Freelancer
 */

export const afterPgQuestions = [
  {
    id: "apg_q1",
    text: "Now that you hold a postgraduate degree, what pulls you forward?",
    options: [
      { text: "The urge to discover something no one has found yet — research excites me.", weights: { phd: 5, teaching: 2 } },
      { text: "I want to apply my knowledge in a top corporate role with real impact.", weights: { senior_role: 5, consulting: 2 } },
      { text: "I want to build something of my own using my advanced expertise.", weights: { entrepreneurship: 5 } },
      { text: "I want to shape the minds of the next generation as a professor or trainer.", weights: { teaching: 5, phd: 2 } }
    ]
  },
  {
    id: "apg_q2",
    text: "Which activity did you enjoy most during your postgraduate programme?",
    options: [
      { text: "Writing the thesis, conducting experiments, and publishing papers.", weights: { phd: 5, teaching: 2 } },
      { text: "Internships, live projects, and industry interactions.", weights: { senior_role: 5, consulting: 3 } },
      { text: "Strategy simulations, consulting case competitions, and presentations.", weights: { consulting: 5, entrepreneurship: 2 } },
      { text: "Policy debates, seminars, and engaging with social or governance issues.", weights: { policy: 5, teaching: 2 } }
    ]
  },
  {
    id: "apg_q3",
    text: "Which of these is your biggest professional strength today?",
    options: [
      { text: "Deep domain expertise that very few people in the country possess.", weights: { phd: 5, senior_role: 2 } },
      { text: "The ability to manage teams, projects, and stakeholders at scale.", weights: { senior_role: 5, consulting: 3 } },
      { text: "Pattern recognition and the ability to solve complex business problems.", weights: { consulting: 5, senior_role: 2 } },
      { text: "Communication, simplifying complexity, and inspiring others.", weights: { teaching: 5, policy: 3 } }
    ]
  },
  {
    id: "apg_q4",
    text: "How do you feel about 3–5 more years of academic life for a PhD?",
    options: [
      { text: "Excited — I want to contribute original knowledge to my field.", weights: { phd: 5 } },
      { text: "Not for me — I'd rather apply knowledge in the real world.", weights: { senior_role: 4, consulting: 3 } },
      { text: "I'd consider it only if it directly helps my business or consultancy.", weights: { entrepreneurship: 3, consulting: 2 } },
      { text: "I'd consider it if it leads to a tenured professorship I'm proud of.", weights: { teaching: 5, phd: 3 } }
    ]
  },
  {
    id: "apg_q5",
    text: "How do you prefer to make decisions?",
    options: [
      { text: "Based on rigorous data analysis, peer-reviewed evidence, and experimentation.", weights: { phd: 5, senior_role: 2 } },
      { text: "Based on business KPIs, ROI projections, and team consensus.", weights: { senior_role: 5, consulting: 3 } },
      { text: "Based on market gaps, gut instinct validated by quick experiments.", weights: { entrepreneurship: 5 } },
      { text: "Based on policy frameworks, ethical principles, and societal impact.", weights: { policy: 5, teaching: 2 } }
    ]
  },
  {
    id: "apg_q6",
    text: "What kind of recognition matters most to you?",
    options: [
      { text: "Being published in top journals and cited by peers worldwide.", weights: { phd: 5 } },
      { text: "A Director / VP title and a high-compensation package at a top firm.", weights: { senior_role: 5 } },
      { text: "Clients paying premium fees for your unique strategic insight.", weights: { consulting: 5, freelance: 3 } },
      { text: "Students saying you changed their career and life trajectory.", weights: { teaching: 5 } }
    ]
  },
  {
    id: "apg_q7",
    text: "Which of these professional identities resonates most with you?",
    options: [
      { text: "Principal Researcher / Associate Professor / Scientist.", weights: { phd: 5, teaching: 2 } },
      { text: "Senior Manager / Director / VP at a Fortune 500 company.", weights: { senior_role: 5 } },
      { text: "Founder / Co-founder of a high-growth startup.", weights: { entrepreneurship: 5 } },
      { text: "Independent Consultant / Subject Matter Expert / Freelancer.", weights: { freelance: 5, consulting: 3 } }
    ]
  },
  {
    id: "apg_q8",
    text: "How important is work-life predictability to you at this stage?",
    options: [
      { text: "I want flexibility — research or academia gives me schedule control.", weights: { phd: 4, teaching: 4 } },
      { text: "I'm okay with demanding hours if it leads to career advancement fast.", weights: { senior_role: 5, consulting: 4 } },
      { text: "Completely unpredictable — I want to build something and I accept that chaos.", weights: { entrepreneurship: 5 } },
      { text: "I want complete control — I'll set my own hours as a consultant or freelancer.", weights: { freelance: 5, consulting: 2 } }
    ]
  },
  {
    id: "apg_q9",
    text: "Which sector would you most like to influence through your career?",
    options: [
      { text: "Science, technology, or academia — knowledge creation.", weights: { phd: 5, teaching: 2 } },
      { text: "Business, finance, or industry — economic value creation.", weights: { senior_role: 5, consulting: 3 } },
      { text: "Governance, public policy, or social development.", weights: { policy: 5, teaching: 2 } },
      { text: "Innovation — I want to create a new sector or disrupt an existing one.", weights: { entrepreneurship: 5 } }
    ]
  },
  {
    id: "apg_q10",
    text: "How do you feel about networking, client pitching, and business development?",
    options: [
      { text: "I prefer to let my research output speak for itself.", weights: { phd: 5 } },
      { text: "It's part of the job — I can do it but it's not my primary strength.", weights: { senior_role: 3, teaching: 3 } },
      { text: "I love it — pitching ideas and winning clients energizes me.", weights: { consulting: 5, entrepreneurship: 4, freelance: 4 } },
      { text: "I'd rather influence through policy papers and public speaking.", weights: { policy: 5, teaching: 3 } }
    ]
  }
];
