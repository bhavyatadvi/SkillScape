import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { useCareerProfile } from '@/context/CareerProfileContext';
import {
  GraduationCap, Briefcase, Wrench, Heart,
  Target, ChevronRight, CheckCircle2, ChevronLeft, Loader2
} from 'lucide-react';

/* ─── Steps definition ─────────────────────────────── */
const SECTIONS = [
  { id: 'education',    title: 'Education',         icon: GraduationCap },
  { id: 'career',      title: 'Current Career',     icon: Briefcase     },
  { id: 'skills',      title: 'Skills & Certificates', icon: Wrench      },
  { id: 'preferences', title: 'Work Preferences',   icon: Heart         },
  { id: 'goals',       title: 'Career Goals',       icon: Target        },
];

/* ─── Default empty form ───────────────────────────── */
const defaultForm = {
  educationLevel: '', degree: '', broadField: '', specialization: '',
  institution: '', graduationYear: '', additionalQualifications: '',
  educationStatus: '', currentEmploymentStatus: '', currentOccupation: '',
  currentRole: '', currentIndustry: '', organizationType: '',
  experienceYears: '', relevantExperience: '', previousRoles: '', previousIndustries: '',
  currentSkills: [], certifications: [],
  careerLikes: [], careerDislikes: [],
  workPreferences: { environment: [], style: [] },
  motivations: [], constraints: [],
  careerChangePreference: '', location: '', preferredWorkMode: '', salaryRange: '', careerGoals: '',
  profileSource: 'manual', resumeFileName: '',
};

/* ─── Map API profile dto → form shape ─────────────── */
function profileToForm(p) {
  if (!p) return { ...defaultForm };
  return {
    ...defaultForm,
    educationLevel: p.educationLevel ?? '',
    degree: p.degree ?? '',
    broadField: p.broadField ?? '',
    specialization: p.specialization ?? '',
    institution: p.institution ?? '',
    graduationYear: p.graduationYear ?? '',
    additionalQualifications: p.additionalQualifications ?? '',
    educationStatus: p.educationStatus ?? '',
    currentEmploymentStatus: p.currentEmploymentStatus ?? '',
    currentOccupation: p.currentOccupation ?? '',
    currentRole: p.currentRole ?? '',
    currentIndustry: p.currentIndustry ?? '',
    organizationType: p.organizationType ?? '',
    experienceYears: p.experienceYears ?? '',
    relevantExperience: p.relevantExperience ?? '',
    previousRoles: p.previousRoles ?? '',
    previousIndustries: p.previousIndustries ?? '',
    // Skills may come from API as {name, proficiency} or {name, level}
    currentSkills: Array.isArray(p.currentSkills)
      ? p.currentSkills.map(s => ({ name: s.name ?? '', proficiency: s.proficiency ?? s.level ?? 'Beginner' }))
      : [],
    certifications: Array.isArray(p.certifications) ? p.certifications : [],
    careerLikes: Array.isArray(p.careerLikes) ? p.careerLikes : [],
    careerDislikes: Array.isArray(p.careerDislikes) ? p.careerDislikes : [],
    workPreferences: p.workPreferences && typeof p.workPreferences === 'object'
      ? { environment: p.workPreferences.environment ?? [], style: p.workPreferences.style ?? [] }
      : { environment: [], style: [] },
    motivations: Array.isArray(p.motivations) ? p.motivations : [],
    constraints: Array.isArray(p.constraints) ? p.constraints : [],
    careerChangePreference: p.careerChangePreference ?? '',
    location: p.location ?? '',
    preferredWorkMode: p.preferredWorkMode ?? '',
    salaryRange: p.salaryRange ?? '',
    careerGoals: p.careerGoals ?? '',
    profileSource: p.profileSource ?? 'manual',
    resumeFileName: p.resumeFileName ?? '',
  };
}

