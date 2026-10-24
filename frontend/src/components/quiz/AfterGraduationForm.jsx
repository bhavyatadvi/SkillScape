import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { ChevronRight, ArrowLeft, Target, Compass, Replace } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

const EDUCATION_LEVELS = [
  'Final Year Undergraduate',
  'Recently Graduated',
  'Graduate with Work Experience',
  'Graduate Currently Unemployed',
  'Other'
];

const DEGREES = [
  'B.E.', 'B.Tech', 'B.Sc.', 'BCA', 'BBA', 'B.Com', 'BA', 'B.Arch', 'B.Pharm', 'MBBS', 'LLB', 'B.Des', 'Other'
];

const DEGREE_BRANCHES = {
  'B.E.': ['Computer Science', 'Information Technology', 'Civil Engineering', 'Mechanical Engineering', 'Electrical Engineering', 'Electronics Engineering', 'Chemical Engineering', 'Other'],
  'B.Tech': ['Computer Science', 'Information Technology', 'Civil Engineering', 'Mechanical Engineering', 'Electrical Engineering', 'Electronics Engineering', 'Chemical Engineering', 'Other'],
  'B.Sc.': ['Physics', 'Chemistry', 'Mathematics', 'Biology', 'Biotechnology', 'Environmental Science', 'Computer Science', 'IT', 'Other'],
  'BCA': ['Computer Applications', 'Software Development', 'Data Analytics', 'Other'],
  'BBA': ['Marketing', 'Finance', 'Human Resources', 'International Business', 'Operations', 'Other'],
  'B.Com': ['Accounting', 'Finance', 'Banking', 'Taxation', 'Economics', 'Business Analytics', 'Management', 'Other'],
  // Keep the BA major list aligned with the specialization-specific quiz coverage.
  'BA': ['Psychology', 'Sociology', 'Political Science'],
  'B.Arch': ['Architecture', 'Interior Design', 'Other'],
  'B.Pharm': ['Pharmacy', 'Clinical Research', 'Other'],
  'MBBS': ['General Medicine', 'Surgery', 'Other'],
  'LLB': ['Corporate Law', 'Criminal Law', 'Civil Law', 'Other'],
  'B.Des': ['Fashion Design', 'Industrial Design', 'Graphic Design', 'Other'],
  'Other': ['Other']
};

const STATUSES = [
  'Final Year Student', 'Recently Graduated', 'Working', 'Unemployed', 'Preparing for Higher Studies', 'Other'
];

