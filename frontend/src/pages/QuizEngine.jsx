import React, { useState, useEffect, useMemo } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Sparkles, CheckCircle2, ChevronRight, HelpCircle, Loader2, ArrowRight, 
  FlaskConical, Landmark, Palette, GraduationCap, Briefcase, RefreshCw, 
  Trophy, BookOpen, AlertCircle, Bookmark, Star, Wallet, Compass,
  TrendingUp, ShieldAlert, Award, School, Target, Zap, LayoutGrid, BarChart3, 
  LineChart as LineChartIcon, ThumbsUp, HelpCircle as HelpIcon, ChevronDown, ChevronUp,
  BrainCircuit, ArrowLeft, ListChecks
} from 'lucide-react';
import { ResponsiveContainer, AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, Legend } from 'recharts';
import { careerGuidanceApi } from '@/services/api/careerGuidance';
import { roadmapApi } from '@/services/api/roadmap';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import confetti from 'canvas-confetti';
import PageHeader from '@/components/layout/PageHeader';
import { CareerStageSlider } from '@/components/quiz/CareerStageSlider';
import BackgroundForm from '@/components/quiz/BackgroundForm';
import { useAuth } from '@/context/AuthContext';
import { userStorageKey } from '@/lib/userStorage';
import { getCareerEducation } from '@/data/careerEducationMap';
import { formatCareerReferences, getCareerDisplayName } from '@/data/careerDisplayNames';
import QuizProcessingLoader from '@/components/quiz/QuizProcessingLoader';

const STAGE_METADATA = {
  After10th: { title: 'After 10th Grade', desc: 'Select streams, diplomas, or vocational training paths', color: 'from-blue-500 to-blue-600', icon: <BookOpen className="w-6 h-6" /> },
  After12th: { title: 'After 12th Grade', desc: 'Select undergraduate degrees, engineering, arts, or defence streams', color: 'from-blue-600 to-blue-600', icon: <GraduationCap className="w-6 h-6" /> },
  AfterGraduation: { title: 'After Graduation', desc: 'Select specialized fields, industry domains, and job role routes', color: 'bg-blue-600', icon: <Trophy className="w-6 h-6" /> },
  AfterPostGraduation: { title: 'After Post Graduation', desc: 'Navigate advanced academic research, PhD pathways, or professional practices', color: 'from-blue-600 to-blue-500', icon: <School className="w-6 h-6" /> },
  CareerSwitch: { title: 'Career Switch', desc: 'Transition your career into software development, product management, or design', color: 'from-amber-500 to-slate-600', icon: <RefreshCw className="w-6 h-6" /> },
  GovernmentJobs: { title: 'Government Jobs', desc: 'Target IAS, Civil Services, Banking PO, or Defence Officer commissions', color: 'from-red-500 to-slate-600', icon: <Landmark className="w-6 h-6" /> }
};

const STAGE_PROCESSING_MESSAGES = {
  After10th: [
    'Analyzing your academic interests…',
    'Evaluating stream compatibility (Science, Commerce, Arts)…',
    'Exploring vocational & diploma pathways…',
    'Matching foundational strengths…',
    'Building your stream & career recommendations…',
  ],
  After12th: [
    'Evaluating subject strengths & interests…',
    'Exploring undergraduate degrees & entrance exams…',
    'Comparing professional & technical disciplines…',
    'Analyzing career trajectories & market demand…',
    'Formulating your degree & career roadmap…',
  ],
  AfterGraduation: [
    'Analyzing your degree specialization & skills…',
    'Evaluating high-demand industry domains…',
    'Matching job roles & emerging tech careers…',
    'Benchmarking salary expectations & job readiness…',
    'Building your tailored career pathways…',
  ],
  AfterPostGraduation: [
    'Assessing advanced domain expertise & research interests…',
    'Evaluating leadership, specialized industry & PhD routes…',
    'Matching research bodies & executive opportunities…',
    'Assessing global career scope & compensation…',
    'Generating your advanced career recommendations…',
  ],
  CareerSwitch: [
    'Analyzing your profile…',
    'Comparing career paths…',
    'Mapping your transferable skills…',
    'Checking skill gaps…',
    'Building your transition roadmap…',
  ],
  GovernmentJobs: [
    'Analyzing your public service preferences…',
    'Matching central, state & PSU opportunities…',
    'Evaluating exam patterns (UPSC, SSC, Banking, Defence)…',
    'Benchmarking preparation timeline & competition fit…',
    'Generating your government career & exam roadmap…',
  ],
};

const getProcessingMessages = (stream) => {
  return STAGE_PROCESSING_MESSAGES[stream] || STAGE_PROCESSING_MESSAGES.CareerSwitch;
};

const formatINR = (amount) => {
  if (amount == null) return '';
  const num = Number(amount);
  if (isNaN(num)) return amount;
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0
  }).format(num);
};

const formatLPA = (amount) => {
  if (amount == null) return '';
  const num = Number(amount);
  if (isNaN(num)) return amount;
  return `₹${(num / 100000).toFixed(2)} LPA`;
};

