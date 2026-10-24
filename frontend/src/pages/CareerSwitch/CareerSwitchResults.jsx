import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/context/AuthContext';
import { userStorageKey } from '@/lib/userStorage';
import { roadmapApi } from '@/services/api';
import { toast } from 'sonner';
import {
  CheckCircle2, XCircle, ChevronDown, ChevronUp,
  ArrowRight, BookOpen, Briefcase, RotateCcw, Star,
  TrendingUp, Zap, AlertCircle, MapPin, Trophy, Target,
  Map, Loader2
} from 'lucide-react';
import {
  calculateTransferableSkills,
  calculateMissingSkills,
  generateMatchReasons,
  generateRoadmap,
  getTransitionDifficulty,
} from '@/data/scoringEngine';

// ---- Utility Components ----

function ScoreBadge({ score }) {
  return (
    <div className="bg-[#EFF6FF] border border-[#DBEAFE] text-[#2563EB] font-semibold text-xs rounded px-2 py-1 whitespace-nowrap">
      {score}%
    </div>
  );
}

function DifficultyBadge({ difficulty }) {
  const styles = {
    Easy: 'bg-[#F8FAFC] text-[#475569] border-[#E2E8F0]',
    Moderate: 'bg-[#F8FAFC] text-[#475569] border-[#E2E8F0]',
    Challenging: 'bg-[#F8FAFC] text-[#475569] border-[#E2E8F0]',
    Difficult: 'bg-[#F8FAFC] text-[#475569] border-[#E2E8F0]',
  };
  return (
    <span className={`px-2 py-0.5 rounded text-xs font-medium border ${styles[difficulty] || styles.Moderate}`}>
      {difficulty}
    </span>
  );
}

function DistanceBadge({ distance }) {
  const labels = ['Same', 'Adjacent', 'Transferable', 'Moderate', 'Major', 'Complete Change'];
  const label = labels[Math.min(distance, 5)] || 'Unknown';
  return (
    <span className="px-2 py-0.5 rounded text-xs font-medium bg-[#F8FAFC] text-[#475569] border border-[#E2E8F0]">
      {label} ({distance}/5)
    </span>
  );
}

function ConfidenceBadge({ confidence }) {
  const styles = {
    High: 'bg-[#EFF6FF] text-[#2563EB]',
    Moderate: 'bg-[#EFF6FF] text-[#2563EB]',
    Low: 'bg-[#F8FAFC] text-[#475569]',
  };
  return (
    <span className={`px-3 py-1 rounded-md text-sm font-semibold ${styles[confidence]}`}>
      {confidence} Confidence
    </span>
  );
}

// ---- Section: Hero Career Card ----

