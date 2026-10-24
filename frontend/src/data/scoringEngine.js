// Career Scoring Engine
// Calculates compatibility scores for all candidate careers based on assessment answers.

import { detectCurrentDomain } from './careerGraph';

export function initializeDimensionScores() {
  return {
    projectManagement: 0,
    leadership: 0,
    operations: 0,
    technicalDepth: 0,
    dataAnalytics: 0,
    creativity: 0,
    research: 0,
    peopleOrientation: 0,
    businessStrategy: 0,
    finance: 0,
    healthcare: 0,
    codingIntensity: 0,
  };
}

export function applyAnswerToScores(scores, selectedOption, questionWeight = 1.0) {
  const updated = { ...scores };
  if (!selectedOption?.scores) return updated;

  for (const [dim, value] of Object.entries(selectedOption.scores)) {
    if (updated[dim] !== undefined) {
      updated[dim] += value * questionWeight;
    }
  }

  // Apply explicit penalties if any
  if (selectedOption.penalizeDimensions) {
    for (const [dim, value] of Object.entries(selectedOption.penalizeDimensions)) {
      if (updated[dim] !== undefined) {
        updated[dim] += value * questionWeight;
      }
    }
  }

  return updated;
}

export function applyMultipleAnswers(scores, selectedOptions, questionWeight = 1.0) {
  let updated = { ...scores };
  for (const opt of selectedOptions) {
    updated = applyAnswerToScores(updated, opt, questionWeight);
  }
  return updated;
}

export function getHardExclusions(answers) {
  const excluded = new Set();
  for (const answerSet of Object.values(answers)) {
    const options = Array.isArray(answerSet) ? answerSet : [answerSet];
    for (const opt of options) {
      if (opt?.hardExcludesFamilies) {
        opt.hardExcludesFamilies.forEach(f => excluded.add(f));
      }
    }
  }
  return excluded;
}

function normalizedSet(values = []) {
  return new Set(values.map(value => String(value).trim().toLowerCase()));
}

export function isHardExcludedByConstraint(career, constraints = []) {
  const excluded = normalizedSet(constraints);
  const coding = career.dimensions?.codingIntensity || 0;
  const environments = normalizedSet(career.workEnvironments || []);
  const activities = normalizedSet(career.workActivities || []);
  const tags = normalizedSet(career.tags || []);

  if (excluded.has('heavy coding') && coding >= 7) return true;
  if (excluded.has('sales') && (career.family === 'sales_business_dev' || tags.has('sales'))) return true;
  if (excluded.has('field work') && (environments.has('field') || environments.has('site') || activities.has('site_visits'))) return true;
  if (excluded.has('night shifts') && (environments.has('hospital/site') || environments.has('hospital'))) return true;
  if (excluded.has('customer-facing work') && (activities.has('client_management') || activities.has('client-facing'))) return true;
  return false;
}

