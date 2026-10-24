import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { ArrowRight, ArrowLeft } from 'lucide-react';
import { useCareerProfile } from '@/context/CareerProfileContext';
import { useAuth } from '@/context/AuthContext';

const BACKGROUND_QUESTIONS = [
  {
    id: 'bg_field',
    title: "What is your current or most recent career field / domain?",
    subtitle: "Reverse Career Matching will map realistic transition routes starting from your domain.",
    multi: false,
    options: [
      "Software & IT / Tech",
      "Civil Engineering & Construction",
      "Mechanical & Manufacturing Engineering",
      "Electrical & Electronics Engineering",
      "Medicine & Healthcare (Doctor / MBBS / Clinical)",
      "Pharmacy & Pharmaceutical Sciences",
      "Business Administration & Management",
      "Banking, Finance & Accounting",
      "Teaching, Education & Academia",
      "Marketing, Sales & Growth",
      "Law & Legal Services",
      "Science & Laboratory Research",
      "Design, UI/UX & Creative",
      "Other / Non-traditional Field"
    ]
  },
  {
    id: 'bg_degree',
    title: "What is your highest qualification or degree?",
    subtitle: "Helps tailor qualification prerequisites for alternative career roles.",
    multi: false,
    options: [
      "B.Tech / B.E. (Engineering)",
      "MBBS / BDS / Medical Degree",
      "B.Com / M.Com (Commerce & Finance)",
      "MBA / PGDM (Business & Management)",
      "B.Sc / M.Sc (Sciences)",
      "BCA / MCA (Computer Applications)",
      "B.A. / M.A. (Arts, Humanities & Social Sciences)",
      "Diploma / Professional Certification",
      "Other Degree"
    ]
  },
  {
    id: 'bg_experience',
    title: "How many years of professional experience do you have?",
    subtitle: "Assesses seniority transferability and transition difficulty.",
    multi: false,
    options: [
      "Fresher / Less than 1 year",
      "1 – 3 years",
      "3 – 5 years",
      "5 – 8 years",
      "8+ years"
    ]
  }
];

const PRE_QUESTIONS = [
  {
    id: 1,
    title: "Why are you considering a career change?",
    multi: true,
    options: [
      "Better salary", "Better work-life balance", "More interesting work",
      "More creativity", "More technical work", "More leadership",
      "More job security", "Remote/flexible work", "More meaningful work",
      "Career growth", "Current field has limited opportunities",
      "I dislike my current work", "I want to use different skills",
      "I want to enter a growing industry", "Other"
    ]
  },
  {
    id: 2,
    title: "How far are you willing to move from your current career?",
    multi: false,
    options: [
      "1. Very close to current career",
      "2. Related role, different responsibilities",
      "3. Related industry, transferable skills",
      "4. Different industry, similar skill base",
      "5. Major career change requiring reskilling",
      "6. Completely different career",
      "7. Not sure"
    ]
  },
  {
    id: 3,
    title: "Which parts of your current experience would you like to carry into your next career?",
    multi: true,
    options: [
      "Technical knowledge", "Domain knowledge", "Industry knowledge",
      "Problem solving", "Analytical skills", "Communication", "Leadership",
      "Project management", "Research", "Customer interaction", "Operations",
      "Documentation", "Design", "Planning", "Teaching", "Other"
    ]
  },
  {
    id: 4,
    title: "Which activities do you NOT want in your next career?",
    multi: true,
    options: [
      "Heavy coding", "Sales", "Public speaking", "Field work",
      "Customer-facing work", "Repetitive work", "High-pressure work",
      "Night shifts", "Manual work", "None of these"
    ]
  },
  {
    id: 5,
    title: "What kinds of work genuinely interest you?",
    multi: true,
    options: [
      "Building things", "Designing things", "Analyzing data",
      "Solving technical problems", "Research", "Helping people",
      "Healthcare", "Managing projects", "Leading teams",
      "Working with machines", "Working with technology", "Writing",
      "Communication", "Teaching", "Business strategy", "Finance",
      "Law", "Government/public service", "Environment", "Science",
      "Creative work", "Working outdoors", "Working with customers",
      "Entrepreneurship"
    ]
  },
  {
    id: 6,
    title: "What is your preferred level of reskilling?",
    multi: false,
    options: [
      "Minimal reskilling",
      "Some upskilling",
      "Moderate reskilling",
      "Major reskilling",
      "I am willing to study for a new qualification"
    ]
  },
  {
    id: 7,
    title: "How much time can you realistically spend transitioning?",
    multi: false,
    options: [
      "< 5 hours/week",
      "5–10 hours/week",
      "10–15 hours/week",
      "15–20 hours/week",
      "20+ hours/week"
    ]
  },
  {
    id: 8,
    title: "What is your target transition timeframe?",
    multi: false,
    options: [
      "< 3 months",
      "3–6 months",
      "6–12 months",
      "1–2 years",
      "2+ years",
      "No fixed timeline"
    ]
  }
];

const ALL_QUESTIONS = [...BACKGROUND_QUESTIONS, ...PRE_QUESTIONS];

