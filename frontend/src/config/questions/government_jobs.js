/**
 * GOVERNMENT JOBS QUIZ
 * Purpose: Help aspirants identify which government exam / sector suits them best
 * based on interest, aptitude, and lifestyle preferences.
 *
 * Weight dimensions:
 *   ias_ips      → UPSC Civil Services (IAS / IPS / IFS)
 *   banking      → Bank PO / Clerk / RBI / NABARD
 *   ssc          → SSC CGL / CHSL / MTS
 *   defence      → NDA / CDS / Air Force / Navy / Indian Army
 *   railways     → RRB / Indian Railways
 *   psu          → PSU / GATE-based jobs (ONGC, BHEL, NTPC)
 *   teaching     → TET / CTET / UGC-NET / KVS / NVS
 *   police       → State Police / CRPF / BSF / CISF
 */

export const governmentJobsQuestions = [
  {
    id: "gj_q1",
    text: "What is your primary motivation for pursuing a government job?",
    options: [
      { text: "Power, prestige, and making policy-level impact for India.", weights: { ias_ips: 5 } },
      { text: "Job security, a stable salary, and a comfortable retirement.", weights: { banking: 4, ssc: 4, railways: 4 } },
      { text: "Serving the nation in a disciplined, action-oriented environment.", weights: { defence: 5, police: 4 } },
      { text: "Passing on knowledge — I want to be a respected teacher or professor.", weights: { teaching: 5 } }
    ]
  },
  {
    id: "gj_q2",
    text: "Which subject comes most naturally to you?",
    options: [
      { text: "General Studies — History, Polity, Geography, Current Affairs.", weights: { ias_ips: 5, ssc: 3 } },
      { text: "Quantitative Aptitude, Reasoning, and English.", weights: { banking: 5, ssc: 4, railways: 3 } },
      { text: "Physics, Mathematics, or Engineering subjects.", weights: { psu: 5, defence: 3 } },
      { text: "Education, Child Psychology, or a specific subject like Hindi/Science/Math.", weights: { teaching: 5 } }
    ]
  },
  {
    id: "gj_q3",
    text: "How do you feel about physical fitness tests as part of selection?",
    options: [
      { text: "I love it — I'm physically fit and enjoy sports and outdoor activity.", weights: { defence: 5, police: 5 } },
      { text: "I'm okay with basic fitness tests but prefer an exam-only process.", weights: { ias_ips: 3, banking: 3, ssc: 3 } },
      { text: "I'd rather skip physical tests — I excel at written exams.", weights: { banking: 4, ssc: 4, teaching: 4 } },
      { text: "I prefer technical assessments — GATE score or engineering test.", weights: { psu: 5, railways: 3 } }
    ]
  },
  {
    id: "gj_q4",
    text: "How willing are you to be transferred to any state or remote location in India?",
    options: [
      { text: "Fully willing — I want to serve wherever the nation needs me.", weights: { ias_ips: 5, defence: 5 } },
      { text: "Somewhat willing — but I'd prefer postings near home eventually.", weights: { police: 4, ssc: 3 } },
      { text: "I prefer to stay in one region or city — stability is important.", weights: { banking: 5, teaching: 4, railways: 3 } },
      { text: "I prefer fixed-location technical roles — plant or unit-based jobs.", weights: { psu: 5 } }
    ]
  },
  {
    id: "gj_q5",
    text: "How many years are you willing to dedicate to exam preparation?",
    options: [
      { text: "4–6 years — I understand UPSC is a marathon, not a sprint.", weights: { ias_ips: 5 } },
      { text: "2–3 years — I want to appear within 2 attempts maximum.", weights: { banking: 4, ssc: 4 } },
      { text: "1–2 years — I want to qualify fast and start working.", weights: { railways: 4, police: 4, ssc: 3 } },
      { text: "I'm already preparing — I just need to crack GATE and apply to PSUs.", weights: { psu: 5 } }
    ]
  },
  {
    id: "gj_q6",
    text: "Which of these roles sounds most impressive to you?",
    options: [
      { text: "District Collector (IAS) — governing an entire district.", weights: { ias_ips: 5 } },
      { text: "Probationary Officer at SBI or Reserve Bank of India officer.", weights: { banking: 5 } },
      { text: "Captain in the Indian Army or Wing Commander in the Air Force.", weights: { defence: 5 } },
      { text: "Engineer at ONGC, NTPC, BHEL, or ISRO.", weights: { psu: 5 } }
    ]
  },
  {
    id: "gj_q7",
    text: "How do you feel about essay writing and GD/interview rounds?",
    options: [
      { text: "I love it — I can write and speak fluently on governance and policy.", weights: { ias_ips: 5 } },
      { text: "I'm okay — banking interviews are manageable with preparation.", weights: { banking: 4 } },
      { text: "I prefer structured exams with no subjective components.", weights: { ssc: 4, railways: 4, police: 3 } },
      { text: "I prefer technical viva / domain-specific questions over general GK.", weights: { psu: 5, teaching: 3 } }
    ]
  },
  {
    id: "gj_q8",
    text: "Which statement best describes your long-term aspiration?",
    options: [
      { text: "I want to shape national policy and hold constitutional authority.", weights: { ias_ips: 5 } },
      { text: "I want a respectful, stable job with career growth through promotions.", weights: { banking: 5, ssc: 4, railways: 4 } },
      { text: "I want to wear a uniform, protect my nation, and lead a disciplined life.", weights: { defence: 5, police: 5 } },
      { text: "I want to educate the next generation and contribute to human development.", weights: { teaching: 5 } }
    ]
  }
];
