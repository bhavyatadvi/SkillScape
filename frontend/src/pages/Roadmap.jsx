import React, { useState, useEffect, useRef, startTransition } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui/button';
import {
  BookOpen,
  Code,
  Server,
  Database,
  Cloud,
  Palette,
  ExternalLink,
  Loader2,
  CheckCircle2,
  Circle,
  Play,
  Map,
  HelpCircle,
  Award,
  Sparkles,
  Layers,
  ChevronDown,
  ChevronUp,
  Bookmark,
  RotateCcw,
  Trophy,
  Target,
  ArrowRightLeft,
  Brain
} from 'lucide-react';
import { roadmapApi } from '@/services/api';
import { toast } from 'sonner';
import PageHeader from '@/components/layout/PageHeader';
import { useAuth } from '@/context/AuthContext';
import { removeCareerFromUserStorage, userStorageKey } from '@/lib/userStorage';
import RoadmapModuleQuiz from '@/components/roadmap/RoadmapModuleQuiz';
import MyRoadmapsSidebar from '@/components/roadmap/MyRoadmapsSidebar';
import AskAIRoadmapBuilder from '@/components/roadmap/AskAIRoadmapBuilder';
import RoadmapVisualHeader from '@/components/roadmap/RoadmapVisualHeader';

const domainIcons = {
  frontend: Palette,
  backend: Server,
  fullstack: Code,
  data: Database,
  devops: Cloud,
  ml: BookOpen,
};

const getDomainIcon = (domainId) => {
  return domainIcons[domainId] || Code;
};

const getLevelStyle = (level) => {
  switch (level?.toLowerCase()) {
    case 'beginner':
      return 'bg-emerald-50 text-emerald-700 border-emerald-200';
    case 'intermediate':
      return 'bg-blue-50 text-blue-700 border-blue-200';
    case 'advanced':
      return 'bg-indigo-50 text-indigo-700 border-indigo-200';
    case 'prolevel':
    case 'job-ready':
      return 'bg-purple-50 text-purple-700 border-purple-200';
    default:
      return 'bg-slate-50 text-slate-700 border-slate-200';
  }
};

const normalizeKey = (val) => {
  if (!val) return '';
  return String(val)
    .toLowerCase()
    .replace(/^career[-_]?/, '')
    .replace(/[^a-z0-9]/g, '');
};

const isCareerMatch = (item, careerObj) => {
  if (!item || !careerObj) return false;
  const itemKeyNorm = normalizeKey(item.careerKey || item.domainId || item.id);
  const itemTitleNorm = normalizeKey(item.careerTitle || item.domainName || item.title);

  const careerKeyNorm = normalizeKey(careerObj.id || careerObj.careerKey || careerObj.domainId);
  const careerTitleNorm = normalizeKey(careerObj.title || careerObj.careerTitle || careerObj.name);

  return Boolean(
    (careerKeyNorm && (itemKeyNorm === careerKeyNorm || itemTitleNorm === careerKeyNorm)) ||
    (careerTitleNorm && (itemKeyNorm === careerTitleNorm || itemTitleNorm === careerTitleNorm))
  );
};

