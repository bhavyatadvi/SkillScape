/**
 * UPSKILLING QUIZ
 * Purpose: Help employed or job-seeking individuals identify which skill or
 * certification to invest in next for career growth.
 *
 * Weight dimensions:
 *   data_tech    → Data Science / ML / AI / Cloud
 *   software     → Software Development / DevOps / Cybersecurity
 *   business     → Business Analysis / Product Management / Project Management
 *   finance_cert → CFA / FRM / Financial Certifications
 *   marketing    → Digital Marketing / SEO / Growth Hacking
 *   design       → UI/UX Design / Graphic Design / Motion
 *   communication → Public Speaking / Writing / Leadership
 *   hr_people    → HR Tech / L&D / Coaching
 */

export const upskillingQuestions = [
  {
    id: "us_q1",
    text: "What is the primary goal of your upskilling effort?",
    options: [
      { text: "Get promoted or move into a more senior technical role.", weights: { data_tech: 4, software: 4, business: 3 } },
      { text: "Switch to a completely new, higher-paying career domain.", weights: { data_tech: 5, software: 4, finance_cert: 3 } },
      { text: "Start freelancing or build a side income stream.", weights: { marketing: 5, design: 4, communication: 3 } },
      { text: "Improve my effectiveness in my current role.", weights: { business: 4, hr_people: 4, communication: 4 } }
    ]
  },
  {
    id: "us_q2",
    text: "What type of learning do you enjoy most?",
    options: [
      { text: "Building projects — writing code, building models, deploying apps.", weights: { software: 5, data_tech: 5 } },
      { text: "Case studies, frameworks, and real-world business scenarios.", weights: { business: 5, finance_cert: 3 } },
      { text: "Visual learning — creating designs, videos, or presentations.", weights: { design: 5, marketing: 3 } },
      { text: "Reading, writing, and verbal communication practice.", weights: { communication: 5, hr_people: 3 } }
    ]
  },
  {
    id: "us_q3",
    text: "Which certification or course would excite you most to complete?",
    options: [
      { text: "Google Data Analytics Certificate or AWS Cloud Practitioner.", weights: { data_tech: 5, software: 3 } },
      { text: "PMP (Project Management), Agile/Scrum, or Google Project Management.", weights: { business: 5 } },
      { text: "CFA Level 1, FRM, or Bloomberg Market Concepts.", weights: { finance_cert: 5 } },
      { text: "Google Digital Marketing & E-commerce or HubSpot Marketing.", weights: { marketing: 5, design: 2 } }
    ]
  },
  {
    id: "us_q4",
    text: "How many hours per week can you realistically commit to upskilling?",
    options: [
      { text: "10+ hours — I'm treating this like a second job.", weights: { software: 4, data_tech: 4, finance_cert: 4 } },
      { text: "5–10 hours — I can do structured courses on weekends.", weights: { business: 4, marketing: 4, design: 4 } },
      { text: "2–5 hours — bite-sized, practical micro-learning.", weights: { communication: 4, hr_people: 4, marketing: 3 } },
      { text: "Less than 2 hours — I need on-the-job or podcast-based learning.", weights: { communication: 5, hr_people: 3 } }
    ]
  },
  {
    id: "us_q5",
    text: "Which skill gap do you feel most urgently in your current role?",
    options: [
      { text: "I don't understand data well enough — Python, SQL, dashboards.", weights: { data_tech: 5 } },
      { text: "I struggle to manage projects, timelines, or stakeholder communication.", weights: { business: 5, communication: 3 } },
      { text: "I need to understand finance and ROI better for decision making.", weights: { finance_cert: 5 } },
      { text: "My personal brand and digital presence are non-existent.", weights: { marketing: 4, communication: 4, design: 3 } }
    ]
  },
  {
    id: "us_q6",
    text: "Which professional would you most want to be in 2 years?",
    options: [
      { text: "A Data Scientist or ML Engineer at a product-led company.", weights: { data_tech: 5 } },
      { text: "A Product Manager or Scrum Master driving agile teams.", weights: { business: 5 } },
      { text: "A UI/UX Designer whose portfolio lands top-tier clients.", weights: { design: 5 } },
      { text: "A confident public speaker, coach, or L&D specialist.", weights: { communication: 5, hr_people: 4 } }
    ]
  },
  {
    id: "us_q7",
    text: "What is your budget for skill development (per year)?",
    options: [
      { text: "₹50,000+ — I'll invest in a quality bootcamp or certification.", weights: { software: 4, data_tech: 4, finance_cert: 4 } },
      { text: "₹10,000–50,000 — I'll buy structured online courses.", weights: { business: 4, design: 4, marketing: 4 } },
      { text: "Under ₹10,000 — mostly free resources and YouTube.", weights: { communication: 4, marketing: 3, hr_people: 3 } },
      { text: "Zero — I learn entirely on the job through projects.", weights: { software: 3, business: 2 } }
    ]
  },
  {
    id: "us_q8",
    text: "What motivates you most about learning a new skill?",
    options: [
      { text: "Solving a technical problem I couldn't solve before.", weights: { software: 5, data_tech: 5 } },
      { text: "Getting tangible results — more leads, more revenue, more followers.", weights: { marketing: 5, business: 3 } },
      { text: "Helping others learn or grow as a result of my new skill.", weights: { hr_people: 5, communication: 4 } },
      { text: "Creating something visually or creatively impressive.", weights: { design: 5, marketing: 2 } }
    ]
  }
];
