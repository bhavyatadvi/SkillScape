import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import { ChevronRight, ArrowLeft } from 'lucide-react';

export default function After10thForm({ onComplete, onBack }) {
  const [formData, setFormData] = useState({});

  const handleChange = (name, value) => {
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = () => {
    onComplete({
      ...formData,
      degree: '10th',
      major: formData.favSubjects
    });
  };

  const OptionCard = ({ title, value, selectedValue, onClick }) => (
    <button
      type="button"
      onClick={() => onClick(value)}
      className={`p-6 rounded-2xl border text-left transition-all duration-300 ${
        selectedValue === value 
          ? 'border-blue-600 bg-blue-50/50 shadow-md ring-1 ring-blue-600' 
          : 'border-slate-200 bg-white hover:border-blue-300 hover:shadow-sm'
      }`}
    >
      <h3 className={`font-bold text-lg ${selectedValue === value ? 'text-blue-900' : 'text-slate-700'}`}>{title}</h3>
    </button>
  );

  return (
    <div className="space-y-6 max-w-2xl mx-auto">
      <div className="text-center mb-8">
        <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">Tell us about your academics</h2>
        <p className="text-slate-500 mt-2">What subjects do you enjoy the most?</p>
      </div>
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-bold text-slate-700 mb-2">Favorite Subjects</label>
          <input type="text" placeholder="e.g. Science, Mathematics" value={formData.favSubjects || ''} onChange={(e) => handleChange('favSubjects', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 focus:border-transparent outline-none transition-all" />
        </div>
        <div>
          <label className="block text-sm font-bold text-slate-700 mb-2">Learning Style Preference</label>
          <div className="grid grid-cols-2 gap-4">
            <OptionCard title="Theoretical / Academic" value="academic" selectedValue={formData.learningStyle} onClick={(v) => handleChange('learningStyle', v)} />
            <OptionCard title="Practical / Hands-on" value="practical" selectedValue={formData.learningStyle} onClick={(v) => handleChange('learningStyle', v)} />
          </div>
        </div>
      </div>
      <div className="flex justify-between mt-8 pt-6 border-t border-slate-100">
        <Button variant="ghost" onClick={onBack} className="text-slate-500 hover:bg-slate-50">
          <ArrowLeft className="w-4 h-4 mr-2" /> Back
        </Button>
        <Button onClick={handleSubmit} disabled={!formData.favSubjects || !formData.learningStyle} className="bg-blue-600 hover:bg-blue-700 text-white px-8">
          Start Quiz <ChevronRight className="w-4 h-4 ml-2" />
        </Button>
      </div>
    </div>
  );
}
