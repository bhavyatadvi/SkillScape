import { internalCourseCatalog } from '@/data/upskillingCatalog';
import { learningPathTemplates } from '@/data/learningPaths';

const SAVED_KEY = 'skillscape-saved-learning-items';
const PROGRESS_KEY = 'skillscape-course-progress';
const safeRead = (key, fallback) => { try { return JSON.parse(localStorage.getItem(key) || JSON.stringify(fallback)); } catch { return fallback; } };
const normalise = course => ({ ...course, courseSkills: course.courseSkills || (course.skills || []).map(skillName => ({ skillName })), courseCareers: course.courseCareers || (course.careerRelevance || []).map(careerTitle => ({ careerTitle })) });
const words = value => String(value || '').toLowerCase().split(/[^a-z0-9+#]+/).filter(Boolean);
const getProfileContext = () => {
  let activePlan = null;
  let activeGovPlan = null;
  try {
    for (let i = 0; i < window.localStorage.length; i++) {
      const key = window.localStorage.key(i);
      if (key.startsWith('skillscape-quiz-learning-plans:')) {
        const plans = safeRead(key, []);
        if (plans && plans.length > 0) activePlan = plans[0];
      }
      if (key.startsWith('skillscape-government-learning:')) {
        const gov = safeRead(key, null);
        if (gov?.selectedCareer) activeGovPlan = gov;
      }
    }
  } catch {}

  const profile = safeRead('skillscape-career-profile', {});
  const switchResult = safeRead('skillscape-career-switch-assessment', {});
  const quizResult = safeRead('skillscape-learning-quiz', {});
  
  const targetCareer = activePlan?.selectedCareer?.title || activeGovPlan?.selectedCareer?.title || switchResult.selectedCareer?.title || switchResult.rankedCareers?.[0]?.title || quizResult.selectedCareer?.title || quizResult.result?.primaryCareer || profile.targetCareer || '';
  
  const currentSkills = (profile.currentSkills || []).map(item => typeof item === 'string' ? item : item.name || item.skillName).filter(Boolean);
  
  const planGaps = activePlan?.selectedCareer?.skillGaps || activeGovPlan?.selectedCareer?.skillGaps || [];
  const skillGaps = [...planGaps, ...(switchResult.selectedCareer?.skillGaps || []), ...(quizResult.result?.skillGaps || [])].map(item => typeof item === 'string' ? item : item.name || item.skillName).filter(Boolean);
  
  return { profile, targetCareer, currentSkills, skillGaps, skills: [...currentSkills, ...skillGaps] };
};
import { courseApi } from '@/services/api/courses';

export const matchCourse = (course, query, filters) => {
  const haystack = [course.title, course.provider?.name, course.domain, ...(course.courseSkills || []).map(s => s.skillName), ...(course.courseCareers || []).map(c => c.careerTitle)].join(' ').toLowerCase();
  const queryMatches = !query || words(query).some(term => haystack.includes(term));
  return queryMatches && (!filters.domain || course.domain === filters.domain) && (!filters.level || course.level === filters.level) && (!filters.duration || String(course.durationUnit || '').toLowerCase().includes(filters.duration.toLowerCase())) && (!filters.isFree || course.isFree) && (!filters.type || course.certificateAvailable);
};
export const courseRepository = {
  async discover(filters = {}) { return internalCourseCatalog.filter(course => matchCourse(course, filters.query, filters)).map(normalise); },
  saved() { const ids = safeRead(SAVED_KEY, []); return internalCourseCatalog.filter(course => ids.includes(course.id)).map(normalise); },
  completed() { const progress = safeRead(PROGRESS_KEY, {}); const completedIds = Object.keys(progress).filter(id => progress[id].status === 'completed'); return internalCourseCatalog.filter(c => completedIds.includes(c.id)).map(normalise); },
  savedIds() { return new Set(safeRead(SAVED_KEY, [])); },
  toggleSaved(id) { const ids = new Set(safeRead(SAVED_KEY, [])); ids.has(id) ? ids.delete(id) : ids.add(id); localStorage.setItem(SAVED_KEY, JSON.stringify([...ids])); return ids; },
  progress() { return safeRead(PROGRESS_KEY, {}); },
  setProgress(id, status) { const progress = safeRead(PROGRESS_KEY, {}); progress[id] = { status, progressPercentage: status === 'completed' ? 100 : status === 'in_progress' ? Math.max(progress[id]?.progressPercentage || 0, 10) : 0, startedAt: status === 'in_progress' ? (progress[id]?.startedAt || new Date().toISOString()) : progress[id]?.startedAt, completedAt: status === 'completed' ? new Date().toISOString() : null }; localStorage.setItem(PROGRESS_KEY, JSON.stringify(progress)); return progress; },
  learningPaths() { const { targetCareer } = getProfileContext(); return learningPathTemplates.filter(path => !targetCareer || path.match.some(term => targetCareer.toLowerCase().includes(term))).map(path => ({ ...path, courses: path.courseIds.map(id => internalCourseCatalog.find(course => course.id === id)).filter(Boolean).map(normalise) })); },
  certifications() { return internalCourseCatalog.filter(course => course.certificateAvailable).map(course => ({ ...normalise(course), certificationLabel: course.provider?.providerType === 'Professional' ? 'Professional / industry certificate' : 'Certificate of completion' })); },
  context: getProfileContext,
};
