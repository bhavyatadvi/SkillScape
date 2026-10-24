// Browser-only learning data must be isolated by account.  We intentionally
// do not read the older unscoped keys: those values may belong to somebody
// who used this browser before the current learner signed in.
export const userStorageKey = (name, user) => {
  const owner = user?.id || user?.userId || user?.email || 'anonymous';
  return `${name}:${encodeURIComponent(String(owner))}`;
};

export const readUserStorage = (name, user, fallback = null) => {
  try {
    const stored = window.localStorage.getItem(userStorageKey(name, user));
    return stored ? JSON.parse(stored) : fallback;
  } catch {
    return fallback;
  }
};

export const writeUserStorage = (name, user, value) => {
  window.localStorage.setItem(userStorageKey(name, user), JSON.stringify(value));
};

export const clearUserAnalysisStorage = (user) => {
  const keysToClear = [
    'skillscape-skill-analyzer-analysis',
    'skillscape-skill-analyzer-roadmap',
    'skillscape-career-switch-assessment',
    'skillscape-career-switch-answers',
    'skillscape-quiz-learning-plans'
  ];
  keysToClear.forEach(key => {
    try {
      window.localStorage.removeItem(userStorageKey(key, user));
    } catch {}
  });
};

const normalizeCareerValue = (value) => String(value || '')
  .toLowerCase()
  .replace(/^government-/, '')
  .replace(/^custom-/, '')
  .replace(/[^a-z0-9]+/g, '');

const careerTokens = (value) => String(value || '')
  .toLowerCase()
  .replace(/^(government|custom)[-_]/, '')
  .split(/[^a-z0-9]+/)
  .filter(Boolean)
  .map(token => token.length > 5 && token.endsWith('ing') ? token.slice(0, -3) : token);

const careerMatches = (career, careerKey) => {
  if (!career) return false;
  const target = normalizeCareerValue(careerKey);
  return [career.id, career.careerId, career.nodeId, career.title, career.name, career.careerName]
    .filter(Boolean)
    .some(value => {
      const normalized = normalizeCareerValue(value);
      if (normalized === target) return true;
      const targetWords = careerTokens(careerKey);
      const valueWords = careerTokens(value);
      return targetWords.length > 0
        && targetWords.every(word => valueWords.includes(word));
    });
};

/**
 * Remove a deleted roadmap career from browser-backed dashboard snapshots.
 * Other users and other saved careers remain untouched.
 */
export const removeCareerFromUserStorage = (careerKey, user) => {
  const plansKey = userStorageKey('skillscape-quiz-learning-plans', user);

  try {
    const plans = JSON.parse(window.localStorage.getItem(plansKey) || '[]');
    if (Array.isArray(plans)) {
      window.localStorage.setItem(
        plansKey,
        JSON.stringify(plans.filter(plan => !careerMatches(plan?.selectedCareer, careerKey)))
      );
    }
  } catch {}

  const snapshotKeys = [
    'skillscape-government-learning',
    'skillscape-career-switch-assessment',
    'skillscape-skill-analyzer-roadmap',
  ];

  snapshotKeys.forEach(name => {
    const key = userStorageKey(name, user);
    try {
      const snapshot = JSON.parse(window.localStorage.getItem(key) || 'null');
      if (snapshot?.selectedCareer && careerMatches(snapshot.selectedCareer, careerKey)) {
        window.localStorage.removeItem(key);
      }
    } catch {}
  });
};
