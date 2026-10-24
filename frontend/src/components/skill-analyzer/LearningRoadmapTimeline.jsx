import React from 'react';
import { motion } from 'framer-motion';
import { Check, Clock3, Flag, Map, Target } from 'lucide-react';

const phaseStyle = (index) => index === 0
  ? { accent: 'bg-blue-600', surface: 'border-blue-200 bg-blue-50/45', label: 'text-blue-700 bg-blue-100' }
  : index === 1
    ? { accent: 'bg-indigo-600', surface: 'border-indigo-200 bg-indigo-50/40', label: 'text-indigo-700 bg-indigo-100' }
    : { accent: 'bg-slate-700', surface: 'border-slate-200 bg-slate-50', label: 'text-slate-700 bg-slate-200' };

const phaseIntent = (item, index, total) => {
  if (index === 0) return 'Build the essentials';
  if (index === total - 1) return 'Demonstrate readiness';
  return item.priority === 'High' ? 'Strengthen core capability' : 'Apply and consolidate';
};

export const LearningRoadmapTimeline = ({ roadmap = [], completedPhaseIds = [], onTogglePhaseRead }) => {
  if (!roadmap?.length) return null;
  const hasGroqSubtopics = roadmap.some(item => item.subtopics?.length > 0);
  const phaseId = (item, index) => `${item.phase || index + 1}-${item.title || 'phase'}`;
  const completedPhases = new Set(completedPhaseIds);
  return (
    <motion.section initial={{ opacity: 0, y: 14 }} animate={{ opacity: 1, y: 0 }} className="rounded-2xl border border-slate-200 bg-white shadow-sm">
      <header className="border-b border-slate-100 px-6 py-7 sm:px-10">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="flex items-start gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-blue-50 text-blue-600"><Map className="h-5 w-5" /></div>
            <div><p className="text-xs font-semibold uppercase tracking-[0.16em] text-blue-600">Skill-gap action plan</p><h2 className="mt-1 text-xl font-bold tracking-tight text-slate-900">Your learning roadmap</h2><p className="mt-1 text-sm leading-6 text-slate-500">Complete each phase in order to turn the most important gaps into job-relevant capability.</p></div>
          </div>
          <div className="flex flex-wrap items-center gap-2"><div className="flex items-center gap-2 rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-xs font-semibold text-slate-600"><Flag className="h-4 w-4 text-blue-600" /> {roadmap.length} focused phases</div>{hasGroqSubtopics && <span className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-xs font-semibold text-emerald-700">Role-specific subtopics by Groq</span>}</div>
        </div>
      </header>

      <div className="px-5 py-8 pb-12 sm:px-10 sm:py-10 sm:pb-14">
        <div className="mb-8 grid grid-cols-1 gap-3 sm:grid-cols-3">
          {roadmap.slice(0, 3).map((item, index) => {
            const style = phaseStyle(index);
            return <div key={`overview-${index}`} className="relative overflow-hidden rounded-xl border border-slate-200 bg-slate-50 px-4 py-3"><div className={`absolute inset-x-0 top-0 h-1 ${style.accent}`} /><p className="mt-1 text-[11px] font-bold uppercase tracking-wider text-slate-400">Phase {item.phase || index + 1}</p><p className="mt-1 truncate text-sm font-bold text-slate-800">{item.title}</p></div>;
          })}
        </div>

        <div className="space-y-6">
          {roadmap.map((item, index) => {
            const style = phaseStyle(index);
            const skills = item.targetSkills?.length ? item.targetSkills : (item.skills || []);
            const duration = item.estimatedDuration || item.estimatedTime;
            const id = phaseId(item, index);
            const isRead = completedPhases.has(id);
            return <article key={id} className={`rounded-xl border px-5 py-6 sm:px-7 sm:py-7 ${style.surface}`}>
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div><div className="flex flex-wrap items-center gap-2"><span className={`flex h-7 w-7 items-center justify-center rounded-lg text-xs font-bold text-white ${style.accent}`}>{item.phase || index + 1}</span><span className={`rounded-md px-2 py-1 text-[11px] font-bold uppercase tracking-wider ${style.label}`}>{phaseIntent(item, index, roadmap.length)}</span>{item.priority && <span className="text-xs font-semibold text-slate-500">{item.priority} priority</span>}</div><h3 className="mt-3 text-lg font-bold text-slate-900">{item.title}</h3></div>
                  {duration && <span className="inline-flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-2.5 py-1.5 text-xs font-semibold text-slate-600"><Clock3 className="h-3.5 w-3.5 text-blue-600" /> {duration}</span>}
                </div>
                {item.description && <p className="mt-4 max-w-4xl text-sm leading-6 text-slate-600">{item.description}</p>}
                {skills.length > 0 && <div className="mt-6 border-t border-slate-200/80 pt-5"><div className="mb-3 flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-500"><Target className="h-3.5 w-3.5 text-blue-600" /> Capability focus</div><div className="flex flex-wrap gap-2">{skills.map((skill, skillIndex) => <span key={`${skill}-${skillIndex}`} className="rounded-md border border-slate-200 bg-white px-2.5 py-1.5 text-xs font-medium text-slate-700">{skill}</span>)}</div></div>}
                {item.subtopics?.length > 0 && <div className="mt-5"><p className="mb-2 text-xs font-bold uppercase tracking-wider text-slate-500">Learn within this phase</p><ul className="grid gap-2 sm:grid-cols-3">{item.subtopics.map((topic, topicIndex) => <li key={`${topic}-${topicIndex}`} className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-xs leading-5 text-slate-700">{topic}</li>)}</ul></div>}
                {onTogglePhaseRead && <div className="mt-6 border-t border-slate-200/80 pt-5"><button type="button" onClick={() => onTogglePhaseRead(id)} className={`inline-flex items-center gap-2 rounded-md border px-3 py-2 text-xs font-semibold transition-colors ${isRead ? 'border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100' : 'border-blue-200 bg-white text-blue-700 hover:bg-blue-50'}`}><Check className="h-3.5 w-3.5" />{isRead ? 'Completed' : 'Mark as complete'}</button></div>}
            </article>;
          })}
        </div>
      </div>
    </motion.section>
  );
};

export default LearningRoadmapTimeline;
