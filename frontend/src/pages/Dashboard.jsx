import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useAuth } from '@/context/AuthContext';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  ArrowRight,
  BookOpen,
  Clock,
  Compass,
  Trophy,
  CheckCircle2,
  Loader2,
  PlayCircle,
  LayoutDashboard,
  Map,
  Brain,
  Target,
  TrendingUp,
  Sparkles,
  ClipboardCheck,
  RefreshCw,
} from 'lucide-react';
import { Link, Navigate } from 'react-router-dom';
import { quizApi, roadmapApi } from '@/services/api';
import { userStorageKey } from '@/lib/userStorage';

const containerVariants = {
  hidden: { opacity: 0 },
  visible: { opacity: 1, transition: { staggerChildren: 0.06 } },
};

const itemVariants = {
  hidden: { opacity: 0, y: 12 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.4, ease: 'easeOut' } },
};

const normalizeCareer = (value) => String(value || '')
  .toLowerCase()
  .replace(/^government[-_]/, '')
  .replace(/^custom[-_]/, '')
  .replace(/[^a-z0-9]+/g, '');

const isDismissedCareer = (candidate, dismissedKeys) => {
  if (!candidate || !dismissedKeys?.length) return false;
  const values = [
    candidate.id,
    candidate.careerId,
    candidate.nodeId,
    candidate.title,
    candidate.name,
    candidate.careerName,
  ].filter(Boolean).map(normalizeCareer);
  return dismissedKeys.some(key => {
    const normalizedKey = normalizeCareer(key);
    return values.some(value => value === normalizedKey || value.includes(normalizedKey) || normalizedKey.includes(value));
  });
};

