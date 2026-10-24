import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { roadmapApi } from '@/services/api';
import { toast } from 'sonner';
import { Sparkles, Loader2, Compass, ArrowRight, X } from 'lucide-react';

const examplePrompts = [
  {
    title: "12th Fail Entrepreneur",
    goal: "I am a 12th fail and I want to start my own business/become an entrepreneur",
    situation: "No formal degree, basic English, high motivation for hands-on business."
  },
  {
    title: "BA to UI/UX Designer",
    goal: "I am a BA graduate and want to become a UI/UX Designer with no coding experience",
    situation: "Arts background, comfortable with visual design tools like Figma."
  },
  {
    title: "Mechanical to Data Science",
    goal: "I am a Mechanical Engineer wanting to switch to Data Science & AI",
    situation: "Engineering background, basic Python interest, no prior ML experience."
  }
];

export default function AskAIRoadmapBuilder({ onRoadmapGenerated, onClose }) {
  const [goalText, setGoalText] = useState('');
  const [situationText, setSituationText] = useState('');
  const [generating, setGenerating] = useState(false);
  const [loadingStep, setLoadingStep] = useState(0);

  const handleChipClick = (example) => {
    setGoalText(example.goal);
    setSituationText(example.situation);
  };

  const handleGenerate = async () => {
    if (!goalText.trim()) {
      toast.error('Please describe your target goal or career path.');
      return;
    }

    try {
      setGenerating(true);
      setLoadingStep(1);

      const stepTimer1 = setTimeout(() => setLoadingStep(2), 2000);
      const stepTimer2 = setTimeout(() => setLoadingStep(3), 4500);

      const res = await roadmapApi.askAIRoadmap(goalText.trim(), situationText.trim());
      clearTimeout(stepTimer1);
      clearTimeout(stepTimer2);

      const result = res.data?.data;
      toast.success('Your personalized AI roadmap has been generated! 🎉');

      if (onRoadmapGenerated) {
        onRoadmapGenerated(result);
      }
    } catch (err) {
      toast.error(err.message || 'Failed to generate custom AI roadmap.');
    } finally {
      setGenerating(false);
      setLoadingStep(0);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-white rounded-3xl p-6 md:p-8 shadow-sm relative overflow-hidden mb-8 border border-[#E2E8F0]"
    >
      {/* Background ambient glow - very subtle for light mode */}
      <div className="absolute -right-20 -top-20 w-[400px] h-[400px] bg-[#EFF6FF] rounded-full blur-[80px] pointer-events-none" />
      <div className="absolute -left-20 -bottom-20 w-[400px] h-[400px] bg-indigo-50 rounded-full blur-[80px] pointer-events-none" />

      <div className="relative z-10 space-y-7">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-4">
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-[#EFF6FF] text-[#2563EB] border border-[#DBEAFE]">
              <Sparkles className="w-6 h-6" />
            </div>
            <div>
              <span className="text-[10px] font-bold uppercase tracking-[0.2em] text-[#64748B] mb-1 block">Free-form AI Navigator</span>
              <h2 className="text-xl md:text-2xl font-bold text-[#0F172A] tracking-tight">Don't see your path? Ask AI.</h2>
            </div>
          </div>
          {onClose && (
            <button
              onClick={onClose}
              className="p-2 rounded-xl text-[#94A3B8] hover:text-[#0F172A] hover:bg-slate-50 transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          )}
        </div>

        <p className="text-sm text-[#64748B] leading-relaxed max-w-2xl">
          Whether you are a dropout, non-degree learner, career switcher, or aiming for a non-traditional goal — describe your situation and AI will build a tailored curriculum with module quizzes.
        </p>

        {/* Example Prompt Chips */}
        <div>
          <span className="text-[10px] font-bold uppercase tracking-wider text-[#94A3B8] block mb-3">Try an Example Prompt</span>
          <div className="flex flex-wrap gap-2.5">
            {examplePrompts.map((example, idx) => (
              <button
                key={idx}
                type="button"
                onClick={() => handleChipClick(example)}
                className="text-xs font-medium bg-[#FAFBFF] hover:bg-[#F1F5F9] border border-[#E2E8F0] px-4 py-2 rounded-full transition-all text-[#475569] shadow-sm"
              >
                {example.title}
              </button>
            ))}
          </div>
        </div>

        {/* Inputs */}
        <div className="space-y-5">
          <div className="space-y-2">
            <label className="block text-[10px] font-bold text-[#64748B] uppercase tracking-wider">What is your dream goal or target role?</label>
            <textarea
              rows={2}
              value={goalText}
              onChange={(e) => setGoalText(e.target.value)}
              placeholder="e.g. I am a 12th fail and want to become an entrepreneur / start a business in India"
              className="w-full rounded-xl bg-white border border-[#E2E8F0] px-4 py-3 text-sm text-[#0F172A] placeholder-[#94A3B8] focus:outline-none focus:border-[#2563EB] focus:ring-1 focus:ring-[#2563EB] transition-all shadow-sm"
            />
          </div>

          <div className="space-y-2">
            <label className="block text-[10px] font-bold text-[#64748B] uppercase tracking-wider">Current Situation / Constraints (Optional)</label>
            <input
              type="text"
              value={situationText}
              onChange={(e) => setSituationText(e.target.value)}
              placeholder="e.g. No coding knowledge, 1-2 hours daily, prefer Indian market context"
              className="w-full rounded-xl bg-white border border-[#E2E8F0] px-4 py-3 text-sm text-[#0F172A] placeholder-[#94A3B8] focus:outline-none focus:border-[#2563EB] focus:ring-1 focus:ring-[#2563EB] transition-all shadow-sm"
            />
          </div>
        </div>

        {/* Action button & Loading Status */}
        {generating ? (
          <div className="p-5 rounded-2xl bg-[#EFF6FF] border border-[#DBEAFE] text-center space-y-3">
            <Loader2 className="w-6 h-6 animate-spin mx-auto text-[#2563EB]" />
            <p className="text-sm font-medium text-[#1E40AF]">
              {loadingStep === 1 && "Understanding your goal & starting point..."}
              {loadingStep === 2 && "Designing multi-phase curriculum & topics..."}
              {loadingStep === 3 && "Adding module quizzes & finalizing assessment..."}
            </p>
          </div>
        ) : (
          <div className="flex justify-end pt-3">
            <Button
              onClick={handleGenerate}
              className="bg-[#2563EB] hover:bg-[#1D4ED8] text-white font-medium px-8 py-2.5 rounded-xl text-sm shadow-md transition-all"
            >
              <Sparkles className="w-4 h-4 mr-2 text-white" /> Generate My AI Roadmap
            </Button>
          </div>
        )}
      </div>
    </motion.div>
  );
}
