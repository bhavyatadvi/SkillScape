// Adaptive Question Bank
// Questions are scenario-based, field-aware, and dimension-tagged.
// Each option scores into multiple career dimensions.
// dimensions: projectManagement, leadership, operations, technicalDepth, dataAnalytics,
//             creativity, research, peopleOrientation, businessStrategy, finance, healthcare, codingIntensity

// applicableDomains: if empty/null, applies to all. Otherwise only shown for those domains.
// excludeIfRejected: if user has marked this topic as "not interested", skip this question

export const ADAPTIVE_QUESTIONS = [

  // ---- SCENARIO-BASED CORE QUESTIONS (all domains) ----

  {
    id: 'q_scenario_project',
    question: 'You are given a complex project with multiple constraints. Which part would you most naturally want to take ownership of?',
    dimension: 'work_activity',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.5,
    options: [
      {
        text: 'Planning the timeline and coordinating people',
        scores: { projectManagement: 5, leadership: 3, operations: 4, technicalDepth: 1 }
      },
      {
        text: 'Digging into the technical problem',
        scores: { technicalDepth: 5, dataAnalytics: 3, research: 4, codingIntensity: 2 }
      },
      {
        text: 'Designing a creative or improved solution',
        scores: { creativity: 5, technicalDepth: 3, research: 3, businessStrategy: 2 }
      },
      {
        text: 'Analyzing data and uncovering insights',
        scores: { dataAnalytics: 5, research: 4, technicalDepth: 3, businessStrategy: 2 }
      },
      {
        text: 'Managing stakeholders and client communication',
        scores: { peopleOrientation: 5, leadership: 4, businessStrategy: 3, projectManagement: 3 }
      },
      {
        text: 'Researching options and best practices',
        scores: { research: 5, dataAnalytics: 3, technicalDepth: 3, creativity: 2 }
      },
      {
        text: 'None of these / I would prefer not to work on this type of task',
        scores: {},
        isNegative: true,
        rejectsDimension: 'project_work'
      }
    ]
  },

  {
    id: 'q_scenario_deadline',
    question: 'A project is falling behind schedule. What would you most naturally do?',
    dimension: 'work_style',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.3,
    options: [
      {
        text: 'Analyze the schedule and identify bottlenecks',
        scores: { projectManagement: 5, dataAnalytics: 4, operations: 3 }
      },
      {
        text: 'Take responsibility for coordinating the team',
        scores: { leadership: 5, projectManagement: 4, peopleOrientation: 3 }
      },
      {
        text: 'Investigate the technical root cause',
        scores: { technicalDepth: 5, research: 4, dataAnalytics: 3 }
      },
      {
        text: 'Redesign the process to prevent future issues',
        scores: { creativity: 5, businessStrategy: 4, operations: 3 }
      },
      {
        text: 'Communicate with stakeholders about risks and solutions',
        scores: { peopleOrientation: 5, businessStrategy: 4, leadership: 3 }
      },
      {
        text: 'Research best practices from similar situations',
        scores: { research: 5, technicalDepth: 3, dataAnalytics: 3 }
      },
      {
        text: 'I would prefer not to handle this type of situation',
        scores: {},
        isNegative: true,
      }
    ]
  },

  {
    id: 'q_workday_ideal',
    question: 'Which of these describes your ideal workday?',
    dimension: 'work_environment',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.2,
    options: [
      {
        text: 'Solving a mix of technical and organizational problems',
        scores: { technicalDepth: 4, projectManagement: 4, operations: 3 }
      },
      {
        text: 'Deep-diving into data and building models or reports',
        scores: { dataAnalytics: 5, research: 4, technicalDepth: 3, codingIntensity: 3 }
      },
      {
        text: 'Collaborating with clients or stakeholders',
        scores: { peopleOrientation: 5, businessStrategy: 4, leadership: 3 }
      },
      {
        text: 'Writing, documenting, or creating content',
        scores: { creativity: 5, research: 4, technicalDepth: 2 }
      },
      {
        text: 'Leading or mentoring a team',
        scores: { leadership: 6, peopleOrientation: 5, projectManagement: 3 }
      },
      {
        text: 'Researching and learning about complex topics',
        scores: { research: 6, dataAnalytics: 4, creativity: 3 }
      },
      {
        text: 'Working on my own tasks without much coordination',
        scores: { technicalDepth: 4, research: 3 }
      }
    ]
  },

  {
    id: 'q_interest_data',
    question: 'Someone shows you a large dataset about customer behavior. What do you want to do with it?',
    dimension: 'data_orientation',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.4,
    options: [
      {
        text: 'Run a SQL query to find patterns',
        scores: { dataAnalytics: 5, technicalDepth: 4, codingIntensity: 4 }
      },
      {
        text: 'Build a visualization or dashboard to understand it',
        scores: { dataAnalytics: 5, creativity: 4, businessStrategy: 3, codingIntensity: 2 }
      },
      {
        text: 'Look for business insights to improve strategy',
        scores: { businessStrategy: 5, dataAnalytics: 4, peopleOrientation: 3 }
      },
      {
        text: 'Use it to build a predictive model',
        scores: { dataAnalytics: 6, technicalDepth: 5, codingIntensity: 5, research: 4 }
      },
      {
        text: 'Write a report summarizing what it means',
        scores: { creativity: 4, research: 5, dataAnalytics: 3 }
      },
      {
        text: 'I would hand it to someone more comfortable with data',
        scores: {},
        isNegative: true,
        penalizeDimensions: { dataAnalytics: -3, codingIntensity: -3 }
      }
    ]
  },

  {
    id: 'q_people_vs_tech',
    question: 'Which activity energizes you more?',
    dimension: 'people_vs_technical',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.5,
    options: [
      {
        text: 'Working with people — clients, teams, or stakeholders',
        scores: { peopleOrientation: 6, leadership: 4, businessStrategy: 3 }
      },
      {
        text: 'Working with technology or technical problems',
        scores: { technicalDepth: 6, codingIntensity: 3, dataAnalytics: 3 }
      },
      {
        text: 'Working with data and numbers',
        scores: { dataAnalytics: 6, finance: 3, research: 4 }
      },
      {
        text: 'Working on creative or design challenges',
        scores: { creativity: 6, research: 4, technicalDepth: 2 }
      },
      {
        text: 'Working on research and deep investigation',
        scores: { research: 6, dataAnalytics: 4, technicalDepth: 4 }
      },
      {
        text: 'Working on strategy and business decisions',
        scores: { businessStrategy: 6, leadership: 4, finance: 3 }
      }
    ]
  },

  {
    id: 'q_coding_comfort',
    question: 'How do you feel about writing code as part of your regular job?',
    dimension: 'coding_preference',
    applicableDomains: null,
    allowMultiple: false,
    weight: 2.0,
    options: [
      {
        text: 'I love coding — it is one of my key strengths',
        scores: { codingIntensity: 8, technicalDepth: 5, dataAnalytics: 4 }
      },
      {
        text: 'I am comfortable with light scripting or automation',
        scores: { codingIntensity: 4, technicalDepth: 4, dataAnalytics: 3 }
      },
      {
        text: 'I prefer tools over code (Excel, Power BI, etc.)',
        scores: { dataAnalytics: 4, codingIntensity: 1, businessStrategy: 3 }
      },
      {
        text: 'I do not mind some code, but I prefer other work',
        scores: { codingIntensity: 2, projectManagement: 3, operations: 3 }
      },
      {
        text: 'I strongly prefer not to write code',
        scores: { codingIntensity: -5, projectManagement: 4, peopleOrientation: 3, businessStrategy: 3 },
        isNegative: true,
        penalizeDimensions: { codingIntensity: -5 }
      }
    ]
  },

  {
    id: 'q_leadership_aspiration',
    question: 'What is your relationship with leadership and managing others?',
    dimension: 'leadership_orientation',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.2,
    options: [
      {
        text: 'I actively want to lead and manage a team',
        scores: { leadership: 7, peopleOrientation: 5, projectManagement: 4, businessStrategy: 4 }
      },
      {
        text: 'I am comfortable with some leadership but prefer individual contribution',
        scores: { leadership: 3, technicalDepth: 4, dataAnalytics: 3 }
      },
      {
        text: 'I prefer working as an individual contributor',
        scores: { technicalDepth: 5, research: 5, creativity: 4 }
      },
      {
        text: 'I want to influence without formal authority (consulting/advisory)',
        scores: { businessStrategy: 6, leadership: 4, peopleOrientation: 5 }
      }
    ]
  },

  {
    id: 'q_problem_type',
    question: 'Which type of problem excites you most?',
    dimension: 'problem_solving_style',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.3,
    options: [
      {
        text: 'Strategic: figuring out the best path for a business or product',
        scores: { businessStrategy: 6, leadership: 4, projectManagement: 3 }
      },
      {
        text: 'Technical: debugging, optimizing, or engineering a solution',
        scores: { technicalDepth: 6, codingIntensity: 3, research: 4 }
      },
      {
        text: 'Analytical: finding patterns or answers in data',
        scores: { dataAnalytics: 6, research: 5, technicalDepth: 3 }
      },
      {
        text: 'Operational: improving a process to run better or faster',
        scores: { operations: 6, projectManagement: 4, businessStrategy: 3 }
      },
      {
        text: 'Human: understanding people and helping them succeed',
        scores: { peopleOrientation: 6, leadership: 5, healthcare: 3 }
      },
      {
        text: 'Creative: designing or inventing something new',
        scores: { creativity: 7, technicalDepth: 3, research: 3 }
      }
    ]
  },

  {
    id: 'q_work_structure',
    question: 'Which working style fits you best?',
    dimension: 'work_structure',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.0,
    options: [
      {
        text: 'Highly structured: clear deliverables, processes, and timelines',
        scores: { projectManagement: 5, operations: 5, finance: 3 }
      },
      {
        text: 'Research-driven: deep investigation before deciding',
        scores: { research: 6, dataAnalytics: 4, technicalDepth: 4 }
      },
      {
        text: 'Creative and flexible: experimenting and iterating',
        scores: { creativity: 6, businessStrategy: 4, technicalDepth: 3 }
      },
      {
        text: 'Adaptive: responding quickly to changing priorities',
        scores: { leadership: 4, operations: 4, projectManagement: 3, businessStrategy: 3 }
      }
    ]
  },

  // ---- CIVIL ENGINEERING DOMAIN QUESTIONS ----

  {
    id: 'q_civil_project_responsibility',
    question: 'You are given a construction project with cost, schedule, and quality constraints. Which responsibility would you most enjoy?',
    dimension: 'domain_specific',
    applicableDomains: ['civil_engineering'],
    allowMultiple: false,
    weight: 1.6,
    options: [
      {
        text: 'Planning the schedule and coordinating contractors',
        scores: { projectManagement: 6, operations: 5, leadership: 4 }
      },
      {
        text: 'Estimating costs and tracking budgets',
        scores: { finance: 6, dataAnalytics: 4, operations: 4 }
      },
      {
        text: 'Reviewing technical designs and ensuring quality',
        scores: { technicalDepth: 6, research: 4, operations: 3 }
      },
      {
        text: 'Using software to create BIM models and coordinate design',
        scores: { technicalDepth: 5, creativity: 5, dataAnalytics: 3, codingIntensity: 2 }
      },
      {
        text: 'Analyzing data to improve sustainability and ESG',
        scores: { research: 5, dataAnalytics: 5, creativity: 4 }
      },
      {
        text: 'None of these appeal to me',
        scores: {},
        isNegative: true,
      }
    ]
  },

  {
    id: 'q_civil_future',
    question: 'If you moved away from site engineering, what appeals to you most?',
    dimension: 'interest_direction',
    applicableDomains: ['civil_engineering'],
    allowMultiple: true,
    weight: 1.5,
    options: [
      {
        text: 'Managing projects and programs',
        scores: { projectManagement: 6, leadership: 4 }
      },
      {
        text: 'Consulting and advising organizations',
        scores: { businessStrategy: 5, peopleOrientation: 4, leadership: 4 }
      },
      {
        text: 'Working in data / analytics',
        scores: { dataAnalytics: 6, codingIntensity: 3 }
      },
      {
        text: 'Real estate and property',
        scores: { finance: 5, peopleOrientation: 4, businessStrategy: 4 }
      },
      {
        text: 'Supply chain and operations',
        scores: { operations: 6, projectManagement: 4 }
      },
      {
        text: 'Sustainability and green building',
        scores: { research: 5, creativity: 4, technicalDepth: 3 }
      },
      {
        text: 'None of these — I want to stay in construction',
        scores: { technicalDepth: 5, projectManagement: 4 },
      }
    ]
  },
  {
    id: 'q_civil_work_setting',
    question: 'Which civil engineering challenge would you most like to work on?',
    dimension: 'domain_specific',
    applicableDomains: ['civil_engineering'],
    allowMultiple: false,
    weight: 1.5,
    options: [
      { text: 'Design safe structures that meet technical codes', scores: { technicalDepth: 6, research: 4, operations: 2 } },
      { text: 'Coordinate construction teams, schedules, and site decisions', scores: { projectManagement: 6, leadership: 4, operations: 4 } },
      { text: 'Plan transport, water, or other public infrastructure', scores: { research: 4, technicalDepth: 4, projectManagement: 4 } },
      { text: 'Use BIM, surveying, or geospatial data to improve designs', scores: { dataAnalytics: 5, technicalDepth: 5, creativity: 3 } },
      { text: 'Make buildings and infrastructure more sustainable', scores: { research: 5, creativity: 4, technicalDepth: 3 } }
    ]
  },

  // ---- MECHANICAL ENGINEERING DOMAIN QUESTIONS ----

  {
    id: 'q_mechanical_role',
    question: 'Which of these roles interests you most from your mechanical background?',
    dimension: 'domain_specific',
    applicableDomains: ['mechanical_engineering'],
    allowMultiple: false,
    weight: 1.6,
    options: [
      {
        text: 'Supply chain and procurement',
        scores: { operations: 6, finance: 4, projectManagement: 4 }
      },
      {
        text: 'Product development and R&D',
        scores: { technicalDepth: 6, research: 5, creativity: 4 }
      },
      {
        text: 'Operations and production management',
        scores: { operations: 6, leadership: 5, projectManagement: 4 }
      },
      {
        text: 'Data analytics for manufacturing',
        scores: { dataAnalytics: 6, technicalDepth: 4, codingIntensity: 3 }
      },
      {
        text: 'Consulting for industrial clients',
        scores: { businessStrategy: 6, peopleOrientation: 4, leadership: 4 }
      },
      {
        text: 'None of these — I want to stay in core mechanical work',
        scores: { technicalDepth: 6, research: 4 }
      }
    ]
  },
  {
    id: 'q_mechanical_challenge',
    question: 'A machine or production line is underperforming. Which part of the investigation would you choose?',
    dimension: 'domain_specific',
    applicableDomains: ['mechanical_engineering'],
    allowMultiple: false,
    weight: 1.6,
    options: [
      { text: 'Inspect components and diagnose the mechanical cause', scores: { technicalDepth: 7, research: 3, operations: 3 } },
      { text: 'Model and test a design change or new component', scores: { technicalDepth: 6, creativity: 4, research: 4 } },
      { text: 'Analyze sensor and production data to locate the bottleneck', scores: { dataAnalytics: 6, technicalDepth: 4, operations: 4 } },
      { text: 'Organize maintenance and production teams around a fix', scores: { operations: 6, projectManagement: 5, leadership: 4 } },
      { text: 'Compare vendors, costs, and sourcing options', scores: { finance: 4, operations: 5, businessStrategy: 3 } }
    ]
  },

  // ---- COMPUTER SCIENCE / IT DOMAIN QUESTIONS ----

  {
    id: 'q_cs_direction',
    question: 'Within the technology space, which direction appeals most to you?',
    dimension: 'domain_specific',
    applicableDomains: ['computer_science'],
    allowMultiple: false,
    weight: 1.6,
    options: [
      {
        text: 'Building products — feature development and user experience',
        scores: { creativity: 5, businessStrategy: 5, peopleOrientation: 4, codingIntensity: 3 }
      },
      {
        text: 'Data and machine learning',
        scores: { dataAnalytics: 6, research: 5, codingIntensity: 5, technicalDepth: 5 }
      },
      {
        text: 'Cybersecurity and infrastructure',
        scores: { technicalDepth: 6, research: 5, codingIntensity: 4 }
      },
      {
        text: 'Moving into product management (less coding, more strategy)',
        scores: { businessStrategy: 6, leadership: 5, peopleOrientation: 5, codingIntensity: 1 }
      },
      {
        text: 'Business or technical consulting',
        scores: { businessStrategy: 6, peopleOrientation: 5, leadership: 4, codingIntensity: 1 }
      },
      {
        text: 'I want to move entirely away from technology',
        scores: { codingIntensity: -6, technicalDepth: -2 },
        isNegative: true
      }
    ]
  },
  {
    id: 'q_cs_problem_type',
    question: 'Which technology problem would you most enjoy solving?',
    dimension: 'domain_specific',
    applicableDomains: ['computer_science'],
    allowMultiple: false,
    weight: 1.6,
    options: [
      { text: 'Build and maintain reliable software features', scores: { technicalDepth: 6, codingIntensity: 6, creativity: 3 } },
      { text: 'Find patterns in data and turn them into useful predictions', scores: { dataAnalytics: 7, research: 4, codingIntensity: 4 } },
      { text: 'Protect systems and investigate security incidents', scores: { technicalDepth: 6, research: 5, codingIntensity: 3 } },
      { text: 'Make an application easier and more useful for people', scores: { creativity: 5, peopleOrientation: 5, businessStrategy: 3, codingIntensity: 2 } },
      { text: 'Translate customer needs into priorities for a product team', scores: { businessStrategy: 5, peopleOrientation: 5, leadership: 4, projectManagement: 3 } }
    ]
  },

  // ---- MEDICINE DOMAIN QUESTIONS ----

  {
    id: 'q_medicine_transition',
    question: 'As a medical professional, which type of work would you most like to do outside of clinical practice?',
    dimension: 'domain_specific',
    applicableDomains: ['medicine'],
    allowMultiple: false,
    weight: 1.6,
    options: [
      {
        text: 'Public health and disease prevention',
        scores: { healthcare: 6, research: 5, operations: 4 }
      },
      {
        text: 'Clinical research and drug trials',
        scores: { research: 6, healthcare: 5, technicalDepth: 4 }
      },
      {
        text: 'Healthcare management and hospital administration',
        scores: { leadership: 6, operations: 5, healthcare: 4, projectManagement: 4 }
      },
      {
        text: 'Medical affairs in pharma companies',
        scores: { healthcare: 5, businessStrategy: 5, peopleOrientation: 4, research: 4 }
      },
      {
        text: 'Health informatics and digital health',
        scores: { technicalDepth: 5, dataAnalytics: 6, healthcare: 5 }
      },
      {
        text: 'Medical writing and education',
        scores: { creativity: 5, research: 5, healthcare: 4 }
      }
    ]
  },

  {
    id: 'q_medicine_patient_pathway',
    question: 'A patient needs coordinated care across several specialists. Which contribution would you find most engaging?',
    dimension: 'domain_specific',
    applicableDomains: ['medicine'],
    allowMultiple: false,
    weight: 1.6,
    options: [
      { text: 'Assess symptoms, form a diagnosis, and plan treatment', scores: { healthcare: 7, research: 3, peopleOrientation: 4 } },
      { text: 'Use diagnostic data and imaging to clarify what is happening', scores: { healthcare: 5, dataAnalytics: 5, technicalDepth: 3 } },
      { text: 'Explain options and help the patient make an informed choice', scores: { healthcare: 5, peopleOrientation: 7, leadership: 2 } },
      { text: 'Improve the care pathway so teams coordinate safely', scores: { healthcare: 4, operations: 5, projectManagement: 4, leadership: 3 } },
      { text: 'Study evidence to improve future treatment guidelines', scores: { healthcare: 4, research: 7, dataAnalytics: 3 } }
    ]
  },
  {
    id: 'q_medicine_clinical_environment',
    question: 'Which part of a healthcare setting would you prefer to spend most of your time working on?',
    dimension: 'domain_specific',
    applicableDomains: ['medicine'],
    allowMultiple: false,
    weight: 1.5,
    options: [
      { text: 'Direct patient assessment and ongoing care', scores: { healthcare: 7, peopleOrientation: 5, operations: 2 } },
      { text: 'Diagnostic procedures, tests, or medical technology', scores: { healthcare: 5, technicalDepth: 5, dataAnalytics: 3 } },
      { text: 'Clinical studies and interpreting medical evidence', scores: { healthcare: 4, research: 7, dataAnalytics: 4 } },
      { text: 'Organizing safe, efficient services for patients', scores: { healthcare: 4, operations: 6, projectManagement: 4, leadership: 3 } },
      { text: 'Patient education and prevention programs', scores: { healthcare: 5, peopleOrientation: 6, creativity: 2 } }
    ]
  },

  // ---- PHARMACY DOMAIN QUESTIONS ----

  {
    id: 'q_pharmacy_work_focus',
    question: 'Which pharmacy-related task would you most enjoy taking responsibility for?',
    dimension: 'domain_specific',
    applicableDomains: ['pharmacy'],
    allowMultiple: false,
    weight: 1.7,
    options: [
      { text: 'Check prescriptions and counsel people on safe medicine use', scores: { healthcare: 6, peopleOrientation: 5, operations: 3 } },
      { text: 'Study how a drug works and help develop or test medicines', scores: { research: 7, healthcare: 4, technicalDepth: 4 } },
      { text: 'Test samples and ensure medicines meet quality standards', scores: { technicalDepth: 5, dataAnalytics: 4, operations: 4, research: 3 } },
      { text: 'Review regulations and prepare accurate compliance records', scores: { operations: 5, research: 3, businessStrategy: 3, technicalDepth: 2 } },
      { text: 'Plan medicine supply, inventory, and pharmacy services', scores: { operations: 6, projectManagement: 4, leadership: 3, healthcare: 3 } }
    ]
  },
  {
    id: 'q_pharmacy_problem_solving',
    question: 'A medicine supply issue is affecting patients. What would you be most motivated to do?',
    dimension: 'domain_specific',
    applicableDomains: ['pharmacy'],
    allowMultiple: false,
    weight: 1.6,
    options: [
      { text: 'Find a safe alternative and explain the change to patients', scores: { healthcare: 6, peopleOrientation: 5, research: 3 } },
      { text: 'Trace the supply and coordinate a reliable replacement', scores: { operations: 6, projectManagement: 4, leadership: 3 } },
      { text: 'Investigate product quality or manufacturing causes', scores: { technicalDepth: 5, research: 5, dataAnalytics: 3 } },
      { text: 'Check reporting requirements and document the issue', scores: { operations: 5, businessStrategy: 3, research: 3 } },
      { text: 'Analyze usage and demand patterns to prevent recurrence', scores: { dataAnalytics: 6, operations: 4, research: 3 } }
    ]
  },
  {
    id: 'q_pharmacy_career_setting',
    question: 'Which setting would you be most curious to explore as a pharmacy graduate?',
    dimension: 'domain_specific',
    applicableDomains: ['pharmacy'],
    allowMultiple: false,
    weight: 1.5,
    options: [
      { text: 'A hospital or community pharmacy focused on patient care', scores: { healthcare: 6, peopleOrientation: 5, operations: 2 } },
      { text: 'A pharmaceutical research or clinical-trials team', scores: { research: 7, healthcare: 3, dataAnalytics: 3 } },
      { text: 'A manufacturing, quality-control, or quality-assurance lab', scores: { technicalDepth: 6, operations: 4, dataAnalytics: 3 } },
      { text: 'A regulatory affairs or drug-safety team', scores: { research: 4, operations: 5, healthcare: 3, businessStrategy: 2 } },
      { text: 'A pharma business, product, or supply-chain team', scores: { businessStrategy: 5, operations: 4, peopleOrientation: 3, leadership: 2 } }
    ]
  },

  // ---- BUSINESS / COMMERCE DOMAIN QUESTIONS ----

  {
    id: 'q_business_role',
    question: 'A company wants to enter a new market. Which aspect of this problem would you most want to work on?',
    dimension: 'domain_specific',
    applicableDomains: ['business', 'finance', 'marketing'],
    allowMultiple: false,
    weight: 1.6,
    options: [
      {
        text: 'Market research and competitive analysis',
        scores: { research: 5, businessStrategy: 5, dataAnalytics: 4 }
      },
      {
        text: 'Financial modeling and investment analysis',
        scores: { finance: 7, dataAnalytics: 5, businessStrategy: 4 }
      },
      {
        text: 'Defining the go-to-market strategy',
        scores: { businessStrategy: 7, creativity: 5, leadership: 4 }
      },
      {
        text: 'Managing the operational rollout',
        scores: { operations: 6, projectManagement: 5, leadership: 4 }
      },
      {
        text: 'Building customer relationships and managing accounts',
        scores: { peopleOrientation: 6, businessStrategy: 4, leadership: 3 }
      }
    ]
  },

  // ---- RESKILLING PREFERENCE QUESTIONS ----

  {
    id: 'q_learning_style',
    question: 'When you need to learn a new skill, which approach suits you best?',
    dimension: 'learning_preference',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.0,
    options: [
      {
        text: 'Online courses and structured learning paths',
        scores: { research: 3, codingIntensity: 2, dataAnalytics: 2 }
      },
      {
        text: 'Learning on the job while working in the new role',
        scores: { operations: 4, leadership: 3, projectManagement: 3 }
      },
      {
        text: 'Hands-on projects and portfolio building',
        scores: { creativity: 4, technicalDepth: 4, codingIntensity: 3 }
      },
      {
        text: 'Formal qualifications or certifications',
        scores: { research: 4, technicalDepth: 3, healthcare: 2 }
      }
    ]
  },

  {
    id: 'q_risk_tolerance',
    question: 'How do you feel about taking on a role with significant uncertainty?',
    dimension: 'risk_tolerance',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.0,
    options: [
      {
        text: 'I am comfortable with uncertainty and rapid change',
        scores: { businessStrategy: 5, creativity: 4, leadership: 4 }
      },
      {
        text: 'I prefer some structure but can handle ambiguity',
        scores: { projectManagement: 4, operations: 4, businessStrategy: 3 }
      },
      {
        text: 'I prefer well-defined roles with clear expectations',
        scores: { operations: 5, finance: 4, technicalDepth: 4 }
      },
      {
        text: 'I want maximum stability and clear career progression',
        scores: { operations: 6, finance: 5, projectManagement: 4 }
      }
    ]
  },

  {
    id: 'q_stakeholder_preference',
    question: 'Who would you most prefer to work with every day?',
    dimension: 'stakeholder_preference',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.1,
    options: [
      {
        text: 'A cross-functional team across departments',
        scores: { projectManagement: 5, leadership: 4, operations: 4 }
      },
      {
        text: 'Clients or external stakeholders',
        scores: { peopleOrientation: 6, businessStrategy: 5, leadership: 4 }
      },
      {
        text: 'A small technical team working on complex problems',
        scores: { technicalDepth: 6, research: 5, codingIntensity: 3 }
      },
      {
        text: 'Data or computer systems more than people',
        scores: { dataAnalytics: 6, technicalDepth: 5, codingIntensity: 4 }
      },
      {
        text: 'Individual contributors who work independently',
        scores: { creativity: 5, research: 5, technicalDepth: 4 }
      }
    ]
  },

  {
    id: 'q_output_preference',
    question: 'What kind of output from your work excites you most?',
    dimension: 'output_preference',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.1,
    options: [
      {
        text: 'A delivered project or initiative',
        scores: { projectManagement: 5, leadership: 4, operations: 4 }
      },
      {
        text: 'A data insight or analytical finding',
        scores: { dataAnalytics: 6, research: 5, businessStrategy: 3 }
      },
      {
        text: 'A product or feature users love',
        scores: { creativity: 5, businessStrategy: 5, peopleOrientation: 4, codingIntensity: 2 }
      },
      {
        text: 'A report, document, or recommendation',
        scores: { research: 5, creativity: 4, businessStrategy: 4 }
      },
      {
        text: 'A working system or technical solution',
        scores: { technicalDepth: 6, codingIntensity: 4, creativity: 4 }
      },
      {
        text: 'A positive impact on people or communities',
        scores: { peopleOrientation: 6, healthcare: 4, leadership: 3 }
      }
    ]
  },

  {
    id: 'q_financial_interest',
    question: 'How do you feel about financial analysis as part of your work?',
    dimension: 'financial_orientation',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.0,
    options: [
      {
        text: 'I love working with numbers, budgets, and forecasts',
        scores: { finance: 7, dataAnalytics: 5, businessStrategy: 4 }
      },
      {
        text: 'I am comfortable with basic financial reporting',
        scores: { finance: 4, operations: 4, projectManagement: 3 }
      },
      {
        text: 'I prefer to leave financial analysis to others',
        scores: { creativity: 3, technicalDepth: 3, peopleOrientation: 3 }
      },
      {
        text: 'Finance is not my strength or interest',
        scores: {},
        isNegative: true,
        penalizeDimensions: { finance: -2 }
      }
    ]
  },

  {
    id: 'q_healthcare_interest',
    question: 'Does working in or around the healthcare sector interest you?',
    dimension: 'healthcare_orientation',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.3,
    options: [
      {
        text: 'Yes — I am passionate about healthcare',
        scores: { healthcare: 8, research: 3, peopleOrientation: 4 }
      },
      {
        text: 'I am open to it if the role is right',
        scores: { healthcare: 4 }
      },
      {
        text: 'I have no strong preference either way',
        scores: {}
      },
      {
        text: 'No — I prefer to avoid healthcare-related roles',
        scores: { healthcare: -10 },
        isNegative: true,
        hardExcludesFamilies: ['healthcare_admin', 'public_health', 'clinical_research', 'medical_affairs', 'health_informatics']
      }
    ]
  },

  {
    id: 'q_autonomy_preference',
    question: 'How much autonomy do you want in your role?',
    dimension: 'autonomy',
    applicableDomains: null,
    allowMultiple: false,
    weight: 0.9,
    options: [
      {
        text: 'High autonomy — I want to define my own work and strategy',
        scores: { businessStrategy: 5, creativity: 5, leadership: 4 }
      },
      {
        text: 'Mixed — clear objectives but freedom in how I work',
        scores: { projectManagement: 4, creativity: 3, businessStrategy: 3 }
      },
      {
        text: 'Structured — I prefer clear processes and expectations',
        scores: { operations: 5, finance: 4, technicalDepth: 3 }
      }
    ]
  },

  {
    id: 'q_research_interest',
    question: 'How do you feel about spending significant time researching and investigating before producing outputs?',
    dimension: 'research_orientation',
    applicableDomains: null,
    allowMultiple: false,
    weight: 1.0,
    options: [
      {
        text: 'I love deep research — it is one of my strengths',
        scores: { research: 7, dataAnalytics: 4, technicalDepth: 4 }
      },
      {
        text: 'I am comfortable with targeted research',
        scores: { research: 4, dataAnalytics: 3, businessStrategy: 3 }
      },
      {
        text: 'I prefer action over research — I move fast',
        scores: { operations: 5, leadership: 4, projectManagement: 4 }
      },
      {
        text: 'Research is not my strength',
        scores: { operations: 3, peopleOrientation: 3 },
        penalizeDimensions: { research: -2 }
      }
    ]
  },
];

