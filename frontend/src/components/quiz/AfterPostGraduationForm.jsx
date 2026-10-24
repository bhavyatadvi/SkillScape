import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { ChevronRight, ArrowLeft, Target, Compass, Replace } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

const DEGREES = [
  'B.E.', 'B.Tech', 'B.Sc.', 'BCA', 'BBA', 'B.Com', 'BA', 'B.Arch', 'B.Pharm', 'MBBS', 'LLB', 'B.Des', 'Other'
];

const PG_DEGREES = [
  'M.E.', 'M.Tech', 'M.Sc.', 'MCA', 'MBA', 'M.Com', 'MA', 'M.Arch', 'M.Pharm', 'MD/MS', 'LLM', 'M.Des', 'Ph.D', 'Other'
];

const MAJORS = {
  'Engineering & Tech': ['Computer Science / IT', 'Artificial Intelligence / ML', 'Data Science', 'Civil Engineering', 'Mechanical Engineering', 'Electrical/Electronics Engineering', 'Other'],
  'Business & Commerce': ['Finance', 'Marketing', 'HR', 'Operations', 'Business Analytics', 'Accounting', 'Other'],
  'Sciences': ['Physics', 'Chemistry', 'Mathematics', 'Statistics', 'Biology/Biotech', 'Environmental Science', 'Other'],
  'Arts & Humanities': ['Psychology', 'Sociology', 'Political Science', 'English', 'Economics', 'History', 'Geography', 'Journalism / Mass Communication', 'Philosophy', 'Social Work', 'Other'],
  'Other Fields': ['Law', 'Medicine', 'Pharmacy', 'Architecture', 'Design', 'Other']
};

const FIELD_OPTIONS = {
  'Computer Science / IT': ['Artificial Intelligence', 'Machine Learning', 'Data Science', 'Cybersecurity', 'Cloud Computing', 'Software Engineering', 'IT Management'],
  'Artificial Intelligence / ML': ['Machine Learning Engineering', 'Deep Learning', 'Computer Vision', 'NLP', 'Generative AI', 'AI Research', 'Applied AI'],
  'Data Science': ['Data Engineering', 'Data Analytics', 'BI Analytics', 'Machine Learning', 'Research Science'],
  'Finance': ['Investment Banking', 'Corporate Finance', 'Financial Analysis', 'Risk Management', 'Asset Management'],
  'Psychology': ['Clinical Psychology', 'Counseling Psychology', 'Organizational Psychology', 'Educational Psychology', 'Behavioral Research'],
  'Sociology': ['Social Research', 'Social Work', 'Community Development', 'Public Policy', 'Academia / PhD'],
  'Political Science': ['Public Policy', 'International Relations', 'Political Research', 'Civil Services', 'Academia / PhD'],
  'English': ['Teaching / Academia', 'Editing / Publishing', 'Content Strategy', 'Linguistics', 'Research / PhD'],
  'Economics': ['Economic Research', 'Public Policy', 'Data Analysis', 'Banking / Finance', 'Academia / PhD'],
  'History': ['Teaching / Academia', 'Archival Research', 'Museums / Heritage', 'Civil Services', 'Research / PhD'],
  'Geography': ['GIS / Geospatial Analysis', 'Urban Planning', 'Environmental Policy', 'Research / PhD'],
  'Journalism / Mass Communication': ['Reporting', 'Editing', 'Digital Media', 'Public Relations', 'Content Strategy'],
  'Philosophy': ['Ethics / Policy Research', 'Academia / PhD', 'Civil Services', 'Writing / Publishing'],
  'Social Work': ['Community Development', 'NGO Programme Work', 'Policy / Development', 'Research / PhD'],
  // Default fallback
  'Default': ['Research', 'Academia', 'Management', 'Consulting', 'Specialist Practice', 'Industry Application']
};

const ALL_FIELDS = Object.values(FIELD_OPTIONS).flat().filter((value, index, self) => self.indexOf(value) === index);

