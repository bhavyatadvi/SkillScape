import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, ArrowRight, Building2, Check, Landmark, Shield, Wrench, Wallet, GraduationCap, HeartPulse, Leaf, Scale, BarChart3, BookOpen, Laptop } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useCareerProfile } from '@/context/CareerProfileContext';
import { useAuth } from '@/context/AuthContext';
import { userStorageKey } from '@/lib/userStorage';

import QuizProcessingLoader from '@/components/quiz/QuizProcessingLoader';

const ICONS = { admin: Landmark, technical: Wrench, finance: Wallet, police: Shield, education: GraduationCap, health: HeartPulse, environment: Leaf, legal: Scale, data: BarChart3, research: BookOpen, digital: Laptop, community: Building2 };
const make = (value, title, description, icon = 'admin') => ({ value, title, description, icon });
const simple = values => values.map(value => make(value, value, 'A government career preference.'));

const GOVT_LOADING_MESSAGES = [
  'Analyzing your public service preferences…',
  'Matching central, state & PSU opportunities…',
  'Evaluating exam patterns (UPSC, SSC, Banking, Defence)…',
  'Benchmarking preparation timeline & competition fit…',
  'Generating your government career & exam roadmap…',
];

const QUESTIONS = [
  { id:'interests', title:'What kind of impact would you most like to make through government service?', hint:'Choose up to 3.', max:3, options:[make('administration','Civil Administration & Public Policy','Shape public programmes and services.'),make('state','State Administration','Work within state departments.'),make('banking','Banking & Financial Services','Support public financial systems.','finance'),make('revenue','Revenue, Taxation & Audit','Work with compliance and public finance.','finance'),make('police','Police & Law Enforcement','Protect communities and uphold the law.','police'),make('defence','Defence & Armed Forces','Serve in disciplined national roles.','police'),make('engineering','Engineering & Infrastructure','Build and maintain public infrastructure.','technical'),make('railways','Railways & Transport','Support national transport systems.','technical'),make('education','Education & Teaching','Improve learning in public institutions.','education'),make('healthcare','Healthcare & Public Health','Improve health outcomes.','health'),make('science','Science & Research','Contribute to national research.','research'),make('digital','IT & Digital Government','Build digital public services.','digital'),make('environment','Environment & Forest Services','Protect natural resources.','environment'),make('welfare','Social Welfare & Community Development','Strengthen local communities.','community'),make('legal','Judiciary & Legal Services','Support legal institutions.','legal')] },
  { id:'workTypes', title:'Which type of work would you enjoy doing most?', hint:'Choose up to 2.', max:2, options:[make('administrative','Administrative & Decision-Making','Plan, coordinate and make public decisions.'),make('technical','Technical & Engineering','Solve infrastructure and technical problems.','technical'),make('analytical','Financial & Analytical','Interpret financial and operational information.','finance'),make('investigation','Law Enforcement & Investigation','Investigate and protect public safety.','police'),make('community','Public-Facing & Community Work','Work directly with people and communities.','community'),make('teaching','Teaching & Mentoring','Teach and guide learners.','education'),make('research','Research & Scientific Work','Investigate complex questions.','research'),make('medical','Healthcare & Medical Service','Deliver public health services.','health'),make('digital','IT & Digital Systems','Improve public systems through technology.','digital'),make('field','Field Inspection & Operations','Inspect and operate on the ground.','technical')] },
  { id:'governmentLevels', title:'Which government environment would you prefer?', hint:'Choose all that apply.', max:5, options:[make('central','Central Government','National departments and institutions.'),make('state','State Government','State administration and departments.'),make('psu','PSU / Government-Owned Organization','Public-sector technical and operational roles.','technical'),make('regulatory','Regulatory / Autonomous Bodies','Specialised institutions and regulators.','legal'),make('any','Open to Any Government Role','Show the best fit across levels.')] },
  { id:'priorities', title:'What matters most to you in a government career?', hint:'Choose up to 3.', max:3, options:simple(['Job Stability','Public Service & Social Impact','Administrative Authority','Salary & Benefits','Career Growth','Professional Prestige','Work-Life Balance','Technical Expertise','Leadership Opportunities','Location Stability','Learning & Skill Development','Respect & Responsibility']) },
  { id:'selectionProcesses', title:'Which type of recruitment process are you most comfortable preparing for?', hint:'Choose up to 2.', max:2, options:simple(['Highly Competitive National-Level Exam','State-Level Competitive Exam','Technical / Subject-Specific Exam','Aptitude-Based Exam','Written Exam + Interview','Written Exam + Skill Test','Written Exam + Physical/Medical Test','Recruitment Based on Specialized Qualification','Open to Any Selection Process']) },
  { id:'preparation', title:'How much time can you realistically commit to government exam preparation?', hint:'Then choose your weekly study time.', compound:true, options:simple(['Up to 3 months','3–6 months','6–12 months','1–2 years','2+ years','Depends on the opportunity']), weekly:simple(['Less than 5 hours','5–10 hours','10–20 hours','20–30 hours','30+ hours','Full-time preparation']) },
  { id:'location', title:'How flexible are you about where you work?', hint:'Your answer helps balance location fit and opportunity.', options:simple(['Anywhere in India','Any city in India','Prefer my state','Prefer my home region','Prefer my city/district','Only my preferred location']) },
  { id:'avoidConditions', title:'Which work conditions would you prefer to avoid?', hint:'Select everything you would strongly prefer not to have.', max:12, options:simple(['Heavy Field Work','Frequent Transfers','Physical/Fitness Requirements','High Public Interaction','High Administrative Responsibility','Financial/Accounting Work','Long/Irregular Working Hours','Remote/Rural Postings','Uniformed Service','Frequent Travel','High-Pressure Decision Making','Repetitive Desk Work']) },
  { id:'motivations', title:'What is your strongest reason for choosing a government career?', hint:'Choose up to 2.', max:2, options:simple(['Job Security','Serve Society','Leadership & Responsibility','Stable Income','Prestige & Recognition','Technical Career','Work-Life Balance','Social Impact','Career Growth','Opportunity to Work in Public Institutions','Long-Term Stability','Meaningful Work']) },
  { id:'strengths', title:'Which area best describes your strongest ability?', hint:'Self-assessment only — choose up to 2.', max:2, options:simple(['Leadership & Decision-Making','Quantitative & Analytical Thinking','Technical / Engineering Skills','Communication & Language','General Knowledge & Awareness','Financial & Numerical Skills','Scientific Thinking','People & Community Interaction','Technology & Digital Skills','Physical Fitness & Discipline','Teaching & Explanation','Research & Problem Solving']) },
  { id:'competitionTolerance', title:'How competitive an examination are you willing to prepare for?', hint:'This only calibrates the suggestions.', options:simple(['I am comfortable targeting extremely competitive exams','I am comfortable with highly competitive exams','I prefer moderate competition','I prefer a wider range of recruitment opportunities','I am not sure yet']) },
  { id:'targetDirection', title:'If you had to choose today, which direction would you most like to explore?', hint:'This is a preference signal, not the final decision.', options:simple(['Administrative Services','State Government Services','Banking & Finance','Engineering & Technical Services','Police & Defence','Railways & Transport','Education','Healthcare','Research & Science','IT & Digital Government','Revenue / Tax / Audit','Environment / Forest','Not Sure — Show Me My Best Matches']) },
];