export function getInitialQuestions(domain) {
  return ADAPTIVE_QUESTIONS.filter(q => !q.applicableDomains || q.applicableDomains.includes(domain));
}

export function selectNextQuestion(askedIds, careerScores, domain, rejectedFamilies, rejectedDimensions) {
  const rejected = new Set(rejectedDimensions || []);
  const available = ADAPTIVE_QUESTIONS.filter(q => {
    if (askedIds.includes(q.id)) return false;
    if (q.applicableDomains && !q.applicableDomains.includes(domain)) return false;
    // Once an activity is explicitly rejected, do not keep drilling into it.
    // This preserves useful broad questions while skipping coding/data follow-ups.
    const questionDimensions = q.options.flatMap(option => [
      ...Object.keys(option.scores || {}),
      ...Object.keys(option.penalizeDimensions || {}),
    ]);
    if (rejected.has('codingIntensity') && (q.dimension === 'coding_preference' || questionDimensions.filter(d => d === 'codingIntensity').length >= 2)) return false;
    if (rejected.has('dataAnalytics') && (q.dimension === 'data_orientation' || q.dimension === 'data_affinity')) return false;
    if (rejected.has('healthcare') && q.applicableDomains?.some(d => ['medicine', 'pharmacy'].includes(d))) return false;
    return true;
  });

  // Prioritize domain-specific questions first, then general
  const domainQ = available.filter(q => q.applicableDomains && q.applicableDomains.includes(domain));
  const generalQ = available.filter(q => !q.applicableDomains);

  // Sort by weight descending
  const sorted = [...domainQ, ...generalQ].sort((a, b) => (b.weight || 1) - (a.weight || 1));

  return sorted[0] || null;
}