export default function AfterPostGraduationForm({ onComplete, onBack }) {
  const navigate = useNavigate();
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState({
    previousDegree: '',
    postGraduationDegree: '',
    postGraduationMajorCategory: '',
    postGraduationMajor: '',
    postGraduationSpecialization: '',
    workExperience: '',
    currentStatus: '',
    preferenceMode: '', // 'SELECTED', 'EXPLORE', 'CAREER_CHANGE'
    selectedFields: [],
    selectedCareers: '' // free text input for specific roles
  });

  const getRelevantFields = () => {
    if (formData.preferenceMode === 'CAREER_CHANGE') return ALL_FIELDS;
    return FIELD_OPTIONS[formData.postGraduationMajor] || FIELD_OPTIONS[formData.postGraduationMajorCategory] || FIELD_OPTIONS['Default'];
  };

  const toggleField = (field) => {
    setFormData(prev => {
      const fields = prev.selectedFields || [];
      if (fields.includes(field)) {
        return { ...prev, selectedFields: fields.filter(f => f !== field) };
      } else {
        return { ...prev, selectedFields: [...fields, field] };
      }
    });
  };

  const handleChange = (name, value) => setFormData(prev => ({
    ...prev,
    [name]: value,
    ...(name === 'preferenceMode' && value !== 'SELECTED' ? { selectedFields: [] } : {})
  }));

  const handleNext = () => {
    if (step === 2 && formData.preferenceMode === 'EXPLORE') {
      handleSubmit();
    } else {
      setStep(prev => prev + 1);
    }
  };

  const handlePrev = () => {
    if (step === 1) onBack();
    else setStep(prev => prev - 1);
  };

  const handleSubmit = () => {
    const profile = {
      educationLevel: 'after_post_graduation',
      previousDegree: formData.previousDegree,
      postGraduationDegree: formData.postGraduationDegree,
      postGraduationMajor: formData.postGraduationMajor,
      postGraduationSpecialization: formData.postGraduationSpecialization,
      workExperience: formData.workExperience,
      currentStatus: formData.currentStatus,
      preferenceMode: formData.preferenceMode,
      selectedFields: JSON.stringify(formData.selectedFields || []),
      targetCareers: formData.selectedCareers,
      careerChange: (formData.preferenceMode === 'CAREER_CHANGE').toString()
    };
    onComplete(profile);
  };

  return (
    <div className="space-y-6 max-w-2xl mx-auto">
      <div className="text-center mb-8">
        <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">Post-Graduation Profile</h2>
        <p className="text-slate-500 mt-2">Help us customize your advanced career assessment.</p>
      </div>

      <AnimatePresence mode="wait">
        {step === 1 && (
          <motion.div key="step1" initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -20 }} className="space-y-4">
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Previous UG Degree</label>
                <select value={formData.previousDegree} onChange={(e) => handleChange('previousDegree', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                  <option value="">Select Degree...</option>
                  {DEGREES.map(d => <option key={d} value={d}>{d}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">PG Degree</label>
                <select value={formData.postGraduationDegree} onChange={(e) => handleChange('postGraduationDegree', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                  <option value="">Select PG Degree...</option>
                  {PG_DEGREES.map(d => <option key={d} value={d}>{d}</option>)}
                </select>
              </div>
            </div>

            <div>
              <label className="block text-sm font-bold text-slate-700 mb-2">Broad Field of PG Study</label>
              <select value={formData.postGraduationMajorCategory} onChange={(e) => { handleChange('postGraduationMajorCategory', e.target.value); handleChange('postGraduationMajor', ''); }} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                <option value="">Select Category...</option>
                {Object.keys(MAJORS).map(c => <option key={c} value={c}>{c}</option>)}
              </select>
            </div>
            
            {formData.postGraduationMajorCategory && (
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Specific Major / Branch</label>
                <select value={formData.postGraduationMajor} onChange={(e) => handleChange('postGraduationMajor', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                  <option value="">Select Major...</option>
                  {MAJORS[formData.postGraduationMajorCategory].map(m => <option key={m} value={m}>{m}</option>)}
                </select>
              </div>
            )}

            <div>
              <label className="block text-sm font-bold text-slate-700 mb-2">PG Specialization / Research Area (Optional)</label>
              <input type="text" placeholder="e.g. Deep Learning, Corporate Finance..." value={formData.postGraduationSpecialization} onChange={(e) => handleChange('postGraduationSpecialization', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none" />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Work Experience</label>
                <select value={formData.workExperience} onChange={(e) => handleChange('workExperience', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                  <option value="">Select Experience...</option>
                  <option value="None">None</option>
                  <option value="Less than 1 year">Less than 1 year</option>
                  <option value="1-3 years">1-3 years</option>
                  <option value="3-5 years">3-5 years</option>
                  <option value="5+ years">5+ years</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Current Status</label>
                <select value={formData.currentStatus} onChange={(e) => handleChange('currentStatus', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                  <option value="">Select Status...</option>
                  <option value="Student">Student</option>
                  <option value="Working">Working</option>
                  <option value="Unemployed">Unemployed</option>
                  <option value="Research">Research / PhD</option>
                </select>
              </div>
            </div>
            
            <div className="flex justify-between mt-8 pt-6 border-t border-slate-100">
              <Button variant="ghost" onClick={handlePrev} className="text-slate-500 hover:bg-slate-50">
                <ArrowLeft className="w-4 h-4 mr-2" /> Back
              </Button>
              <Button onClick={handleNext} disabled={!formData.previousDegree || !formData.postGraduationDegree || !formData.postGraduationMajor} className="bg-blue-600 hover:bg-blue-700 text-white px-8">
                Next <ChevronRight className="w-4 h-4 ml-2" />
              </Button>
            </div>
          </motion.div>
        )}

        {step === 2 && (
          <motion.div key="step2" initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -20 }} className="space-y-4">
            <label className="block text-sm font-bold text-slate-700 mb-4">What is your career objective?</label>
            <div className="space-y-3">
              <button type="button" onClick={() => handleChange('preferenceMode', 'SELECTED')} className={`w-full p-4 rounded-xl border text-left flex items-center gap-3 transition-colors ${formData.preferenceMode === 'SELECTED' ? 'border-blue-600 bg-blue-50 ring-1 ring-blue-600' : 'border-slate-200 bg-white hover:border-blue-300'}`}>
                <Target className={`w-5 h-5 ${formData.preferenceMode === 'SELECTED' ? 'text-blue-600' : 'text-slate-400'}`} />
                <div>
                  <div className={`font-semibold ${formData.preferenceMode === 'SELECTED' ? 'text-blue-900' : 'text-slate-700'}`}>I have specific career goals in mind</div>
                  <div className="text-sm text-slate-500">I want the quiz to evaluate my fit for specific roles (e.g. ML Engineer vs Data Scientist).</div>
                </div>
              </button>

              <button type="button" onClick={() => handleChange('preferenceMode', 'EXPLORE')} className={`w-full p-4 rounded-xl border text-left flex items-center gap-3 transition-colors ${formData.preferenceMode === 'EXPLORE' ? 'border-blue-600 bg-blue-50 ring-1 ring-blue-600' : 'border-slate-200 bg-white hover:border-blue-300'}`}>
                <Compass className={`w-5 h-5 ${formData.preferenceMode === 'EXPLORE' ? 'text-blue-600' : 'text-slate-400'}`} />
                <div>
                  <div className={`font-semibold ${formData.preferenceMode === 'EXPLORE' ? 'text-blue-900' : 'text-slate-700'}`}>I want to explore options in my PG field</div>
                  <div className="text-sm text-slate-500">Discover the best paths within my current academic domain.</div>
                </div>
              </button>

              <button type="button" onClick={() => navigate('/career-switch')} className={`w-full p-4 rounded-xl border text-left flex items-center gap-3 transition-colors ${formData.preferenceMode === 'CAREER_CHANGE' ? 'border-blue-600 bg-blue-50 ring-1 ring-blue-600' : 'border-slate-200 bg-white hover:border-blue-300'}`}>
                <Replace className={`w-5 h-5 ${formData.preferenceMode === 'CAREER_CHANGE' ? 'text-blue-600' : 'text-slate-400'}`} />
                <div>
                  <div className={`font-semibold ${formData.preferenceMode === 'CAREER_CHANGE' ? 'text-blue-900' : 'text-slate-700'}`}>I want a Career Change</div>
                  <div className="text-sm text-slate-500">Switch to a different field outside my PG major.</div>
                </div>
              </button>
            </div>
            
            <div className="flex justify-between mt-8 pt-6 border-t border-slate-100">
              <Button variant="ghost" onClick={handlePrev} className="text-slate-500 hover:bg-slate-50">
                <ArrowLeft className="w-4 h-4 mr-2" /> Back
              </Button>
              <Button onClick={handleNext} disabled={!formData.preferenceMode} className="bg-blue-600 hover:bg-blue-700 text-white px-8">
                {formData.preferenceMode === 'EXPLORE' ? 'Start Assessment' : 'Next'} <ChevronRight className="w-4 h-4 ml-2" />
              </Button>
            </div>
          </motion.div>
        )}

        {step === 3 && (formData.preferenceMode === 'SELECTED' || formData.preferenceMode === 'CAREER_CHANGE') && (
          <motion.div key="step3" initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -20 }} className="space-y-6">
            
            <div>
              <label className="block text-sm font-bold text-slate-700 mb-3">
                {formData.preferenceMode === 'CAREER_CHANGE' ? 'Which new fields are you considering?' : 'Which paths are you deciding between?'}
              </label>
              <div className="flex flex-wrap gap-2 max-h-64 overflow-y-auto p-2 border border-slate-200 rounded-xl bg-slate-50">
                {getRelevantFields().map(field => (
                  <button 
                    key={field} 
                    type="button" 
                    onClick={() => toggleField(field)}
                    className={`px-4 py-2 rounded-full border text-sm font-medium transition-all ${formData.selectedFields?.includes(field) ? 'bg-blue-100 border-blue-500 text-blue-700' : 'bg-white border-slate-200 text-slate-600 hover:border-blue-300'}`}
                  >
                    {field}
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label className="block text-sm font-bold text-slate-700 mb-2">Specific Target Careers (Optional)</label>
              <p className="text-xs text-slate-500 mb-2">List specific roles (e.g. AI Research Scientist, Product Manager).</p>
              <input type="text" placeholder="e.g. Machine Learning Engineer, Risk Analyst..." value={formData.selectedCareers} onChange={(e) => handleChange('selectedCareers', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none" />
            </div>
            
            <div className="flex justify-between mt-8 pt-6 border-t border-slate-100">
              <Button variant="ghost" onClick={handlePrev} className="text-slate-500 hover:bg-slate-50">
                <ArrowLeft className="w-4 h-4 mr-2" /> Back
              </Button>
              <Button onClick={handleSubmit} disabled={formData.selectedFields.length === 0 && !formData.selectedCareers} className="bg-blue-600 hover:bg-blue-700 text-white px-8">
                Start Assessment <ChevronRight className="w-4 h-4 ml-2" />
              </Button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
