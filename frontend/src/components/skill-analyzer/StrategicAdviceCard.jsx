import React from 'react';
import { motion } from 'framer-motion';
import { Award, BriefcaseBusiness, CheckCircle2, Code2, Lightbulb, ListChecks, Target } from 'lucide-react';

const genericLearningText = /online courses, tutorials, and hands-on projects/i;

export const StrategicAdviceCard = ({ data }) => {
  const recommendations = data.recommendations || {};
  const targetRole = data.targetCareerTitle || data.careerName || 'your target role';
  const summary = data.strategicSummary || recommendations.readinessExplanation;
  const priorities = recommendations.whatToLearnFirst || [];
  const skillAdvice = recommendations.skillAdvice || [];
  const actions = recommendations.careerAdvice || [];
  const projects = data.recommendedProjects || [];
  const certifications = data.recommendedCertifications || [];
  const isAiPlan = recommendations.isAIGenerated === true && !recommendations.aiFailed;
  const aiProvider = recommendations.aiProvider || 'Groq';
  const adviceFor = (skill) => skillAdvice.find(item => item.skill?.toLowerCase() === skill?.toLowerCase());
  const nextMove = (advice, skill) => !advice?.howToLearn || genericLearningText.test(advice.howToLearn)
    ? `Create one focused ${skill} work sample for ${targetRole}, documenting the objective, method, and outcome.`
    : advice.howToLearn;
  const priorityCards = priorities.length ? priorities : skillAdvice.map(item => item.skill).filter(Boolean);

  return (
    <motion.section initial={{ opacity: 0, y: 14 }} animate={{ opacity: 1, y: 0 }} className="overflow-hidden rounded-xl border border-slate-200 bg-white p-6 sm:p-8 shadow-sm space-y-6">
      <div className="border-b border-slate-100 pb-6">
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-blue-50 text-blue-600 border border-blue-100"><BriefcaseBusiness className="h-5 w-5" /></div>
          <div className="flex-1"><div className="flex flex-wrap items-center gap-2"><p className="text-xs font-semibold uppercase tracking-wider text-blue-600">Career action plan</p><span className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${isAiPlan ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-amber-50 text-amber-700 border border-amber-200'}`}>{isAiPlan ? `Personalized by ${aiProvider}` : 'Role-based fallback'}</span></div><h2 className="mt-1 text-xl font-bold tracking-tight text-slate-900">What to do next for {targetRole}</h2><p className="mt-1 text-sm text-slate-500">A focused plan based on your profile, target role, and the gaps that most affect your readiness.</p></div>
        </div>
      </div>

      <div className="space-y-8">
        {summary && <div className="rounded-xl border border-slate-200 bg-slate-50/50 p-5"><div className="flex items-center gap-2 text-sm font-semibold text-slate-800"><Lightbulb className="h-4 w-4 text-amber-500" /> Readiness assessment</div><p className="mt-2 max-w-4xl text-sm leading-6 text-slate-600">{summary}</p></div>}

        {priorityCards.length > 0 && <div>
          <div className="mb-4 flex items-center gap-2"><Target className="h-4 w-4 text-blue-600" /><h3 className="text-base font-bold text-slate-900">Priority skill plan</h3><span className="text-sm text-slate-500">Start in this order</span></div>
          <div className="grid grid-cols-1 gap-3 lg:grid-cols-3">{priorityCards.slice(0, 3).map((skill, index) => {
            const advice = adviceFor(skill);
            return <article key={`${skill}-${index}`} className="rounded-xl border border-slate-200 bg-slate-50/70 p-5"><div className="flex items-center gap-3"><span className="flex h-7 w-7 items-center justify-center rounded-full bg-blue-600 text-xs font-bold text-white">{index + 1}</span><h4 className="text-sm font-bold text-slate-900">{skill}</h4></div><ol className="mt-4 space-y-3 text-sm leading-5 text-slate-600"><li className="flex gap-2"><span className="font-bold text-blue-600">1.</span><span><strong className="text-slate-800">Why it matters:</strong> {advice?.whyItMatters || `${skill} is an important capability for effective work in this role.`}</span></li><li className="flex gap-2 border-t border-slate-200 pt-3"><span className="font-bold text-blue-600">2.</span><span><strong className="text-slate-800">What to do:</strong> {nextMove(advice, skill)}</span></li></ol></article>;
          })}</div>
        </div>}

        {actions.length > 0 && <div className="rounded-xl border border-slate-200 p-5"><div className="flex items-center gap-2"><ListChecks className="h-4 w-4 text-blue-600" /><h3 className="text-base font-bold text-slate-900">Execution checklist</h3></div><ol className="mt-4 grid gap-3 md:grid-cols-3">{actions.slice(0, 3).map((action, index) => <li key={index} className="flex gap-3 rounded-lg bg-slate-50 px-3 py-3 text-sm leading-5 text-slate-700"><span className="font-bold text-blue-600">0{index + 1}</span><span>{action}</span></li>)}</ol></div>}

        {(projects.length > 0 || certifications.length > 0) && <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
          {projects.length > 0 && <div><div className="mb-3 flex items-center gap-2"><Code2 className="h-4 w-4 text-indigo-600" /><h3 className="text-base font-bold text-slate-900">Portfolio evidence</h3></div><div className="space-y-3">{projects.slice(0, 2).map((project, index) => <div key={index} className="rounded-xl border border-slate-200 p-4"><div className="flex items-start justify-between gap-3"><h4 className="text-sm font-semibold text-slate-900">{project.title}</h4><span className="shrink-0 rounded-md bg-indigo-50 px-2 py-1 text-[11px] font-semibold text-indigo-700">{project.complexity}</span></div><p className="mt-2 text-sm leading-5 text-slate-600">{project.description}</p></div>)}</div></div>}
          {certifications.length > 0 && <div><div className="mb-3 flex items-center gap-2"><Award className="h-4 w-4 text-emerald-600" /><h3 className="text-base font-bold text-slate-900">Credentials to consider</h3></div><div className="space-y-2">{certifications.slice(0, 4).map((certification, index) => <div key={index} className="flex items-start gap-2 rounded-lg border border-slate-200 px-3 py-3 text-sm text-slate-700"><CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600" />{certification}</div>)}</div></div>}
        </div>}
      </div>
    </motion.section>
  );
};

export default StrategicAdviceCard;