export function calculateCareerScore(career, dimensionScores, preQuizAnswers, careerProfile, hardExclusions) {
  // Hard exclusion first
  if (hardExclusions.has(career.family)) return -999;

  // Check user-stated constraints against career's work activities
  const constraints = careerProfile?.constraints || [];
  let constraintPenalty = 0;
  if (isHardExcludedByConstraint(career, constraints)) return -999;
  if (constraints.includes('Heavy coding') && (career.dimensions?.codingIntensity || 0) >= 7) {
    constraintPenalty -= 40;
  }
  if (constraints.includes('Sales') && career.family === 'sales_business_dev') {
    constraintPenalty -= 40;
  }
  if (constraints.includes('Field work') && career.workEnvironments?.includes('Field')) {
    constraintPenalty -= 20;
  }

  // Career distance preference
  const distanceAnswer = preQuizAnswers?.[2] || careerProfile?.careerChangePreference || '3';
  const distancePref = String(distanceAnswer).includes('Not sure') ? 5 : (parseInt(distanceAnswer, 10) || 3);
  const careerDist = career.baseDistance || 2;
  let distanceFit = 0;
  if (careerDist <= distancePref) {
    distanceFit = 15;
  } else if (careerDist === distancePref + 1) {
    distanceFit = 5;
  } else if (careerDist > distancePref + 1) {
    distanceFit = -10 * (careerDist - distancePref);
  }

  // Reskilling willingness
  const reskillingMap = {
    'Minimal reskilling': 1,
    'Some upskilling': 2,
    'Moderate reskilling': 3,
    'Major reskilling': 4,
    'I am willing to study for a new qualification': 5,
  };
  const userReskilling = reskillingMap[preQuizAnswers?.[6]] || 3;
  const careerReskilling = career.reskillingLevel || 2;
  let reskillingFit = 0;
  if (careerReskilling <= userReskilling) {
    reskillingFit = 10;
  } else {
    reskillingFit = -5 * (careerReskilling - userReskilling);
  }

  // Dimension match score — compare user's dimension scores with career's dimension profile
  const careerDims = career.dimensions || {};
  const maxPossibleDimScore = 10;
  let dimMatchScore = 0;
  let dimCount = 0;

  for (const [dim, careerVal] of Object.entries(careerDims)) {
    const userVal = dimensionScores[dim] || 0;
    const normalizedUser = Math.min(Math.max(userVal / 5, 0), 1); // normalize to 0–1
    const normalizedCareer = careerVal / maxPossibleDimScore;
    // Similarity score: 1 - abs difference
    const similarity = 1 - Math.abs(normalizedUser - normalizedCareer);
    dimMatchScore += similarity * careerVal; // weight by how important this dim is to the career
    dimCount += careerVal;
  }

  const normalizedDimMatch = dimCount > 0 ? (dimMatchScore / dimCount) * 100 : 50;

  // User interest keywords check from pre-assessment
  const interests = preQuizAnswers?.[5] || [];
  let interestBoost = 0;
  const interestMapping = {
    'Managing projects': ['project_manager', 'operations_manager'],
    'Analyzing data': ['data_analyst', 'data_scientist', 'business_analyst'],
    'Business strategy': ['management_consultant', 'product_manager', 'business_analyst'],
    'Research': ['data_scientist', 'clinical_research', 'public_health'],
    'Helping people': ['public_health', 'hr_manager', 'instructional_designer'],
    'Healthcare': ['public_health', 'clinical_research', 'medical_affairs', 'health_informatics'],
    'Teaching': ['instructional_designer', 'hr_manager'],
    'Environment': ['sustainability_consultant'],
    'Entrepreneurship': ['product_manager'],
    'Working with technology': ['software_engineer', 'data_analyst', 'bim_specialist'],
    'Leading teams': ['project_manager', 'operations_manager', 'management_consultant'],
  };
  for (const interest of interests) {
    const boostedCareers = interestMapping[interest] || [];
    if (boostedCareers.includes(career.id)) {
      interestBoost += 10;
    }
  }

  // Demand as secondary factor (max 10 points)
  const demandScore = ((career.demand || 70) / 100) * 10;

  const totalScore = normalizedDimMatch + distanceFit + reskillingFit + interestBoost + demandScore + constraintPenalty;

  return Math.max(0, Math.min(100, totalScore));
}

export function rankCareers(careers, dimensionScores, preQuizAnswers, careerProfile) {
  const hardExclusions = getHardExclusions({});
  
  // Check pre-quiz Q4 for activities to avoid
  const notWanted = preQuizAnswers?.[4] || [];
  const extendedExclusions = new Set([...hardExclusions]);
  if (notWanted.includes('Heavy coding')) {
    // Will be penalized via constraints
  }

  // Pre-assessment aversions are as important as saved profile constraints.
  // They must participate in both filtering and explanations, even when the
  // user has not yet saved a profile edit.
  const constraints = [...new Set([...(careerProfile?.constraints || []), ...notWanted])]
    .filter(value => value && value !== 'None of these');

  return careers
    .filter(career => !isHardExcludedByConstraint(career, constraints))
    .map(career => ({
      ...career,
      compatibilityScore: Math.round(calculateCareerScore(career, dimensionScores, preQuizAnswers, { ...careerProfile, constraints }, hardExclusions)),
    }))
    .sort((a, b) => b.compatibilityScore - a.compatibilityScore);
}

export function getConfidenceLevel(rankedCareers) {
  if (!rankedCareers.length) return 'Low';
  const topScore = rankedCareers[0].compatibilityScore;
  const secondScore = rankedCareers[1]?.compatibilityScore || 0;
  const gap = topScore - secondScore;
  if (gap >= 15 && topScore >= 75) return 'High';
  if (gap >= 8 && topScore >= 60) return 'Moderate';
  return 'Low';
}