export default function CareerSwitchPreAssessment() {
  const navigate = useNavigate();
  const { careerProfile, updateProfile } = useCareerProfile();
  const { user } = useAuth();
  const [step, setStep] = useState(0);
  
  const [answers, setAnswers] = useState(() => {
    const initial = ALL_QUESTIONS.reduce((acc, q) => ({ ...acc, [q.id]: q.multi ? [] : '' }), {});
    if (careerProfile?.currentOccupation) initial.bg_field = careerProfile.currentOccupation;
    if (careerProfile?.degree) initial.bg_degree = careerProfile.degree;
    if (careerProfile?.experienceYears) initial.bg_experience = `${careerProfile.experienceYears} years`;
    return initial;
  });

  const currentQ = ALL_QUESTIONS[step];

  const goToNext = (currentAnswers = answers, currentStep = step) => {
    if (currentStep < ALL_QUESTIONS.length - 1) {
      setStep(currentStep + 1);
    } else {
      const enrichedProfile = {
        ...(careerProfile || {}),
        currentOccupation: currentAnswers.bg_field || careerProfile?.currentOccupation || "General",
        degree: currentAnswers.bg_degree || careerProfile?.degree || "Bachelor's",
        experienceYears: currentAnswers.bg_experience || careerProfile?.experienceYears || "1-3 years",
        constraints: currentAnswers[4] || [],
        motivations: currentAnswers[1] || [],
        careerChangePreference: currentAnswers[2] || '',
      };

      if (updateProfile) {
        try { updateProfile(enrichedProfile); } catch (e) {}
      }

      navigate('/career-switch/assessment', {
        state: {
          preQuizAnswers: currentAnswers,
          careerProfile: enrichedProfile,
        }
      });
    }
  };

  const handleToggleArray = (id, value) => {
    setAnswers(prev => {
      const arr = prev[id] || [];
      if (arr.includes(value)) {
        return { ...prev, [id]: arr.filter(i => i !== value) };
      }
      return { ...prev, [id]: [...arr, value] };
    });
  };

  const handleSelectOne = (id, value) => {
    const updated = { ...answers, [id]: value };
    setAnswers(updated);
    // Auto advance for single select
    setTimeout(() => {
      goToNext(updated, step);
    }, 250);
  };

  const isValid = () => {
    const ans = answers[currentQ.id];
    return currentQ.multi ? (ans && ans.length > 0) : !!ans;
  };

  const progress = ((step + 1) / ALL_QUESTIONS.length) * 100;

  return (
    <div className="min-h-screen pt-24 pb-12 px-4 bg-slate-50">
      <div className="max-w-5xl lg:max-w-6xl mx-auto">
        
        {/* Progress Bar */}
        <div className="mb-8">
          <div className="flex justify-between text-sm font-medium text-slate-500 mb-2">
            <span>{step < 3 ? 'Current Background' : 'Career Switch Preferences'}</span>
            <span>Question {step + 1} of {ALL_QUESTIONS.length}</span>
          </div>
          <div className="w-full bg-slate-200 h-2 rounded-full overflow-hidden">
            <div 
              className="bg-blue-600 h-full transition-all duration-300"
              style={{ width: `${progress}%` }}
            ></div>
          </div>
        </div>

        <AnimatePresence mode="wait">
          <motion.div
            key={step}
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.2 }}
            className="bg-white rounded-2xl shadow-sm border border-slate-200 p-8 sm:p-10"
          >
            <Button variant="ghost" className="mb-6 -ml-4" onClick={() => step > 0 ? setStep(step - 1) : navigate('/quiz')}>
              <ArrowLeft className="w-4 h-4 mr-2" /> Back
            </Button>
            
            <h2 className="text-2xl font-bold text-slate-900 mb-1">{currentQ.title}</h2>
            {currentQ.subtitle && <p className="text-sm text-slate-500 mb-4">{currentQ.subtitle}</p>}
            {currentQ.multi && <p className="text-xs font-semibold text-blue-600 uppercase tracking-wider mb-6">Select all that apply</p>}

            <div className={`grid gap-2.5 ${currentQ.options.length > 8 ? 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3' : 'grid-cols-1 sm:grid-cols-2'}`}>
              {currentQ.options.map((option, idx) => {
                const isChecked = currentQ.multi 
                  ? (answers[currentQ.id] || []).includes(option) 
                  : answers[currentQ.id] === option;
                  
                return (
                  <div
                    key={idx}
                    className={`p-3.5 rounded-xl border cursor-pointer transition-all flex items-center ${
                      isChecked 
                        ? 'border-blue-600 bg-blue-50/50 shadow-sm' 
                        : 'border-slate-200 hover:border-blue-300 hover:bg-slate-50'
                    }`}
                    onClick={() => currentQ.multi ? handleToggleArray(currentQ.id, option) : handleSelectOne(currentQ.id, option)}
                  >
                    {currentQ.multi ? (
                      <Checkbox checked={isChecked} className="mr-3 pointer-events-none" />
                    ) : (
                      <div className={`w-5 h-5 rounded-full border-2 mr-3 flex items-center justify-center shrink-0 ${isChecked ? 'border-blue-600' : 'border-slate-300'}`}>
                        {isChecked && <div className="w-2.5 h-2.5 rounded-full bg-blue-600" />}
                      </div>
                    )}
                    <span className={`text-sm font-medium leading-snug ${isChecked ? 'text-blue-900 font-semibold' : 'text-slate-700'}`}>{option}</span>
                  </div>
                );
              })}
            </div>

            <div className="mt-10 flex justify-end">
              {currentQ.multi && (
                <Button
                  size="lg"
                  className="bg-blue-600 hover:bg-blue-700 text-white shadow-md shadow-blue-200 px-8"
                  onClick={() => goToNext(answers, step)}
                  disabled={!isValid()}
                >
                  {step === ALL_QUESTIONS.length - 1 ? 'Start Adaptive Assessment' : 'Continue'} 
                  <ArrowRight className="w-4 h-4 ml-2" />
                </Button>
              )}
            </div>
          </motion.div>
        </AnimatePresence>

      </div>
    </div>
  );
}