const DashboardPage = () => {
  const { user } = useAuth();

  if (user?.role === 'Admin') return <Navigate to="/admin" replace />;

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [roadmapData, setRoadmapData] = useState(null);
  const [defaultSavedRoadmap, setDefaultSavedRoadmap] = useState(null);
  const [quizResult, setQuizResult] = useState(null);
  const [careerSwitch, setCareerSwitch] = useState(null);
  const [learningQuiz, setLearningQuiz] = useState(null);
  const [governmentLearning, setGovernmentLearning] = useState(null);
  const [skillAnalyzerAnalysis, setSkillAnalyzerAnalysis] = useState(null);
  const [skillAnalyzerPlan, setSkillAnalyzerPlan] = useState(null);
  const [dismissedRoadmapKeys, setDismissedRoadmapKeys] = useState([]);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setLoading(true);
        // Do not block the dashboard on roadmap generation/web review. A roadmap
        // can involve database persistence, web search and AI generation, while
        // the dashboard can render immediately from the quiz/local snapshot.
        const [quizRes, dismissedRes, savedRoadmapsRes] = await Promise.all([
          quizApi.getMyResult().catch(() => null),
          roadmapApi.getDismissedRoadmaps().catch(() => null),
          roadmapApi.getMyRoadmaps().catch(() => null),
        ]);
        const dismissedKeys = dismissedRes?.data?.data || [];
        const savedRoadmaps = savedRoadmapsRes?.data?.data || savedRoadmapsRes?.data?.Data || [];
        setDefaultSavedRoadmap(Array.isArray(savedRoadmaps)
          ? savedRoadmaps.find(roadmap => roadmap.isDefault || roadmap.IsDefault) || null
          : null);
        setDismissedRoadmapKeys(dismissedKeys);
        setQuizResult(quizRes);
        try { setCareerSwitch(JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-career-switch-assessment', user)) || 'null')); } catch { setCareerSwitch(null); }
        try {
          const plans = JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-quiz-learning-plans', user)) || '[]');
          // Skill Analyzer plans have their own dashboard card below; do not
          // render them again as a saved quiz-result panel.
          setLearningQuiz(Array.isArray(plans) ? plans.find(plan => plan.assessmentType !== 'Skill Analyzer') || null : null);
        } catch { setLearningQuiz(null); }
        try { setGovernmentLearning(JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-government-learning', user)) || 'null')); } catch { setGovernmentLearning(null); }
        try { setSkillAnalyzerAnalysis(JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-skill-analyzer-analysis', user)) || 'null')); } catch { setSkillAnalyzerAnalysis(null); }
        try { setSkillAnalyzerPlan(JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-skill-analyzer-roadmap', user)) || 'null')); } catch { setSkillAnalyzerPlan(null); }

        setLoading(false);

        // Hydrate roadmap progress after the first dashboard paint.
        roadmapApi.getMyInterestRoadmap()
          .then((roadmapRes) => setRoadmapData(roadmapRes.data?.data || null))
          .catch((roadmapError) => console.warn('Roadmap snapshot unavailable:', roadmapError));
      } catch (err) {
        console.error('Failed to fetch dashboard data:', err);
        setError('Failed to load dashboard data');
        setLoading(false);
      }
    };
    fetchDashboardData();
  }, [user?.id, user?.userId, user?.email]);

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh]">
        <Loader2 className="h-6 w-6 animate-spin text-[#2563EB] mb-3" />
        <p className="text-sm text-[#64748B] font-medium">Loading your journey...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] text-center max-w-sm mx-auto">
        <p className="text-[#DC2626] mb-4 font-medium">{error}</p>
        <Button onClick={() => window.location.reload()} variant="outline" className="rounded-md">Try Again</Button>
      </div>
    );
  }

  const firstName = user?.fullName?.split(' ')[0] || user?.name?.split(' ')[0] || 'Learner';
  
  // Try to use roadmap first, fallback to quiz result
  let targetCareer = 'Undecided';
  let targetCareerKey = null;
  let careerMatch = 0;

  if (roadmapData?.domainName || roadmapData?.targetCareer) {
    targetCareer = roadmapData.domainName || roadmapData.targetCareer;
    targetCareerKey = roadmapData.domainId || roadmapData.careerKey || roadmapData.targetCareer;
    careerMatch = roadmapData.matchPercentage || 100;
  } else if (quizResult) {
    if (quizResult.primaryRecommendation && quizResult.primaryRecommendation.name) {
      targetCareer = quizResult.primaryRecommendation.name;
      targetCareerKey = quizResult.primaryRecommendation.id || quizResult.primaryRecommendation.careerId || quizResult.primaryRecommendation.nodeId || quizResult.primaryRecommendation.name;
      careerMatch = quizResult.confidenceScore || quizResult.primaryRecommendation.matchPercent || 0;
    } else if (quizResult.recommendedCareer && (quizResult.recommendedCareer.careerName || quizResult.recommendedCareer.name)) {
      targetCareer = quizResult.recommendedCareer.careerName || quizResult.recommendedCareer.name;
      targetCareerKey = quizResult.recommendedCareer.nodeId || quizResult.recommendedCareer.id || targetCareer;
      careerMatch = quizResult.recommendedCareer.score || quizResult.confidenceScore || 0;
    } else if (quizResult.recommendedDegree && (quizResult.recommendedDegree.careerName || quizResult.recommendedDegree.name)) {
      targetCareer = quizResult.recommendedDegree.careerName || quizResult.recommendedDegree.name;
      targetCareerKey = quizResult.recommendedDegree.nodeId || quizResult.recommendedDegree.id || targetCareer;
      careerMatch = quizResult.recommendedDegree.score || quizResult.confidenceScore || 0;
    } else if (quizResult.bestMatchName) {
      targetCareer = quizResult.bestMatchName;
      targetCareerKey = quizResult.primaryTaxonomyNodeId || quizResult.bestMatchCareerId || quizResult.bestMatchName;
      careerMatch = quizResult.confidenceScore || 0;
    } else {
      let predictions = [];
      try {
        predictions = JSON.parse(quizResult.topPredictionsJson || '[]');
      } catch(e) {}
      
      if (predictions.length > 0) {
        targetCareer = predictions[0].name || predictions[0].nodeId;
        targetCareerKey = predictions[0].nodeId || predictions[0].id || targetCareer;
        careerMatch = predictions[0].matchPercent || predictions[0].probability * 100 || 0;
      } else {
        try {
          let subPredictions = JSON.parse(quizResult.topSubdomainsJson || '[]');
          if (subPredictions.length > 0) {
            targetCareer = subPredictions[0].name || subPredictions[0].nodeId;
            targetCareerKey = subPredictions[0].nodeId || subPredictions[0].id || targetCareer;
            careerMatch = subPredictions[0].matchPercent || subPredictions[0].probability * 100 || 0;
          }
        } catch(e) {}
      }
    }
  }
  if (careerSwitch?.selectedCareer) {
    targetCareer = careerSwitch.selectedCareer.title;
    targetCareerKey = careerSwitch.selectedCareer.id || careerSwitch.selectedCareer.careerId || careerSwitch.selectedCareer.title;
    careerMatch = careerSwitch.selectedCareer.compatibilityScore;
  }
  if (learningQuiz?.selectedCareer && !careerSwitch?.selectedCareer) {
    targetCareer = learningQuiz.selectedCareer.title || learningQuiz.selectedCareer.name || targetCareer;
    targetCareerKey = learningQuiz.selectedCareer.id || learningQuiz.selectedCareer.careerId || targetCareerKey || targetCareer;
    careerMatch = learningQuiz.selectedCareer.matchScore || learningQuiz.result?.confidenceScore || careerMatch;
  }
  if (governmentLearning?.selectedCareer && !careerSwitch?.selectedCareer) {
    targetCareer = governmentLearning.selectedCareer.title;
    targetCareerKey = governmentLearning.selectedCareer.id || governmentLearning.selectedCareer.title;
    careerMatch = governmentLearning.selectedCareer.score;
  }

  const activePlan = [
    careerSwitch?.selectedCareer && { type: 'switch', data: careerSwitch, date: careerSwitch.acceptedAt || careerSwitch.savedAt },
    learningQuiz?.selectedCareer && { type: 'quiz', data: learningQuiz, date: learningQuiz.savedAt },
    governmentLearning?.selectedCareer && { type: 'government', data: governmentLearning, date: governmentLearning.savedAt },
  ].filter(Boolean).sort((a, b) => new Date(b.date || 0) - new Date(a.date || 0))[0];
  if (activePlan) {
    const career = activePlan.data.selectedCareer;
    targetCareer = career.title || career.name || targetCareer;
    targetCareerKey = career.id || career.careerId || career.title || career.name || targetCareerKey;
    careerMatch = career.compatibilityScore || career.matchScore || career.score || careerMatch;
  }
  if (skillAnalyzerAnalysis) {
    targetCareer = skillAnalyzerAnalysis.targetCareerTitle || skillAnalyzerAnalysis.careerName || targetCareer;
    targetCareerKey = skillAnalyzerAnalysis.targetCareerId || skillAnalyzerAnalysis.careerId || targetCareerKey;
    careerMatch = skillAnalyzerAnalysis.overallReadinessScore ?? careerMatch;
  }

  // The latest completed adaptive quiz is the freshest career prediction.
  // Keep older saved roadmaps available, but don't let one mask a new result.
  const quizPrimary = quizResult?.primaryRecommendation || quizResult?.PrimaryRecommendation ||
    quizResult?.recommendedCareer || quizResult?.RecommendedCareer ||
    quizResult?.recommendedDegree || quizResult?.RecommendedDegree ||
    quizResult?.recommendations?.[0] || quizResult?.Recommendations?.[0] ||
    quizResult?.targetCareerComparison?.[0] || quizResult?.TargetCareerComparison?.[0];
  const latestQuizCareer = quizPrimary?.title || quizPrimary?.Title || quizPrimary?.name || quizPrimary?.Name ||
    quizPrimary?.careerName || quizPrimary?.CareerName || quizResult?.bestMatchName || quizResult?.BestMatchName || '';
  if (latestQuizCareer) {
    targetCareer = latestQuizCareer;
    targetCareerKey = quizPrimary?.id || quizPrimary?.Id || quizPrimary?.nodeId || quizPrimary?.NodeId ||
      quizResult?.primaryTaxonomyNodeId || quizResult?.PrimaryTaxonomyNodeId || latestQuizCareer;
    careerMatch = quizResult?.confidenceScore ?? quizResult?.ConfidenceScore ??
      quizPrimary?.matchPercent ?? quizPrimary?.MatchPercent ?? quizPrimary?.score ?? quizPrimary?.Score ?? careerMatch;
  }

  // A starred roadmap is the user's explicit active career choice. It takes
  // priority over older quiz, learning-plan, and analysis suggestions.
  const starredCareerTitle = defaultSavedRoadmap?.careerTitle || defaultSavedRoadmap?.CareerTitle;
  const starredCareerKey = defaultSavedRoadmap?.careerKey || defaultSavedRoadmap?.CareerKey;
  if (starredCareerTitle || starredCareerKey) {
    targetCareer = starredCareerTitle || starredCareerKey;
    targetCareerKey = starredCareerKey || starredCareerTitle;
  }

  const roadmapLink = (targetCareer && targetCareer !== 'Undecided')
    ? `/roadmap?domain=${encodeURIComponent(targetCareerKey || targetCareer)}`
    : '/roadmap';

  const hour = new Date().getHours();
  const greeting = hour < 12 ? 'Good morning' : hour < 17 ? 'Good afternoon' : 'Good evening';
  const roadmapSkills = roadmapData?.skills || [];
  const completedSkills = roadmapSkills.filter(skill => skill.isCompleted).length;
  const roadmapSkillReadiness = roadmapSkills.length
    ? Math.round((completedSkills / roadmapSkills.length) * 100)
    : (roadmapData?.skillReadinessPercentage ?? null);
  const skillReadiness = skillAnalyzerAnalysis?.overallReadinessScore ?? roadmapSkillReadiness;
  const roadmapProgress = roadmapData?.progressPercentage ?? null;

  const snapshotCards = [
    { label: 'Career Direction', value: targetCareer, icon: Target, color: '#2563EB' },
    { label: 'Skill Readiness', value: skillReadiness === null ? 'Not assessed' : `${skillReadiness}%`, icon: Brain, color: '#4F46E5' },
    { label: 'Roadmap Progress', value: roadmapProgress === null ? 'Not started' : `${Math.round(roadmapProgress)}%`, icon: Map, color: '#2563EB' },
    { label: 'Career Match', value: careerMatch ? `${Math.round(careerMatch)}%` : 'Not assessed', icon: TrendingUp, color: '#4F46E5' },
  ];

  const steps = [
    { num: '01', title: 'Complete Career Assessment', done: !!quizResult || !!roadmapData, link: '/quiz' },
    { num: '02', title: 'Review Career Match', done: !!quizResult || !!roadmapData, link: '/career-explorer' },
    { num: '03', title: 'Start Your Roadmap', done: !!roadmapData && (roadmapProgress || 0) > 0, link: roadmapLink },
    { num: '04', title: 'Build Required Skills', done: !!skillAnalyzerAnalysis, link: '/placement' },
  ];

  return (
    <motion.div
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="max-w-5xl mx-auto space-y-10 pb-12"
    >
      {/* ── Hero Section ── */}
      <motion.div variants={itemVariants}>
        <h1 className="text-[32px] sm:text-[36px] font-bold text-[#0F172A] tracking-tight leading-tight">
          {greeting}, {firstName}.
        </h1>
        <p className="text-[17px] text-[#64748B] mt-2 max-w-xl leading-relaxed">
          Your career journey starts with the next step. Continue building the skills needed for your target career.
        </p>
        <div className="flex flex-wrap gap-3 mt-5">
          <Link to={roadmapLink}>
            <Button className="bg-[#2563EB] hover:bg-[#1D4ED8] text-white font-medium rounded-md h-10 px-5 text-sm">
              Continue Learning
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          </Link>
          <Link to="/career-explorer">
            <Button variant="outline" className="border-[#E2E8F0] text-[#475569] hover:bg-[#F1F5F9] font-medium rounded-md h-10 px-5 text-sm">
              Explore Careers
            </Button>
          </Link>
          <Link to="/learning-plans">
            <Button variant="outline" className="border-[#E2E8F0] text-[#475569] hover:bg-[#F1F5F9] font-medium rounded-md h-10 px-5 text-sm">My learning plans</Button>
          </Link>
          <Link to="/placement">
            <Button variant="outline" className="border-[#E2E8F0] text-[#475569] hover:bg-[#F1F5F9] font-medium rounded-md h-10 px-5 text-sm">Analyze your skills</Button>
          </Link>
        </div>
      </motion.div>

      {/* ── Career Snapshot ── */}
      <motion.div variants={itemVariants}>
        <h2 className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest mb-4">Your Career Snapshot</h2>
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          {snapshotCards.map((card) => {
            const isClickable = (card.label === 'Career Direction' || card.label === 'Roadmap Progress') && targetCareer !== 'Undecided';
            const cardEl = (
              <div
                key={card.label}
                className={`bg-white border border-[#E2E8F0] rounded-[10px] p-4 transition-all ${
                  isClickable ? 'hover:border-[#2563EB] hover:shadow-sm cursor-pointer' : 'hover:border-[#DBEAFE]'
                }`}
              >
                <div className="flex items-center gap-2 mb-3">
                  <div className="w-8 h-8 rounded-md bg-[#EFF6FF] flex items-center justify-center">
                    <card.icon className="h-4 w-4" style={{ color: card.color }} />
                  </div>
                </div>
                <p className="text-xs text-[#64748B] font-medium">{card.label}</p>
                <p className="text-lg font-bold text-[#0F172A] mt-0.5 truncate">{card.value}</p>
              </div>
            );
            return isClickable ? (
              <Link key={card.label} to={roadmapLink} className="block">
                {cardEl}
              </Link>
            ) : cardEl;
          })}
        </div>
      </motion.div>

      {/* ── Next Step Editorial Block ── */}
      {activePlan?.type === 'switch' && (
        <motion.div variants={itemVariants} className="bg-white border border-[#E2E8F0] rounded-[10px] p-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-5">
            <div><p className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest mb-2">Career switch</p><h2 className="text-xl font-bold text-[#0F172A]">{careerSwitch.selectedCareer ? `${careerSwitch.selectedCareer.title} learning path` : 'Your Career Switch results are ready'}</h2><p className="text-sm text-[#64748B] mt-1">{careerSwitch.selectedCareer ? 'Your selected path now guides your dashboard and roadmap.' : 'Review every answer or choose a recommended direction.'}</p></div>
            <div className="flex flex-wrap gap-2"><Link to="/career-switch/answers"><Button variant="outline" className="border-[#E2E8F0] text-[#475569]"><ClipboardCheck className="w-4 h-4 mr-2" />Review answers</Button></Link><Link to={careerSwitch.selectedCareer ? roadmapLink : '/career-switch/results'}><Button className="bg-[#2563EB]"><Map className="w-4 h-4 mr-2" />{careerSwitch.selectedCareer ? 'Open learning path' : 'Choose a path'}</Button></Link></div>
          </div>
        </motion.div>
      )}

      {activePlan?.type === 'quiz' && (
        <motion.div variants={itemVariants} className="bg-white border border-[#E2E8F0] rounded-[10px] p-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-5">
            <div><p className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest mb-2">Saved quiz result</p><h2 className="text-xl font-bold text-[#0F172A]">{learningQuiz.selectedCareer?.title || learningQuiz.selectedCareer?.name || 'Learning recommendation'}</h2><p className="text-sm text-[#64748B] mt-1">Added from your {learningQuiz.assessmentType} quiz. Continue building the recommended skills.</p></div>
            <div className="flex gap-2"><Link to={roadmapLink}><Button className="bg-[#2563EB]"><BookOpen className="w-4 h-4 mr-2" />Continue learning</Button></Link><Link to="/quiz"><Button variant="outline" className="border-[#E2E8F0] text-[#475569]">Retake quiz</Button></Link></div>
          </div>
        </motion.div>
      )}

      {activePlan?.type === 'government' && (
        <motion.div variants={itemVariants} className="bg-white border border-[#E2E8F0] rounded-[10px] p-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-5"><div><p className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest mb-2">Government career plan</p><h2 className="text-xl font-bold text-[#0F172A]">{governmentLearning.selectedCareer.title}</h2><p className="text-sm text-[#64748B] mt-1">{governmentLearning.selectedCareer.exam} · {governmentLearning.selectedCareer.score}% career fit</p></div><Link to={roadmapLink}><Button className="bg-[#2563EB]"><BookOpen className="w-4 h-4 mr-2"/>Continue learning</Button></Link></div>
        </motion.div>
      )}

      {skillAnalyzerAnalysis && (
        <motion.div variants={itemVariants} className="bg-white border border-[#DBEAFE] rounded-[10px] p-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-5">
            <div>
              <p className="text-xs font-semibold text-[#2563EB] uppercase tracking-widest mb-2">Latest skill analysis</p>
              <h2 className="text-xl font-bold text-[#0F172A]">{skillAnalyzerAnalysis.targetCareerTitle || skillAnalyzerAnalysis.careerName || 'Career readiness analysis'}</h2>
              <p className="text-sm text-[#64748B] mt-1">{skillAnalyzerAnalysis.overallReadinessScore ?? 0}% readiness · {skillAnalyzerAnalysis.matchedSkills?.length ?? 0} matched skills · {skillAnalyzerAnalysis.missingSkills?.length ?? 0} skills to build</p>
            </div>
            <div className="flex flex-wrap gap-2">
              {skillAnalyzerPlan && <Link to={roadmapLink}><Button className="bg-[#2563EB]"><Map className="w-4 h-4 mr-2" />Open roadmap</Button></Link>}
              <Link to="/placement"><Button variant="outline" className="border-[#E2E8F0] text-[#475569]"><RefreshCw className="w-4 h-4 mr-2" />Retake skill analysis</Button></Link>
            </div>
          </div>
        </motion.div>
      )}

      {targetCareer !== 'Undecided' && (
        <motion.div variants={itemVariants} className="bg-white border border-[#E2E8F0] rounded-[10px] p-6">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest">
              {roadmapData ? 'Your Next Step' : 'Your Learning Path'}
            </h3>
            <span className="text-xs text-[#94A3B8] flex items-center gap-1">
              <Clock className="w-3.5 h-3.5" /> Est. 45 minutes
            </span>
          </div>
          <h2 className="text-xl font-bold text-[#0F172A] mb-1.5">
            {roadmapData ? 'Introduction to Core Concepts' : `${targetCareer} Roadmap`}
          </h2>
          <p className="text-[15px] text-[#64748B] mb-5 leading-relaxed">
            {roadmapData
              ? `Begin mastering the foundational principles required for ${targetCareer}. This module covers the basic syntax and theory you need before moving on to practical projects.`
              : `Continue your journey in ${targetCareer}. Open your personalized roadmap to master required skills, complete practical milestones, and track your progress.`}
          </p>
          <Link to={roadmapLink}>
            <Button className="bg-[#2563EB] hover:bg-[#1D4ED8] text-white rounded-md h-10 px-5 text-sm font-medium">
              {roadmapData ? 'Start Module' : 'Continue Learning'} <PlayCircle className="w-4 h-4 ml-2" />
            </Button>
          </Link>
        </motion.div>
      )}

      {/* ── Your Next Steps (Timeline) ── */}
      <motion.div variants={itemVariants}>
        <h2 className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest mb-5">Your Next Steps</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
          {steps.map((step, i) => (
            <Link
              key={step.num}
              to={step.link}
              className="group bg-white border border-[#E2E8F0] rounded-[10px] p-4 hover:border-[#DBEAFE] hover:bg-[#FAFBFF] transition-all"
            >
              <div className="flex items-center gap-3 mb-2">
                <span className="text-2xl font-bold text-[#E2E8F0] group-hover:text-[#DBEAFE] transition-colors">{step.num}</span>
                {step.done && <CheckCircle2 className="w-4 h-4 text-[#2563EB]" />}
              </div>
              <p className="text-sm font-medium text-[#0F172A]">{step.title}</p>
              <ArrowRight className="w-4 h-4 text-[#94A3B8] group-hover:text-[#2563EB] mt-2 transition-colors" />
            </Link>
          ))}
        </div>
      </motion.div>

      {/* ── Recommended Actions ── */}
      <motion.div variants={itemVariants}>
        <h2 className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest mb-5">Recommended For You</h2>
        <div className="space-y-2">
          {[
            { to: '/upskilling', icon: Sparkles, title: 'Upskilling & Certifications', desc: 'Build skills with verified courses and certifications' },
            { to: `/salary-forecast${targetCareer !== 'Undecided' ? `?id=${encodeURIComponent(targetCareerKey || targetCareer)}` : ''}`, icon: TrendingUp, title: 'Market Salary Outlook', desc: 'Forecast your future earnings based on your path' },
            { to: '/career-explorer', icon: Compass, title: 'Explore Career Options', desc: `Discover roles related to ${targetCareer}` },
            { to: '/placement', icon: Trophy, title: 'Assess Your Skills', desc: 'Evaluate your readiness for your target career' },
          ].map((item) => (
            <Link
              key={item.to}
              to={item.to}
              className="group flex items-center justify-between p-4 rounded-[10px] border border-[#E2E8F0] bg-white hover:border-[#DBEAFE] hover:bg-[#FAFBFF] transition-all"
            >
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-md bg-[#EFF6FF] flex items-center justify-center group-hover:bg-[#DBEAFE] transition-colors">
                  <item.icon className="w-4 h-4 text-[#2563EB]" />
                </div>
                <div>
                  <h4 className="text-sm font-semibold text-[#0F172A]">{item.title}</h4>
                  <p className="text-xs text-[#64748B]">{item.desc}</p>
                </div>
              </div>
              <ArrowRight className="w-4 h-4 text-[#94A3B8] group-hover:text-[#2563EB] transition-colors" />
            </Link>
          ))}
        </div>
      </motion.div>
    </motion.div>
  );
};

export default DashboardPage;