export function getTransitionDifficulty(career, careerProfile) {
  const distance = career.baseDistance || 2;
  const reskilling = career.reskillingLevel || 2;
  const avg = (distance + reskilling) / 2;
  if (avg <= 1) return 'Easy';
  if (avg <= 2) return 'Moderate';
  if (avg <= 3.5) return 'Challenging';
  return 'Difficult';
}

export function calculateTransferableSkills(career, careerProfile, preQuizAnswers = {}) {
  const userSkills = (careerProfile?.currentSkills || []).map(s => (typeof s === 'string' ? s : s.name || '').toLowerCase());
  const found = new Set();

  // Check for skill overlaps with manual profile skills
  for (const skill of userSkills) {
    if (!skill) continue;
    for (const req of (career.requiredSkills || [])) {
      if (req.toLowerCase().includes(skill) || skill.includes(req.toLowerCase())) {
        found.add(req);
      }
    }
    for (const pref of (career.preferredSkills || [])) {
      if (pref.toLowerCase().includes(skill) || skill.includes(pref.toLowerCase())) {
        found.add(pref);
      }
    }
  }

  // Add generic transferable skills based on domain
  const domain = detectCurrentDomain(careerProfile);
  const domainTransferables = {
    civil_engineering: ['Technical documentation', 'Site management', 'Cost estimation', 'Project coordination', 'Analytical thinking'],
    mechanical_engineering: ['Process engineering', 'Technical documentation', 'Quality management', 'Manufacturing knowledge', 'Analytical problem solving'],
    electrical_engineering: ['System analysis', 'Troubleshooting', 'Technical design', 'Data analysis', 'Quality assurance'],
    computer_science: ['Problem solving', 'Logical thinking', 'Technical knowledge', 'System design', 'Analytical reasoning'],
    medicine: ['Domain expertise', 'Evidence-based decision making', 'Research', 'Patient & stakeholder communication', 'Critical thinking'],
    pharmacy: ['Regulatory knowledge', 'Scientific research', 'Quality control', 'Analytical thinking', 'Compliance'],
    business: ['Business acumen', 'Communication', 'Stakeholder management', 'Financial literacy', 'Strategic planning'],
    finance: ['Financial analysis', 'Excel & quantitative modeling', 'Analytical skills', 'Attention to detail', 'Reporting'],
    law: ['Legal research', 'Documentation', 'Compliance & ethics', 'Negotiation', 'Analytical writing'],
    teaching: ['Communication & presentation', 'Mentoring & training', 'Curriculum planning', 'People management', 'Empathy'],
    marketing: ['Market research', 'Campaign strategy', 'Data storytelling', 'Communication', 'Brand awareness'],
    science: ['Research methodology', 'Data analysis', 'Hypothesis testing', 'Technical reporting', 'Attention to detail'],
    design: ['Visual communication', 'User-centric thinking', 'Creative ideation', 'Prototyping', 'Design systems'],
  };

  const domainSkills = domainTransferables[domain] || ['Problem solving', 'Communication', 'Analytical skills', 'Project coordination'];
  for (const ds of domainSkills) {
    found.add(ds);
  }

  // Also include skills chosen in pre-assessment Q3 ("Which parts of your current experience would you like to carry into your next career?")
  const carryForward = preQuizAnswers?.[3] || preQuizAnswers?.['3'] || [];
  if (Array.isArray(carryForward)) {
    for (const skill of carryForward) {
      if (skill && skill !== 'Other') {
        found.add(skill);
      }
    }
  }

  return Array.from(found).slice(0, 6);
}

export function calculateMissingSkills(career, careerProfile) {
  const userSkills = (careerProfile?.currentSkills || []).map(s => s.name.toLowerCase());
  const missing = [];

  for (const req of (career.requiredSkills || [])) {
    const hasIt = userSkills.some(s => req.toLowerCase().includes(s) || s.includes(req.toLowerCase()));
    if (!hasIt) {
      missing.push({ skill: req, category: 'Required' });
    }
  }

  // Also add template missing skills
  for (const ms of (career.missingSkillsTemplate || [])) {
    const alreadyAdded = missing.some(m => m.skill === ms);
    const hasIt = userSkills.some(s => ms.toLowerCase().includes(s) || s.includes(ms.toLowerCase()));
    if (!alreadyAdded && !hasIt) {
      missing.push({ skill: ms, category: 'Key Gap' });
    }
  }

  return missing.slice(0, 6); // limit to top 6
}