function PrimaryCareerCard({
  career,
  careerProfile,
  dimensionScores,
  preQuizAnswers,
  confidence,
  onAccept,
  isSaving,
  isAccepted,
  onViewRoadmap
}) {
  const reasons = generateMatchReasons(career, dimensionScores, preQuizAnswers, careerProfile);
  const transferable = calculateTransferableSkills(career, careerProfile, preQuizAnswers);
  const missing = calculateMissingSkills(career, careerProfile);
  const difficulty = getTransitionDifficulty(career, careerProfile);

  return (
    <div className="mb-8 text-center">
      <div className="inline-flex items-center gap-2 bg-[#EFF6FF] border border-[#DBEAFE] text-[#2563EB] text-xs font-semibold uppercase tracking-widest px-4 py-2 rounded-full mb-5">
        <Trophy className="w-4 h-4" /> Your best career match
      </div>
      <div>
          <p className="text-[#2563EB] text-xs font-semibold uppercase tracking-widest mb-1">Recommended career direction</p>
          <h2 className="text-4xl font-bold text-[#0F172A]">{career.title}</h2>
          <p className="text-[#64748B] mt-2 max-w-2xl mx-auto">{career.description}</p>
      </div>
      <div className="flex items-center justify-center gap-2 mt-5">
        <div className="text-5xl font-bold text-[#2563EB]">{career.compatibilityScore}<span className="text-2xl">%</span></div>
        <div className="text-left"><div className="font-semibold text-[#0F172A]">Compatibility</div><div className="text-sm text-[#64748B]">{confidence} confidence</div></div>
      </div>
      <div className="max-w-sm mx-auto h-2 bg-[#E2E8F0] rounded-full mt-5"><div className="h-full bg-[#2563EB] rounded-full" style={{ width: `${career.compatibilityScore}%` }} /></div>
      <div className="flex flex-wrap justify-center gap-2 mt-5">
        <ConfidenceBadge confidence={confidence} />
        <span className="px-3 py-1 rounded-full text-sm font-semibold bg-slate-100 border border-slate-200 text-slate-700">
          Difficulty: {difficulty}
        </span>
        <span className="px-3 py-1 rounded-full text-sm font-semibold bg-slate-100 border border-slate-200 text-slate-700">
          Distance: {career.baseDistance}/5
        </span>
        {career.salaryRange && (
          <span className="px-3 py-1 rounded-full text-sm font-semibold bg-slate-100 border border-slate-200 text-slate-700">
            {career.salaryRange}
          </span>
        )}
      </div>

      <div className="max-w-2xl mx-auto bg-white border border-[#E2E8F0] rounded-[10px] p-5 mt-7 text-left">
        <p className="font-semibold text-[#0F172A] mb-3">Why this matches you</p>
        <div className="space-y-2">
        {reasons.slice(0, 3).map((reason, i) => (
          <div key={i} className="flex items-start gap-2">
            <CheckCircle2 className="w-4 h-4 text-[#2563EB] mt-0.5 flex-shrink-0" />
            <span className="text-sm text-[#334155]">{reason}</span>
          </div>
        ))}
        </div>
      </div>

      {/* Prominent Primary Call-to-Action */}
      <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
        <Button
          size="lg"
          disabled={isSaving}
          onClick={onAccept}
          className="bg-[#2563EB] hover:bg-[#1D4ED8] text-white font-semibold px-8 py-3.5 rounded-xl shadow-lg shadow-blue-600/25 flex items-center gap-2.5 text-base transition-all transform hover:-translate-y-0.5 cursor-pointer"
        >
          {isSaving ? (
            <>
              <Loader2 className="w-5 h-5 animate-spin mr-1" />
              Adding to My Roadmap...
            </>
          ) : (
            <>
              <Map className="w-5 h-5 mr-1" />
              {isAccepted ? 'Added to My Roadmap (View Now)' : 'Add to My Roadmap & Start Learning'}
              <ArrowRight className="w-4 h-4 ml-1" />
            </>
          )}
        </Button>
        <Button
          variant="outline"
          size="lg"
          onClick={onViewRoadmap}
          className="border-[#E2E8F0] text-[#334155] hover:bg-slate-50 font-medium px-6 py-3.5 rounded-xl text-base cursor-pointer"
        >
          View in Roadmap
        </Button>
      </div>
    </div>
  );
}

// ---- Section: Why This Matches ----

