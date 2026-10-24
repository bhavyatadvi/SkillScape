import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import After10thForm from './After10thForm';
import After12thForm from './After12thForm';
import AfterGraduationForm from './AfterGraduationForm';
import AfterPostGraduationForm from './AfterPostGraduationForm';

export default function BackgroundForm({ quizType, onComplete, onBack }) {
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState({});

  const handleComplete = (data) => {
    onComplete(data);
  };

  const renderContent = () => {
    switch(quizType) {
      case 'After10th':
        return <After10thForm onComplete={handleComplete} onBack={onBack} />;
      case 'After12th':
        return <After12thForm onComplete={handleComplete} onBack={onBack} />;
      case 'AfterGraduation':
        return <AfterGraduationForm quizType={quizType} onComplete={handleComplete} onBack={onBack} />;
      case 'AfterPostGraduation':
        return <AfterPostGraduationForm onComplete={handleComplete} onBack={onBack} />;
      case 'CareerChange':
      case 'Upskilling':
      case 'GovernmentJobs':
        return <AfterGraduationForm quizType={quizType} onComplete={handleComplete} onBack={onBack} />;
      default:
        return (
          <div className="text-center p-8">
            <h2 className="text-2xl font-bold">Generic forms are no longer supported.</h2>
            <p className="text-slate-500 mt-2">Please select a specific education level.</p>
          </div>
        );
    }
  };

  return (
    <div className="w-full max-w-4xl mx-auto py-8">
      <AnimatePresence mode="wait">
        <motion.div
          key={`form-wrapper-${quizType}`}
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -20 }}
          transition={{ duration: 0.3 }}
        >
          {renderContent()}
        </motion.div>
      </AnimatePresence>
    </div>
  );
}
