import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, ChevronLeft, ChevronRight, ClipboardCheck } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/context/AuthContext';
import { userStorageKey } from '@/lib/userStorage';

const labels = {
  bg_field: 'What is your current or most recent career field / domain?',
  bg_degree: 'What is your highest qualification or degree?',
  bg_experience: 'How many years of professional experience do you have?',
  1: 'Why are you considering a career change?',
  2: 'How far are you willing to move from your current career?',
  3: 'Which parts of your current experience would you like to carry forward?',
  4: 'Which activities do you not want in your next career?',
  5: 'What kinds of work genuinely interest you?',
  6: 'What is your preferred level of reskilling?',
  7: 'How much time can you spend transitioning each week?',
  8: 'What is your target transition timeframe?'
};

export default function CareerSwitchAnswers() {
  const { user } = useAuth();
  const saved = JSON.parse(window.localStorage.getItem(userStorageKey('skillscape-career-switch-assessment', user)) || '{}');
  const entries = [...Object.entries(saved.preQuizAnswers || {}).map(([id, answer]) => ({ question: labels[id], answers: Array.isArray(answer) ? answer : [answer] })), ...(saved.assessmentAnswers || [])];
  const [index, setIndex] = useState(0);
  if (!entries.length) return <div className="max-w-2xl mx-auto text-center py-20"><ClipboardCheck className="w-10 h-10 text-[#2563EB] mx-auto mb-4" /><h1 className="text-2xl font-bold text-[#0F172A]">No saved Career Switch assessment</h1><p className="text-[#64748B] mt-2 mb-5">Complete an assessment to review your answers here.</p><Link to="/career-switch"><Button className="bg-[#2563EB]">Start Career Switch</Button></Link></div>;
  const entry = entries[index];
  return <div className="max-w-3xl mx-auto pb-12"><Link to="/dashboard" className="inline-flex items-center text-sm text-[#64748B] hover:text-[#2563EB] mb-7"><ArrowLeft className="w-4 h-4 mr-2" />Dashboard</Link><p className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest mb-2">Saved assessment</p><h1 className="text-[32px] font-bold tracking-tight text-[#0F172A]">Career Switch answers</h1><p className="text-[#64748B] mt-2 mb-7">Question {index + 1} of {entries.length}</p><div className="bg-white border border-[#E2E8F0] rounded-[10px] p-6"><p className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest">Your response</p><h2 className="text-xl font-semibold text-[#0F172A] mt-3 leading-relaxed">{entry.question}</h2><div className="mt-6 space-y-2">{entry.answers.map(answer => <div key={answer} className="bg-[#EFF6FF] border border-[#DBEAFE] text-[#1E40AF] px-4 py-3 rounded-lg text-sm font-medium">{answer}</div>)}</div><div className="flex justify-between mt-8 pt-5 border-t border-[#E2E8F0]"><Button variant="outline" disabled={!index} onClick={() => setIndex(index - 1)}><ChevronLeft className="w-4 h-4 mr-1" />Previous</Button><Button disabled={index === entries.length - 1} onClick={() => setIndex(index + 1)} className="bg-[#2563EB]">Next<ChevronRight className="w-4 h-4 ml-1" /></Button></div></div></div>;
}