const FIELD_OPTIONS = {
  'Computer Science': ['Artificial Intelligence', 'Machine Learning', 'Data Science', 'Data Engineering', 'Cybersecurity', 'Cloud Computing', 'Software Engineering', 'DevOps', 'Computer Networks'],
  'Information Technology': ['Cybersecurity', 'Cloud Computing', 'Software Engineering', 'Data Analytics', 'IT Management'],
  'IT': ['Cybersecurity', 'Cloud Computing', 'Software Engineering', 'Data Analytics', 'IT Management'],
  'Computer Applications': ['Software Development', 'Mobile Applications', 'Web Applications', 'Data Analytics', 'Cloud Computing'],
  'Software Development': ['Web Development', 'Mobile Applications', 'Backend Systems', 'Software Testing', 'Cloud Applications'],
  'Data Analytics': ['Business Intelligence', 'Data Visualization', 'Statistical Analysis', 'Product Analytics', 'Data Engineering'],
  'Mechanical Engineering': ['Automobile Engineering', 'Aerospace Engineering', 'Mechatronics', 'Robotics & Automation', 'Thermal Engineering', 'Machine Design', 'Manufacturing Engineering'],
  'Electrical Engineering': ['Power Systems', 'Renewable Energy', 'Electrical Machines', 'Control Systems', 'Power Electronics', 'Electrical Design'],
  'Aerospace Engineering': ['Aerodynamics', 'Propulsion Systems', 'Avionics', 'Aircraft Structures', 'Space Systems'],
  'Automobile Engineering': ['Electric Vehicles', 'Vehicle Design', 'Powertrain Engineering', 'Vehicle Safety', 'Automotive Manufacturing'],
  'Mechatronics Engineering': ['Robotics & Automation', 'Control Systems', 'Embedded Systems', 'Industrial Automation', 'Autonomous Systems'],
  'Biomedical Engineering': ['Medical Devices', 'Biomechanics', 'Medical Imaging', 'Biomaterials', 'Clinical Engineering'],
  'Environmental Engineering': ['Water & Wastewater', 'Air Quality', 'Environmental Remediation', 'Sustainable Infrastructure', 'Environmental Impact Assessment'],
  'Industrial Engineering': ['Supply Chain', 'Operations Research', 'Process Improvement', 'Quality Engineering', 'Production Systems'],
  'Agricultural Engineering': ['Precision Agriculture', 'Irrigation Systems', 'Farm Machinery', 'Food Processing', 'Soil & Water Conservation'],
  'Electronics Engineering': ['Electronics & VLSI', 'Embedded Systems', 'Telecommunications', 'Signal Processing', 'Control Systems'],
  'Electronics & Communication Engineering': ['Electronics & VLSI', 'Embedded Systems', 'Telecommunications', 'Signal Processing', 'Control Systems'],
  'Chemical Engineering': ['Process Engineering', 'Petroleum & Refinery', 'Pharmaceutical Process Engineering', 'Food Process Engineering', 'Environmental Process Engineering'],
  'Civil Engineering': ['Structural Engineering', 'Geotechnical Engineering', 'Transportation Engineering', 'Construction Management', 'Environmental Engineering'],
  'Finance': ['Corporate Finance', 'Financial Analysis', 'Risk Management', 'Investment Banking', 'Financial Planning'],
  'Accounting': ['Audit & Assurance', 'Taxation', 'Corporate Accounting', 'Forensic Accounting', 'Financial Analysis'],
  'Banking': ['Retail Banking', 'Investment Banking', 'Risk Management', 'Credit Analysis', 'Financial Technology'],
  'Taxation': ['Direct Tax', 'Indirect Tax / GST', 'International Tax', 'Tax Audit', 'Tax Advisory'],
  'Business Analytics': ['Data Analysis', 'Business Intelligence', 'Operations Analytics', 'Marketing Analytics', 'Risk Analytics'],
  'Management': ['Marketing', 'Finance', 'Human Resources', 'Operations', 'Strategy & Consulting'],
  'Marketing': ['Brand Management', 'Digital Marketing', 'Market Research', 'Product Marketing', 'Sales Strategy'],
  'Human Resources': ['Talent Acquisition', 'Learning & Development', 'People Analytics', 'Employee Relations', 'Organizational Development'],
  'International Business': ['Global Trade', 'International Marketing', 'Global Supply Chain', 'Cross-Border Finance', 'International Strategy'],
  'Operations': ['Supply Chain', 'Operations Analytics', 'Process Improvement', 'Project Management', 'Quality Management'],
  'Architecture': ['Sustainable Architecture', 'Urban Design', 'Interior Design', 'Landscape Architecture', 'BIM / Digital Design'],
  'Interior Design': ['Residential Interiors', 'Commercial Interiors', 'Lighting Design', 'Furniture Design', 'Interior Project Management'],
  'Pharmacy': ['Pharmaceutical Research', 'Clinical Pharmacy', 'Pharmacovigilance', 'Drug Regulatory Affairs', 'Hospital Pharmacy'],
  'Clinical Research': ['Clinical Trial Operations', 'Clinical Data Management', 'Pharmacovigilance', 'Regulatory Affairs', 'Medical Writing'],
  'General Medicine': ['Internal Medicine', 'Public Health', 'Medical Research', 'Healthcare Administration', 'Clinical Practice'],
  'Surgery': ['General Surgery', 'Surgical Research', 'Clinical Practice', 'Public Health', 'Healthcare Administration'],
  'Corporate Law': ['Corporate Transactions', 'Compliance', 'Mergers & Acquisitions', 'Intellectual Property', 'In-House Counsel'],
  'Criminal Law': ['Criminal Litigation', 'Prosecution', 'Legal Research', 'Forensic Law', 'Policy & Justice'],
  'Civil Law': ['Civil Litigation', 'Dispute Resolution', 'Property Law', 'Legal Research', 'Public Interest Law'],
  'Fashion Design': ['Fashion Design', 'Textile Design', 'Fashion Management', 'Fashion Communication', 'Sustainable Fashion'],
  'Industrial Design': ['Product Design', 'User Research', 'Design Engineering', 'Service Design', 'Sustainable Design'],
  'Graphic Design': ['Visual Identity', 'Digital Product Design', 'Publication Design', 'Motion Graphics', 'Art Direction'],
  'Physics': ['Applied Physics', 'Materials Science', 'Renewable Energy', 'Instrumentation', 'Research & Academia'],
  'Chemistry': ['Analytical Chemistry', 'Materials Chemistry', 'Industrial Chemistry', 'Chemical Research', 'Environmental Chemistry'],
  'Mathematics': ['Applied Mathematics', 'Statistics', 'Data Science', 'Operations Research', 'Research & Academia'],
  'Biology': ['Biotechnology', 'Genetics', 'Clinical Research', 'Ecology', 'Molecular Biology'],
  'Biotechnology': ['Biopharmaceuticals', 'Genetic Engineering', 'Bioinformatics', 'Clinical Research', 'Industrial Biotechnology'],
  'Environmental Science': ['Climate Science', 'Environmental Impact Assessment', 'Conservation', 'Sustainability', 'Environmental Policy'],
  'Commerce': ['Chartered Accountancy', 'Company Secretary', 'Cost & Management Accounting', 'Investment Banking', 'Corporate Finance', 'Financial Analysis', 'Risk Management'],
  'Psychology': ['Counselling Psychology', 'Clinical Psychology', 'Organizational Psychology', 'Educational Psychology', 'Behavioral Research', 'Human Resources'],
  'Sociology': ['Social Research', 'Social Work', 'Community Development', 'Public Policy', 'Civil Services'],
  'Political Science': ['Public Policy', 'Civil Services', 'International Relations', 'Political Research', 'Journalism'],
  'History': ['Teaching / Academia', 'Archival Research', 'Museums / Heritage', 'Civil Services', 'Journalism'],
  'Geography': ['GIS / Geospatial Analysis', 'Urban Planning', 'Environmental Policy', 'Civil Services', 'Research'],
  'Economics': ['Economic Research', 'Public Policy', 'Data Analysis', 'Banking / Finance', 'Civil Services'],
  'English': ['Editing / Publishing', 'Content Writing', 'Teaching / Academia', 'Journalism', 'Communications'],
  'Languages': ['Translation', 'Interpretation', 'Linguistics', 'Teaching', 'Publishing'],
  'Journalism / Mass Communication': ['Reporting', 'Editing', 'Digital Media', 'Public Relations', 'Content Strategy'],
  'Philosophy': ['Ethics / Policy Research', 'Academia', 'Civil Services', 'Writing / Publishing'],
  'Social Work': ['Community Development', 'Counselling Support', 'NGO Programme Work', 'Public Policy'],
  'Fine Arts': ['Arts Administration', 'Visual Communication', 'Museums / Curation', 'Creative Practice'],
  // Fallback for fields not explicitly mapped above
  'Default': ['Management', 'Research', 'Data Analysis', 'Consulting', 'Operations']
};