export function generateMatchReasons(career, dimensionScores, preQuizAnswers, careerProfile) {
  const reasons = new Set();

  const dims = career.dimensions || {};
  const topDim = Object.entries(dims).sort((a, b) => b[1] - a[1])[0];

  const dimLabels = {
    projectManagement: 'project coordination and management',
    leadership: 'leadership and team management',
    operations: 'operational thinking',
    technicalDepth: 'technical depth',
    dataAnalytics: 'data analysis',
    creativity: 'creative problem solving',
    research: 'research orientation',
    peopleOrientation: 'people and stakeholder engagement',
    businessStrategy: 'business strategy',
    finance: 'financial analysis',
    healthcare: 'healthcare domain knowledge',
    codingIntensity: 'coding and technical implementation',
  };

  if (topDim && dimensionScores[topDim[0]] > 5) {
    reasons.add(`Strong alignment with ${dimLabels[topDim[0]]}`);
  }

  const distance = career.baseDistance || 2;
  if (distance <= 2) {
    reasons.add('Your existing background transfers directly into this field');
  }

  const constraints = careerProfile?.constraints || [];
  if (constraints.length > 0) {
    const avoidCoding = constraints.includes('Heavy coding');
    const careerCoding = dims.codingIntensity || 0;
    if (avoidCoding && careerCoding <= 2) {
      reasons.add('Matches your preference for low-code work');
    }
  }

  const interests = preQuizAnswers?.[5] || [];
  const whyChange = preQuizAnswers?.[1] || [];
  if (whyChange.includes('Better salary') && (career.salaryRange || '').includes('LPA')) {
    reasons.add('Offers competitive salary potential relative to your current field');
  }
  if (whyChange.includes('Career growth')) {
    reasons.add('Strong growth trajectory with clear advancement paths');
  }
  if (whyChange.includes('Better work-life balance')) {
    const envs = career.workEnvironments || [];
    if (envs.includes('Remote') || envs.includes('Hybrid')) {
      reasons.add('Supports remote or hybrid work enabling better work-life balance');
    }
  }

  const keepSkills = preQuizAnswers?.[3] || [];
  if (keepSkills.includes('Technical knowledge') && (dims.technicalDepth || 0) >= 4) {
    reasons.add('Allows you to continue leveraging your technical knowledge');
  }
  if (keepSkills.includes('Project management') && (dims.projectManagement || 0) >= 4) {
    reasons.add('Your project management experience directly applies');
  }
  if (keepSkills.includes('Leadership') && (dims.leadership || 0) >= 5) {
    reasons.add('Utilizes your existing leadership capabilities');
  }

  // Limit to 5–6 unique reasons
  return [...reasons].slice(0, 6);
}

export function generateRoadmap(career, careerProfile) {
  const missing = calculateMissingSkills(career, careerProfile);
  const certifications = career.certifications || [];

  const phases = [
    {
      phase: 'PHASE 1 — FOUNDATION',
      weeks: 'Weeks 1–4',
      items: missing.slice(0, 2).map(m => `Learn ${m.skill}`).concat(['Understand the industry landscape']),
    },
    {
      phase: 'PHASE 2 — SKILL DEVELOPMENT',
      weeks: 'Weeks 5–10',
      items: missing.slice(2, 4).map(m => `Build ${m.skill}`).concat(['Start building your portfolio']),
    },
    {
      phase: 'PHASE 3 — PRACTICAL EXPERIENCE',
      weeks: 'Weeks 11–16',
      items: ['Complete 2 real-world projects', 'Contribute to open or community projects', 'Build your case studies'],
    },
    {
      phase: 'PHASE 4 — CREDENTIAL',
      weeks: 'Weeks 17–20',
      items: certifications.slice(0, 2).map(c => `Complete: ${c}`).concat(['Update LinkedIn and resume']),
    },
    {
      phase: 'PHASE 5 — TRANSITION',
      weeks: 'Weeks 20+',
      items: (career.entryRoles || []).map(r => `Apply for: ${r}`).concat(['Network in the target field']),
    },
  ];

  return phases;
}
