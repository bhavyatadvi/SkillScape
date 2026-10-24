import React from 'react';
import { motion } from 'framer-motion';
import { 
  ShieldCheck, Briefcase, GraduationCap, Code2, Sparkles, Award
} from 'lucide-react';

export const ReadinessScoreCard = ({ data }) => {
  const score = data.overallReadinessScore || 0;
  const label = data.readinessLabel || 'Early Stage';
  const baseline = data.baselineSummary;
  const ats = data.resumeAtsScore;

  // Determine accent colors based on score
  const getBadgeStyle = (sc) => {
    if (sc >= 80) return 'bg-emerald-50 border-emerald-200 text-emerald-700';
    if (sc >= 60) return 'bg-blue-50 border-blue-200 text-blue-700';
    if (sc >= 40) return 'bg-amber-50 border-amber-200 text-amber-700';
    return 'bg-rose-50 border-rose-200 text-rose-700';
  };

  const getScoreColor = () => {
    return '#2563eb'; // Brand Blue
  };

  const strokeDashoffset = 440 - (440 * score) / 100;

  return (
    <motion.div 
      initial={{ opacity: 0, scale: 0.99 }}
      animate={{ opacity: 1, scale: 1 }}
      className="bg-white border border-slate-200 rounded-xl p-8 shadow-sm space-y-8"
    >
      {/* Header Banner */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 pb-6">
        <div>
          <div className="flex items-center gap-2 mb-2">
            <span className="px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider bg-blue-50 border border-blue-200 text-blue-700">
              {data.targetLevel || 'Mid-Level'}
            </span>
            <span className="px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider bg-slate-100 border border-slate-200 text-slate-700">
              {data.targetWorkType || 'Full-Time'}
            </span>
            {baseline?.hasProfile && (
              <span className="px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider bg-blue-50 border border-blue-200 text-blue-700 flex items-center gap-1">
                <ShieldCheck className="w-3.5 h-3.5" /> Verified Profile
              </span>
            )}
          </div>
          <h1 className="text-3xl font-black text-slate-900 tracking-tight">
            {data.targetCareerTitle}
          </h1>
          <p className="text-slate-500 text-sm font-medium mt-0.5">
            Career Readiness & Skill Gap Intelligence Report
          </p>
        </div>

        <div className={`px-5 py-2.5 rounded-xl border text-sm font-bold flex items-center gap-2 shadow-sm ${getBadgeStyle(score)}`}>
          <Award className="w-5 h-5" />
          {label}
        </div>
      </div>

      {ats && (
        <div className="bg-violet-50 border border-violet-200 rounded-xl p-5 space-y-3">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <h3 className="font-bold text-violet-950 text-sm">ATS-Style Resume Match: {ats.score}%</h3>
              <p className="text-xs text-violet-800 mt-1">{ats.summary}</p>
            </div>
            <span className="px-3 py-1 rounded-full text-xs font-bold bg-white border border-violet-200 text-violet-800">{ats.label}</span>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-xs">
            <div><span className="font-bold text-violet-900">Matched keywords: </span>{(ats.matchedKeywords || []).join(', ') || 'None detected yet'}</div>
            <div><span className="font-bold text-violet-900">Add to your resume: </span>{(ats.missingKeywords || []).join(', ') || 'No critical keywords missing'}</div>
          </div>
          <p className="text-[11px] text-violet-700">This is a transparent role-match estimate from your parsed resume profile, not a score from a third-party ATS.</p>
        </div>
      )}

      {/* Main Readiness Gauge Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-center">
        {/* Score Dial Gauge (5 cols) */}
        <div className="lg:col-span-5 flex flex-col items-center justify-center p-6 bg-slate-50 rounded-xl border border-slate-200">
          <div className="relative w-48 h-48 flex items-center justify-center">
            <svg className="w-full h-full transform -rotate-90" viewBox="0 0 160 160">
              <circle
                cx="80"
                cy="80"
                r="70"
                className="stroke-slate-200"
                strokeWidth="12"
                fill="transparent"
              />
              <circle
                cx="80"
                cy="80"
                r="70"
                stroke={getScoreColor(score)}
                strokeWidth="12"
                strokeDasharray="440"
                strokeDashoffset={strokeDashoffset}
                strokeLinecap="round"
                fill="transparent"
                className="transition-all duration-1000 ease-out"
              />
            </svg>
            <div className="absolute flex flex-col items-center text-center">
              <span className="text-5xl font-black text-slate-900 tracking-tight">{score}%</span>
              <span className="text-xs font-bold uppercase tracking-widest text-slate-500 mt-1">
                Readiness
              </span>
            </div>
          </div>

          <p className="text-xs text-slate-500 font-medium text-center mt-4 max-w-xs">
            Technical readiness score based on your verified skills. Experience and education fit are shown separately.
          </p>
        </div>

        {/* Breakdown Meters (7 cols) */}
        <div className="lg:col-span-7 space-y-6">
          <h3 className="text-lg font-bold text-slate-900 flex items-center gap-2">
            <Sparkles className="w-5 h-5 text-blue-600" />
            Readiness Breakdown
          </h3>

          {/* Technical Skill Alignment */}
          <div className="space-y-2">
            <div className="flex justify-between text-sm">
              <span className="font-semibold text-slate-700 flex items-center gap-2">
                <Code2 className="w-4 h-4 text-blue-600" /> Technical Skill Alignment
              </span>
              <span className="font-bold text-blue-600">{data.skillAlignmentScore}%</span>
            </div>
            <div className="h-3 w-full bg-slate-100 rounded-full overflow-hidden">
              <motion.div
                className="h-full bg-blue-600 rounded-full"
                initial={{ width: 0 }}
                animate={{ width: `${data.skillAlignmentScore}%` }}
                transition={{ duration: 1 }}
              />
            </div>
          </div>

          {/* Experience Match */}
          <div className="space-y-2">
            <div className="flex justify-between text-sm">
              <span className="font-semibold text-slate-700 flex items-center gap-2">
                <Briefcase className="w-4 h-4 text-blue-600" /> Experience Fit ({baseline?.yearsOfExperience || 0} YOE)
              </span>
              <span className="font-bold text-blue-600">{data.experienceFitScore}%</span>
            </div>
            <div className="h-3 w-full bg-slate-100 rounded-full overflow-hidden">
              <motion.div
                className="h-full bg-blue-600 rounded-full"
                initial={{ width: 0 }}
                animate={{ width: `${data.experienceFitScore}%` }}
                transition={{ duration: 1, delay: 0.1 }}
              />
            </div>
          </div>

          {/* Education Match */}
          <div className="space-y-2">
            <div className="flex justify-between text-sm">
              <span className="font-semibold text-slate-700 flex items-center gap-2">
                <GraduationCap className="w-4 h-4 text-blue-600" /> Education Match ({baseline?.degree || 'Degree Baseline'})
              </span>
              <span className="font-bold text-blue-600">{data.educationFitScore}%</span>
            </div>
            <div className="h-3 w-full bg-slate-100 rounded-full overflow-hidden">
              <motion.div
                className="h-full bg-blue-600 rounded-full"
                initial={{ width: 0 }}
                animate={{ width: `${data.educationFitScore}%` }}
                transition={{ duration: 1, delay: 0.2 }}
              />
            </div>
          </div>

          {/* Baseline Summary Pill Grid */}
          <div className="pt-4 border-t border-slate-200 grid grid-cols-3 gap-3">
            <div className="bg-slate-50 p-3 rounded-xl border border-slate-200 text-center">
              <span className="text-xs text-slate-500 font-medium block">Extracted Skills</span>
              <span className="text-lg font-black text-slate-900">{baseline?.extractedSkillsCount || 0}</span>
            </div>
            <div className="bg-slate-50 p-3 rounded-xl border border-slate-200 text-center">
              <span className="text-xs text-slate-500 font-medium block">Projects Built</span>
              <span className="text-lg font-black text-slate-900">{baseline?.projectsCount || 0}</span>
            </div>
            <div className="bg-slate-50 p-3 rounded-xl border border-slate-200 text-center">
              <span className="text-xs text-slate-500 font-medium block">Education Level</span>
              <span className="text-xs font-bold text-blue-600 truncate block mt-1">
                {baseline?.degree || 'Undergrad'}
              </span>
            </div>
          </div>
        </div>
      </div>
    </motion.div>
  );
};

export default ReadinessScoreCard;
