/**
 * ENTREPRENEURSHIP QUIZ
 * Purpose: Help aspiring entrepreneurs identify what kind of business they should build,
 * and what their entrepreneurial strengths and domain should be.
 *
 * Weight dimensions:
 *   tech_startup   → SaaS / Tech product startup
 *   d2c            → Direct-to-Consumer / E-commerce / Product brand
 *   service_biz    → Agency / Consulting / Service business
 *   social_enterprise → Social enterprise / NGO / Impact startup
 *   franchise      → Franchise / Brick-and-mortar traditional business
 *   content_creator → Creator economy / YouTube / Influencer / Course creator
 *   fintech        → Finance / Lending / Payments startup
 *   edtech         → EdTech / Training / Coaching platform
 */

export const entrepreneurshipQuestions = [
  {
    id: "ent_q1",
    text: "What is your strongest motivation for starting a business?",
    options: [
      { text: "I have a technology idea that can scale to millions of users.", weights: { tech_startup: 5 } },
      { text: "I want to sell a product I believe in — build a brand.", weights: { d2c: 5, franchise: 2 } },
      { text: "I want to offer a specialized service and be my own boss.", weights: { service_biz: 5, content_creator: 2 } },
      { text: "I want to solve a social problem and create sustainable impact.", weights: { social_enterprise: 5 } }
    ]
  },
  {
    id: "ent_q2",
    text: "How do you feel about raising external funding (VC / Angel investment)?",
    options: [
      { text: "I'd actively seek it — VCs bring capital AND connections for tech growth.", weights: { tech_startup: 5, fintech: 4 } },
      { text: "I'd prefer bootstrapping — I want full control and no equity dilution.", weights: { d2c: 4, service_biz: 4, content_creator: 3 } },
      { text: "I'd consider grants or impact investors for a social venture.", weights: { social_enterprise: 5, edtech: 3 } },
      { text: "I'd get a bank loan — I'm building a traditional, profitable business.", weights: { franchise: 5, d2c: 2 } }
    ]
  },
  {
    id: "ent_q3",
    text: "Which of these best describes your idea of success in 5 years?",
    options: [
      { text: "A 100-person company with Series B funding and a product used globally.", weights: { tech_startup: 5 } },
      { text: "A profitable brand with ₹10 crore revenue selling on Amazon and D2C.", weights: { d2c: 5 } },
      { text: "A premium consulting or agency with 50 high-paying clients.", weights: { service_biz: 5 } },
      { text: "1 million subscribers / followers and a passive income of ₹3 LPM.", weights: { content_creator: 5 } }
    ]
  },
  {
    id: "ent_q4",
    text: "What is your core skill that you'd build a business around?",
    options: [
      { text: "Coding, product development, and tech architecture.", weights: { tech_startup: 5, fintech: 3 } },
      { text: "Marketing, storytelling, brand building, and aesthetics.", weights: { d2c: 5, content_creator: 4 } },
      { text: "Domain expertise — I'm exceptionally good at one professional skill.", weights: { service_biz: 5, edtech: 3 } },
      { text: "Community building, empathy, and grassroots mobilization.", weights: { social_enterprise: 5, edtech: 3 } }
    ]
  },
  {
    id: "ent_q5",
    text: "How do you feel about the uncertainty and risk of starting from zero?",
    options: [
      { text: "I thrive on it — uncertainty is where opportunity lives.", weights: { tech_startup: 5, content_creator: 4 } },
      { text: "I'm okay with calculated risk — I'll validate before I commit.", weights: { d2c: 4, service_biz: 4 } },
      { text: "I prefer lower risk — I'll follow a proven model (franchise / agency).", weights: { franchise: 5, service_biz: 3 } },
      { text: "I want sustainable impact — I can tolerate risk if the cause is worth it.", weights: { social_enterprise: 5 } }
    ]
  },
  {
    id: "ent_q6",
    text: "What is your biggest challenge in starting a business right now?",
    options: [
      { text: "Finding technical co-founders and building the MVP.", weights: { tech_startup: 5 } },
      { text: "Getting the first 100 customers and cracking marketing.", weights: { d2c: 4, content_creator: 4 } },
      { text: "Landing the first paying client for my skills or service.", weights: { service_biz: 5 } },
      { text: "Convincing investors or donors that my social idea is viable.", weights: { social_enterprise: 5, edtech: 3 } }
    ]
  },
  {
    id: "ent_q7",
    text: "Which entrepreneur inspires you the most?",
    options: [
      { text: "Kunal Shah (CRED), Vijay Shekhar Sharma (Paytm) — tech & fintech disruptors.", weights: { tech_startup: 5, fintech: 5 } },
      { text: "Vineeta Singh (Sugar Cosmetics), Ghazal Alagh (Mamaearth) — D2C brand builders.", weights: { d2c: 5 } },
      { text: "Byju Raveendran (BYJU'S), Ronnie Screwvala (upGrad) — EdTech visionaries.", weights: { edtech: 5 } },
      { text: "A local small business owner who built a ₹5 crore business from scratch.", weights: { franchise: 4, service_biz: 4 } }
    ]
  },
  {
    id: "ent_q8",
    text: "What product or service would you be most excited to sell?",
    options: [
      { text: "A subscription SaaS tool that automates a business problem.", weights: { tech_startup: 5, fintech: 3 } },
      { text: "A premium physical product with a strong emotional brand story.", weights: { d2c: 5 } },
      { text: "Online courses or coaching that transform careers.", weights: { edtech: 5, content_creator: 3 } },
      { text: "A service that helps other businesses grow — marketing, HR, or operations.", weights: { service_biz: 5 } }
    ]
  }
];
