import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { 
  ChevronRight, ArrowLeft, FlaskConical, Landmark, Palette, Briefcase, 
  Sparkles, BookOpen, Target, BrainCircuit, GraduationCap, HardHat 
} from 'lucide-react';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';

export default function After12thForm({ onComplete, onBack }) {
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState({
    phaseKey: 'After12th',
    stream: '',
    subjects: [],
    degreeIntent: '',
    pcmbSubjectInterests: [],
    pcmbWorkInterests: [],
    engineeringPreferenceMode: '',
    preferredEngineeringFields: [],
    selectedFields: [],
    currentStatus: '',
    academicProfile: '',
    interests: [],
    careerGoal: ''
  });

  const handleChange = (name, value) => {
    setFormData(prev => ({
      ...prev,
      [name]: value,
      ...(name === 'stream' && value !== prev.stream ? { selectedFields: [], preferredEngineeringFields: [] } : {}),
      ...(name === 'careerGoal' && value !== 'narrow_down' ? { selectedFields: [] } : {})
    }));
  };

  const handleCheckboxToggle = (name, value) => {
    setFormData(prev => {
      const list = prev[name] || [];
      if (list.includes(value)) {
        return { ...prev, [name]: list.filter(item => item !== value) };
      } else {
        return { ...prev, [name]: [...list, value] };
      }
    });
  };

  // Define steps dynamically based on choices
  const getSteps = () => {
    const steps = [
      { id: 'stream', type: 'single' }
    ];

    if (formData.stream === 'pcm') {
      steps.push({ id: 'degreeIntent', type: 'single' });
      if (formData.degreeIntent === 'BE_BTECH' || formData.degreeIntent === 'CONSIDERING_BE_BTECH') {
        steps.push({ id: 'engineeringPreferenceMode', type: 'single' });
        if (formData.engineeringPreferenceMode === 'KNOWN' || formData.engineeringPreferenceMode === 'FEW_FIELDS') {
          steps.push({ id: 'preferredEngineeringFields', type: 'multi' });
        }
      }
    }

    // PCMB deliberately begins with broad discovery rather than assuming
    // medicine or engineering from the subject combination alone.
    if (formData.stream === 'pcmb') {
      steps.push({ id: 'pcmbSubjectInterests', type: 'multi' });
      steps.push({ id: 'pcmbWorkInterests', type: 'multi' });
    }

    steps.push({ id: 'academicProfile', type: 'single' });
    steps.push({ id: 'careerGoal', type: 'single' });
    if (formData.careerGoal === 'narrow_down') {
      steps.push({ id: 'selectedFields', type: 'multi' });
    }

    return steps;
  };

  const currentSteps = getSteps();
  const currentStepId = currentSteps[step - 1]?.id;

  const nextStep = () => setStep(s => s + 1);
  const prevStep = () => {
    if (step === 1) onBack();
    else setStep(s => s - 1);
  };

  const handleSubmit = () => {
    const profile = {
      ...formData,
      subPhaseKey: formData.stream,
      stream: formData.stream,
      major: formData.stream,
      degree: '12th',
      degreeIntent: formData.degreeIntent,
      engineeringPreferenceMode: formData.engineeringPreferenceMode,
      preferredEngineeringFields: formData.preferredEngineeringFields.join(','),
      academicProfile: formData.academicProfile,
      primaryGoal: formData.careerGoal,
      preferenceMode: formData.careerGoal === 'narrow_down' ? 'SELECTED' : 'EXPLORE',
      selectedFields: JSON.stringify(formData.selectedFields),
      pcmbSubjectInterests: formData.pcmbSubjectInterests.join(','),
      pcmbWorkInterests: formData.pcmbWorkInterests.join(',')
    };
    onComplete(profile);
  };

  const OptionCard = ({ icon: Icon, title, desc, value, selectedValue, onClick }) => (
    <button
      type="button"
      onClick={() => onClick(value)}
      className={`p-5 rounded-xl border text-left transition-all duration-300 ${
        selectedValue === value 
          ? 'border-blue-600 bg-blue-50/50 shadow-md ring-1 ring-blue-600' 
          : 'border-slate-200 bg-white hover:border-blue-300 hover:shadow-sm'
      }`}
    >
      <div className="flex items-start">
        {Icon && <Icon className={`w-6 h-6 mr-3 mt-0.5 ${selectedValue === value ? 'text-blue-600' : 'text-slate-400'}`} />}
        <div>
          <h3 className={`font-semibold text-lg ${selectedValue === value ? 'text-blue-900' : 'text-slate-700'}`}>{title}</h3>
          {desc && <p className="text-sm text-slate-500 mt-1">{desc}</p>}
        </div>
      </div>
    </button>
  );

  const CheckboxCard = ({ title, value, checked, onChange }) => (
    <div className={`p-4 rounded-xl border flex items-center space-x-3 cursor-pointer transition-colors ${
      checked ? 'border-blue-600 bg-blue-50' : 'border-slate-200 hover:bg-slate-50'
    }`} onClick={() => onChange(value)}>
      <Checkbox checked={checked} onCheckedChange={() => onChange(value)} />
      <Label className="cursor-pointer font-medium">{title}</Label>
    </div>
  );

  const getSubjectOptions = () => {
    switch (formData.stream) {
      case 'pcm': return ['Mathematics', 'Physics', 'Chemistry', 'Computer Science', 'Economics'];
      case 'pcb': return ['Biology', 'Physics', 'Chemistry', 'Psychology', 'Biotechnology'];
      case 'pcmb': return ['Physics', 'Chemistry', 'Mathematics', 'Biology', 'Computer Science', 'Biotechnology'];
      case 'commerce': return ['Accountancy', 'Business Studies', 'Economics', 'Mathematics', 'Informatics Practices'];
      case 'arts': return ['History', 'Political Science', 'Geography', 'Psychology', 'Sociology', 'Economics'];
      default: return ['English', 'Mathematics', 'Science', 'Social Science'];
    }
  };

  const engineeringDomains = [
    "Computer Science Engineering", "Information Technology", "Artificial Intelligence & Machine Learning", 
    "Data Science / Data Engineering", "Cybersecurity", "Electronics & Communication Engineering", 
    "Electrical Engineering", "Mechanical Engineering", "Civil Engineering", "Chemical Engineering", 
    "Aerospace Engineering", "Automobile / Automotive Engineering", "Mechatronics Engineering", 
    "Robotics & Automation", "Biomedical Engineering", "Biotechnology", "Industrial Engineering", 
    "Environmental Engineering", "Materials / Metallurgical Engineering", "Petroleum / Energy Engineering"
  ];

  const getRelevantFields = () => {
    const fieldsByStream = {
      pcm: [...engineeringDomains, 'Architecture', 'Pure Sciences', 'Defence & Aviation', 'Economics & Finance'],
      pcb: ['Medicine & Healthcare', 'Pharmacy', 'Biotechnology', 'Life Sciences', 'Psychology', 'Veterinary Science', 'Agriculture', 'Scientific Research', 'Nutrition & Public Health'],
      pcmb: ['Medicine & Healthcare', 'Engineering & Technology', 'Computer Science & IT', 'Pharmacy', 'Biotechnology', 'Life Sciences', 'Psychology', 'Veterinary Science', 'Agriculture', 'Scientific Research', 'Architecture'],
      commerce: ['Accounting & Chartered Accountancy', 'Finance & Banking', 'Business Management', 'Economics', 'Law', 'Marketing', 'Entrepreneurship', 'Business Analytics'],
      arts: ['Psychology', 'Law & Public Policy', 'Civil Services', 'Journalism & Media', 'Design & Fine Arts', 'Teaching & Academia', 'Social Sciences', 'Languages & Literature'],
      vocational: ['Engineering Diplomas', 'Information Technology', 'Skilled Trades', 'Design', 'Healthcare Support', 'Hospitality & Tourism', 'Business & Entrepreneurship']
    };

    return fieldsByStream[formData.stream] || [];
  };

  return (
    <div className="space-y-6">
      <AnimatePresence mode="wait">
        <motion.div
          key={`step-${currentStepId}`}
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -20 }}
          transition={{ duration: 0.2 }}
        >
          {currentStepId === 'stream' && (
            <div className="space-y-6">
              <div className="text-center mb-8">
                <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">What is your 12th stream?</h2>
                <p className="text-slate-500 mt-2">Select your academic focus to personalize the quiz pathways.</p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                <OptionCard icon={FlaskConical} title="Science - PCM" desc="Physics, Chemistry, Math" value="pcm" selectedValue={formData.stream} onClick={(v) => handleChange('stream', v)} />
                <OptionCard icon={FlaskConical} title="Science - PCB" desc="Physics, Chemistry, Biology" value="pcb" selectedValue={formData.stream} onClick={(v) => handleChange('stream', v)} />
                <OptionCard icon={FlaskConical} title="Science - PCMB" desc="Physics, Chemistry, Mathematics, Biology" value="pcmb" selectedValue={formData.stream} onClick={(v) => handleChange('stream', v)} />
                <OptionCard icon={Landmark} title="Commerce" desc="Accounts, Economics, Business" value="commerce" selectedValue={formData.stream} onClick={(v) => handleChange('stream', v)} />
                <OptionCard icon={Palette} title="Arts / Humanities" desc="History, Politics, Sociology" value="arts" selectedValue={formData.stream} onClick={(v) => handleChange('stream', v)} />
                <OptionCard icon={Briefcase} title="Vocational" desc="Skills, ITI, Diplomas" value="vocational" selectedValue={formData.stream} onClick={(v) => handleChange('stream', v)} />
              </div>
            </div>
          )}

          {currentStepId === 'pcmbSubjectInterests' && (
            <div className="space-y-6">
              <div className="text-center mb-8"><h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">What subjects or areas do you enjoy most?</h2><p className="text-slate-500 mt-2">Select every area you would genuinely like to explore.</p></div>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                {['Mathematics & problem solving', 'Biology & human health', 'Physics & engineering', 'Chemistry & laboratory work', 'Computer science & technology', 'Research & scientific discovery', 'Environment & life sciences', 'Not sure yet'].map(item => <CheckboxCard key={item} title={item} value={item} checked={formData.pcmbSubjectInterests.includes(item)} onChange={(value) => handleCheckboxToggle('pcmbSubjectInterests', value)} />)}
              </div>
            </div>
          )}

          {currentStepId === 'pcmbWorkInterests' && (
            <div className="space-y-6">
              <div className="text-center mb-8"><h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">Which type of work sounds most interesting?</h2><p className="text-slate-500 mt-2">These answers decide which PCMB pathways the adaptive quiz explores next.</p></div>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                {['Designing / building things', 'Treating / helping patients', 'Programming / technology', 'Laboratory experimentation', 'Scientific research', 'Data & mathematical analysis', 'Working with nature / environment', 'Not sure yet'].map(item => <CheckboxCard key={item} title={item} value={item} checked={formData.pcmbWorkInterests.includes(item)} onChange={(value) => handleCheckboxToggle('pcmbWorkInterests', value)} />)}
              </div>
            </div>
          )}



          {currentStepId === 'degreeIntent' && (
            <div className="space-y-6">
              <div className="text-center mb-8">
                <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">Are you planning to pursue Engineering?</h2>
                <p className="text-slate-500 mt-2">Do you intend to do a B.E. or B.Tech degree?</p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <OptionCard icon={HardHat} title="Yes, definitely" desc="I want to pursue B.E./B.Tech" value="BE_BTECH" selectedValue={formData.degreeIntent} onClick={(v) => handleChange('degreeIntent', v)} />
                <OptionCard icon={Target} title="Considering it" desc="I am considering B.E./B.Tech along with other options" value="CONSIDERING_BE_BTECH" selectedValue={formData.degreeIntent} onClick={(v) => handleChange('degreeIntent', v)} />
                <OptionCard icon={BrainCircuit} title="Unsure / No" desc="I am exploring pure sciences, architecture, or other fields" value="UNSURE" selectedValue={formData.degreeIntent} onClick={(v) => handleChange('degreeIntent', v)} />
              </div>
            </div>
          )}

          {currentStepId === 'engineeringPreferenceMode' && (
            <div className="space-y-6">
              <div className="text-center mb-8">
                <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">Engineering Field Preference</h2>
                <p className="text-slate-500 mt-2">How much do you already know about the engineering field you want to pursue?</p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <OptionCard icon={Target} title="I know my preferred field" desc="I have a specific branch in mind" value="KNOWN" selectedValue={formData.engineeringPreferenceMode} onClick={(v) => handleChange('engineeringPreferenceMode', v)} />
                <OptionCard icon={BookOpen} title="I have a few fields in mind" desc="I'm deciding between 2-3 branches" value="FEW_FIELDS" selectedValue={formData.engineeringPreferenceMode} onClick={(v) => handleChange('engineeringPreferenceMode', v)} />
                <OptionCard icon={Sparkles} title="I have no idea yet" desc="Help me discover the best engineering field for me" value="NO_IDEA" selectedValue={formData.engineeringPreferenceMode} onClick={(v) => handleChange('engineeringPreferenceMode', v)} />
              </div>
            </div>
          )}

          {currentStepId === 'preferredEngineeringFields' && (
            <div className="space-y-6">
              <div className="text-center mb-8">
                <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">Which fields are you considering?</h2>
                <p className="text-slate-500 mt-2">{formData.engineeringPreferenceMode === 'KNOWN' ? 'Select your preferred engineering branch.' : 'Select the 2-3 branches you are considering.'}</p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 h-96 overflow-y-auto p-2 border rounded-xl bg-slate-50/50">
                {engineeringDomains.map(field => (
                  <CheckboxCard 
                    key={field} 
                    title={field} 
                    value={field} 
                    checked={(formData.preferredEngineeringFields || []).includes(field)}
                    onChange={(v) => handleCheckboxToggle('preferredEngineeringFields', v)} 
                  />
                ))}
              </div>
            </div>
          )}

          {currentStepId === 'academicProfile' && (
            <div className="space-y-6">
              <div className="text-center mb-8">
                <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">How is your academic performance?</h2>
                <p className="text-slate-500 mt-2">This helps us gauge whether to recommend highly competitive entrance-based degrees.</p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <OptionCard title="Excellent" desc="90%+" value="excellent" selectedValue={formData.academicProfile} onClick={(v) => handleChange('academicProfile', v)} />
                <OptionCard title="Good" desc="75% - 90%" value="good" selectedValue={formData.academicProfile} onClick={(v) => handleChange('academicProfile', v)} />
                <OptionCard title="Average" desc="Below 75%" value="average" selectedValue={formData.academicProfile} onClick={(v) => handleChange('academicProfile', v)} />
              </div>
            </div>
          )}



          {currentStepId === 'careerGoal' && (
            <div className="space-y-6">
              <div className="text-center mb-8">
                <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">What is your primary goal right now?</h2>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <OptionCard icon={BrainCircuit} title="Discover Options" desc="I have no idea what I want to do." value="discover" selectedValue={formData.careerGoal} onClick={(v) => handleChange('careerGoal', v)} />
                <OptionCard icon={Target} title="Narrow Down" desc="I have a few fields in mind." value="narrow_down" selectedValue={formData.careerGoal} onClick={(v) => handleChange('careerGoal', v)} />
              </div>
            </div>
          )}

          {currentStepId === 'selectedFields' && (
            <div className="space-y-6">
              <div className="text-center mb-8">
                <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">Which fields are you considering?</h2>
                <p className="text-slate-500 mt-2">Select the areas you want the quiz to focus on. Choose at least one.</p>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3 max-h-96 overflow-y-auto p-2">
                {getRelevantFields().map(field => (
                  <button
                    key={field}
                    type="button"
                    aria-pressed={formData.selectedFields.includes(field)}
                    onClick={() => handleCheckboxToggle('selectedFields', field)}
                    className={`rounded-xl border p-4 text-left font-medium transition-colors ${
                      formData.selectedFields.includes(field)
                        ? 'border-blue-600 bg-blue-50 text-blue-900 ring-1 ring-blue-600'
                        : 'border-slate-200 bg-white text-slate-700 hover:border-blue-300 hover:bg-slate-50'
                    }`}
                  >
                    {field}
                  </button>
                ))}
              </div>
            </div>
          )}
        </motion.div>
      </AnimatePresence>

      <div className="flex justify-between mt-8 pt-6 border-t border-slate-100">
        <Button variant="ghost" onClick={prevStep} className="text-slate-500 hover:bg-slate-50">
          <ArrowLeft className="w-4 h-4 mr-2" /> Back
        </Button>
        {step < currentSteps.length ? (
          <Button onClick={nextStep} disabled={
            (currentStepId === 'stream' && !formData.stream) ||
            (currentStepId === 'degreeIntent' && !formData.degreeIntent) ||
            (currentStepId === 'engineeringPreferenceMode' && !formData.engineeringPreferenceMode) ||
            (currentStepId === 'preferredEngineeringFields' && formData.preferredEngineeringFields.length === 0) ||
            (currentStepId === 'pcmbSubjectInterests' && formData.pcmbSubjectInterests.length === 0) ||
            (currentStepId === 'pcmbWorkInterests' && formData.pcmbWorkInterests.length === 0) ||
            (currentStepId === 'selectedFields' && formData.selectedFields.length === 0) ||
            (currentStepId === 'currentStatus' && !formData.currentStatus) ||
            (currentStepId === 'academicProfile' && !formData.academicProfile)
          } className="bg-blue-600 hover:bg-blue-700 text-white px-8">
            Next <ChevronRight className="w-4 h-4 ml-2" />
          </Button>
        ) : (
          <Button onClick={handleSubmit} disabled={!formData.careerGoal || (currentStepId === 'selectedFields' && formData.selectedFields.length === 0)} className="bg-blue-600 hover:bg-blue-700 text-white px-8">
            Start Quiz <ChevronRight className="w-4 h-4 ml-2" />
          </Button>
        )}
      </div>
    </div>
  );
}