function WhyMatchSection({ career, careerProfile, dimensionScores, preQuizAnswers }) {
  const reasons = generateMatchReasons(career, dimensionScores, preQuizAnswers, careerProfile);
  return (
    <div className="bg-white rounded-[10px] border border-[#E2E8F0] p-6 mb-6">
      <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
        <Star className="w-5 h-5 text-[#2563EB]" /> Why This Matches You
      </h3>
      <div className="space-y-3">
        {reasons.map((reason, i) => (
          <div key={i} className="flex items-start gap-3">
            <CheckCircle2 className="w-5 h-5 text-[#2563EB] flex-shrink-0 mt-0.5" />
            <span className="text-[#475569]">{reason}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

// ---- Section: Transferable Skills + Skill Gap ----

function SkillsSection({ career, careerProfile, preQuizAnswers }) {
  const transferable = calculateTransferableSkills(career, careerProfile, preQuizAnswers);
  const missing = calculateMissingSkills(career, careerProfile);

  // Also show user's own skills that match
  const userSkills = careerProfile?.currentSkills || [];
  const haveSkills = userSkills.length > 0
    ? userSkills.slice(0, 5).map(s => s.name)
    : (career.requiredSkills || []).slice(0, 2).map(s => s);

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
      {/* Have */}
      <div className="bg-white rounded-[10px] border border-[#E2E8F0] p-6">
        <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
          <CheckCircle2 className="w-5 h-5 text-[#2563EB]" /> You Already Have
        </h3>
        <div className="space-y-2">
          {haveSkills.map((skill, i) => (
            <div key={i} className="flex items-center gap-2 bg-[#F8FAFC] text-[#475569] text-sm font-medium px-3 py-2 rounded-lg border border-[#E2E8F0]">
              <CheckCircle2 className="w-4 h-4 flex-shrink-0" /> {skill}
            </div>
          ))}
          {transferable.slice(0, 3).map((skill, i) => (
            <div key={`t-${i}`} className="flex items-center gap-2 bg-[#F8FAFC] text-[#475569] text-sm font-medium px-3 py-2 rounded-lg border border-[#E2E8F0]">
              <CheckCircle2 className="w-4 h-4 flex-shrink-0 text-[#2563EB]" /> {skill} <span className="text-[#64748B] text-xs">(transferable)</span>
            </div>
          ))}
        </div>
      </div>

      {/* Missing */}
      <div className="bg-white rounded-[10px] border border-[#E2E8F0] p-6">
        <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
          <Target className="w-5 h-5 text-[#2563EB]" /> Skill Gaps to Bridge
        </h3>
        <div className="space-y-2">
          {missing.map((item, i) => (
            <div key={i} className="flex items-start gap-2 bg-[#F8FAFC] text-[#475569] text-sm font-medium px-3 py-2 rounded-lg border border-[#E2E8F0]">
              <XCircle className="w-4 h-4 flex-shrink-0 mt-0.5 text-[#64748B]" />
              <div>
                <span>{item.skill}</span>
                {item.category && <span className="ml-2 text-xs text-amber-500">({item.category})</span>}
              </div>
            </div>
          ))}
          {missing.length === 0 && (
            <p className="text-emerald-700 text-sm">No major gaps detected! You are well-positioned for this career.</p>
          )}
        </div>
      </div>
    </div>
  );
}

// ---- Section: Alternative Careers ----

function AlternativeCareerCard({ career, careerProfile, isExpanded, onToggle, onSelect }) {
  const difficulty = getTransitionDifficulty(career, careerProfile);
  const missing = calculateMissingSkills(career, careerProfile);

  return (
    <div className={`bg-white rounded-lg border overflow-hidden transition-colors ${isExpanded ? 'border-[#93C5FD]' : 'border-[#E2E8F0] hover:border-[#93C5FD]'}`}>
      <div
        className="p-3 cursor-pointer hover:bg-[#FAFBFF] transition-colors"
        onClick={onToggle}
      >
        <div className="flex items-start gap-2.5">
          <ScoreBadge score={career.compatibilityScore} />
          <div className="min-w-0 flex-1">
            <h4 className="font-semibold text-sm text-[#0F172A] leading-snug truncate">{career.title}</h4>
            <div className="flex flex-wrap gap-1 mt-1.5">
              <DifficultyBadge difficulty={difficulty} />
              <DistanceBadge distance={career.baseDistance} />
            </div>
          </div>
          {isExpanded ? <ChevronUp className="w-4 h-4 text-[#64748B] mt-1 flex-shrink-0" /> : <ChevronDown className="w-4 h-4 text-[#64748B] mt-1 flex-shrink-0" />}
        </div>
      </div>

      {isExpanded && (
        <div className="p-3 pt-3 border-t border-[#E2E8F0]">
          <p className="text-[#64748B] text-sm mb-4">{career.description}</p>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
            <div>
              <p className="text-xs font-semibold text-slate-500 uppercase mb-2">Skill Gaps</p>
              {missing.slice(0, 3).map((m, i) => (
                <div key={i} className="text-sm text-[#475569] flex items-center gap-1.5 mb-1">
                  <XCircle className="w-3.5 h-3.5 text-[#64748B] flex-shrink-0" /> {m.skill}
                </div>
              ))}
            </div>
            <div>
              <p className="text-xs font-semibold text-slate-500 uppercase mb-2">Entry Roles</p>
              {(career.entryRoles || []).slice(0, 3).map((r, i) => (
                <div key={i} className="text-sm text-slate-700 flex items-center gap-1.5 mb-1">
                  <Briefcase className="w-3.5 h-3.5 text-slate-400 flex-shrink-0" /> {r}
                </div>
              ))}
            </div>
          </div>
          <div className="flex gap-2">
            <span className="text-xs bg-[#F8FAFC] px-2 py-1 rounded-md text-[#64748B] border border-[#E2E8F0]">
              {career.salaryRange || 'Salary varies'}
            </span>
            {(career.certifications || []).slice(0, 2).map((c, i) => (
              <span key={i} className="text-xs bg-[#EFF6FF] px-2 py-1 rounded-md text-[#2563EB] border border-[#DBEAFE]">{c}</span>
            ))}
          </div>
          <Button size="sm" onClick={(event) => { event.stopPropagation(); onSelect(); }} className="mt-4 bg-[#2563EB] hover:bg-[#1D4ED8] text-white rounded-lg text-xs h-9 px-4 font-semibold flex items-center gap-1.5 shadow-sm cursor-pointer">
            <Map className="w-3.5 h-3.5" /> Add to My Roadmap & Switch
          </Button>
        </div>
      )}
    </div>
  );
}

// ---- Section: Roadmap ----

function RoadmapSection({ career, careerProfile, onAccept, isSaving, isAccepted }) {
  const phases = generateRoadmap(career, careerProfile);
  return (
    <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6 mb-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
        <div>
          <h3 className="text-lg font-bold text-slate-900 flex items-center gap-2">
            <MapPin className="w-5 h-5 text-blue-600" /> Career Transition Pathway for {career.title}
          </h3>
          <p className="text-xs text-slate-500 mt-1">Sequential step-by-step transition blueprint from your current background</p>
        </div>
        <span className="text-xs font-semibold px-3 py-1 rounded-full bg-blue-50 text-blue-700 border border-blue-100 self-start sm:self-auto">
          5 Phases · Practical Reskilling
        </span>
      </div>
      <div className="space-y-4">
        {phases.map((phase, idx) => (
          <div key={idx} className="flex gap-4">
            <div className="flex flex-col items-center">
              <div className="w-8 h-8 rounded-full bg-blue-600 text-white text-sm font-bold flex items-center justify-center flex-shrink-0">
                {idx + 1}
              </div>
              {idx < phases.length - 1 && <div className="w-0.5 bg-blue-200 flex-1 mt-1" />}
            </div>
            <div className="pb-6 flex-1">
              <div className="flex items-center gap-3 mb-1">
                <h4 className="font-bold text-slate-800 text-sm">{phase.phase}</h4>
                <span className="text-xs text-slate-400">{phase.weeks}</span>
              </div>
              <ul className="space-y-1">
                {phase.items.map((item, i) => (
                  <li key={i} className="text-sm text-slate-600 flex items-start gap-2">
                    <ArrowRight className="w-3.5 h-3.5 text-blue-500 mt-1 flex-shrink-0" /> {item}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        ))}
      </div>
      <div className="pt-4 border-t border-slate-100 flex items-center justify-end">
        <Button
          disabled={isSaving}
          onClick={onAccept}
          className="bg-[#2563EB] hover:bg-[#1D4ED8] text-white font-semibold px-6 py-2.5 rounded-xl shadow flex items-center gap-2 cursor-pointer"
        >
          {isSaving ? (
            <>
              <Loader2 className="w-4 h-4 animate-spin" /> Saving...
            </>
          ) : (
            <>
              <Map className="w-4 h-4 mr-1" />
              {isAccepted ? 'Added to My Roadmap (View Now)' : 'Add to My Roadmap & Start Learning'}
            </>
          )}
        </Button>
      </div>
    </div>
  );
}

// ---- Section: Entry Roles ----

function JobRolesSection({ career }) {
  return (
    <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6 mb-6">
      <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
        <Briefcase className="w-5 h-5 text-blue-500" /> Target Job Roles
      </h3>
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div>
          <p className="text-xs font-bold text-slate-400 uppercase mb-2">Entry Roles</p>
          {(career.entryRoles || []).map((r, i) => (
            <div key={i} className="flex items-center gap-2 mb-2 text-sm text-slate-700 bg-slate-50 px-3 py-2 rounded-lg">
              <Zap className="w-4 h-4 text-blue-400" /> {r}
            </div>
          ))}
        </div>
        <div>
          <p className="text-xs font-bold text-slate-400 uppercase mb-2">Mid Roles</p>
          {(career.intermediateRoles || []).map((r, i) => (
            <div key={i} className="flex items-center gap-2 mb-2 text-sm text-slate-700 bg-slate-50 px-3 py-2 rounded-lg">
              <TrendingUp className="w-4 h-4 text-violet-400" /> {r}
            </div>
          ))}
        </div>
        <div>
          <p className="text-xs font-bold text-slate-400 uppercase mb-2">Senior Roles</p>
          {(career.seniorRoles || []).map((r, i) => (
            <div key={i} className="flex items-center gap-2 mb-2 text-sm text-slate-700 bg-slate-50 px-3 py-2 rounded-lg">
              <Trophy className="w-4 h-4 text-amber-400" /> {r}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

// ---- Section: Certifications ----

function CertificationsSection({ career }) {
  return (
    <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6 mb-6">
      <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
        <BookOpen className="w-5 h-5 text-emerald-500" /> Recommended Certifications
      </h3>
      <div className="flex flex-wrap gap-3">
        {(career.certifications || []).map((cert, i) => (
          <a
            key={i}
            href={`/upskilling`}
            className="flex items-center gap-2 bg-emerald-50 text-emerald-800 text-sm font-semibold px-4 py-2.5 rounded-xl border border-emerald-200 hover:bg-emerald-100 transition-colors cursor-pointer"
          >
            <BookOpen className="w-4 h-4" /> {cert}
            <ArrowRight className="w-3 h-3 ml-1" />
          </a>
        ))}
      </div>
      <p className="text-xs text-slate-400 mt-3">Clicking a certification will take you to the Upskilling module to find relevant courses.</p>
    </div>
  );
}

// ---- Section: Deprioritized Careers ----

function DeprioritizedSection({ deprioritized }) {
  if (deprioritized.length === 0) return null;
  return (
    <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6 mb-6">
      <h3 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
        <AlertCircle className="w-5 h-5 text-slate-400" /> Careers We Deprioritized
      </h3>
      <p className="text-sm text-slate-500 mb-4">These careers were a poor match based on your responses.</p>
      <div className="space-y-3">
        {deprioritized.map((c, i) => (
          <div key={i} className="flex items-start gap-3 bg-slate-50 p-3 rounded-lg border border-slate-100">
            <XCircle className="w-5 h-5 text-rose-400 flex-shrink-0 mt-0.5" />
            <div>
              <p className="text-sm font-semibold text-slate-700">{c.title} — {c.compatibilityScore}% match</p>
              <p className="text-xs text-slate-500 mt-0.5">
                {c.deprioritizedReason || 'Score below recommendation threshold based on your preferences.'}
              </p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

// ---- Main Results Page ----

export default function CareerSwitchResults() {
  const location = useLocation();
  const navigate = useNavigate();
  const { user } = useAuth();
  let savedAssessment = {};
  try { savedAssessment = JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-career-switch-assessment', user)) || '{}'); } catch {}

  const {
    rankedCareers = [],
    dimensionScores = {},
    preQuizAnswers = {},
    careerProfile = {},
    confidence = 'Moderate',
  } = location.state || savedAssessment;

  const [selectedCareerIdx, setSelectedCareerIdx] = useState(0);
  const [expandedIdx, setExpandedIdx] = useState(null);
  const [acceptedCareerId, setAcceptedCareerId] = useState(null);
  const [isSaving, setIsSaving] = useState(false);

  // If no data, redirect to assessment
  useEffect(() => {
    if (rankedCareers.length === 0) {
      navigate('/career-switch');
    }
  }, [rankedCareers]);

  if (rankedCareers.length === 0) {
    return (
      <div className="min-h-screen pt-24 flex items-center justify-center">
        <p className="text-slate-500">No results found. Redirecting…</p>
      </div>
    );
  }

  const primaryCareer = rankedCareers[selectedCareerIdx];
  const alternatives = rankedCareers.slice(1, 8);
  const deprioritized = rankedCareers
    .slice(8)
    .filter(c => c.compatibilityScore < 45)
    .slice(0, 4)
    .map(c => ({
      ...c,
      deprioritizedReason: c.compatibilityScore < 30
        ? 'Very low match based on your interest and constraint responses.'
        : 'Below match threshold based on skill gaps and preference mismatches.',
    }));

  const acceptSelectedCareer = async (career = primaryCareer) => {
    setIsSaving(true);
    const rawKey = career.id || career.targetCareerNodeId || career.nodeId || career.title || '';
    const careerKey = rawKey.toLowerCase().replace(/[\s_]+/g, '-');
    const careerTitle = career.title || career.targetCareerTitle || 'Career Alternative';

    try {
      await roadmapApi.acceptCareerRoadmap(careerKey, careerTitle);
      toast.success(`${careerTitle} added to My Roadmaps!`);
    } catch (e) {
      console.warn('Failed to persist roadmap via accept, trying saveMyRoadmap fallback:', e);
      try {
        await roadmapApi.saveMyRoadmap(careerKey, careerTitle);
        await roadmapApi.setDefaultMyRoadmap(careerKey);
        toast.success(`${careerTitle} added to My Roadmaps!`);
      } catch (err2) {
        console.warn('Fallback save also failed:', err2);
      }
    } finally {
      setIsSaving(false);
    }

    const assessmentKey = userStorageKey('skillscape-career-switch-assessment', user);
    const existing = JSON.parse(window.localStorage.getItem(assessmentKey) || '{}');
    const savedAssessment = {
      ...existing,
      acceptedAt: new Date().toISOString(),
      selectedCareer: career,
      selectedRoadmap: generateRoadmap(career, careerProfile),
    };
    window.localStorage.setItem(assessmentKey, JSON.stringify(savedAssessment));

    setAcceptedCareerId(career.id || careerKey);
    navigate(`/roadmap?domain=${encodeURIComponent(careerKey)}`);
  };

  return (
    <div className="min-h-screen pb-12">
      <div className="max-w-4xl mx-auto">

        {/* Top nav */}
        <div className="flex items-center justify-between mb-8">
          <div>
            <p className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest mb-2">Career switch assessment</p>
            <h1 className="text-[32px] font-bold text-[#0F172A] tracking-tight">Your Career Match</h1>
            <p className="text-[#64748B] mt-1">Based on your profile, preferences, and assessment responses.</p>
          </div>
          <div className="flex items-center gap-2">
          <Button variant="ghost" onClick={() => navigate('/quiz')}>
            <ArrowRight className="w-4 h-4 rotate-180" /> Back to Career Quiz
          </Button>
          <Button
            variant="outline"
            className="flex items-center gap-2"
            onClick={() => navigate('/career-switch/assessment', { state: { preQuizAnswers } })}
          >
            <RotateCcw className="w-4 h-4" /> Retake
          </Button>
          </div>
        </div>

        {/* Primary Career Hero */}
        <PrimaryCareerCard
          career={primaryCareer}
          careerProfile={careerProfile}
          dimensionScores={dimensionScores}
          preQuizAnswers={preQuizAnswers}
          confidence={confidence}
          onAccept={() => acceptSelectedCareer(primaryCareer)}
          isSaving={isSaving}
          isAccepted={acceptedCareerId === primaryCareer.id || acceptedCareerId === (primaryCareer.id || '').replace(/_/g, '-')}
          onViewRoadmap={() => navigate('/roadmap')}
        />

        {/* Why This Matches */}
        <WhyMatchSection
          career={primaryCareer}
          careerProfile={careerProfile}
          dimensionScores={dimensionScores}
          preQuizAnswers={preQuizAnswers}
        />

        {/* Skills You Have + Skill Gaps */}
        <SkillsSection career={primaryCareer} careerProfile={careerProfile} preQuizAnswers={preQuizAnswers} />

        {/* Career Switch Transition Roadmap */}
        <RoadmapSection
          career={primaryCareer}
          careerProfile={careerProfile}
          onAccept={() => acceptSelectedCareer(primaryCareer)}
          isSaving={isSaving}
          isAccepted={acceptedCareerId === primaryCareer.id || acceptedCareerId === (primaryCareer.id || '').replace(/_/g, '-')}
        />

        {/* Transition Details */}
        <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6 mb-6 grid grid-cols-1 sm:grid-cols-3 gap-6">
          <div className="text-center">
            <p className="text-xs font-semibold text-slate-400 uppercase mb-2">Transition Difficulty</p>
            <DifficultyBadge difficulty={getTransitionDifficulty(primaryCareer, careerProfile)} />
          </div>
          <div className="text-center">
            <p className="text-xs font-semibold text-slate-400 uppercase mb-2">Career Distance</p>
            <DistanceBadge distance={primaryCareer.baseDistance} />
          </div>
          <div className="text-center">
            <p className="text-xs font-semibold text-slate-400 uppercase mb-2">Estimated Prep Time</p>
            <span className="px-3 py-1 rounded-full text-sm font-semibold bg-slate-100 text-slate-700 border border-slate-200">
              {primaryCareer.reskillingLevel <= 2 ? '3–6 months' : primaryCareer.reskillingLevel <= 3 ? '6–12 months' : '1–2 years'}
            </span>
          </div>
        </div>

        {/* Job Roles */}
        <JobRolesSection career={primaryCareer} />

        {/* Certifications → Upskilling Links */}
        <CertificationsSection career={primaryCareer} />

        {/* Alternative Careers */}
        <div className="mb-6">
          <h3 className="text-lg font-bold text-[#0F172A] mb-3 flex items-center gap-2">
            <TrendingUp className="w-5 h-5 text-[#2563EB]" /> Other suitable directions
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
            {alternatives.map((career, idx) => (
              <AlternativeCareerCard
                key={career.id}
                career={career}
                careerProfile={careerProfile}
                isExpanded={expandedIdx === idx}
                onToggle={() => {
                  setExpandedIdx(prev => prev === idx ? null : idx);
                  setSelectedCareerIdx(idx + 1);
                }}
                onSelect={() => {
                  setSelectedCareerIdx(idx + 1);
                  acceptSelectedCareer(career);
                }}
              />
            ))}
          </div>
        </div>

        {/* Deprioritized */}
        <DeprioritizedSection deprioritized={deprioritized} />

        {/* Bottom Actions */}
        <div className="flex flex-wrap gap-3 pt-4">
          <Button
            disabled={isSaving}
            onClick={() => acceptSelectedCareer(primaryCareer)}
            className="bg-[#2563EB] hover:bg-[#1D4ED8] text-white rounded-xl h-11 px-6 text-sm font-semibold shadow-md flex items-center gap-2 cursor-pointer"
          >
            {isSaving ? <Loader2 className="w-4 h-4 animate-spin" /> : <Map className="w-4 h-4" />}
            Add to My Roadmap & Start Learning
          </Button>
          <Button onClick={() => navigate('/upskilling')} variant="outline" className="border-[#E2E8F0] text-[#475569] rounded-xl h-11 px-5 text-sm font-medium">
            <BookOpen className="w-4 h-4 mr-2" /> Explore Courses
          </Button>
          <Button variant="outline" className="border-[#E2E8F0] text-[#475569] rounded-xl h-11 px-5 text-sm font-medium" onClick={() => navigate('/career-switch/profile')}>
            Update Profile
          </Button>
          <Button variant="outline" className="border-[#E2E8F0] text-[#475569] rounded-xl h-11 px-5 text-sm font-medium" onClick={() => navigate('/career-switch/assessment', { state: { preQuizAnswers } })}>
            <RotateCcw className="w-4 h-4 mr-2" /> Retake Assessment
          </Button>
        </div>

      </div>
    </div>
  );
}