export default function QuizEngine() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAuth();
  const [step, setStep] = useState('landing'); // landing, quiz, loading, result
  const [selectedStream, setSelectedStream] = useState('');
  const [selectedSubStream, setSelectedSubStream] = useState('');
  const [activeQuestions, setActiveQuestions] = useState([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [answers, setAnswers] = useState([]); // Array of { questionId, optionIndex }
  const [backgroundContext, setBackgroundContext] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [resultProfile, setResultProfile] = useState(null);
  const [bookmarked, setBookmarked] = useState(false);
  const [accepting, setAccepting] = useState(false);
  const [adaptiveSessionId, setAdaptiveSessionId] = useState('');
  const [quizProgress, setQuizProgress] = useState(0);
  const [isLoadingNext, setIsLoadingNext] = useState(false);
  const [fiveYearSalary, setFiveYearSalary] = useState(null);
  const [isSalaryLoading, setIsSalaryLoading] = useState(false);
  const [estimatedRemaining, setEstimatedRemaining] = useState(15);
  const [isAcceptingRoadmap, setIsAcceptingRoadmap] = useState(false);
  const [loadingStep, setLoadingStep] = useState(0);


  // Result view state
  const [activeResultTab, setActiveResultTab] = useState('overview');
  const [expandedMatchId, setExpandedMatchId] = useState(null);
  const [savedLearningPlanId, setSavedLearningPlanId] = useState('');
  const [selectedMatch, setSelectedMatch] = useState(null); // for other-matches modal
  const [isDark, setIsDark] = useState(document.documentElement.classList.contains('dark'));

  // Monitor theme changes on root html element
  useEffect(() => {
    const observer = new MutationObserver(() => {
      setIsDark(document.documentElement.classList.contains('dark'));
    });
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['class'] });
    return () => observer.disconnect();
  }, []);

  // My Learning Plans can reopen a saved result without re-running the quiz.
  // Keep the selected saved career first so an alternative plan is shown as
  // the plan the learner deliberately chose, not the original top match.
  useEffect(() => {
    const savedPlan = location.state?.savedPlan;
    if (!savedPlan?.selectedCareer) return;

    const savedCareer = savedPlan.selectedCareer;
    const savedTitle = savedCareer.title || savedCareer.careerName || savedCareer.name;
    if (!savedTitle) return;

    const savedScore = savedCareer.matchScore ?? savedCareer.score ?? savedPlan.result?.confidenceScore ?? 0;
    const savedComparison = {
      nodeId: savedCareer.id || savedCareer.nodeId || '',
      careerName: savedTitle,
      score: savedScore,
      expectedSalaryRange: savedCareer.expectedSalaryRange || '',
      expectedTimeToJobReady: savedCareer.expectedTimeToJobReady || '',
    };
    const priorComparison = savedPlan.result?.targetCareerComparison || [];

    setSelectedStream(savedPlan.assessmentType || 'Career Discovery');
    setResultProfile({
      ...savedPlan.result,
      resultStatus: 'Success',
      bestMatchName: savedTitle,
      recommendedCareer: { ...savedCareer, title: savedTitle },
      expectedSalaryRange: savedCareer.expectedSalaryRange || savedPlan.result?.expectedSalaryRange || '',
      expectedTimeToJobReady: savedCareer.expectedTimeToJobReady || savedPlan.result?.expectedTimeToJobReady || '',
      targetCareerComparison: [
        savedComparison,
        ...priorComparison.filter(item => (item.careerName || item.title) !== savedTitle),
      ],
    });
    setStep('result');
  }, [location.state]);

  const handleAcceptCareer = async (pathId, pathTitle = null) => {
    if (!pathId) return;
    try {
      setAccepting(true);
      const accepted = await careerGuidanceApi.finalizeDecision(adaptiveSessionId, pathId, pathTitle);
      const chosenKey = accepted?.chosenCareerId || pathId;
      const chosenTitle = accepted?.careerTitle || pathTitle || chosenKey;
      
      // Sync to backend roadmap
      try {
        await roadmapApi.acceptCareerRoadmap(chosenKey, chosenTitle);
      } catch (e) {
        console.warn('Backend accept roadmap failed:', e);
      }

      toast.success('Career selected! Your comprehensive roadmap is ready.');
      setResultProfile(prev => ({ ...prev, resultStatus: 'Success' }));
      navigate(`/roadmap?domain=${encodeURIComponent(chosenKey)}`);
    } catch (err) {
      console.error(err);
      toast.error('Failed to accept career path.');
    } finally {
      setAccepting(false);
    }
  };

  // Trigger quiz start
  const handleStartQuiz = async (stream) => {
    if (stream === 'GovernmentJobs') {
      navigate('/government-jobs');
      return;
    }
    if (stream === 'CareerSwitch') {
      navigate('/career-switch/pre-assessment');
      return;
    }

    setSelectedStream(stream);
    setIsLoading(true);
    
    // After10th does NOT need background context — skip straight to quiz
    if (stream === 'After10th') {
      setBackgroundContext({});
      await startAdaptiveSession(stream, {}, '');
      return;
    }

    // All other quizzes require background context first
    setStep('background_form');
  };

  const handleBackgroundComplete = async (contextData) => {
    setBackgroundContext(contextData);
    
    let targetStream = selectedStream;
    if (selectedStream === 'After12th') {
      const subStream = contextData.stream;
      if (!subStream) {
        toast.error('Stream is required for After 12th');
        return;
      }
      setSelectedSubStream(subStream);
      targetStream = subStream;
    }

    await startAdaptiveSession(selectedStream, contextData, selectedStream === 'After12th' ? targetStream : '');
  };

  const startAdaptiveSession = async (phaseKey, profile, subPhaseKey) => {
    setStep('initializing');
    setIsLoading(true);
    try {
      const data = await careerGuidanceApi.startAdaptiveQuiz(phaseKey, profile, subPhaseKey);
      const sessionId = data?.session?.sessionId;
      const firstQData = data?.firstQuestion || {};
      const firstQuestion = normalizeQuestion(firstQData.nextQuestion || firstQData.NextQuestion || firstQData.question || firstQData.Question || firstQData);
      if (!sessionId || !firstQuestion) throw new Error('The adaptive question engine returned no valid first question.');
      setAdaptiveSessionId(sessionId);
      setActiveQuestions([firstQuestion]);
      setCurrentQuestionIndex(0);
      setQuizProgress(firstQData.progress || 0);
      setEstimatedRemaining(firstQData.estimatedRemaining || 15);
      setAnswers([]);
      setStep('quiz');
    } catch (err) {
      console.error(err);
      toast.error('Failed to start the adaptive assessment.');
      setStep('landing');
    } finally {
      setIsLoading(false);
    }
  };

  // Option select
  const handleSelectOption = async (optionIndex) => {
    const currentQuestion = activeQuestions[currentQuestionIndex];
    if (!adaptiveSessionId || !currentQuestion || isLoadingNext) return;
    setIsLoadingNext(true);
    try {
      const next = await careerGuidanceApi.answerAdaptiveQuiz(adaptiveSessionId, currentQuestion.id, optionIndex);
      setAnswers(prev => [...prev, { questionId: currentQuestion.id, optionIndex }]);
      const nextQuestion = normalizeQuestion(next?.nextQuestion || next?.NextQuestion || next?.question || next?.Question);
      const isComplete = next?.isComplete === true || next?.IsComplete === true;
      if (isComplete) {
        setStep('loading');
        setLoadingStep(0);
        const messages = getProcessingMessages(selectedStream);

        // Fetch complete adaptive quiz in background
        const resultPromise = (async () => {
          try {
            return await careerGuidanceApi.completeAdaptiveQuiz(adaptiveSessionId);
          } catch (completeErr) {
            console.error('completeAdaptiveQuiz error, attempting fallback to latest-result:', completeErr);
            try {
              const latest = await careerGuidanceApi.getLatestResult();
              if (latest) return latest;
            } catch (latestErr) {
              console.error('Failed to get latest result as fallback:', latestErr);
            }
            throw completeErr;
          }
        })();

        let stepIndex = 0;
        const interval = setInterval(async () => {
          stepIndex++;
          setLoadingStep(stepIndex);
          if (stepIndex >= messages.length) {
            clearInterval(interval);
            try {
              const result = await resultPromise;
              finishWithAdaptiveResult(result);
            } catch (completeErr) {
              toast.error('The assessment could not be finalized. Returning to options.');
              setStep('landing');
            }
          }
        }, 750);
      } else if (nextQuestion) {
        setActiveQuestions(prev => [...prev, nextQuestion]);
        setCurrentQuestionIndex(prev => prev + 1);
        setQuizProgress(next.progress || 0);
        setEstimatedRemaining(next.estimatedRemaining || 10);
      } else {
        throw new Error('The quiz returned no next question and did not mark the assessment complete.');
      }
    } catch (err) {
      console.error(err);
      toast.error('The answer could not be recorded.');
    } finally {
      setIsLoadingNext(false);
    }
  };

  const normalizeQuestion = (question) => {
    const id = question?.id || question?.Id;
    const text = question?.text || question?.Text;
    const options = question?.options || question?.Options;
    if (!id || !text || !Array.isArray(options) || options.length < 2) return null;

    const normalizedOptions = options.map(option => ({
      index: option?.index ?? option?.Index,
      text: option?.text || option?.Text
    }));
    if (normalizedOptions.some(option => !option.text)) return null;

    return { id, text, questionType: question?.questionType || question?.QuestionType || '', options: normalizedOptions };
  };

  const finishWithAdaptiveResult = async (result) => {
    try {
      // The adaptive API returns both the V2 taxonomy format and the
      // presentation-oriented result format. Normalize both without inventing
      // a career name when the API has not produced a recommendation.
      const recommendations = result?.recommendations || result?.Recommendations || [];
      const comparison = result?.targetCareerComparison || result?.TargetCareerComparison || [];
      const apiRecommendedCareer = result?.recommendedCareer || result?.RecommendedCareer;
      const apiDegree = result?.recommendedDegree || result?.RecommendedDegree;
      const isPostGraduation = selectedStream === 'AfterPostGraduation';
      const primary = apiRecommendedCareer || (!isPostGraduation ? apiDegree : null) ||
        (!isPostGraduation ? result?.primaryRecommendation || result?.PrimaryRecommendation : null) || recommendations[0] || comparison[0];
      const primaryId = primary?.nodeId || primary?.NodeId || primary?.id || primary?.Id ||
        (isPostGraduation && !apiRecommendedCareer ? '' : result?.primaryTaxonomyNodeId || result?.PrimaryTaxonomyNodeId || '');
      const rawPrimaryName = primary?.name || primary?.Name || primary?.careerName || primary?.CareerName ||
        (isPostGraduation ? '' : result?.bestMatchName || result?.BestMatchName || result?.primaryDomain?.name || result?.PrimaryDomain?.Name || '');
      const idBasedName = primaryId.includes('.')
        ? primaryId.split('.').slice(1).join(' ').replace(/[-_]/g, ' ').replace(/\b\w/g, letter => letter.toUpperCase())
        : '';
      const primaryName = rawPrimaryName && !['career exploration', 'unknown', 'n/a'].includes(rawPrimaryName.trim().toLowerCase())
        ? getCareerDisplayName(rawPrimaryName)
        : getCareerDisplayName(idBasedName);
      const normalizedMatches = [
        ...recommendations.map(match => ({
          id: match?.nodeId || match?.NodeId || '',
          title: getCareerDisplayName(match?.name || match?.Name || ''),
          matchScore: match?.matchPercent ?? match?.MatchPercent ?? match?.score ?? match?.Score ?? 0,
          domainName: match?.hierarchyPath || match?.HierarchyPath || '',
          expectedSalaryRange: match?.expectedSalaryRange || match?.ExpectedSalaryRange || '',
          expectedTimeToJobReady: match?.expectedTimeToJobReady || match?.ExpectedTimeToJobReady || '',
        })),
        ...comparison.map(match => ({
          id: match?.nodeId || match?.NodeId || match?.id || match?.Id || '',
          title: getCareerDisplayName(match?.careerName || match?.CareerName || match?.name || match?.Name || ''),
          matchScore: match?.score ?? match?.Score ?? match?.matchPercent ?? match?.MatchPercent ?? 0,
          domainName: match?.hierarchyPath || match?.HierarchyPath || '',
          expectedSalaryRange: match?.expectedSalaryRange || match?.ExpectedSalaryRange || '',
          expectedTimeToJobReady: match?.expectedTimeToJobReady || match?.ExpectedTimeToJobReady || '',
        })),
      ].filter(match => match.title);
      const recommendedNextSteps = result?.recommendedNextSteps || result?.RecommendedNextSteps || [
        `Review admission requirements for ${primaryName || 'the recommended career'} programs and shortlist recognized institutions.`,
        `Build practical experience through projects, internships, clinics, placements, or supervised work relevant to ${primaryName || 'this career'}.`,
        'Check the required entrance exams, registrations, or professional licensing steps for your location.',
      ];
      const adaptedResult = {
        ...result,
        // The result API may omit the quiz phase, but education recommendations
        // depend on it (especially for post-graduation users).
        phaseKey: result?.phaseKey || result?.PhaseKey || selectedStream || '',
        backgroundProfile: backgroundContext || {},
        // Keep the existing career-result screen when the API returned a named
        // recommendation, even if its confidence flag is conservative.
        resultStatus: (result?.resultStatus || result?.ResultStatus) === 'RequiresDecision'
          ? 'RequiresDecision'
          : (result?.resultStatus || result?.ResultStatus) === 'InsufficientConfidence'
            ? 'InsufficientConfidence'
            : primaryName
              ? 'Success'
              : (result?.resultStatus || result?.ResultStatus || 'InsufficientConfidence'),
        confidenceScore: result?.confidenceScore ?? result?.ConfidenceScore ?? result?.confidence?.score ?? result?.Confidence?.Score ?? 0,
        whyRecommended: formatCareerReferences(result?.whyRecommended || result?.WhyRecommended || result?.explanation?.whyRecommended || result?.Explanation?.WhyRecommended || ''),
        bestMatchName: primaryName || result?.bestMatchName || result?.BestMatchName || '',
        recommendedCareer: primaryName ? { id: primaryId, title: primaryName } : null,
        topMatches: normalizedMatches,
        expectedSalaryRange: result?.expectedSalaryRange || result?.ExpectedSalaryRange || normalizedMatches[0]?.expectedSalaryRange || '',
        expectedTimeToJobReady: result?.expectedTimeToJobReady || result?.ExpectedTimeToJobReady || normalizedMatches[0]?.expectedTimeToJobReady || '',
        recommendedNextSteps,
        skillGap: result?.skillGap || result?.SkillGap || []
      };
      setBookmarked(false);
      
      try {
        setResultProfile(adaptedResult);
        setStep('result');
      } catch (profileErr) {
        console.error('Error applying resultProfile:', profileErr);
        setStep('result');
      }

      try {
        confetti({
          particleCount: 150,
          spread: 80,
          origin: { y: 0.6 }
        });
      } catch (confettiErr) {
        // Non-blocking
      }

      // Fetch 5-year salary forecast safely
      const pathId = primaryId || result?.targetCareerComparison?.[0]?.id || result?.targetCareerComparison?.[0]?.nodeId;
      if (pathId) {
        setIsSalaryLoading(true);
        try {
           const payload = {
              pathId: pathId,
              targetYear: new Date().getFullYear() + 5,
              location: 'Tier 1',
              skillLevel: 'Intermediate'
           };
           const res = await careerGuidanceApi.forecastSalary(payload);
           setFiveYearSalary(res);
        } catch(e) { console.error('Failed to fetch salary forecast', e); }
        setIsSalaryLoading(false);
      }
    } catch (outerErr) {
      console.error('Critical error in finishWithAdaptiveResult:', outerErr);
      setResultProfile({
        ...result,
        phaseKey: result?.phaseKey || result?.PhaseKey || selectedStream || '',
        backgroundProfile: backgroundContext || {},
      });
      setStep('result');
    }
  };

  // Toggle Bookmark
  const handleToggleBookmark = async () => {
    if (!resultProfile?.recommendedCareer) return;
    try {
      const isBookmarked = await careerGuidanceApi.toggleBookmark(resultProfile.recommendedCareer.id);
      setBookmarked(isBookmarked);
      toast.success(isBookmarked ? 'Career path saved to bookmarks' : 'Removed from bookmarks');
    } catch (err) {
      toast.error('Failed to toggle bookmark');
    }
  };


  // Reset quiz
  const handleReset = () => {
    setStep('landing');
    setSelectedStream('');
    setSelectedSubStream('');
    setActiveQuestions([]);
    setCurrentQuestionIndex(0);
    setAnswers([]);
    setResultProfile(null);
    setActiveResultTab('overview');
    setExpandedMatchId(null);
    setQuizProgress(0);
    setEstimatedRemaining(15);
  };

  const addResultToLearning = (careerOverride, navigateAfter = true) => {
    if (!resultProfile) return;
    // React supplies the click event as the first argument for a direct
    // onClick handler. It is not serializable and must never be saved.
    if (careerOverride?.nativeEvent || careerOverride?.currentTarget) careerOverride = undefined;
    const displayedCareer = resultProfile.bestMatchName || resultProfile.BestMatchName || resultProfile.primaryDomain?.name || resultProfile.PrimaryDomain?.Name || '';
    const selectedMatch = careerOverride || resultProfile.recommendedCareer || resultProfile.topMatches?.[0] || {
      id: resultProfile.primaryTaxonomyNodeId || resultProfile.PrimaryTaxonomyNodeId || '',
      title: displayedCareer,
      matchScore: resultProfile.confidenceScore || resultProfile.ConfidenceScore || 0,
      expectedSalaryRange: resultProfile.expectedSalaryRange || resultProfile.ExpectedSalaryRange || '',
      expectedTimeToJobReady: resultProfile.expectedTimeToJobReady || resultProfile.ExpectedTimeToJobReady || '',
    };
    const key = userStorageKey('skillscape-quiz-learning-plans', user);
    let existing = [];
    try {
      const stored = JSON.parse(window.localStorage.getItem(key) || '[]');
      existing = Array.isArray(stored) ? stored : [];
    } catch {
      // A corrupt browser value should never stop a learner from saving a result.
      existing = [];
    }
    const careerTitle = selectedMatch.title || selectedMatch.careerName || selectedMatch.name || selectedMatch.Name || displayedCareer;
    if (!careerTitle) { toast.error('No career result is available to save yet.'); return; }
    const careerKey = (selectedMatch.id || selectedMatch.nodeId || selectedMatch.NodeId || careerTitle).toLowerCase().replace(/\s+/g, '-').replace(/\//g, '-');
    const newEntry = {
      id: `${careerKey}-${Date.now()}`,
      savedAt: new Date().toISOString(),
      assessmentType: selectedStream || 'Career Discovery',
      selectedCareer: {
        id: careerKey,
        title: careerTitle,
        matchScore: selectedMatch.matchScore ?? selectedMatch.score,
        expectedSalaryRange: selectedMatch.expectedSalaryRange || resultProfile.expectedSalaryRange,
        expectedTimeToJobReady: selectedMatch.expectedTimeToJobReady || resultProfile.expectedTimeToJobReady,
      },
      result: resultProfile,
    };
    // Avoid exact-title duplicates
    const filtered = existing.filter(e => (e.selectedCareer?.title || e.selectedCareer?.careerName) !== careerTitle);
    window.localStorage.setItem(key, JSON.stringify([newEntry, ...filtered]));
    setSavedLearningPlanId(newEntry.id);

    // Also sync to backend saved roadmaps
    roadmapApi.saveMyRoadmap(careerKey, careerTitle).catch(() => {});

    toast.success(`${careerTitle} was added to My Learning Plans.`);
    if (navigateAfter) {
      navigate('/learning-plans');
    }
  };

  const handleAcceptAndStartRoadmap = async (overrideKey, overrideTitle) => {
    if (!resultProfile) return;
    setIsAcceptingRoadmap(true);
    const displayedCareer = resultProfile.bestMatchName || resultProfile.BestMatchName || resultProfile.primaryDomain?.name || resultProfile.PrimaryDomain?.Name || '';
    const rawKey = overrideKey || resultProfile.primaryTaxonomyNodeId || resultProfile.PrimaryTaxonomyNodeId || resultProfile.recommendedCareer?.id || resultProfile.recommendedCareer?.nodeId || displayedCareer;
    const title = overrideTitle || displayedCareer || rawKey;
    const cleanKey = (rawKey || title).toLowerCase().replace(/\s+/g, '-').replace(/\//g, '-');

    try {
      // 1. Persist locally to learning plans without navigating away
      addResultToLearning({
        id: cleanKey,
        title: title,
        matchScore: resultProfile.confidenceScore || 100,
        expectedSalaryRange: resultProfile.expectedSalaryRange,
        expectedTimeToJobReady: resultProfile.expectedTimeToJobReady
      }, false);

      // 2. Call backend accept API to set default roadmap
      await roadmapApi.acceptCareerRoadmap(cleanKey, title);
      toast.success(`${title} roadmap activated! Opening your curriculum...`);
    } catch (err) {
      console.warn('Failed to accept career roadmap on backend:', err);
      try {
        await roadmapApi.saveMyRoadmap(cleanKey, title);
      } catch {}
    } finally {
      setIsAcceptingRoadmap(false);
      navigate(`/roadmap?domain=${encodeURIComponent(cleanKey)}`);
    }
  };

  // Trajectory chart calculation
  const salaryChartData = useMemo(() => {
    if (!resultProfile || !resultProfile.careerGrowthGraph) return [];
    return resultProfile.careerGrowthGraph.map(point => ({
      year: point.year || point.Year,
      India: Math.round((point.salaryIndia || point.SalaryIndia) / 100000 * 10) / 10, // Lakhs
      Global: Math.round((point.salaryGlobal || point.SalaryGlobal) / 100000 * 10) / 10 // Lakhs
    }));
  }, [resultProfile]);

  // Extract Strengths and Weaknesses
  const strengths = useMemo(() => {
    if (!resultProfile?.strengthsWeaknesses) return [];
    return resultProfile.strengthsWeaknesses.Strengths || resultProfile.strengthsWeaknesses.strengths || [];
  }, [resultProfile]);

  const weaknesses = useMemo(() => {
    if (!resultProfile?.strengthsWeaknesses) return [];
    return resultProfile.strengthsWeaknesses.Weaknesses || resultProfile.strengthsWeaknesses.weaknesses || [];
  }, [resultProfile]);

  const gridColor = isDark ? '#1e293b' : '#e2e8f0';
  const labelColor = isDark ? '#94a3b8' : '#475569';
  const tooltipStyle = isDark 
    ? { backgroundColor: '#0f172a', borderColor: '#1e293b', borderRadius: '12px', color: '#f8fafc', fontSize: 11 }
    : { backgroundColor: '#ffffff', borderColor: '#e2e8f0', borderRadius: '12px', color: '#0f172a', fontSize: 11 };

  return (
    <div className="min-h-[calc(100vh-80px)] w-full bg-background text-foreground flex flex-col items-center pt-4 md:pt-6 px-4 relative">
      {/* Background gradients */}
      <div className="absolute top-10 left-10 w-96 h-96 bg-blue-600/5  rounded-full blur-[100px] pointer-events-none" />
      <div className="absolute bottom-10 right-10 w-96 h-96 bg-blue-600/5  rounded-full blur-[100px] pointer-events-none" />

      {step !== 'landing' && step !== 'loading' && step !== 'initializing' && (
        <Button variant="ghost" onClick={handleReset} className="absolute left-4 top-4 z-20">
          <ArrowLeft className="w-4 h-4" /> Back to Career Quiz
        </Button>
      )}
      <AnimatePresence mode="wait">
        {/* STEP 1: Landing Page */}
        {step === 'landing' && (
          <motion.div
            key="quiz-landing"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            className="w-full max-w-2xl mx-auto space-y-2 z-10 pb-2"
          >
            <div className="border-b border-slate-200 pb-6 mb-6">
              <div className="flex items-start gap-4">
                <div className="w-10 h-10 bg-blue-50 text-blue-600 rounded-[10px] flex items-center justify-center shrink-0">
                  <Compass className="w-5 h-5" />
                </div>
                <div className="flex flex-col gap-1">
                  <h1 className="text-2xl sm:text-[28px] font-bold text-slate-900 leading-tight tracking-tight">Career Discovery Quiz</h1>
                  <p className="text-[15px] text-slate-500 leading-relaxed max-w-xl">Identify your interests and strengths to find your ideal path.</p>
                </div>
              </div>
            </div>

            <div>
              <CareerStageSlider 
                stages={Object.entries(STAGE_METADATA).map(([id, meta]) => ({ id, meta }))} 
                onSelect={handleStartQuiz} 
              />
            </div>
          </motion.div>
        )}

        {/* STEP 1.5: Background Form */}
        {step === 'background_form' && (
          <motion.div key="quiz-background" className="w-full max-w-5xl mx-auto z-10 py-6">
            <BackgroundForm
              quizType={selectedStream}
              onComplete={handleBackgroundComplete}
              onBack={() => setStep('landing')}
            />
          </motion.div>
        )}

        {/* STEP 2: Quiz Engine */}
        {step === 'quiz' && activeQuestions && activeQuestions.length > 0 && (
          <motion.div
            key="quiz"
            initial={{ opacity: 0, scale: 0.98 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0 }}
            className="w-full max-w-5xl mx-auto z-10 py-6"
          >
            {/* Progress Bar */}
            <div className="mb-8 max-w-md mx-auto">
              <div className="flex items-center justify-center gap-2 mb-2">
                <div className="text-sm font-bold text-blue-600">Question {currentQuestionIndex + 1}</div>
                <div className="text-sm font-medium text-slate-400">• Adaptive Assessment</div>
                {isLoadingNext && <Loader2 className="w-4 h-4 text-blue-500 animate-spin" />}
              </div>
              <div className="h-2 bg-slate-100 rounded-full overflow-hidden">
                <motion.div 
                  className="h-full bg-blue-600" 
                  initial={{ width: 0 }} 
                  animate={{ width: `${Math.max(5, quizProgress * 100)}%` }}
                />
              </div>
            </div>

            {/* Sliding Question Content */}
            <AnimatePresence mode="wait">
              <motion.div
                key={activeQuestions[currentQuestionIndex]?.id || `question-${currentQuestionIndex}`}
                initial={{ opacity: 0, y: 15 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -15 }}
                transition={{ duration: 0.3, ease: 'easeOut' }}
                className="bg-white border border-slate-200 rounded-3xl p-6 sm:p-8 shadow-sm"
              >
                {/* Question Text */}
                <div className="mb-6 text-center">
                  <h2 className="text-2xl sm:text-3xl font-extrabold text-[#0F172A] leading-snug">
                    {activeQuestions[currentQuestionIndex]?.text || activeQuestions[currentQuestionIndex]?.questionText || activeQuestions[currentQuestionIndex]?.QuestionText}
                  </h2>
                </div>


                {/* Options Grid (2x2) */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {(activeQuestions[currentQuestionIndex]?.options || activeQuestions[currentQuestionIndex]?.Options || []).map((opt, idx) => {
                    const isSelected = answers[currentQuestionIndex]?.optionIndex === idx;
                    const letter = String.fromCharCode(65 + idx); // A, B, C, D
                    
                    return (
                      <button
                        key={idx}
                        onClick={() => !isLoadingNext && handleSelectOption(idx)}
                        disabled={isLoadingNext}
                        className={`w-full text-left p-4 rounded-2xl border-2 transition-all duration-200 group ${
                          isLoadingNext ? 'cursor-wait opacity-60' :
                          isSelected
                            ? 'border-[#2563EB] bg-[#EFF6FF] shadow-sm transform -translate-y-0.5 cursor-pointer'
                            : 'border-slate-100 bg-white hover:border-[#2563EB]/40 hover:bg-slate-50 hover:shadow-sm cursor-pointer'
                        }`}
                      >
                        <div className="flex items-start gap-4">
                          <div className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-xl text-sm font-bold transition-colors ${
                            isSelected ? 'bg-[#2563EB] text-white' : 'bg-slate-100 text-[#475569] group-hover:bg-[#DBEAFE] group-hover:text-[#2563EB]'
                          }`}>
                            {letter}
                          </div>
                          <div className="flex-1 pt-1">
                            <span className={`text-base font-semibold ${isSelected ? 'text-[#0F172A]' : 'text-[#334155]'}`}>
                              {opt.text || opt.Text}
                            </span>
                          </div>
                          {isSelected && (
                            <div className="shrink-0 pt-1">
                              <CheckCircle2 className="w-5 h-5 text-[#2563EB]" />
                            </div>
                          )}
                        </div>
                      </button>
                    );
                  })}
                </div>

                {/* Navigation Controls */}
                <div className="flex justify-between items-center mt-6 pt-4 border-t border-slate-100">
                  <Button
                    variant="ghost"
                    onClick={() => {
                      if (currentQuestionIndex > 0) {
                        setCurrentQuestionIndex(currentQuestionIndex - 1);
                      } else {
                        handleReset();
                      }
                    }}
                    className="text-slate-500 hover:text-slate-900 rounded-xl px-4"
                  >
                    ← Previous
                  </Button>
                  <span className="text-xs font-medium text-slate-400">
                    Auto-advances on selection
                  </span>
                </div>
              </motion.div>
            </AnimatePresence>
          </motion.div>
        )}

        {/* Initializing Quiz */}
        {step === 'initializing' && (
          <motion.div
            key="quiz-initializing"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="text-center space-y-4 z-10 py-20"
          >
            <div className="relative w-12 h-12 mx-auto">
              <Loader2 className="w-12 h-12 text-blue-600 animate-spin" />
            </div>
            <div className="space-y-1">
              <h3 className="font-bold text-lg text-foreground">Preparing Your Assessment</h3>
              <p className="text-xs text-muted-foreground max-w-xs mx-auto">
                Loading questions for {STAGE_METADATA[selectedStream]?.title || selectedStream}…
              </p>
            </div>
          </motion.div>
        )}

        {/* STEP 3: Loading calculations / Processing Analysis */}
        {step === 'loading' && (
          <QuizProcessingLoader
            title="Processing your results…"
            messages={getProcessingMessages(selectedStream)}
            loadingStep={loadingStep}
          />
        )}

        {/* STEP 4: Results Display */}
        {step === 'result' && resultProfile && resultProfile.resultStatus === 'InsufficientConfidence' && (
          <motion.div
            key="quiz-insufficient-confidence"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            className="w-full max-w-4xl px-4 mx-auto py-12"
          >
            <div className="w-full bg-white border border-slate-200 rounded-3xl p-8 lg:p-12 shadow-sm text-center flex flex-col items-center">
               <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mb-6">
                 <AlertCircle className="w-8 h-8 text-amber-600" />
               </div>
               <h2 className="text-3xl font-bold text-slate-900 mb-4">Inconclusive Profile</h2>
               <p className="text-slate-600 mb-8 max-w-lg text-lg leading-relaxed">
                 {resultProfile.whyRecommended || "Your answers were highly varied and did not strongly align with a single clear career path. We need more consistent data to make a confident recommendation."}
               </p>
               <Button onClick={handleReset} className="bg-[#2563EB] hover:bg-[#1D4ED8] text-white px-8 py-6 h-auto text-lg rounded-xl font-bold">
                 Retake Assessment
               </Button>
            </div>
          </motion.div>
        )}

        {step === 'result' && resultProfile && resultProfile.resultStatus === 'RequiresDecision' && (
          <motion.div
            key="quiz-requires-decision"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            className="w-full max-w-4xl px-4 mx-auto py-12"
          >
            <div className="w-full bg-white border border-slate-200 rounded-3xl p-8 lg:p-12 shadow-sm text-center flex flex-col items-center">
               <h2 className="text-3xl font-bold text-slate-900 mb-4">Review Your Goal</h2>
               <p className="text-slate-600 mb-8 max-w-lg text-lg leading-relaxed">
                 Your responses suggest that your original goal might not be the strongest fit. Based on your profile, the following alternatives may fit you better. What would you like to pursue?
               </p>
               <div className="w-full flex flex-col gap-4 mb-8">
                 {resultProfile.recommendations?.slice(0, 3).map((rec, i) => (
                   <Button key={i} onClick={() => handleAcceptCareer(rec.nodeId, rec.name)} className="w-full text-left justify-start bg-slate-50 hover:bg-blue-50 text-slate-800 hover:text-blue-700 border border-slate-200 hover:border-blue-300 px-6 py-4 h-auto rounded-xl">
                     <span className="font-bold text-lg">{rec.name}</span>
                     <span className="ml-auto text-sm text-slate-500">{rec.matchPercent}% Match</span>
                   </Button>
                 ))}
                 <Button variant="outline" onClick={() => handleAcceptCareer(resultProfile.recommendedCareer?.nodeId || resultProfile.recommendations?.[0]?.nodeId, resultProfile.recommendedCareer?.name || resultProfile.recommendations?.[0]?.name)} className="w-full text-left justify-start border-slate-200 text-slate-600 hover:bg-slate-50 px-6 py-4 h-auto rounded-xl mt-4">
                   Continue with Original Goal
                 </Button>
               </div>
            </div>
          </motion.div>
        )}

        {step === 'result' && resultProfile && resultProfile.resultStatus === 'Success' && (() => {
          // Normalize data from multiple possible formats
          // The API may return alternatives under either the legacy comparison
          // field or the normalized recommendations list. Combine both so the
          // result shows a useful range of matching career directions.
          const comparison = [
            ...(resultProfile.targetCareerComparison || []),
            ...(resultProfile.topMatches || []).map(match => ({
              careerName: match.title,
              score: match.matchScore,
              hierarchyPath: match.domainName,
            })),
          ].filter((career, index, list) => {
            const name = career.careerName || career.CareerName || career.title;
            return name && list.findIndex(item => (item.careerName || item.CareerName || item.title) === name) === index;
          });
          const bestCareer = comparison[0];
          let bestScore = bestCareer?.score ?? bestCareer?.Score ?? 0;
          if (bestScore === 0) {
            const prof = resultProfile.professionalQualification;
            const deg = resultProfile.recommendedDegree;
            if (prof) {
              bestScore = prof.score || prof.Score || 0;
            } else if (deg) {
              bestScore = deg.score || deg.Score || 0;
            }
          }
          const bestName = resultProfile.bestMatchName || bestCareer?.careerName || bestCareer?.CareerName || 'N/A';
          const specialization = (resultProfile.specializationText || '').replace('Primary: ', '').trim();
          const strengths = resultProfile.strongestCompetencies || [];
          const devAreas = resultProfile.developmentAreas || [];
          const nextSteps = resultProfile.recommendedNextSteps || [];
          const otherMatches = comparison.slice(1, 9);
          const evidenceItems = resultProfile.evidenceItems || [];
          const hasCareerPathway = Boolean(resultProfile.recommendedDegree || resultProfile.professionalQualification);

          let formattedBestName = getCareerDisplayName(bestName);
          const lowerName = formattedBestName.toLowerCase();
          if (lowerName === 'ba') formattedBestName = 'BA';
          else if (lowerName === 'bsc') formattedBestName = 'B.Sc';
          else if (lowerName === 'bcom') formattedBestName = 'B.Com';
          else if (lowerName === 'btech') formattedBestName = 'B.Tech';
          else if (lowerName === 'bba') formattedBestName = 'BBA';
          else if (lowerName === 'bca') formattedBestName = 'BCA';
          else if (lowerName === 'mbbs') formattedBestName = 'MBBS';
          else if (formattedBestName.length <= 3 && !formattedBestName.includes(' ')) formattedBestName = formattedBestName.toUpperCase();

          return (
            <motion.div
              key="quiz-result"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0 }}
              className="w-full mx-auto pb-20 pt-6"
              style={{ width: 'calc(100% - 48px)', maxWidth: '1440px' }}
            >
              {/* ── SECTION 1: CAREER HERO ─────────────────────────── */}
              <div className="mb-8 text-center pb-8 border-b border-slate-200">
                <div className="inline-flex items-center gap-2 bg-blue-50 border border-blue-200 text-blue-700 text-[11px] font-bold uppercase tracking-widest px-3 py-1.5 rounded-full mb-5">
                  <Trophy className="w-3.5 h-3.5" /> YOUR BEST CAREER MATCH
                </div>
                <h1 className="text-4xl md:text-[44px] lg:text-[52px] font-extrabold text-slate-900 leading-tight mb-3">
                  {formattedBestName}
                </h1>
                <div className="flex items-center justify-center gap-3 mb-4">
                  <div className="text-[38px] font-black text-blue-600 leading-none">{Math.round(bestScore)}<span className="text-2xl">%</span></div>
                  <div className="flex flex-col items-start">
                    <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest">Compatibility</div>
                    <span className="text-sm font-bold text-emerald-600 uppercase tracking-wider">High</span>
                  </div>
                </div>
                {resultProfile.whyRecommended && (
                  <p className="text-slate-600 text-base max-w-2xl mx-auto">
                    {formatCareerReferences(resultProfile.whyRecommended)}
                  </p>
                )}
              </div>

              {/* ── SECTION 2: INTELLIGENCE SNAPSHOT GRID ─────────────── */}
              <div className="grid grid-cols-1 lg:grid-cols-[1.15fr_0.85fr] gap-6 mb-6">
                {/* RECOMMENDED EDUCATION */}
                {(() => {
                  const careerNodeId =
                    resultProfile?.career?.nodeId ||
                    resultProfile?.career?.id ||
                    resultProfile?.recommendedCareer?.nodeId ||
                    resultProfile?.recommendedCareer?.id ||
                    resultProfile?.targetCareerComparison?.[0]?.nodeId ||
                    resultProfile?.targetCareerComparison?.[0]?.id ||
                    resultProfile?.primaryDomain?.id ||
                    resultProfile?.primaryDomain?.nodeId ||
                    resultProfile?.primaryTaxonomyNodeId ||
                    resultProfile?.PrimaryTaxonomyNodeId ||
                    resultProfile?.bestMatchName ||
                    resultProfile?.BestMatchName ||
                    '';
                  const educationPhase = resultProfile?.phaseKey || resultProfile?.PhaseKey || selectedStream || '';
                  const careerDisplayName = getCareerDisplayName(resultProfile?.bestMatchName || resultProfile?.BestMatchName || '');
                  const eduData = getCareerEducation(careerNodeId, careerDisplayName, educationPhase, resultProfile?.backgroundProfile || {});
                  const recommendedEducation = resultProfile?.education || resultProfile?.Education;
                  const canonicalQualification = recommendedEducation?.qualificationName || '';
                  const hasCanonicalQualification = canonicalQualification && !/^(relevant|doctoral \/ research qualification)/i.test(canonicalQualification.trim());
                  const hasSpecificEducation = (eduData?.primaryDegrees || []).some(deg =>
                    deg.name && !deg.name.toLowerCase().includes('stream-relevant') && !/^(relevant|doctoral \/ research qualification)/i.test(deg.name.trim())
                  );
                  const educationDegrees = hasSpecificEducation
                    ? eduData.primaryDegrees
                    : hasCanonicalQualification
                      ? [{ name: canonicalQualification, shortName: canonicalQualification, duration: recommendedEducation.duration || 3, type: 'Primary', streams: [] }]
                      : eduData?.primaryDegrees || [];
                  const displayEducation = { ...eduData, primaryDegrees: educationDegrees };
                  const isGenericFallback = educationDegrees.length === 0;
                  const nextSteps = resultProfile?.recommendedNextSteps || resultProfile?.RecommendedNextSteps || [];
                  return (
                    <div className="bg-white border border-slate-200 p-6 rounded-[18px] shadow-sm flex flex-col h-full">
                      <div className="flex items-center gap-2 mb-4 pb-4 border-b border-slate-100">
                        <GraduationCap className="w-5 h-5 text-blue-600" />
                        <h3 className="font-bold text-[17px] text-slate-900">Recommended Education</h3>
                      </div>

                      {isGenericFallback ? (
                        <div className="text-slate-500 text-sm italic py-2">We don’t have a verified degree pathway for this career yet.</div>
                      ) : (displayEducation?.primaryDegrees || []).length > 0 ? (
                        <div className="flex flex-col gap-2.5 flex-1">
                          {displayEducation.primaryDegrees.map((deg, i) => {
                            // Skip entries where name looks generic
                            if (deg.name?.toLowerCase().includes('stream-relevant')) return null;
                            return (
                              <div key={i} className={`flex items-start gap-3 p-3.5 rounded-xl border ${
                                  deg.type === 'Primary' ? 'bg-blue-50/50 border-blue-200' : 'bg-slate-50 border-slate-200'
                              }`}>
                                <div className="text-[11px] font-black text-slate-400 mt-0.5 w-5 shrink-0">0{i + 1}</div>
                                <div className="flex-1">
                                  <div className="font-bold text-slate-900 text-sm leading-snug mb-1">{deg.name}</div>
                                  <div className="flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-slate-500">
                                    {deg.duration ? <span>{deg.duration} yrs</span> : null}
                                    {/* Only show streams if they're meaningful (not 'Any') */}
                                    {(deg.streams || []).filter(s => s && s.toLowerCase() !== 'any').map(s => (
                                      <span key={s} className="bg-blue-50 border border-blue-100 text-blue-700 px-2 py-0.5 rounded text-[11px] font-semibold">{s}</span>
                                    ))}
                                  </div>
                                </div>
                              </div>
                            );
                          })}
                          {displayEducation.afterDegree && (
                            <div className="mt-1 flex items-start gap-3 p-3.5 rounded-xl border border-slate-100 bg-white">
                              <div className="text-blue-400 text-sm shrink-0 mt-0.5">↓</div>
                              <div>
                                <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-0.5">Next Step</div>
                                <div className="text-sm font-semibold text-slate-800">{displayEducation.afterDegree}</div>
                              </div>
                            </div>
                          )}
                          {/* Only show notes if non-generic */}
                          {displayEducation.notes && (
                            <div className="text-[12px] text-slate-400 italic pt-1">{displayEducation.notes}</div>
                          )}
                        </div>
                      ) : (
                        <div className="text-slate-400 text-sm italic py-2">Education pathway information unavailable.</div>
                      )}
                      {nextSteps.length > 0 && (
                        <div className="mt-5 pt-4 border-t border-slate-100">
                          <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-2">Recommended Next Steps</div>
                          <ol className="space-y-2">
                            {nextSteps.slice(0, 4).map((step, index) => (
                              <li key={`${index}-${step}`} className="flex items-start gap-2 text-sm text-slate-700">
                                <span className="text-blue-600 font-bold">{index + 1}.</span>
                                <span>{step}</span>
                              </li>
                            ))}
                          </ol>
                        </div>
                      )}
                    </div>
                  );
                })()}

                {/* INDIA ENTRY-LEVEL SALARY */}
                <div className="bg-white border border-slate-200 p-6 rounded-[18px] shadow-sm flex flex-col h-full">
                  <div className="flex items-center gap-2 mb-5 pb-4 border-b border-slate-100">
                    <Wallet className="w-5 h-5 text-blue-600" />
                    <h3 className="font-bold text-[17px] text-slate-900">India Entry-Level Salary</h3>
                  </div>

                  {resultProfile.salary?.status === 'Unavailable' || !resultProfile.salary ? (
                    <div className="text-slate-500 italic text-sm py-4">Salary benchmark unavailable for this career.</div>
                  ) : (
                    <div className="flex flex-col flex-1 justify-between gap-6">
                      <div>
                        <div className="text-[32px] font-black text-slate-900 tracking-tight leading-tight">
                          {formatINR(resultProfile.salary.minSalary)} – {formatINR(resultProfile.salary.maxSalary)}
                        </div>
                        <div className="text-[15px] font-semibold text-blue-600 mt-1">
                          {formatLPA(resultProfile.salary.minSalary)} – {formatLPA(resultProfile.salary.maxSalary)}
                        </div>
                      </div>

                      <div className="grid grid-cols-2 gap-y-4 gap-x-2 pt-4 border-t border-slate-100">
                        <div>
                          <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-1">Category</div>
                          <div className="text-[13px] font-semibold text-slate-800 line-clamp-1">{getCareerDisplayName(resultProfile.salary.salaryRoleName || resultProfile.career?.name || formattedBestName)}</div>
                        </div>
                        <div>
                          <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-1">Experience</div>
                          <div className="text-[13px] font-semibold text-slate-800">{resultProfile.salary.experienceMinYears}–{resultProfile.salary.experienceMaxYears} yrs</div>
                        </div>
                        <div className="col-span-2">
                          <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-1">Source</div>
                          <div className="text-[13px] font-semibold text-slate-500">{resultProfile.salary.source || 'SkillScape Intelligence'}</div>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* ── SECTION 3: FIVE-YEAR SALARY OUTLOOK ─────────────────────── */}
              <div className="bg-white border border-slate-200 p-6 rounded-[18px] shadow-sm mb-6 w-full overflow-hidden">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6 pb-4 border-b border-slate-100">
                  <div className="flex items-center gap-2">
                    <TrendingUp className="w-5 h-5 text-blue-600" />
                    <h3 className="font-bold text-[17px] text-slate-900">Five-Year Salary Outlook</h3>
                  </div>
                  <div className="text-xs font-semibold text-slate-500">India • Post Graduation</div>
                </div>

                {isSalaryLoading ? (
                  <div className="py-8 flex flex-col items-center justify-center gap-3">
                    <Loader2 className="w-6 h-6 text-blue-600 animate-spin" />
                    <div className="text-xs text-slate-500">Loading forecast...</div>
                  </div>
                ) : (() => {
                  const baseMin = resultProfile.salary?.minSalary || 350000;
                  const baseMax = resultProfile.salary?.maxSalary || 750000;
                  const projectionFallback = Array.from({ length: 6 }).map((_, i) => ({
                    year: i,
                    p25: baseMin * Math.pow(1.12, i),
                    p75: baseMax * Math.pow(1.12, i),
                    growth: i > 0 ? `+${Math.round((Math.pow(1.12, i) - 1) * 100)}%` : ''
                  }));
                  const displayProjection = (fiveYearSalary && fiveYearSalary.projection && fiveYearSalary.projection.length > 0) 
                    ? fiveYearSalary.projection.slice(0, 6) 
                    : projectionFallback;

                  return (
                    <>
                      <div className="relative pt-6 pb-2">
                        {/* Connector line */}
                        <div className="absolute top-[42px] left-[calc(100%/12)] right-[calc(100%/12)] h-[2px] bg-slate-100 z-0 hidden sm:block" />
                        
                        <div className="grid grid-cols-2 sm:grid-cols-6 gap-6 sm:gap-0 relative z-10">
                          {displayProjection.map((yr, i) => {
                            const isCurrent = i === 0;
                            return (
                              <div key={i} className="flex flex-col items-center relative bg-white sm:bg-transparent">
                                <div className={`text-[10px] font-bold uppercase tracking-widest mb-3 ${isCurrent ? 'text-blue-700' : 'text-slate-400'}`}>
                                  Year {i}
                                </div>
                                <div className={`w-4 h-4 rounded-full border-2 border-blue-600 flex items-center justify-center mb-4 bg-white z-10 hidden sm:flex ${isCurrent ? 'ring-4 ring-blue-50' : ''}`}>
                                  <div className="w-1.5 h-1.5 bg-blue-600 rounded-full" />
                                </div>
                                <div className={`text-[12px] font-bold text-center ${isCurrent ? 'text-blue-700' : 'text-slate-700'}`}>
                                  ₹{(yr.p25 / 100000).toFixed(1)}–{(yr.p75 / 100000).toFixed(1)} <span className="text-[10px] font-semibold text-slate-400">LPA</span>
                                </div>
                                {isCurrent ? (
                                  <div className="mt-1.5 text-[9px] font-bold text-blue-400 uppercase tracking-wider">Current</div>
                                ) : (
                                  <div className="mt-1.5 text-[10px] font-bold text-emerald-600">
                                    {yr.growth || 'Projected'}
                                  </div>
                                )}
                              </div>
                            );
                          })}
                        </div>
                      </div>
                      {(!fiveYearSalary || !fiveYearSalary.projection || fiveYearSalary.projection.length === 0) && (
                        <div className="mt-6 text-[11px] text-slate-400 border-t border-slate-100 pt-4">
                          Year 0 = current India entry-level benchmark. Future years projected at ~12% annual growth.
                        </div>
                      )}
                    </>
                  );
                })()}
              </div>



              {/* ── SECTION 5: OTHER CAREER MATCHES ─────────────────────── */}
              {otherMatches.length > 0 && (
                <div className="bg-white border border-slate-200 p-6 rounded-[18px] shadow-sm mb-6 w-full">
                  <div className="flex items-center gap-2 mb-4 pb-4 border-b border-slate-100">
                    <BarChart3 className="w-5 h-5 text-blue-600" />
                    <h3 className="font-bold text-[17px] text-slate-900">Other Career Matches</h3>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                    {otherMatches.slice(0, 6).map((career, idx) => {
                      const matchScore = career.score ?? career.Score ?? career.matchScore ?? 0;
                      const matchName = career.careerName || career.CareerName || career.title || 'Additional direction';
                      const matchNodeId = career.nodeId || career.NodeId || career.id || '';
                      const matchSalaryMin = career.salary?.minSalary || 0;
                      const matchSalaryMax = career.salary?.maxSalary || 0;
                      const matchEdu = getCareerEducation(matchNodeId, matchName, resultProfile?.phaseKey || resultProfile?.PhaseKey || selectedStream || '', resultProfile?.backgroundProfile || {});
                      const primaryDeg = matchEdu?.primaryDegrees?.find(d => !d.name?.toLowerCase().includes('stream-relevant'));
                      return (
                        <button
                          key={idx}
                          onClick={() => setSelectedMatch({ career, matchName, matchScore, matchNodeId, matchSalaryMin, matchSalaryMax, matchEdu })}
                          className="text-left border border-slate-200 rounded-xl p-4 bg-white hover:border-blue-300 hover:shadow-sm transition-all group cursor-pointer flex flex-col"
                        >
                          <div className="flex items-start justify-between gap-2 mb-2">
                            <span className="font-bold text-slate-900 text-sm leading-snug group-hover:text-blue-700">{matchName}</span>
                            <span className="text-[11px] font-black text-blue-700 bg-blue-50 px-2 py-0.5 rounded border border-blue-100 shrink-0">{Math.round(matchScore)}%</span>
                          </div>
                          <div className="text-[12px] text-slate-500 mb-3">
                            {matchSalaryMin > 0
                              ? `₹${(matchSalaryMin/100000).toFixed(1)}–${(matchSalaryMax/100000).toFixed(1)} LPA`
                              : 'Salary data pending'}
                          </div>
                          {primaryDeg && (
                            <div className="text-[12px] text-slate-600 mb-3 line-clamp-1">
                              {primaryDeg.shortName || primaryDeg.name} · {primaryDeg.duration} yrs
                            </div>
                          )}
                          <div className="text-[12px] font-bold text-blue-600 group-hover:text-blue-700 flex items-center gap-1 mt-auto pt-3 border-t border-slate-100">
                            View details <ArrowRight className="w-3 h-3" />
                          </div>
                        </button>
                      );
                    })}
                  </div>
                </div>
              )}



              {/* ── ACTION BAR ──────────────────────────────────────────────── */}
              <div className="sticky bottom-6 z-40 bg-white border border-slate-200 p-4 rounded-xl shadow-sm flex flex-col md:flex-row gap-3 justify-between items-center w-full">
                <div className="flex gap-3 w-full md:w-auto">
                  <Button onClick={handleReset} variant="outline" className="flex-1 md:flex-none border-slate-200 text-slate-600 rounded-xl h-[44px] text-[13px] font-bold bg-white">
                    Retake
                  </Button>
                  <Button onClick={() => navigate('/dashboard')} variant="outline" className="flex-1 md:flex-none border-slate-200 text-slate-600 rounded-xl h-[44px] text-[13px] font-bold bg-white">
                    Dashboard
                  </Button>
                </div>
                
                <div className="flex gap-3 w-full md:w-auto">
                  <Button onClick={() => addResultToLearning()} variant="outline" className="flex-1 md:flex-none border-blue-200 bg-blue-50 text-blue-700 hover:bg-blue-100 hover:border-blue-300 rounded-xl h-[44px] text-[13px] font-bold px-5">
                    <Bookmark className="w-4 h-4 mr-1.5" /> Save
                  </Button>
                  <Button
                    disabled={isAcceptingRoadmap}
                    onClick={() => handleAcceptAndStartRoadmap(resultProfile.primaryTaxonomyNodeId || bestCareer?.nodeId || bestCareer?.careerName || formattedBestName, formattedBestName)}
                    className="flex-[2] md:flex-none bg-blue-600 hover:bg-blue-700 text-white rounded-xl h-[44px] text-[14px] font-bold px-6 shadow-md"
                  >
                    {isAcceptingRoadmap ? (
                      <><Loader2 className="w-4 h-4 mr-2 animate-spin" /> Preparing...</>
                    ) : (
                      <><Sparkles className="w-4 h-4 mr-2" /> Accept & Start Roadmap</>
                    )}
                  </Button>
                </div>
              </div>

              {/* ── OTHER MATCH MODAL ────────────────────────────────────────── */}
              {selectedMatch && (() => {
                const { matchName, matchScore, matchNodeId, matchSalaryMin, matchSalaryMax, matchEdu, career } = selectedMatch;
                return (
                  <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm" onClick={() => setSelectedMatch(null)}>
                    <div className="bg-white rounded-[24px] shadow-2xl w-full max-w-[800px] max-h-[90vh] flex flex-col overflow-hidden" onClick={e => e.stopPropagation()}>
                      
                      <div className="bg-blue-600 p-6 flex items-start justify-between gap-4 text-white">
                        <div>
                          <div className="text-[10px] font-black uppercase tracking-widest text-blue-200 mb-1">Career Match</div>
                          <h2 className="text-[24px] font-extrabold leading-tight">{matchName}</h2>
                          <div className="mt-2 flex items-center gap-2">
                            <span className="text-xl font-black">{Math.round(matchScore)}%</span>
                            <span className="text-[12px] font-bold text-blue-200 uppercase tracking-widest">Compatibility</span>
                          </div>
                        </div>
                        <button onClick={() => setSelectedMatch(null)} className="w-8 h-8 flex items-center justify-center bg-white/10 hover:bg-white/20 rounded-full text-white transition-colors">✕</button>
                      </div>

                      <div className="p-6 overflow-y-auto flex flex-col gap-6">
                        {/* Education */}
                        <div>
                          <div className="text-[12px] font-black text-slate-400 uppercase tracking-widest mb-3 flex items-center gap-2"><GraduationCap className="w-4 h-4 text-blue-600"/> Recommended Education</div>
                          {(matchEdu?.primaryDegrees || []).length > 0 ? (
                            <div className="flex flex-col gap-3">
                              {matchEdu.primaryDegrees.map((deg, i) => (
                                <div key={i} className={`flex items-start gap-3 p-4 rounded-xl border ${deg.type==='Primary' ? 'border-blue-100 bg-blue-50/50' : 'border-slate-100 bg-slate-50/50'}`}>
                                  <div className="w-6 h-6 rounded bg-slate-200 text-slate-600 font-bold text-[11px] flex items-center justify-center mt-0.5">{i + 1}</div>
                                  <div>
                                    <div className="font-bold text-slate-800 text-[13px]">{deg.name}</div>
                                    <div className="text-[12px] text-slate-500 mt-1">{deg.duration} years</div>
                                  </div>
                                </div>
                              ))}
                            </div>
                          ) : <div className="text-sm text-slate-500 italic border border-slate-100 rounded-xl p-4 bg-slate-50">Education data pending.</div>}
                        </div>

                        {/* Salary */}
                        <div>
                          <div className="text-[12px] font-black text-slate-400 uppercase tracking-widest mb-3 flex items-center gap-2"><TrendingUp className="w-4 h-4 text-blue-600"/> Five-Year Salary Outlook</div>
                          <div className="p-4 rounded-xl border border-slate-100 bg-slate-50 relative">
                            {matchSalaryMin > 0 ? (() => {
                              const baseMin = matchSalaryMin || 350000;
                              const baseMax = matchSalaryMax || 750000;
                              const projection = Array.from({ length: 6 }).map((_, i) => ({
                                year: i,
                                p25: baseMin * Math.pow(1.12, i),
                                p75: baseMax * Math.pow(1.12, i),
                                growth: i > 0 ? `+${Math.round((Math.pow(1.12, i) - 1) * 100)}%` : ''
                              }));
                              
                              return (
                                <div className="relative pt-6 pb-2">
                                  {/* Connector line */}
                                  <div className="absolute top-[42px] left-[calc(100%/12)] right-[calc(100%/12)] h-[2px] bg-slate-200 z-0 hidden sm:block" />
                                  
                                  <div className="grid grid-cols-2 sm:grid-cols-6 gap-6 sm:gap-0 relative z-10">
                                    {projection.map((yr, i) => {
                                      const isCurrent = i === 0;
                                      return (
                                        <div key={i} className="flex flex-col items-center relative bg-slate-50 sm:bg-transparent">
                                          <div className={`text-[10px] font-bold uppercase tracking-widest mb-3 ${isCurrent ? 'text-blue-700' : 'text-slate-400'}`}>
                                            Year {i}
                                          </div>
                                          <div className={`w-4 h-4 rounded-full border-2 border-blue-600 flex items-center justify-center mb-4 bg-white z-10 hidden sm:flex ${isCurrent ? 'ring-4 ring-blue-50' : ''}`}>
                                            <div className="w-1.5 h-1.5 bg-blue-600 rounded-full" />
                                          </div>
                                          <div className={`text-[12px] font-bold text-center ${isCurrent ? 'text-blue-700' : 'text-slate-700'}`}>
                                            ₹{(yr.p25 / 100000).toFixed(1)}–{(yr.p75 / 100000).toFixed(1)} <span className="text-[10px] font-semibold text-slate-400">LPA</span>
                                          </div>
                                          {isCurrent ? (
                                            <div className="mt-1.5 text-[9px] font-bold text-blue-400 uppercase tracking-wider">Current</div>
                                          ) : (
                                            <div className="mt-1.5 text-[10px] font-bold text-emerald-600">
                                              {yr.growth}
                                            </div>
                                          )}
                                        </div>
                                      );
                                    })}
                                  </div>
                                </div>
                              );
                            })() : (
                              <div className="text-[13px] text-slate-500">Salary data pending</div>
                            )}
                          </div>
                        </div>
                      </div>

                      <div className="p-5 border-t border-slate-100 bg-white flex gap-3">
                        <Button
                          onClick={() => { addResultToLearning({ ...career, title: matchName, matchScore }, false); setSelectedMatch(null); }}
                          variant="outline"
                          className="flex-1 rounded-xl font-bold h-12 text-blue-700 border-blue-200 bg-blue-50 hover:bg-blue-100"
                        >
                          Save
                        </Button>
                        <Button
                          onClick={() => { handleAcceptAndStartRoadmap(matchNodeId || matchName, matchName); setSelectedMatch(null); }}
                          className="flex-[2] bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-bold h-12 shadow-md"
                        >
                          <Sparkles className="w-4 h-4 mr-2" /> Start Roadmap
                        </Button>
                      </div>

                    </div>
                  </div>
                );
              })()}

            </motion.div>
          );
        })()}
      </AnimatePresence>
    </div>
  );
}
