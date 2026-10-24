import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { toast } from 'sonner';
import { 
  Trophy, UserCheck, AlertCircle, ArrowRight, Loader2, Sparkles, RefreshCw, User, Map
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { userStorageKey } from '@/lib/userStorage';
import PageHeader from '@/components/layout/PageHeader';
import { skillAnalyzerApi } from '@/services/api/skillAnalyzer';
import { roadmapApi } from '@/services/api';
import TargetCareerForm from './TargetCareerForm';
import ReadinessScoreCard from './ReadinessScoreCard';
import SkillGapTable from './SkillGapTable';
import LearningRoadmapTimeline from './LearningRoadmapTimeline';
import StrategicAdviceCard from './StrategicAdviceCard';

// Increment this whenever analyzer result semantics change. Old browser-cached
// reports must not be presented as if they were calculated by the current engine.
const ANALYSIS_RESULT_VERSION = 4;

export const SkillAnalyzerContainer = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [profileStatus, setProfileStatus] = useState(null);
  const [checkingStatus, setCheckingStatus] = useState(true);
  const [loadingAnalysis, setLoadingAnalysis] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [analysisResult, setAnalysisResult] = useState(null);

  useEffect(() => {
    // Reopen the latest result for this account when the learner returns to
    // Skill Analyzer. This key is deliberately separate from all quiz data.
    try {
      const savedResult = JSON.parse(
        window.localStorage.getItem(userStorageKey('skillscape-skill-analyzer-analysis', user)) || 'null'
      );
      if (savedResult?.analysisVersion === ANALYSIS_RESULT_VERSION && (savedResult?.targetCareerTitle || savedResult?.careerName)) {
        setAnalysisResult(savedResult);
      } else if (savedResult) {
        window.localStorage.removeItem(userStorageKey('skillscape-skill-analyzer-analysis', user));
      }
    } catch {
      // An invalid browser value should fall back to a fresh analysis form.
      setAnalysisResult(null);
    }
    checkProfileStatus();
  }, [user?.id, user?.userId, user?.email]);

  const checkProfileStatus = async () => {
    try {
      setCheckingStatus(true);
      const res = await skillAnalyzerApi.getProfileStatus();
      if (res.data?.success) {
        const data = res.data.data;
        setProfileStatus(data);
        if (!data?.hasProfile) {
          setAnalysisResult(null);
          try {
            window.localStorage.removeItem(userStorageKey('skillscape-skill-analyzer-analysis', user));
            window.localStorage.removeItem(userStorageKey('skillscape-skill-analyzer-roadmap', user));
          } catch {}
        }
      }
    } catch (err) {
      console.error('Error fetching profile status:', err);
    } finally {
      setCheckingStatus(false);
    }
  };

  const handleRunAnalysis = async (formData) => {
    try {
      setLoadingAnalysis(true);
      const res = await skillAnalyzerApi.analyze(formData);
      if (res.data?.success) {
        const result = res.data.data;
        setAnalysisResult(result);
        // Keep the latest analysis available to the dashboard before it is saved as a roadmap.
        window.localStorage.setItem(userStorageKey('skillscape-skill-analyzer-analysis', user), JSON.stringify({
          ...result,
          analysisVersion: ANALYSIS_RESULT_VERSION,
          analyzedAt: result.analyzedAt || new Date().toISOString(),
        }));
        toast.success('Career readiness & skill gap analysis completed!');
      } else {
        toast.error(res.data?.error || 'Analysis failed.');
      }
    } catch (err) {
      toast.error(err.response?.data?.error || 'Failed to analyze career readiness.');
    } finally {
      setLoadingAnalysis(false);
    }
  };

  const handleReset = () => {
    setAnalysisResult(null);
    // Retake always means a fresh server calculation. Do not let an old result
    // return after a refresh before the learner submits the new target role.
    try {
      window.localStorage.removeItem(userStorageKey('skillscape-skill-analyzer-analysis', user));
    } catch {}
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleAddToRoadmap = async () => {
    if (!analysisResult || isSaving) return;

    const targetCareer = analysisResult.targetCareerTitle || analysisResult.careerName || 'Target Career';
    const rawKey = analysisResult.careerId || targetCareer;
    const careerKey = rawKey.replace(/^career_/, '').replace(/[\s_]+/g, '-').toLowerCase();

    setIsSaving(true);
    try {
      try {
        await roadmapApi.acceptCareerRoadmap(careerKey, targetCareer);
      } catch (apiErr) {
        console.warn('acceptCareerRoadmap failed, attempting fallback save:', apiErr);
        try {
          await roadmapApi.saveMyRoadmap(careerKey, targetCareer);
          await roadmapApi.setDefaultMyRoadmap(careerKey);
        } catch (fallbackErr) {
          console.warn('Fallback save error:', fallbackErr);
        }
      }

      const savedAt = new Date().toISOString();
      const plan = {
        id: `${careerKey}-${Date.now()}`,
        savedAt,
        assessmentType: 'Skill Analyzer',
        selectedCareer: {
          id: careerKey,
          title: targetCareer,
          matchScore: analysisResult.overallReadinessScore || 0,
          expectedTimeToJobReady: analysisResult.timeline || '',
        },
        result: analysisResult,
      };

      const plansKey = userStorageKey('skillscape-quiz-learning-plans', user);
      let existingPlans = [];
      try {
        const stored = JSON.parse(window.localStorage.getItem(plansKey) || '[]');
        existingPlans = Array.isArray(stored) ? stored : [];
      } catch {}

      const updatedPlans = [plan, ...existingPlans.filter(existing =>
        !(existing.assessmentType === 'Skill Analyzer' &&
          (existing.selectedCareer?.id === plan.selectedCareer.id || existing.selectedCareer?.title === plan.selectedCareer.title))
      )];
      window.localStorage.setItem(plansKey, JSON.stringify(updatedPlans));
      window.localStorage.setItem(userStorageKey('skillscape-skill-analyzer-roadmap', user), JSON.stringify(plan));

      toast.success(`${targetCareer} added to My Roadmaps as your active roadmap!`);
      navigate(`/roadmap?domain=${encodeURIComponent(careerKey)}`);
    } catch (err) {
      console.error('Failed to add to roadmap:', err);
      toast.error('Failed to save to My Roadmaps. Please try again.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="w-full pb-16">
      <div className="w-full mx-auto px-4 space-y-8">
        
        {/* Page Header & Integrated Career Profile Status */}
        <div className="border-b border-slate-200 pb-8">
          <PageHeader
            icon={Trophy}
            title="Career Readiness & Skill Gap Analyzer"
            description="Profile-driven intelligence engine that evaluates your verified skills against target career requirements to reveal readiness, skill gaps, and learning roadmaps."
            className="mb-0"
          />

          {/* Career Profile Status Card */}
          {!checkingStatus && profileStatus && (
            <motion.div 
              initial={{ opacity: 0, y: -8 }}
              animate={{ opacity: 1, y: 0 }}
              className="mt-6 bg-white border border-slate-200/90 rounded-xl p-4 shadow-sm flex flex-wrap items-center justify-between gap-4"
            >
              <div className="flex items-center gap-3.5">
                <div className={`w-10 h-10 rounded-lg flex items-center justify-center shrink-0 ${
                  profileStatus.hasProfile 
                    ? 'bg-emerald-50 text-emerald-600 border border-emerald-100' 
                    : 'bg-amber-50 text-amber-600 border border-amber-100'
                }`}>
                  {profileStatus.hasProfile ? <UserCheck className="w-5 h-5" /> : <AlertCircle className="w-5 h-5" />}
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <span className="font-bold text-slate-900 text-sm">
                      {profileStatus.hasProfile ? 'Central Career Profile Active' : 'Career Profile Missing'}
                    </span>
                    {profileStatus.hasProfile && (
                      <span className="bg-emerald-50 text-emerald-700 text-[11px] font-bold px-2 py-0.5 rounded border border-emerald-200">
                        {profileStatus.completenessScore ?? profileStatus.profileCompletionPercent ?? 0}% Complete
                      </span>
                    )}
                  </div>
                  <p className="text-xs text-slate-500 font-medium mt-0.5">
                    {profileStatus.hasProfile 
                      ? `Verified baseline: ${profileStatus.extractedSkillsCount ?? 0} extracted skills • ${profileStatus.yearsOfExperience ?? 0} YOE • ${profileStatus.degree || profileStatus.broadField || 'Undergrad'}`
                      : 'Setup your Career Profile to enable personalized evidence-based gap analysis.'}
                  </p>
                </div>
              </div>

              <Button
                onClick={() => navigate('/career-profile')}
                variant="outline"
                size="sm"
                className="border-slate-200 bg-slate-50 hover:bg-slate-100 text-slate-700 font-semibold text-xs flex items-center gap-1.5 shadow-sm"
              >
                <User className="w-3.5 h-3.5 text-slate-500" />
                {profileStatus.hasProfile ? 'Manage Profile' : 'Create Profile'}
                <ArrowRight className="w-3.5 h-3.5 text-slate-400" />
              </Button>
            </motion.div>
          )}
        </div>

        {/* Main Content Area */}
        <div>
          {checkingStatus ? (
            <div className="flex flex-col items-center justify-center py-24 gap-3">
              <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
              <p className="text-slate-500 font-medium text-sm">Loading career intelligence engine...</p>
            </div>
          ) : !analysisResult ? (
            <TargetCareerForm 
              onAnalyze={handleRunAnalysis} 
              loading={loadingAnalysis} 
              profileStatus={profileStatus}
            />
          ) : (
            <AnimatePresence mode="wait">
              <motion.div 
                key="results"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="space-y-8"
              >


                {/* 1. Readiness Score Card */}
                <ReadinessScoreCard data={analysisResult} />

                {/* 2. Detailed Skill Gap Matrix */}
                <SkillGapTable 
                  matchedSkills={analysisResult.matchedSkills} 
                  missingSkills={analysisResult.missingSkills} 
                />

                {/* 3. Skill Gap Bridging Roadmap */}
                <LearningRoadmapTimeline 
                  roadmap={analysisResult.skillGapRoadmap} 
                />

                <div className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
                  <div>
                    <h2 className="font-bold text-slate-900">Ready to follow this plan?</h2>
                    <p className="mt-1 text-sm text-slate-600">Save this personalized skill-gap roadmap to access it from your Roadmap and Dashboard.</p>
                  </div>
                  <Button onClick={handleAddToRoadmap} disabled={isSaving} className="bg-blue-600 text-white hover:bg-blue-700 font-semibold shadow-sm">
                    {isSaving ? (
                      <><Loader2 className="mr-2 h-4 w-4 animate-spin" /> Saving...</>
                    ) : (
                      <><Map className="mr-2 h-4 w-4" /> Add to My Roadmap &amp; Start Learning</>
                    )}
                  </Button>
                </div>

                {/* 4. Strategic Advice & AI Guidance */}
                <StrategicAdviceCard 
                  data={analysisResult} 
                  onReset={handleReset} 
                />

                <div className="flex flex-col items-center rounded-xl border border-slate-200 bg-white p-6 text-center shadow-sm">
                  <h2 className="font-bold text-slate-900">Want to analyze a different career goal?</h2>
                  <p className="mt-1 text-sm text-slate-600">Retake the Skill Analyzer with a new field, specialization, or target role.</p>
                  <Button onClick={handleReset} variant="outline" className="mt-4 border-slate-200 bg-slate-50 text-slate-700 hover:bg-slate-100 font-semibold">
                    <RefreshCw className="mr-2 h-4 w-4" /> Retake skill analysis
                  </Button>
                </div>
              </motion.div>
            </AnimatePresence>
          )}
        </div>
      </div>
    </div>
  );
};

export default SkillAnalyzerContainer;
