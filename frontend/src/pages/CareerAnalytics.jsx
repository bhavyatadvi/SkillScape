import React, { useMemo } from 'react';
import { Link } from 'react-router-dom';
import { BarChart3, CheckCircle2, Compass, Lightbulb, Map, Target, TrendingUp, UserRound, Wrench } from 'lucide-react';
import { Bar, BarChart, Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import PageHeader from '@/components/layout/PageHeader';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/context/AuthContext';
import { useCareerProfile } from '@/context/CareerProfileContext';
import { userStorageKey } from '@/lib/userStorage';

// A single blue family keeps the distribution aligned with the SkillScape visual system.
const blueShades = ['#1D4ED8', '#2563EB', '#3B82F6', '#60A5FA', '#93C5FD', '#BFDBFE'];
const tooltipStyle = { background: '#fff', border: '1px solid #E2E8F0', borderRadius: 10, color: '#0F172A', fontSize: 12 };
const list = value => Array.isArray(value) ? value.filter(Boolean) : [];
const object = value => value && typeof value === 'object' && !Array.isArray(value) ? value : {};
const score = value => Math.max(0, Math.min(100, Math.round(Number(value) || 0)));
const read = (key, user) => { try { return JSON.parse(localStorage.getItem(userStorageKey(key, user)) || '{}'); } catch { return {}; } };

function Panel({ title, icon: Icon, children, className = '' }) {
  return <section className={`rounded-xl border border-slate-200 bg-white p-5 shadow-soft ${className}`}><div className="mb-4 flex items-center gap-2"><span className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-50"><Icon className="h-4 w-4 text-[#315FEF]" /></span><h2 className="text-base font-bold text-slate-900">{title}</h2></div>{children}</section>;
}
function Empty({ children, to, action }) { return <div className="rounded-lg border border-dashed border-slate-200 bg-slate-50 p-4 text-sm text-slate-500"><p>{children}</p>{to && <Link to={to} className="mt-2 inline-flex font-semibold text-[#315FEF]">{action} →</Link>}</div>; }
function Stat({ label, value, detail }) { return <div className="rounded-lg border border-slate-100 bg-slate-50 p-4"><p className="text-xs font-semibold uppercase tracking-wide text-slate-500">{label}</p><p className="mt-1 text-2xl font-black text-slate-900">{value}</p><p className="mt-1 text-xs text-slate-500">{detail}</p></div>; }

export default function CareerAnalytics() {
  const { user } = useAuth();
  const { profile, loading } = useCareerProfile();
  const analysis = useMemo(() => object(read('skillscape-skill-analyzer-analysis', user)), [user]);
  const analyzerPlan = useMemo(() => object(read('skillscape-skill-analyzer-roadmap', user)), [user]);
  const switchResult = useMemo(() => object(read('skillscape-career-switch-assessment', user)), [user]);
  const progress = useMemo(() => { try { return object(JSON.parse(localStorage.getItem('skillscape-course-progress') || '{}')); } catch { return {}; } }, []);

  const current = profile?.currentRole || profile?.currentOccupation || profile?.currentIndustry || 'Current Career Not Set';
  const hasAnalysis = Boolean(analysis.targetCareerTitle || analysis.careerName || analyzerPlan.selectedCareer?.title);
  const target = analysis.targetCareerTitle || analysis.careerName || analyzerPlan.selectedCareer?.title || 'Target Career Not Selected';
  const gaps = list(analysis.missingSkills?.length ? analysis.missingSkills : analysis.skillGaps);
  const matched = list(analysis.matchedSkills);
  const strengths = list(analysis.strengths);
  const phases = list(analyzerPlan.result?.skillGapRoadmap?.length ? analyzerPlan.result.skillGapRoadmap : (analysis.skillGapRoadmap?.length ? analysis.skillGapRoadmap : analysis.learningRoadmap));
  const completedPhaseIds = new Set(list(analyzerPlan.completedPhaseIds));
  const roadmapCompleted = phases.length > 0 && phases.every((phase, index) => completedPhaseIds.has(`${phase.phase || index + 1}-${phase.title || 'phase'}`)) ? 1 : 0;
  const courses = Object.values(progress);
  const completed = courses.filter(item => object(item).status === 'completed').length;
  const readiness = hasAnalysis ? score(analysis.overallReadinessScore) : 0;
  
  const fitData = hasAnalysis ? [
    { name: 'Skills', value: score(analysis.skillAlignmentScore) }, 
    { name: 'Experience', value: score(analysis.experienceFitScore) }, 
    { name: 'Education', value: score(analysis.educationFitScore) }
  ] : [
    { name: 'Skills', value: 0 }, 
    { name: 'Experience', value: 0 }, 
    { name: 'Education', value: 0 }
  ];

  // These are separate, real record groups—not a readiness score or duplicated skill count.
  const portfolioData = [
    { name: 'Profile skills', value: list(profile?.currentSkills).length },
    { name: 'Certifications', value: list(profile?.certifications).length },
    { name: 'Projects', value: Number(analysis.baselineSummary?.projectsCount) || 0 },
    { name: 'Learning completed', value: completed },
    { name: 'Roadmaps', value: phases.length ? 1 : 0 },
    { name: 'Roadmaps complete', value: roadmapCompleted },
  ].filter(item => item.value > 0);

  const learningData = [
    { name: 'Roadmap complete', value: roadmapCompleted },
    { name: 'Roadmap remaining', value: phases.length ? 1 - roadmapCompleted : 0 },
  ];

  const gapData = hasAnalysis ? gaps.slice(0, 5).map(item => ({ 
    name: item.skillName || item.name || 'Skill', 
    current: score((item.userProficiencyLevel || Math.round((item.currentLevel || 0) / 20)) * 20), 
    required: score((item.requiredProficiencyLevel || Math.round((item.requiredLevel || 0) / 20)) * 20) 
  })) : [];

  const alternatives = list(switchResult.rankedCareers).filter(item => item?.title !== switchResult.selectedCareer?.title).slice(0, 3);
  const aiActions = list(analysis.recommendations?.whatToLearnFirst).slice(0, 3);
  const actions = aiActions.length ? aiActions : (hasAnalysis ? gaps.slice(0, 3).map(item => `Close the ${item.skillName || item.name} gap.`) : []);

  if (loading) return <div className="flex min-h-[45vh] items-center justify-center text-sm font-medium text-slate-500">Loading career analytics…</div>;
  return <div className="mx-auto max-w-7xl space-y-6 pb-16">
    <PageHeader icon={BarChart3} eyebrow="Personal dashboard" title="Career Analytics" description="A data-driven view of your profile, career readiness, skills, and learning progress." />
    
    {/* Career Overview Banner */}
    <section className="rounded-xl border border-blue-100 bg-gradient-to-r from-[#EEF4FF] via-white to-indigo-50 p-6 shadow-soft">
      <div className="flex flex-col justify-between gap-5 lg:flex-row lg:items-center">
        <div>
          <p className="text-xs font-bold uppercase tracking-widest text-[#315FEF]">Career overview</p>
          <h2 className="mt-2 text-2xl font-bold tracking-tight text-slate-900">{current} <span className="mx-2 text-[#315FEF]">→</span> {target}</h2>
          <p className="mt-2 text-sm text-slate-600">{hasAnalysis ? `${analysis.domainName || analysis.domain || 'Career domain'} → ${analysis.subDomainName || 'Specialization'} → ${target}` : 'Run Skill Analyzer to populate your readiness analysis.'}</p>
        </div>
        <div className="min-w-44 rounded-xl border border-blue-100 bg-white px-5 py-4 text-center shadow-sm">
          <p className="text-3xl font-black text-[#315FEF]">{readiness}%</p>
          <p className="mt-1 text-xs font-semibold text-slate-600">{hasAnalysis ? (analysis.readinessLabel || 'Readiness') : 'No Analysis Done'}</p>
        </div>
      </div>
    </section>

    {/* Metric Stats Cards */}
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <Stat label="Readiness" value={`${readiness}%`} detail={hasAnalysis ? "From Skill Analyzer" : "0% — Skill Analysis Pending"} />
      <Stat label="Profile skills" value={list(profile?.currentSkills).length} detail="Recorded in Career Profile" />
      <Stat label="Skill gaps" value={gaps.length} detail={hasAnalysis ? "Against target requirements" : "0 Gaps Identified"} />
      <Stat label="Roadmaps complete" value={roadmapCompleted} detail={phases.length ? (roadmapCompleted ? 'Your saved roadmap is complete' : 'Complete every phase to finish this roadmap') : "No roadmap saved yet"} />
    </div>

    {/* Breakdown Charts */}
    <div className="grid gap-6 xl:grid-cols-3">
      <Panel title="Career fit breakdown" icon={TrendingUp}>
        <div className="h-64">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={fitData} layout="vertical" margin={{ left: 12, right: 16 }}>
              <XAxis type="number" domain={[0, 100]} hide />
              <YAxis type="category" dataKey="name" width={82} tickLine={false} axisLine={false} tick={{ fill: '#64748B', fontSize: 12 }} />
              <Tooltip contentStyle={tooltipStyle} formatter={value => [`${value}%`, 'Fit']} />
              <Bar dataKey="value" fill="#315FEF" radius={[0, 5, 5, 0]} barSize={20} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </Panel>

      <Panel title="Career portfolio distribution" icon={Wrench}>
        <div className="relative h-64">
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie data={portfolioData.length ? portfolioData : [{ name: 'No profile records', value: 1 }]} dataKey="value" nameKey="name" innerRadius={62} outerRadius={86} paddingAngle={4}>
                {portfolioData.length ? portfolioData.map((item, index) => <Cell key={item.name} fill={blueShades[index % blueShades.length]} />) : <Cell fill="#DBEAFE" />}
              </Pie>
              <Tooltip contentStyle={tooltipStyle} />
              <Legend iconType="circle" iconSize={8} wrapperStyle={{ fontSize: 12, paddingTop: 8 }} />
            </PieChart>
          </ResponsiveContainer>
          <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center pb-7">
            <b className="text-2xl text-slate-900">{portfolioData.reduce((total, item) => total + item.value, 0)}</b>
            <span className="text-[11px] text-slate-500">portfolio records</span>
          </div>
        </div>
      </Panel>

      <Panel title="Learning progress" icon={CheckCircle2}>
        <div className="relative h-64">
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie data={phases.length ? learningData : [{ name: 'No Progress', value: 1 }]} dataKey="value" nameKey="name" innerRadius={62} outerRadius={86} paddingAngle={4}>
                {phases.length ? learningData.map((item, index) => <Cell key={item.name} fill={blueShades[index + 2] || blueShades[5]} />) : <Cell fill="#DBEAFE" />}
              </Pie>
              <Tooltip contentStyle={tooltipStyle} />
              <Legend iconType="circle" iconSize={8} wrapperStyle={{ fontSize: 12, paddingTop: 8 }} />
            </PieChart>
          </ResponsiveContainer>
          <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center pb-7">
            <b className="text-2xl text-slate-900">{roadmapCompleted}</b>
            <span className="text-[11px] text-slate-500">roadmaps complete</span>
          </div>
        </div>
      </Panel>
    </div>

    {/* Skill Level Comparison Chart */}
    <Panel title="Current vs required skill level" icon={Target}>
      {gapData.length ? (
        <div className="h-72">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={gapData} margin={{ top: 8, right: 12, left: -18, bottom: 0 }}>
              <XAxis dataKey="name" interval={0} tick={{ fill: '#64748B', fontSize: 11 }} tickLine={false} axisLine={false} />
              <YAxis domain={[0, 100]} tickFormatter={value => `${value}%`} tick={{ fill: '#94A3B8', fontSize: 11 }} tickLine={false} axisLine={false} />
              <Tooltip contentStyle={tooltipStyle} formatter={value => [`${value}%`]} cursor={{ fill: 'transparent' }} />
              <Legend iconType="circle" iconSize={8} />
              <Bar dataKey="current" name="Your level" fill="#315FEF" radius={[4, 4, 0, 0]} />
              <Bar dataKey="required" name="Required level" fill="#A5B4FC" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      ) : (
        <div className="py-12 text-center text-slate-400 font-medium text-sm">
          No skill gap analysis data available yet. Run Skill Analyzer to view skill proficiency comparisons.
        </div>
      )}
    </Panel>

    {/* Profile & Transition */}
    <div className="grid gap-6 xl:grid-cols-3">
      <Panel title="Profile summary" icon={UserRound}>
        <div className="space-y-3 text-sm">
          <div><p className="text-slate-500">Education</p><p className="font-semibold text-slate-800">{profile?.degree || profile?.broadField || 'Not added yet'}</p></div>
          <div><p className="text-slate-500">Experience</p><p className="font-semibold text-slate-800">{profile?.experienceYears ?? 0} years{profile?.currentRole ? ` · ${profile.currentRole}` : ''}</p></div>
          <div><p className="text-slate-500">Certifications / projects</p><p className="font-semibold text-slate-800">{list(profile?.certifications).length} certifications · {analysis.baselineSummary?.projectsCount ?? 0} projects</p></div>
          <Link to="/career-profile" className="inline-flex font-semibold text-[#315FEF]">Update profile →</Link>
        </div>
      </Panel>

      <Panel title="Career transition" icon={Map} className="xl:col-span-2">
        {switchResult.selectedCareer ? (
          <div className="space-y-4">
            <p className="text-sm font-semibold text-slate-800">{current} <span className="mx-2 text-[#315FEF]">→</span> {switchResult.selectedCareer.title}</p>
            <div className="grid gap-3 md:grid-cols-2">
              <div className="rounded-lg bg-emerald-50 p-3">
                <p className="text-xs font-bold uppercase tracking-wide text-emerald-700">Transferable skills</p>
                <p className="mt-2 text-sm text-emerald-900">{list(profile?.currentSkills).slice(0, 5).map(item => item?.name || item).join(', ') || 'Add skills to your Career Profile.'}</p>
              </div>
              <div className="rounded-lg bg-amber-50 p-3">
                <p className="text-xs font-bold uppercase tracking-wide text-amber-700">Priority gaps</p>
                <p className="mt-2 text-sm text-amber-900">{gaps.slice(0, 4).map(item => item.skillName || item.name).join(', ') || 'Run Skill Analyzer for target skill gaps.'}</p>
              </div>
            </div>
            <Link to="/career-switch/results" className="font-semibold text-[#315FEF]">Open Career Switch path →</Link>
          </div>
        ) : (
          <div className="py-6 text-center text-slate-400 font-medium text-sm">
            No active career transition path set.
          </div>
        )}
      </Panel>
    </div>

    {/* Options & Actions */}
    <div className="grid gap-6 xl:grid-cols-2">
      <Panel title="Adjacent career options" icon={Compass}>
        {alternatives.length ? (
          <div className="space-y-2">
            {alternatives.map(option => (
              <div key={option.id || option.title} className="flex items-center justify-between rounded-lg border border-slate-100 p-3">
                <span className="text-sm font-semibold text-slate-800">{option.title}</span>
                <span className="rounded-full bg-indigo-50 px-2 py-1 text-xs font-bold text-indigo-700">{score(option.compatibilityScore)}% fit</span>
              </div>
            ))}
          </div>
        ) : (
          <div className="py-6 text-center text-slate-400 font-medium text-sm">
            No adjacent career recommendations calculated yet.
          </div>
        )}
      </Panel>

      <Panel title="Next actions" icon={Lightbulb}>
        {actions.length ? (
          <div className="space-y-3">
            {actions.map((action, index) => (
              <div key={action} className="flex items-start gap-3 rounded-lg bg-blue-50 p-3">
                <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-[#315FEF] text-xs font-bold text-white">{index + 1}</span>
                <p className="pt-0.5 text-sm font-medium text-slate-700">{action}</p>
              </div>
            ))}
          </div>
        ) : (
          <div className="py-6 text-center text-slate-400 font-medium text-sm">
            Run Skill Analyzer to generate personalized next actions.
          </div>
        )}
      </Panel>
    </div>
  </div>;
}
