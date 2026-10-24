import React from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Replace } from 'lucide-react';
import { useAuth } from '@/context/AuthContext';
import { useCareerProfile } from '@/context/CareerProfileContext';

export default function CareerSwitchEntry() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const { careerProfile } = useCareerProfile();

  // Use CareerProfile Context first, fallback to Auth user profile
  const occupation = careerProfile?.currentOccupation || user?.profile?.occupation || "Unknown";
  const education = careerProfile?.degree || user?.profile?.degree || "Unknown";
  const exp = careerProfile?.experienceYears || user?.profile?.yearsOfExperience || "Unknown";

  return (
    <div className="min-h-screen pt-24 pb-12 px-4 bg-slate-50 flex items-center justify-center">
      <motion.div
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        className="text-center p-8 max-w-2xl mx-auto w-full"
      >
        <div className="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-6 shadow-inner">
          <Replace className="w-10 h-10 text-blue-600" />
        </div>
        <h1 className="text-4xl font-extrabold text-slate-900 mb-4 tracking-tight">REVERSE CAREER FINDER</h1>
        <p className="text-xl text-slate-600 mb-10 leading-relaxed">
          Your current career is only your starting point. Discover realistic alternative careers based on 
          your skills, experience, interests, and goals.
        </p>

        <div className="bg-white rounded-2xl shadow-md shadow-slate-200/50 border border-slate-200 p-8 text-left mb-10">
          <h3 className="text-sm font-bold text-slate-500 uppercase tracking-widest mb-6">Current Profile Overview</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="bg-slate-50 p-4 rounded-xl border border-slate-100">
              <p className="text-xs text-slate-500 font-medium mb-1">Highest Education</p>
              <p className="font-semibold text-slate-900 text-lg">{education}</p>
            </div>
            <div className="bg-slate-50 p-4 rounded-xl border border-slate-100">
              <p className="text-xs text-slate-500 font-medium mb-1">Current Occupation</p>
              <p className="font-semibold text-slate-900 text-lg">{occupation}</p>
            </div>
            <div className="bg-slate-50 p-4 rounded-xl border border-slate-100">
              <p className="text-xs text-slate-500 font-medium mb-1">Experience</p>
              <p className="font-semibold text-slate-900 text-lg">
                {exp} {exp !== "Unknown" && exp !== "" ? "years" : ""}
              </p>
            </div>
          </div>
        </div>

        <div className="flex flex-col sm:flex-row justify-center gap-4">
          <Button 
            size="lg" 
            variant="outline" 
            className="border-2 font-semibold h-14 px-8 text-slate-700"
            onClick={() => navigate('/quiz')}
          >
            Back to Career Quiz
          </Button>
          <Button 
            size="lg" 
            className="bg-blue-600 hover:bg-blue-700 text-white font-semibold h-14 px-8 shadow-lg shadow-blue-600/20"
            onClick={() => navigate('/career-switch/pre-assessment')}
          >
            Start Career Switch Assessment
          </Button>
        </div>
      </motion.div>
    </div>
  );
}