/* ─── Main Editor Component ─────────────────────────── */
export default function CareerProfileEditor() {
  const navigate = useNavigate();
  const location = useLocation();
  const { profile, saveProfile, loading: ctxLoading } = useCareerProfile();

  const { prefillData, mode, section, focus } = location.state ?? {};
  const initialSection = section === 'skills' ? 2 : 0;
  const [currentSection, setCurrentSection] = useState(initialSection);
  const [saving, setSaving] = useState(false);
  const latestAllowedGraduationYear = new Date().getFullYear() + 6;

  // Determine initial form data from route state or existing profile
  const [formData, setFormData] = useState(() => {
    if (prefillData) return profileToForm(prefillData);
    if (profile) return profileToForm(profile);
    return { ...defaultForm };
  });

  // Skill input refs
  const skillNameRef = useRef(null);
  const skillProfRef = useRef(null);
  const certificationNameRef = useRef(null);
  const certificationSectionRef = useRef(null);

  // The profile summary links directly to this editor with a certification
  // focus request. Wait for the Skills section to render, then bring the form
  // into view and focus the input so the next action is immediately clear.
  useEffect(() => {
    if (currentSection !== 2 || focus !== 'certifications') return;

    const frame = requestAnimationFrame(() => {
      certificationSectionRef.current?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      certificationNameRef.current?.focus({ preventScroll: true });
    });

    return () => cancelAnimationFrame(frame);
  }, [currentSection, focus]);

  const handleInputChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleNext = async () => {
    if (currentSection < SECTIONS.length - 1) {
      setCurrentSection(prev => prev + 1);
    } else {
      await handleSave();
    }
  };

  const handleBack = () => {
    if (currentSection > 0) {
      setCurrentSection(prev => prev - 1);
    } else {
      navigate('/career-profile');
    }
  };

  const handleSave = async () => {
    if (formData.graduationYear !== '') {
      const graduationYear = Number(formData.graduationYear);
      if (!Number.isInteger(graduationYear) || graduationYear < 1960 || graduationYear > latestAllowedGraduationYear) {
        toast.error(`Graduation year must be between 1960 and ${latestAllowedGraduationYear}.`);
        return;
      }
    }

    if (formData.experienceYears !== '') {
      const experienceYears = Number(formData.experienceYears);
      if (!Number.isFinite(experienceYears) || experienceYears < 0 || experienceYears > 60) {
        toast.error('Years of experience must be between 0 and 60.');
        return;
      }
    }

    setSaving(true);
    try {
      await saveProfile({
        ...formData,
        graduationYear: formData.graduationYear ? Number(formData.graduationYear) : null,
        experienceYears: formData.experienceYears !== '' ? Number(formData.experienceYears) : null,
      });
      toast.success('Career profile saved successfully!');
      navigate('/career-profile');
    } catch (err) {
      toast.error(err?.message || 'Unable to save your career profile. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  /* ─── Section Renderers ───────────────────────── */

  const renderEducation = () => (
    <div className="space-y-6">
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Highest Education Level</label>
        <select
          className="w-full p-3 border border-slate-200 rounded-lg bg-white"
          value={formData.educationLevel}
          onChange={(e) => handleInputChange('educationLevel', e.target.value)}
        >
          <option value="">Select Level</option>
          <option value="High School">High School</option>
          <option value="Diploma">Diploma</option>
          <option value="Bachelors">Bachelor's Degree</option>
          <option value="Masters">Master's Degree</option>
          <option value="Doctorate">Doctorate (PhD)</option>
        </select>
      </div>
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Degree Name</label>
        <input
          type="text"
          placeholder="e.g. B.Tech, BSc, BCom"
          className="w-full p-3 border border-slate-200 rounded-lg"
          value={formData.degree}
          onChange={(e) => handleInputChange('degree', e.target.value)}
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Broad Field</label>
        <input
          type="text"
          placeholder="e.g. Engineering, Arts, Business"
          className="w-full p-3 border border-slate-200 rounded-lg"
          value={formData.broadField}
          onChange={(e) => handleInputChange('broadField', e.target.value)}
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Specialization / Major</label>
        <input
          type="text"
          placeholder="e.g. Civil Engineering, Marketing"
          className="w-full p-3 border border-slate-200 rounded-lg"
          value={formData.specialization}
          onChange={(e) => handleInputChange('specialization', e.target.value)}
        />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label className="block text-sm font-medium text-slate-700 mb-2">University / Institution <span className="text-slate-400">(optional)</span></label>
          <input type="text" placeholder="Institution name" className="w-full p-3 border border-slate-200 rounded-lg" value={formData.institution} onChange={(e) => handleInputChange('institution', e.target.value)} />
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700 mb-2">Graduation Year</label>
          <input type="number" min="1960" max={latestAllowedGraduationYear} step="1" placeholder="e.g. 2024" className="w-full p-3 border border-slate-200 rounded-lg" value={formData.graduationYear} onChange={(e) => handleInputChange('graduationYear', e.target.value)} />
        </div>
      </div>
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Additional Qualifications <span className="text-slate-400">(optional)</span></label>
        <input type="text" placeholder="e.g. PG Diploma in Data Science" className="w-full p-3 border border-slate-200 rounded-lg" value={formData.additionalQualifications} onChange={(e) => handleInputChange('additionalQualifications', e.target.value)} />
      </div>
    </div>
  );

  const renderCareer = () => (
    <div className="space-y-6">
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Current Employment Status</label>
        <select className="w-full p-3 border border-slate-200 rounded-lg bg-white" value={formData.currentEmploymentStatus} onChange={(e) => handleInputChange('currentEmploymentStatus', e.target.value)}>
          <option value="">Select Status</option>
          <option value="Student">Student</option>
          <option value="Fresher">Fresher</option>
          <option value="Working">Working</option>
          <option value="Self-employed">Self-employed</option>
          <option value="Freelancer">Freelancer</option>
          <option value="Entrepreneur">Entrepreneur</option>
          <option value="Unemployed">Unemployed</option>
          <option value="Career break">Career break</option>
          <option value="Other">Other</option>
        </select>
      </div>
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Current Job Title / Occupation</label>
        <input type="text" placeholder="e.g. Civil Site Engineer, Software Developer" className="w-full p-3 border border-slate-200 rounded-lg" value={formData.currentOccupation} onChange={(e) => handleInputChange('currentOccupation', e.target.value)} />
      </div>
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Current Industry</label>
        <input type="text" placeholder="e.g. Construction, IT, Healthcare" className="w-full p-3 border border-slate-200 rounded-lg" value={formData.currentIndustry} onChange={(e) => handleInputChange('currentIndustry', e.target.value)} />
      </div>
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Years of Experience</label>
        <input type="number" min="0" max="60" step="0.5" placeholder="0" className="w-full p-3 border border-slate-200 rounded-lg" value={formData.experienceYears} onChange={(e) => handleInputChange('experienceYears', e.target.value)} />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label className="block text-sm font-medium text-slate-700 mb-2">Organization Type</label>
          <select className="w-full p-3 border border-slate-200 rounded-lg bg-white" value={formData.organizationType} onChange={(e) => handleInputChange('organizationType', e.target.value)}>
            <option value="">Select type</option>
            <option>Private</option><option>Government</option><option>PSU</option>
            <option>Startup</option><option>NGO</option><option>Academic</option>
            <option>Self-employed</option><option>Other</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700 mb-2">Relevant Experience</label>
          <input type="text" placeholder="e.g. Site execution, cost estimation" className="w-full p-3 border border-slate-200 rounded-lg" value={formData.relevantExperience} onChange={(e) => handleInputChange('relevantExperience', e.target.value)} />
        </div>
      </div>
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Previous Roles <span className="text-slate-400">(optional)</span></label>
        <input type="text" placeholder="e.g. Junior Engineer, Site Supervisor" className="w-full p-3 border border-slate-200 rounded-lg" value={formData.previousRoles} onChange={(e) => handleInputChange('previousRoles', e.target.value)} />
      </div>
    </div>
  );

  const renderSkills = () => (
    <div className="space-y-6">
      <p className="text-slate-600 mb-4">Add your top skills and self-rate your proficiency. This helps map transferable skills.</p>

      <div className="bg-slate-50 p-4 rounded-xl border border-slate-200">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
          <input ref={skillNameRef} type="text" placeholder="Skill Name (e.g. AutoCAD)" className="p-3 border border-slate-200 rounded-lg w-full" />
          <select ref={skillProfRef} className="p-3 border border-slate-200 rounded-lg w-full bg-white">
            <option value="Beginner">Beginner</option>
            <option value="Intermediate">Intermediate</option>
            <option value="Advanced">Advanced</option>
            <option value="Expert">Expert</option>
          </select>
          <Button
            className="w-full"
            onClick={() => {
              const name = skillNameRef.current?.value?.trim();
              const prof = skillProfRef.current?.value ?? 'Beginner';
              if (name) {
                handleInputChange('currentSkills', [...formData.currentSkills, { name, proficiency: prof }]);
                if (skillNameRef.current) skillNameRef.current.value = '';
              }
            }}
          >
            Add Skill
          </Button>
        </div>
      </div>

      <div className="space-y-2">
        {formData.currentSkills.map((skill, idx) => (
          <div key={idx} className="flex justify-between items-center bg-white p-3 border border-slate-200 rounded-lg shadow-sm">
            <span className="font-medium text-slate-800">{skill.name}</span>
            <div className="flex items-center gap-3">
              <select
                className="text-sm px-2 py-1 bg-blue-50 text-blue-700 rounded-md border-none outline-none"
                value={skill.proficiency}
                onChange={(e) => {
                  const updated = [...formData.currentSkills];
                  updated[idx] = { ...updated[idx], proficiency: e.target.value };
                  handleInputChange('currentSkills', updated);
                }}
              >
                <option>Beginner</option>
                <option>Intermediate</option>
                <option>Advanced</option>
                <option>Expert</option>
              </select>
              <button className="text-red-500 text-sm hover:underline" onClick={() => handleInputChange('currentSkills', formData.currentSkills.filter((_, i) => i !== idx))}>
                Remove
              </button>
            </div>
          </div>
        ))}
      </div>

      <div ref={certificationSectionRef} className="border-t border-slate-200 pt-6">
        <div className="mb-4">
          <h3 className="text-base font-bold text-slate-900">Completed Certifications</h3>
          <p className="mt-1 text-sm text-slate-600">Add certificates you have completed. They are saved to your central Career Profile and included in Career Analytics.</p>
        </div>
        <div className={`rounded-xl border p-4 ${focus === 'certifications' ? 'border-blue-300 bg-blue-50/40' : 'border-slate-200 bg-slate-50'}`}>
          <div className="grid grid-cols-1 gap-3 md:grid-cols-[1fr_auto]">
            <input
              ref={certificationNameRef}
              type="text"
              placeholder="Certificate name, e.g. Google Data Analytics Professional Certificate"
              className="w-full rounded-lg border border-slate-200 bg-white p-3"
            />
            <Button
              type="button"
              onClick={() => {
                const certificate = certificationNameRef.current?.value?.trim();
                const existing = formData.certifications.map(item => String(item).trim().toLowerCase());
                if (!certificate) return;
                if (existing.includes(certificate.toLowerCase())) {
                  toast.error('This certification is already in your profile.');
                  return;
                }
                handleInputChange('certifications', [...formData.certifications, certificate]);
                certificationNameRef.current.value = '';
              }}
            >
              Add Certification
            </Button>
          </div>
          {formData.certifications.length > 0 ? (
            <div className="mt-4 flex flex-wrap gap-2">
              {formData.certifications.map((certificate, index) => (
                <span key={`${certificate}-${index}`} className="inline-flex items-center gap-2 rounded-md border border-indigo-100 bg-indigo-50 px-2.5 py-1.5 text-sm font-medium text-indigo-700">
                  {certificate}
                  <button type="button" aria-label={`Remove ${certificate}`} className="text-indigo-500 hover:text-red-600" onClick={() => handleInputChange('certifications', formData.certifications.filter((_, itemIndex) => itemIndex !== index))}>×</button>
                </span>
              ))}
            </div>
          ) : <p className="mt-3 text-sm text-slate-500">No certifications added yet.</p>}
        </div>
      </div>
    </div>
  );

  const renderPreferences = () => (
    <div className="space-y-6">
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Preferred Work Environment</label>
        <select
          className="w-full p-3 border border-slate-200 rounded-lg bg-white"
          value={formData.workPreferences.environment[0] ?? ''}
          onChange={(e) => handleInputChange('workPreferences', { ...formData.workPreferences, environment: [e.target.value] })}
        >
          <option value="">Select Environment</option>
          <option value="Office">Office</option>
          <option value="Remote">Remote</option>
          <option value="Hybrid">Hybrid</option>
          <option value="Field">Field / Site</option>
          <option value="Laboratory">Laboratory</option>
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">What do you want to AVOID? (Constraints)</label>
        <p className="text-xs text-slate-500 mb-3">Select things you explicitly dislike.</p>
        <div className="grid grid-cols-2 gap-3">
          {["Heavy coding", "Sales", "Public speaking", "Night shifts", "Field work", "Desk-only work", "Travel-heavy work", "Repetitive work", "High-pressure work", "Customer-facing work", "Manual work", "Unpredictable schedule", "Long education requirements"].map(item => (
            <label key={item} className="flex items-center space-x-2 p-2 border border-slate-200 rounded-lg cursor-pointer hover:bg-slate-50">
              <input
                type="checkbox"
                className="rounded text-blue-600"
                checked={formData.constraints.includes(item)}
                onChange={(e) => {
                  if (e.target.checked) {
                    handleInputChange('constraints', [...formData.constraints, item]);
                  } else {
                    handleInputChange('constraints', formData.constraints.filter(i => i !== item));
                  }
                }}
              />
              <span className="text-sm font-medium text-slate-700">{item}</span>
            </label>
          ))}
        </div>
      </div>
    </div>
  );

  const renderGoals = () => (
    <div className="space-y-6">
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Career Motivations (rank your priorities)</label>
        <p className="text-xs text-slate-500 mb-3">Select up to five. Selection order sets the ranking.</p>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
          {['Salary', 'Job stability', 'Work-life balance', 'Career growth', 'Prestige', 'Creativity', 'Technical challenge', 'Leadership', 'Social impact', 'Remote work', 'Entrepreneurship', 'Research', 'International opportunities'].map(item => {
            const rank = formData.motivations.indexOf(item);
            return (
              <button
                key={item}
                type="button"
                onClick={() => handleInputChange('motivations', rank >= 0 ? formData.motivations.filter(v => v !== item) : [...formData.motivations, item].slice(0, 5))}
                className={`text-left p-3 rounded-lg border ${rank >= 0 ? 'border-blue-500 bg-blue-50 text-blue-900' : 'border-slate-200 hover:bg-slate-50 text-slate-700'}`}
              >
                {rank >= 0 ? `${rank + 1}. ` : ''}{item}
              </button>
            );
          })}
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">How far are you willing to move career-wise?</label>
        <select className="w-full p-3 border border-slate-200 rounded-lg bg-white" value={formData.careerChangePreference} onChange={(e) => handleInputChange('careerChangePreference', e.target.value)}>
          <option value="">Select willingness</option>
          <option value="1">1. Very close to current career</option>
          <option value="2">2. Related role, different responsibilities</option>
          <option value="3">3. Related industry, transferable skills</option>
          <option value="4">4. Different industry, similar skill base</option>
          <option value="5">5. Major career change requiring reskilling</option>
          <option value="6">6. Completely different career</option>
          <option value="7">7. Not sure</option>
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Preferred Location <span className="text-slate-400">(optional)</span></label>
        <input type="text" placeholder="e.g. Bangalore, Remote India, Abroad" className="w-full p-3 border border-slate-200 rounded-lg" value={formData.location} onChange={(e) => handleInputChange('location', e.target.value)} />
      </div>

      <div>
        <label className="block text-sm font-medium text-slate-700 mb-2">Career Goals <span className="text-slate-400">(optional)</span></label>
        <textarea rows={3} placeholder="Describe your short or long-term career goals..." className="w-full p-3 border border-slate-200 rounded-lg resize-none" value={formData.careerGoals} onChange={(e) => handleInputChange('careerGoals', e.target.value)} />
      </div>
    </div>
  );

  /* ─── Render ────────────────────────────────────── */
  return (
    <div className="min-h-screen bg-slate-50 pt-24 pb-12 px-4">
      <div className="max-w-4xl mx-auto">

        <div className="mb-8">
          <h1 className="text-3xl font-bold text-slate-900 mb-2">Career Profile</h1>
          <p className="text-slate-600">
            {mode === 'resume'
              ? 'We\'ve extracted your information from your resume. Review, edit, and save.'
              : 'Tell us where you are now so we can help you find where you should go.'}
          </p>
          {mode === 'resume' && (
            <span className="inline-block mt-2 bg-blue-50 text-blue-700 text-xs px-2 py-1 rounded-full font-medium">
              ✦ Pre-filled from resume — please review everything
            </span>
          )}
        </div>

        {/* Stepper */}
        <div className="flex justify-between mb-8 overflow-x-auto pb-4 hide-scrollbar">
          {SECTIONS.map((section, idx) => {
            const Icon = section.icon;
            const isActive = idx === currentSection;
            const isPast = idx < currentSection;
            return (
              <div key={section.id} className="flex flex-col items-center flex-1 min-w-[80px]">
                <div className={`w-10 h-10 rounded-full flex items-center justify-center mb-2 transition-colors ${
                  isActive ? 'bg-blue-600 text-white shadow-md shadow-blue-200' :
                  isPast  ? 'bg-green-500 text-white' : 'bg-slate-200 text-slate-400'
                }`}>
                  {isPast ? <CheckCircle2 className="w-5 h-5" /> : <Icon className="w-5 h-5" />}
                </div>
                <span className={`text-xs font-medium text-center ${
                  isActive ? 'text-blue-700' : isPast ? 'text-green-600' : 'text-slate-400'
                }`}>
                  {section.title}
                </span>
              </div>
            );
          })}
        </div>

        {/* Form card */}
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 p-8">
          <motion.div
            key={currentSection}
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.3 }}
          >
            <div className="mb-6">
              <h2 className="text-2xl font-bold text-slate-800">{SECTIONS[currentSection].title}</h2>
              <div className="h-1 w-12 bg-blue-600 rounded-full mt-3" />
            </div>

            {currentSection === 0 && renderEducation()}
            {currentSection === 1 && renderCareer()}
            {currentSection === 2 && renderSkills()}
            {currentSection === 3 && renderPreferences()}
            {currentSection === 4 && renderGoals()}
          </motion.div>

          {/* Controls */}
          <div className="flex justify-between mt-12 pt-6 border-t border-slate-100">
            <Button variant="outline" size="lg" onClick={handleBack} disabled={saving}>
              <ChevronLeft className="w-4 h-4 mr-2" />
              {currentSection === 0 ? 'Cancel' : 'Back'}
            </Button>
            <Button
              size="lg"
              className="bg-blue-600 hover:bg-blue-700 text-white shadow-md shadow-blue-200"
              onClick={handleNext}
              disabled={saving}
            >
              {saving ? (
                <><Loader2 className="w-4 h-4 mr-2 animate-spin" /> Saving...</>
              ) : currentSection === SECTIONS.length - 1 ? (
                'Save Profile'
              ) : (
                <><span>Next Step</span><ChevronRight className="w-4 h-4 ml-2" /></>
              )}
            </Button>
          </div>
        </div>

      </div>
    </div>
  );
}