const RoadmapPage = () => {
  const { user } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const [roadmapData, setRoadmapData] = useState(null);
  const [roadmapOptions, setRoadmapOptions] = useState([]);
  const [savedRoadmaps, setSavedRoadmaps] = useState([]);
  const [selectedDomainId, setSelectedDomainId] = useState('');
  const [loading, setLoading] = useState(true);
  const [switchingRoadmap, setSwitchingRoadmap] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  // Active quiz modal state
  const [activeQuizModule, setActiveQuizModule] = useState(null);

  // Ask AI section state
  const [showAskAI, setShowAskAI] = useState(false);

  // Skill Analyzer & Career Switch state
  const [careerSwitchPath, setCareerSwitchPath] = useState(null);
  const [skillAnalyzerPlan, setSkillAnalyzerPlan] = useState(null);
  const [quizLearningPlans, setQuizLearningPlans] = useState([]);
  const [showSkillAnalyzerRoadmap, setShowSkillAnalyzerRoadmap] = useState(false);
  const roadmapRequestRef = useRef(0);
  // Tracks navigations triggered internally (e.g. clicking a saved roadmap)
  // so the location.search useEffect does not re-run initializeRoadmap.
  const internalNavRef = useRef(false);
  const selectingRoadmapRef = useRef(false);

  const fetchRoadmapByDomain = async (domainId) => {
    if (!domainId) {
      setRoadmapData(null);
      return;
    }

    const requestId = ++roadmapRequestRef.current;
    // Optimistically mark this domain as selected immediately so the button
    // highlights right away — no waiting for the API, no flicker.
    setSelectedDomainId(domainId);
    setSwitchingRoadmap(true);

    try {
      const res = await roadmapApi.getRoadmapByDomain(domainId);
      if (requestId !== roadmapRequestRef.current) return;
      const payload = res.data?.data;
      // Batch both state updates in one render via startTransition
      startTransition(() => {
        setRoadmapData(payload);
        setSelectedDomainId(payload?.domainId || domainId);
        setSwitchingRoadmap(false);
      });
    } catch (err) {
      if (requestId !== roadmapRequestRef.current) return;
      console.error('Error fetching roadmap:', err);
      startTransition(() => setSwitchingRoadmap(false));
      toast.error(err.message || 'Failed to load roadmap details');
    }
  };

  const fetchSavedRoadmaps = async () => {
    try {
      const res = await roadmapApi.getMyRoadmaps();
      const roadmaps = res.data?.data || [];
      setSavedRoadmaps(roadmaps);
      return roadmaps;
    } catch (err) {
      console.error('Failed to fetch saved roadmaps:', err);
      return [];
    }
  };

  const initializeRoadmap = async () => {
    try {
      setLoading(true);

      const optionsRes = await roadmapApi.getRoadmapOptions();
      const options = optionsRes.data?.data || [];
      setRoadmapOptions(options);

      const saved = await fetchSavedRoadmaps();

      const queryDomain = new URLSearchParams(location.search).get('domain');

      // If there's a domain in the URL that isn't saved yet, save it automatically.
      // This handles navigation from Skill Analyzer / Career Switch pages where
      // the roadmap may have been saved via a different API call that hasn't
      // been reflected yet, or where the save failed silently.
      if (queryDomain) {
        const normalizedQuery = queryDomain.toLowerCase().replace(/[\s_]+/g, '-');
        const alreadySaved = saved.some(s =>
          s.careerKey === normalizedQuery ||
          s.careerKey === queryDomain ||
          s.careerKey?.replace(/_/g, '-') === normalizedQuery
        );
        if (!alreadySaved) {
          // Try to get a proper title from localStorage (set by SkillAnalyzer / CareerSwitch)
          let autoTitle = null;
          try {
            const analyzerPlan = JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-skill-analyzer-roadmap', user)) || 'null');
            const analyzerKey = (analyzerPlan?.selectedCareer?.id || '').toLowerCase().replace(/[\s_]+/g, '-');
            if (analyzerKey === normalizedQuery || analyzerKey === queryDomain) {
              autoTitle = analyzerPlan?.selectedCareer?.title || null;
            }
          } catch {}
          try {
            const switchPlan = JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-career-switch-assessment', user)) || 'null');
            if (!autoTitle && switchPlan?.selectedCareer) {
              const switchKey = (switchPlan.selectedCareer.id || switchPlan.selectedCareer.title || '').toLowerCase().replace(/[\s_]+/g, '-');
              if (switchKey === normalizedQuery || switchKey === queryDomain) {
                autoTitle = switchPlan.selectedCareer.title || null;
              }
            }
          } catch {}
          try {
            await roadmapApi.saveMyRoadmap(normalizedQuery, autoTitle);
          } catch (saveErr) {
            console.warn('Auto-save roadmap failed:', saveErr);
          }
        }
      }

      const freshSaved = await fetchSavedRoadmaps();
      const defaultSaved = freshSaved.find(s => s.isDefault) || freshSaved[0];
      const recommendedDomainId = queryDomain || defaultSaved?.careerKey || options.find(o => o.isRecommended)?.domainId;
      const initialDomainId = recommendedDomainId || options[0]?.domainId || '';

      if (initialDomainId) {
        await fetchRoadmapByDomain(initialDomainId);
        await fetchSavedRoadmaps();
      } else {
        setRoadmapData(null);
      }
    } catch (err) {
      console.error('Failed to fetch roadmap:', err);
      setError(err.message || 'Failed to load roadmap.');
      toast.error(err.message || 'Failed to load roadmap.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // If the navigation was triggered internally (e.g., clicking a saved roadmap),
    // skip full reinitialization to prevent the blinking/flickering caused by
    // multiple rapid setState calls from initializeRoadmap.
    if (internalNavRef.current) {
      internalNavRef.current = false;
      return;
    }
    initializeRoadmap();
    try {
      const saved = JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-career-switch-assessment', user)) || '{}');
      if (saved.selectedCareer && saved.selectedRoadmap) setCareerSwitchPath(saved);
    } catch {}
    try {
      const savedAnalyzerPlan = JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-skill-analyzer-roadmap', user)) || 'null');
      if (savedAnalyzerPlan?.selectedCareer && Array.isArray(savedAnalyzerPlan.result?.skillGapRoadmap)) setSkillAnalyzerPlan(savedAnalyzerPlan);
    } catch {}
    try {
      const storedQuizPlans = JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-quiz-learning-plans', user)) || '[]');
      if (Array.isArray(storedQuizPlans)) setQuizLearningPlans(storedQuizPlans);
    } catch {}
  }, [user?.id, user?.userId, user?.email, location.search]);

  const handleDomainChange = async (event) => {
    const domainId = event.target.value;
    try {
      await fetchRoadmapByDomain(domainId);
      internalNavRef.current = true;
      navigate(`/roadmap?domain=${encodeURIComponent(domainId)}`, { replace: true });
    } catch (err) {
      internalNavRef.current = false;
      toast.error(err.message || 'Failed to switch roadmap');
    }
  };

  const handleSelectSavedRoadmap = async (careerKey) => {
    // Prevent concurrent/rapid-fire clicks from triggering multiple fetches
    if (selectingRoadmapRef.current) return;
    selectingRoadmapRef.current = true;
    try {
      await fetchRoadmapByDomain(careerKey);
      // Mark navigation as internal so the location.search useEffect skips
      // re-running initializeRoadmap (which would cause blinking).
      internalNavRef.current = true;
      navigate(`/roadmap?domain=${encodeURIComponent(careerKey)}`, { replace: true });
    } catch (err) {
      internalNavRef.current = false;
      toast.error(err.message || 'Failed to load roadmap');
    } finally {
      selectingRoadmapRef.current = false;
    }
  };

  const handleSaveRoadmap = async (careerKey, title) => {
    try {
      await roadmapApi.saveMyRoadmap(careerKey, title);
      // fetchRoadmapByDomain sets selectedDomainId optimistically — no need to set it separately
      await fetchRoadmapByDomain(careerKey);
      // Refresh the saved list after roadmap data is loaded to avoid extra re-renders
      await fetchSavedRoadmaps();
      internalNavRef.current = true;
      navigate(`/roadmap?domain=${encodeURIComponent(careerKey)}`, { replace: true });
      toast.success('Roadmap saved to My Roadmaps');
    } catch (err) {
      internalNavRef.current = false;
      toast.error(err.message || 'Failed to save roadmap');
    }
  };

  const handleRemoveRoadmap = async (careerKey) => {
    try {
      await roadmapApi.removeMyRoadmap(careerKey);
      removeCareerFromUserStorage(careerKey, user);
      if (careerSwitchPath?.selectedCareer) {
        const switchKey = (careerSwitchPath.selectedCareer.id || careerSwitchPath.selectedCareer.title || '').toLowerCase().replace(/[\s_]+/g, '-');
        const normCareerKey = (careerKey || '').toLowerCase().replace(/[\s_]+/g, '-');
        if (switchKey === normCareerKey) {
          setCareerSwitchPath(null);
        }
      }
      const remainingRoadmaps = await fetchSavedRoadmaps();

      // Do not leave a deleted default/custom roadmap rendered as the active
      // curriculum. Switch to the next saved default, or clear the view.
      if (selectedDomainId === careerKey) {
        const nextRoadmap = remainingRoadmaps.find(roadmap => roadmap.isDefault) || remainingRoadmaps[0];
        if (nextRoadmap) {
          await fetchRoadmapByDomain(nextRoadmap.careerKey);
          internalNavRef.current = true;
          navigate(`/roadmap?domain=${encodeURIComponent(nextRoadmap.careerKey)}`, { replace: true });
        } else {
          setRoadmapData(null);
          setSelectedDomainId('');
          internalNavRef.current = true;
          navigate('/roadmap', { replace: true });
        }
      }

      toast.success('Roadmap removed');
    } catch (err) {
      internalNavRef.current = false;
      toast.error(err.message || 'Failed to remove roadmap');
    }
  };


  const handleSetDefaultRoadmap = async (careerKey) => {
    try {
      await roadmapApi.setDefaultMyRoadmap(careerKey);
      await fetchSavedRoadmaps();
      toast.success('Default roadmap updated');
    } catch (err) {
      toast.error(err.message || 'Failed to set default roadmap');
    }
  };

  const handleRegenerateRoadmap = async (careerKey) => {
    try {
      setLoading(true);
      const res = await roadmapApi.regenerateRoadmap(careerKey);
      setRoadmapData(res.data?.data);
      toast.success('Roadmap regenerated with fresh AI curriculum!');
    } catch (err) {
      toast.error(err.message || 'Failed to regenerate roadmap');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenQuiz = (module) => {
    setActiveQuizModule(module);
  };

  const handleQuizPassed = async () => {
    await fetchRoadmapByDomain(selectedDomainId);
    await fetchSavedRoadmaps();
  };

  const handleToggleAnalyzerPhaseRead = (phaseId) => {
    setSkillAnalyzerPlan(currentPlan => {
      if (!currentPlan) return currentPlan;
      const completedPhaseIds = new Set(currentPlan.completedPhaseIds || []);
      const isCompleted = completedPhaseIds.has(phaseId);
      if (isCompleted) completedPhaseIds.delete(phaseId);
      else completedPhaseIds.add(phaseId);

      const updatedPlan = { ...currentPlan, completedPhaseIds: [...completedPhaseIds] };
      window.localStorage.setItem(userStorageKey('skillscape-skill-analyzer-roadmap', user), JSON.stringify(updatedPlan));
      toast.success(isCompleted ? 'Phase marked as incomplete.' : 'Phase marked as complete.');
      return updatedPlan;
    });
  };

  const progressPercentage = roadmapData?.progressPercentage || 0;
  const phases = roadmapData?.phases || [];
  const legacySkills = roadmapData?.skills || [];
  const savedRoadmapKeys = new Set(savedRoadmaps.map(roadmap => roadmap.careerKey));
  const availableRoadmaps = roadmapOptions.filter(option => !savedRoadmapKeys.has(option.domainId));

  const currentRoadmapItem = {
    careerKey: roadmapData?.domainId,
    careerTitle: roadmapData?.domainName,
  };

  const isCurrentCareerSwitch = Boolean(
    isCareerMatch(currentRoadmapItem, careerSwitchPath?.selectedCareer) ||
    quizLearningPlans.some(p => p.assessmentType === 'Career Switch' && isCareerMatch(currentRoadmapItem, p.selectedCareer))
  );

  const isCurrentSkillAnalyzer = Boolean(
    isCareerMatch(currentRoadmapItem, skillAnalyzerPlan?.selectedCareer) ||
    quizLearningPlans.some(p => p.assessmentType === 'Skill Analyzer' && isCareerMatch(currentRoadmapItem, p.selectedCareer))
  );

  let totalModulesInView = 0;
  let completedModulesInView = 0;

  if (phases.length > 0) {
    phases.forEach(p => {
      p.modules?.forEach(m => {
        totalModulesInView++;
        if (m.isCompleted) completedModulesInView++;
      });
    });
  } else {
    legacySkills.forEach(s => {
      totalModulesInView += s.modules?.length || 0;
      completedModulesInView += s.modules?.filter(m => m.isCompleted).length || 0;
    });
  }

  const DomainIcon = roadmapData ? getDomainIcon(roadmapData.domainId) : Code;
  const isFullyCompleted = totalModulesInView > 0 && completedModulesInView >= totalModulesInView;

  if (loading && !roadmapData) {
    return (
      <div className="flex items-center justify-center min-h-[60vh] bg-slate-50">
        <div className="text-center">
          <Loader2 className="h-8 w-8 animate-spin mx-auto text-blue-600" />
          <p className="mt-4 text-slate-500 font-medium">Loading your multi-phase curriculum roadmap...</p>
        </div>
      </div>
    );
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="w-full"
    >
      <div className="max-w-7xl mx-auto w-full space-y-8">

        {/* Top Header */}
        <PageHeader 
          icon={Map}
          title="Learning Roadmap & Curriculum"
          description="Multi-phase curriculum from beginner to job-ready level with graded module assessments."
        />



        {/* Ask AI Section Banner */}
        <AnimatePresence>
          {showAskAI && (
            <AskAIRoadmapBuilder
              onRoadmapGenerated={async (result) => {
                setShowAskAI(false);
                setRoadmapData(result);
                setSelectedDomainId(result.domainId);
                await fetchSavedRoadmaps();
                window.scrollTo({ top: 0, behavior: 'smooth' });
                toast.success(`Generated customized AI roadmap for ${result.domainName || 'your career goal'}! 🎉`);
              }}
              onClose={() => setShowAskAI(false)}
            />
          )}
        </AnimatePresence>

        {/* Main Grid: My Roadmaps Horizontal Selector + Main Curriculum View */}
        <div className="flex flex-col gap-8">
          
          {/* Top Horizontal Roadmap Selector */}
          <div className="w-full">
            <MyRoadmapsSidebar
              savedRoadmaps={savedRoadmaps}
              availableRoadmaps={availableRoadmaps}
              selectedDomainId={selectedDomainId}
              careerSwitchPath={careerSwitchPath}
              skillAnalyzerPlan={skillAnalyzerPlan}
              quizLearningPlans={quizLearningPlans}
              onSelectRoadmap={handleSelectSavedRoadmap}
              onSaveRoadmap={handleSaveRoadmap}
              onRemoveRoadmap={handleRemoveRoadmap}
              onSetDefaultRoadmap={handleSetDefaultRoadmap}
              onRegenerateRoadmap={handleRegenerateRoadmap}
              onOpenAskAI={() => setShowAskAI(true)}
            />
          </div>

          {/* Bottom Main Content */}
          <div className="w-full space-y-8 relative">

            {!roadmapData && !loading ? (
              <div className="bg-white p-12 rounded-2xl border border-slate-200 shadow-sm flex flex-col items-center justify-center text-center">
                <div className="h-20 w-20 bg-blue-50 text-blue-600 rounded-full flex items-center justify-center mb-6">
                  <Target className="w-10 h-10" />
                </div>
                <h2 className="text-2xl font-bold text-slate-900 mb-2">Discover Your Ideal Career Path</h2>
                <p className="text-slate-500 max-w-md mb-8">Take our AI-powered career assessment to find out which careers best match your skills, interests, and personality. We will build a customized learning roadmap for you.</p>
                <Button asChild size="lg" className="bg-blue-600 hover:bg-blue-700 text-white">
                  <Link to="/career-quiz">Take Career Assessment</Link>
                </Button>
              </div>
            ) : (
              <>
                {/* Target Career Bar */}
                <div className="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-6">
                  <div className="flex items-center gap-4">
                <div className="h-12 w-12 rounded-2xl bg-blue-50 text-blue-600 flex items-center justify-center shrink-0">
                  <DomainIcon className="w-6 h-6" />
                </div>
                <div>
                  <div className="flex items-center gap-2 flex-wrap">
                    <h2 className="text-xl font-bold text-slate-900">{roadmapData?.domainName || 'Select a Roadmap'}</h2>
                    {isCurrentCareerSwitch && (
                      <span
                        className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-indigo-50 text-indigo-700 border border-indigo-200"
                        title="Accepted career switch path"
                      >
                        <ArrowRightLeft className="w-3.5 h-3.5 text-indigo-600" />
                        <span>Accepted career switch path</span>
                      </span>
                    )}
                    {isCurrentSkillAnalyzer && (
                      <span
                        className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-purple-50 text-purple-700 border border-purple-200"
                        title="Saved skill analyzer roadmap"
                      >
                        <Brain className="w-3.5 h-3.5 text-purple-600" />
                        <span>Saved skill analyzer roadmap</span>
                      </span>
                    )}
                    {switchingRoadmap && <Loader2 className="w-4 h-4 animate-spin text-blue-600" aria-label="Loading selected roadmap" />}
                    {roadmapData && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleSaveRoadmap(roadmapData.domainId, roadmapData.domainName)}
                        className="text-slate-400 hover:text-blue-600 p-1 h-auto"
                        title="Bookmark roadmap"
                      >
                        <Bookmark className="w-4 h-4" />
                      </Button>
                    )}
                  </div>
                  <p className="text-xs text-slate-500 font-medium mt-0.5">Comprehensive adaptive curriculum with module quizzes</p>
                  {switchingRoadmap && <p className="text-[11px] text-blue-600 mt-1">Loading the selected roadmap and its modules...</p>}
                  {roadmapData && (
                    <p className="text-[11px] text-blue-600 mt-1">
                      Web-reviewed {roadmapData.lastWebCheckedAt ? new Date(roadmapData.lastWebCheckedAt).toLocaleDateString() : 'recently'}
                      {roadmapData.hasAdaptiveUpdates ? ' · curriculum updated from new industry knowledge' : ''}
                    </p>
                  )}
                </div>
              </div>

              <div className="flex items-center gap-6 border-t md:border-t-0 md:border-l border-slate-200 pt-4 md:pt-0 md:pl-6">
                {isFullyCompleted ? (
                  <div className="flex flex-col items-center gap-1">
                    <div className="text-3xl font-black text-emerald-600">100%</div>
                    <div className="flex items-center gap-1 text-xs font-bold text-emerald-700 bg-emerald-100 border border-emerald-200 px-2.5 py-1 rounded-full">
                      <CheckCircle2 className="w-3.5 h-3.5" /> Status: Completed
                    </div>
                  </div>
                ) : (
                  <div>
                    <div className="text-3xl font-black text-slate-900">{Math.round(progressPercentage)}%</div>
                    <div className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Curriculum Progress</div>
                  </div>
                )}
                <div className="text-xs font-medium text-slate-500">
                  <span className="font-bold text-slate-900">{completedModulesInView}</span> of {totalModulesInView} Modules Passed
                </div>
              </div>
            </div>

            {/* 100% Completion Celebration Banner */}
            {isFullyCompleted && (
              <div className="flex items-center gap-4 bg-emerald-50 border border-emerald-200 rounded-2xl px-6 py-4 shadow-sm">
                <div className="h-10 w-10 rounded-xl bg-emerald-100 flex items-center justify-center shrink-0">
                  <Trophy className="w-5 h-5 text-emerald-600" />
                </div>
                <div>
                  <p className="font-bold text-emerald-900 text-base">🎉 Curriculum Mastered!</p>
                  <p className="text-xs text-emerald-700 mt-0.5">You've completed all modules in this roadmap. Outstanding achievement!</p>
                </div>
              </div>
            )}
            {/* Switching roadmap overlay */}
            {switchingRoadmap && roadmapData && (
              <div className="absolute inset-x-0 top-28 z-10 flex justify-center pointer-events-none">
                <div className="inline-flex items-center gap-2 rounded-full border border-blue-200 bg-white/95 px-4 py-2 text-xs font-semibold text-blue-700 shadow-lg backdrop-blur-sm">
                  <Loader2 className="h-4 w-4 animate-spin" /> Updating curriculum...
                </div>
              </div>
            )}
            {/* Visual Roadmap Path */}
            {!loading && phases.length > 0 && (
              <RoadmapVisualHeader 
                title={`${roadmapData?.domainName || 'Curriculum'} Roadmap`}
                description={roadmapData?.summary}
                phases={phases}
              />
            )}

            {/* Curriculum Timeline */}
            {loading ? (
              <div className="text-center py-20">
                <Loader2 className="h-8 w-8 animate-spin mx-auto text-blue-600" />
              </div>
            ) : phases.length > 0 ? (
              <div className="space-y-10">
                {phases.map((phase, pIdx) => (
                  <motion.div
                    key={phase.phaseId || pIdx}
                    id={`phase-section-${pIdx}`}
                    initial={{ opacity: 0, y: 15 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: pIdx * 0.1 }}
                    className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden"
                  >
                    {/* Phase Header */}
                    <div className="p-6 bg-slate-50/70 border-b border-slate-200 flex flex-col md:flex-row md:items-center justify-between gap-4">
                      <div className="space-y-1">
                        <div className="flex items-center gap-2">
                          <span className={`text-xs font-bold px-2.5 py-0.5 rounded-full border ${getLevelStyle(phase.level)}`}>
                            {phase.level}
                          </span>
                          <span className="text-xs font-semibold text-slate-500">Phase {phase.phaseNumber}</span>
                        </div>
                        <h3 className="text-lg font-bold text-slate-900">{phase.title}</h3>
                        {phase.summary && <p className="text-xs text-slate-600 max-w-2xl">{phase.summary}</p>}
                      </div>
                      {phase.estimatedDuration && (
                        <span className="text-xs font-bold text-slate-500 bg-white border border-slate-200 px-3 py-1.5 rounded-xl shrink-0">
                          {phase.estimatedDuration}
                        </span>
                      )}
                    </div>

                    {/* Phase Modules – numbered timeline style */}
                    <div className="px-6 pb-6 pt-2">
                      <div className="relative">
                        {/* Left vertical line */}
                        <div className="absolute left-5 top-6 bottom-6 w-0.5 bg-slate-100" />

                        <div className="space-y-4">
                          {phase.modules?.map((module, mIdx) => (
                            <div key={module.moduleId || mIdx} className="relative flex gap-5">
                              {/* Number circle */}
                              <div className={`relative z-10 flex-shrink-0 w-10 h-10 rounded-full border-2 flex items-center justify-center text-sm font-black shadow-sm ${
                                module.isCompleted
                                  ? 'bg-emerald-500 border-emerald-500 text-white'
                                  : 'bg-white border-slate-200 text-slate-700'
                              }`}>
                                {module.isCompleted
                                  ? <CheckCircle2 className="w-5 h-5" />
                                  : String(mIdx + 1).padStart(2, '0')
                                }
                              </div>

                              {/* Card */}
                              <div className={`flex-1 rounded-2xl border p-5 transition-all ${
                                module.isCompleted
                                  ? 'bg-emerald-50/40 border-emerald-200'
                                  : 'bg-white border-slate-200 hover:border-blue-300 shadow-sm'
                              }`}>
                                <div className="flex flex-col md:flex-row md:items-start justify-between gap-4 mb-3">
                                  <div className="flex-1 min-w-0">
                                    <h4 className="text-sm font-bold text-slate-900 leading-snug">{module.title}</h4>
                                    <p className="text-xs text-slate-500 leading-relaxed mt-1 max-w-2xl">{module.description}</p>
                                  </div>
                                  <div className="flex items-center gap-2 shrink-0">
                                    <span className="text-xs font-semibold text-slate-400">
                                      ~{module.estimatedHours}h
                                    </span>
                                    <Button
                                      onClick={() => handleOpenQuiz(module)}
                                      variant={module.isCompleted ? 'outline' : 'default'}
                                      size="sm"
                                      className={module.isCompleted
                                        ? 'border-emerald-200 text-emerald-700 hover:bg-emerald-50 text-xs h-8'
                                        : 'bg-blue-600 hover:bg-blue-700 text-white text-xs h-8'
                                      }
                                    >
                                      {module.isCompleted ? (
                                        <><CheckCircle2 className="w-3 h-3 mr-1" /> Retake</>
                                      ) : (
                                        <><HelpCircle className="w-3 h-3 mr-1" /> Take Quiz</>
                                      )}
                                    </Button>
                                  </div>
                                </div>

                                {/* Subtopics */}
                                {module.topics && module.topics.length > 0 && (
                                  <div className="pt-3 border-t border-slate-100 space-y-1.5">
                                    {module.topics.map((topic, tIdx) => (
                                      <div key={topic.topicId || tIdx} className="flex items-center justify-between text-xs">
                                        <div className="flex items-center gap-2 truncate">
                                          <Play className="w-3 h-3 text-blue-400 shrink-0" />
                                          <span className="text-slate-600 truncate">{topic.title}</span>
                                          {topic.isOptional && <span className="text-[10px] text-slate-400">(Optional)</span>}
                                        </div>
                                        {topic.resourceUrl && (
                                          <a href={topic.resourceUrl} target="_blank" rel="noopener noreferrer" className="text-slate-400 hover:text-blue-600 ml-2 shrink-0">
                                            <ExternalLink className="w-3 h-3" />
                                          </a>
                                        )}
                                      </div>
                                    ))}
                                  </div>
                                )}
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  </motion.div>
                ))}
              </div>
            ) : legacySkills.length > 0 ? (
              /* Legacy Fallback */
              <div className="space-y-6">
                {legacySkills.map((skill) => (
                  <div key={skill.skillId} className="bg-white p-6 rounded-2xl border border-slate-200">
                    <h3 className="font-bold text-slate-900 text-lg mb-4">{skill.skillName}</h3>
                    <div className="space-y-3">
                      {skill.modules?.map((mod) => (
                        <div key={mod.moduleId} className="p-4 rounded-xl border border-slate-200 flex items-center justify-between">
                          <div>
                            <h4 className="font-bold text-sm text-slate-900">{mod.title}</h4>
                            <p className="text-xs text-slate-500">{mod.description}</p>
                          </div>
                          <Button size="sm" onClick={() => handleOpenQuiz(mod)}>Take Quiz</Button>
                        </div>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-16 bg-white rounded-2xl border border-slate-200">
                <p className="text-slate-500 text-sm">No curriculum modules found for this career.</p>
              </div>
            )}
            </>
            )}
          </div>

        </div>
      </div>

      {/* Module Quiz Modal */}
      <RoadmapModuleQuiz
        moduleId={activeQuizModule?.moduleId}
        moduleTitle={activeQuizModule?.title}
        isOpen={!!activeQuizModule}
        onClose={() => setActiveQuizModule(null)}
        onQuizPassed={handleQuizPassed}
      />
    </motion.div>
  );
};

export default RoadmapPage;