const ALL_FIELDS = Object.values(FIELD_OPTIONS).flat().filter((value, index, self) => self.indexOf(value) === index);

export default function AfterGraduationForm({ onComplete, onBack }) {
  const navigate = useNavigate();
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState({
    educationLevel: '',
    degree: '',
    majorCategory: '',
    major: '',
    currentStatus: '',
    preferenceMode: '', // 'SELECTED', 'EXPLORE', 'CAREER_CHANGE'
    selectedFields: [],
    selectedCareers: '' // free text input for specific roles
  });

  const getRelevantFields = () => {
    if (formData.preferenceMode === 'CAREER_CHANGE') return ALL_FIELDS;
    return FIELD_OPTIONS[formData.major] || FIELD_OPTIONS[formData.degree] || FIELD_OPTIONS['Default'];
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
      handleSubmit(); // Skip step 3 if explore mode
    } else {
      setStep(prev => prev + 1);
    }
  };

  const handlePrev = () => {
    if (step === 1) onBack();
    else setStep(prev => prev - 1);
  };

  const handleSubmit = () => {
    // Construct normalized profile object
    const profile = {
      educationLevel: formData.educationLevel,
      degree: formData.degree,
      major: `${formData.degree} - ${formData.major}`,
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
        <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">Postgraduate Profile</h2>
        <p className="text-slate-500 mt-2">Help us customize your After Graduation assessment.</p>
      </div>

      <AnimatePresence mode="wait">
        {step === 1 && (
          <motion.div key="step1" initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -20 }} className="space-y-4">
            <div>
              <label className="block text-sm font-bold text-slate-700 mb-2">Education Level</label>
              <select value={formData.educationLevel} onChange={(e) => handleChange('educationLevel', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                <option value="">Select Level...</option>
                {EDUCATION_LEVELS.map(l => <option key={l} value={l}>{l}</option>)}
              </select>
            </div>
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Degree</label>
                <select value={formData.degree} onChange={(e) => { handleChange('degree', e.target.value); handleChange('major', ''); }} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                  <option value="">Select Degree...</option>
                  {DEGREES.map(d => <option key={d} value={d}>{d}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Current Status</label>
                <select value={formData.currentStatus} onChange={(e) => handleChange('currentStatus', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                  <option value="">Select Status...</option>
                  {STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                </select>
              </div>
            </div>

            {formData.degree && (
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Specific Major / Branch</label>
                <select value={formData.major} onChange={(e) => handleChange('major', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none">
                  <option value="">Select Major...</option>
                  {DEGREE_BRANCHES[formData.degree]?.map(m => <option key={m} value={m}>{m}</option>)}
                </select>
              </div>
            )}
            
            <div className="flex justify-between mt-8 pt-6 border-t border-slate-100">
              <Button variant="ghost" onClick={handlePrev} className="text-slate-500 hover:bg-slate-50">
                <ArrowLeft className="w-4 h-4 mr-2" /> Back
              </Button>
              <Button onClick={handleNext} disabled={!formData.educationLevel || !formData.degree || !formData.major || !formData.currentStatus} className="bg-blue-600 hover:bg-blue-700 text-white px-8">
                Next <ChevronRight className="w-4 h-4 ml-2" />
              </Button>
            </div>
          </motion.div>
        )}

        {step === 2 && (
          <motion.div key="step2" initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -20 }} className="space-y-4">
            <label className="block text-sm font-bold text-slate-700 mb-4">What is your objective for this assessment?</label>
            <div className="space-y-3">
              <button type="button" onClick={() => handleChange('preferenceMode', 'SELECTED')} className={`w-full p-4 rounded-xl border text-left flex items-center gap-3 transition-colors ${formData.preferenceMode === 'SELECTED' ? 'border-blue-600 bg-blue-50 ring-1 ring-blue-600' : 'border-slate-200 bg-white hover:border-blue-300'}`}>
                <Target className={`w-5 h-5 ${formData.preferenceMode === 'SELECTED' ? 'text-blue-600' : 'text-slate-400'}`} />
                <div>
                  <div className={`font-semibold ${formData.preferenceMode === 'SELECTED' ? 'text-blue-900' : 'text-slate-700'}`}>I have specific fields/careers in mind</div>
                  <div className="text-sm text-slate-500">I want the quiz to evaluate my fit for these specific paths.</div>
                </div>
              </button>

              <button type="button" onClick={() => handleChange('preferenceMode', 'EXPLORE')} className={`w-full p-4 rounded-xl border text-left flex items-center gap-3 transition-colors ${formData.preferenceMode === 'EXPLORE' ? 'border-blue-600 bg-blue-50 ring-1 ring-blue-600' : 'border-slate-200 bg-white hover:border-blue-300'}`}>
                <Compass className={`w-5 h-5 ${formData.preferenceMode === 'EXPLORE' ? 'text-blue-600' : 'text-slate-400'}`} />
                <div>
                  <div className={`font-semibold ${formData.preferenceMode === 'EXPLORE' ? 'text-blue-900' : 'text-slate-700'}`}>I want to explore options in my field</div>
                  <div className="text-sm text-slate-500">I want to discover the best postgraduate paths within my current academic domain.</div>
                </div>
              </button>

              <button type="button" onClick={() => navigate('/career-switch')} className={`w-full p-4 rounded-xl border text-left flex items-center gap-3 transition-colors ${formData.preferenceMode === 'CAREER_CHANGE' ? 'border-blue-600 bg-blue-50 ring-1 ring-blue-600' : 'border-slate-200 bg-white hover:border-blue-300'}`}>
                <Replace className={`w-5 h-5 ${formData.preferenceMode === 'CAREER_CHANGE' ? 'text-blue-600' : 'text-slate-400'}`} />
                <div>
                  <div className={`font-semibold ${formData.preferenceMode === 'CAREER_CHANGE' ? 'text-blue-900' : 'text-slate-700'}`}>I want a Career Change</div>
                  <div className="text-sm text-slate-500">I want to switch to a completely different field outside my current major.</div>
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
                {formData.preferenceMode === 'CAREER_CHANGE' ? 'Which new fields are you considering?' : 'Which fields are you interested in?'}
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
              <p className="text-xs text-slate-500 mb-2">If you have specific roles in mind (e.g., Machine Learning Engineer, Risk Analyst), list them here.</p>
              <input type="text" placeholder="e.g. Data Scientist, Structural Engineer..." value={formData.selectedCareers} onChange={(e) => handleChange('selectedCareers', e.target.value)} className="w-full p-4 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-blue-600 outline-none" />
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