export default function GovernmentJobsEntry() {
  const navigate = useNavigate(); const { careerProfile } = useCareerProfile(); const { user } = useAuth(); const [step, setStep] = useState(-1); const [answers, setAnswers] = useState({});
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [loadingStep, setLoadingStep] = useState(0);
  const q = QUESTIONS[step]; const selected = answers[q?.id] || (q?.max ? [] : '');
  const select = value => setAnswers(previous => { if (!q.max) return { ...previous, [q.id]:value }; const list = previous[q.id] || []; if (list.includes(value)) return { ...previous, [q.id]:list.filter(item => item !== value) }; return list.length >= q.max ? previous : { ...previous, [q.id]:[...list,value] }; });
  const valid = q?.compound ? answers.preparation && answers.weeklyStudyHours : q?.max ? selected.length > 0 : Boolean(selected);
  const next = () => {
    if (step === QUESTIONS.length - 1) {
      setIsAnalyzing(true);
      setLoadingStep(0);
      const saved = { answers, careerProfile, completedAt: new Date().toISOString() };
      window.localStorage.setItem(userStorageKey('skillscape-government-assessment', user), JSON.stringify(saved));
      let currentStep = 0;
      const interval = setInterval(() => {
        currentStep++;
        setLoadingStep(currentStep);
        if (currentStep >= GOVT_LOADING_MESSAGES.length) {
          clearInterval(interval);
          navigate('/government-jobs/results', { state: saved });
        }
      }, 750);
    } else {
      setStep(step + 1);
    }
  };

  if (isAnalyzing) {
    return (
      <QuizProcessingLoader
        title="Processing your results…"
        messages={GOVT_LOADING_MESSAGES}
        loadingStep={loadingStep}
      />
    );
  }
  if (step === -1) return <div className="max-w-2xl mx-auto pt-10"><Button variant="ghost" onClick={()=>navigate('/quiz')} className="mb-4"><ArrowLeft className="w-4 h-4 mr-2"/>Back to Career Quiz</Button><div className="bg-white border border-[#E2E8F0] rounded-[10px] p-8"><div className="w-10 h-10 rounded-md bg-[#EFF6FF] text-[#2563EB] flex items-center justify-center mb-5"><Building2 className="w-5 h-5" /></div><p className="text-xs font-semibold tracking-widest uppercase text-[#94A3B8]">Government Jobs</p><h1 className="text-[32px] font-bold tracking-tight text-[#0F172A] mt-2">Government Career Assessment</h1><p className="text-[#64748B] mt-3 leading-relaxed">Discover government careers and examinations that fit your profile, preferences and preparation capacity.</p><Button onClick={()=>setStep(0)} className="mt-6 bg-[#2563EB] hover:bg-[#1D4ED8]">Start assessment <ArrowRight className="w-4 h-4 ml-2" /></Button></div></div>;
  const Card=({item,picked,action})=>{const Icon=ICONS[item.icon]||Building2;return <button type="button" onClick={action} className={`text-left p-3 rounded-lg border transition-colors ${picked?'border-[#2563EB] bg-[#EFF6FF]':'border-[#E2E8F0] bg-white hover:border-[#93C5FD] hover:bg-[#FAFBFF]'}`}><div className="flex gap-2.5"><Icon className={`w-4 h-4 mt-0.5 shrink-0 ${picked?'text-[#2563EB]':'text-[#64748B]'}`}/><div className="min-w-0"><div className="flex gap-2"><p className="text-sm font-semibold text-[#0F172A]">{item.title}</p>{picked&&<Check className="w-4 h-4 text-[#2563EB] shrink-0"/>}</div><p className="text-xs text-[#64748B] mt-0.5 leading-snug">{item.description}</p></div></div></button>};
  const grid = (items, getPicked, action) => <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 mt-6">{items.map(item=><Card key={item.value} item={item} picked={getPicked(item)} action={()=>action(item)}/>)}</div>;
  return <div className="max-w-5xl mx-auto pb-10"><div className="flex items-center justify-between mb-5"><Button variant="ghost" onClick={()=>step===0?setStep(-1):setStep(step-1)} className="text-[#64748B]"><ArrowLeft className="w-4 h-4 mr-2"/>Back</Button><span className="text-sm text-[#64748B]">Question {step+1} of {QUESTIONS.length}</span></div><div className="h-1.5 bg-[#E2E8F0] rounded-full mb-7"><div className="h-full bg-[#2563EB] rounded-full" style={{width:`${((step+1)/QUESTIONS.length)*100}%`}}/></div><div className="bg-white border border-[#E2E8F0] rounded-[10px] p-6"><p className="text-xs font-semibold text-[#94A3B8] uppercase tracking-widest">Government Career Assessment</p><h1 className="text-2xl font-bold text-[#0F172A] mt-2">{q.title}</h1><p className="text-sm text-[#64748B] mt-2">{q.hint}{q.max&&<span className="ml-2 text-[#2563EB]">{selected.length} of {q.max} selected</span>}</p>{q.compound?<div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6"><div><p className="text-sm font-semibold text-[#0F172A] mb-2">Preparation duration</p>{grid(q.options,item=>answers.preparation===item.value,item=>setAnswers(a=>({...a,preparation:item.value})))}</div><div><p className="text-sm font-semibold text-[#0F172A] mb-2">Weekly study time</p>{grid(q.weekly,item=>answers.weeklyStudyHours===item.value,item=>setAnswers(a=>({...a,weeklyStudyHours:item.value})))}</div></div>:grid(q.options,item=>q.max?selected.includes(item.value):selected===item.value,item=>select(item.value))}<div className="flex justify-end mt-7 pt-5 border-t border-[#E2E8F0]"><Button disabled={!valid} onClick={next} className="bg-[#2563EB] hover:bg-[#1D4ED8]">{step===QUESTIONS.length-1?'See my matches':'Continue'} <ArrowRight className="w-4 h-4 ml-2"/></Button></div></div></div>;
}
