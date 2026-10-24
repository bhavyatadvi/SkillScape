import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Code2, Cpu, Wrench, Building2, TrendingUp, Calculator, Briefcase, Users, 
  Palette, BookOpen, Atom, Globe, Brain, Scale, Stethoscope, GraduationCap, 
  Layout, Video, MessageSquare, Sprout, Compass, Utensils, Landmark, Database, 
  ShieldCheck, Sparkles, Target, ChevronRight, ArrowLeft, Search, Check, AlertCircle, Loader2, Zap, Layers, RefreshCw
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { skillAnalyzerApi } from '@/services/api/skillAnalyzer';
import { toast } from 'sonner';

// Helper to map domain IDs / names to sleek Lucide icon components
const getDomainIcon = (id, name = '') => {
  const lowerId = (id || '').toLowerCase();
  const lowerName = (name || '').toLowerCase();

  if (lowerId === 'cs' || lowerName.includes('computer science')) return Code2;
  if (lowerId === 'it' || lowerName.includes('information tech')) return Cpu;
  if (lowerId === 'engineering' || lowerName.includes('engineering')) return Wrench;
  if (lowerId === 'commerce' || lowerName.includes('commerce')) return Building2;
  if (lowerId === 'finance' || lowerName.includes('finance')) return TrendingUp;
  if (lowerId === 'accounting' || lowerName.includes('accounting')) return Calculator;
  if (lowerId === 'business' || lowerName.includes('business')) return Briefcase;
  if (lowerId === 'management' || lowerName.includes('management')) return Users;
  if (lowerId === 'arts' || lowerName.includes('arts')) return Palette;
  if (lowerId === 'humanities' || lowerName.includes('humanities')) return BookOpen;
  if (lowerId === 'science' || lowerName.includes('science')) return Atom;
  if (lowerId === 'social_sciences' || lowerName.includes('social')) return Globe;
  if (lowerId === 'psychology' || lowerName.includes('psychology')) return Brain;
  if (lowerId === 'law' || lowerName.includes('law')) return Scale;
  if (lowerId === 'healthcare' || lowerName.includes('health')) return Stethoscope;
  if (lowerId === 'education' || lowerName.includes('education')) return GraduationCap;
  if (lowerId === 'design' || lowerName.includes('design')) return Layout;
  if (lowerId === 'media' || lowerName.includes('media')) return Video;
  if (lowerId === 'communication' || lowerName.includes('communication')) return MessageSquare;
  if (lowerId === 'agriculture' || lowerName.includes('agriculture')) return Sprout;
  if (lowerId === 'architecture' || lowerName.includes('architecture')) return Compass;
  if (lowerId === 'hospitality' || lowerName.includes('hospitality')) return Utensils;
  if (lowerId === 'government' || lowerName.includes('government')) return Landmark;
  if (lowerId === 'data_science' || lowerName.includes('data')) return Database;
  if (lowerId === 'cybersecurity' || lowerName.includes('cyber')) return ShieldCheck;

  return Sparkles;
};

export const TargetCareerForm = ({ onAnalyze, loading, profileStatus }) => {
  // Wizard Steps: 1: Domain -> 2: SubDomain -> 3: Target Role -> 4: Confirm
  const [step, setStep] = useState(1);

  // Data states
  const [domains, setDomains] = useState([]);
  const [subDomains, setSubDomains] = useState([]);
  const [careers, setCareers] = useState([]);

  // Selected states
  const [selectedDomain, setSelectedDomain] = useState(null);
  const [selectedSubDomain, setSelectedSubDomain] = useState(null);
  const [selectedCareer, setSelectedCareer] = useState(null);
  const [customCareerName, setCustomCareerName] = useState('');
  const [searchQuery, setSearchQuery] = useState('');

  // Preference states
  const [targetLevel, setTargetLevel] = useState('Mid-Level');
  const [targetWorkType, setTargetWorkType] = useState('Full-Time');

  // Loading states
  const [fetchingDomains, setFetchingDomains] = useState(true);
  const [fetchingSubDomains, setFetchingSubDomains] = useState(false);
  const [fetchingCareers, setFetchingCareers] = useState(false);

  const hasCareerProfile = profileStatus?.hasProfile === true;

  useEffect(() => {
    // A target role should only be selected after the evidence profile exists.
    // This prevents an unprofiled user from receiving a generic skill analysis.
    if (hasCareerProfile) loadDomains();
  }, [hasCareerProfile]);

  const loadDomains = async () => {
    try {
      setFetchingDomains(true);
      const res = await skillAnalyzerApi.getDomains();
      if (res.data?.success) {
        setDomains(res.data.data || []);
      }
    } catch (err) {
      toast.error('Failed to load career fields.');
    } finally {
      setFetchingDomains(false);
    }
  };

  // Step 1 -> Step 2: Select Domain
  const handleSelectDomain = async (domain) => {
    setSelectedDomain(domain);
    setSelectedSubDomain(null);
    setSelectedCareer(null);
    setCustomCareerName('');
    setSearchQuery('');
    setStep(2);

    try {
      setFetchingSubDomains(true);
      const res = await skillAnalyzerApi.getSubDomains(domain.id);
      if (res.data?.success) {
        setSubDomains(res.data.data || []);
      }
    } catch (err) {
      toast.error('Failed to load specializations.');
    } finally {
      setFetchingSubDomains(false);
    }
  };

  // Step 2 -> Step 3: Select SubDomain
  const handleSelectSubDomain = async (subDomain) => {
    setSelectedSubDomain(subDomain);
    setSelectedCareer(null);
    setCustomCareerName('');
    setSearchQuery('');
    setStep(3);

    try {
      setFetchingCareers(true);
      const res = await skillAnalyzerApi.getCareers(selectedDomain?.id, subDomain.id);
      if (res.data?.success) {
        setCareers(res.data.data || []);
      }
    } catch (err) {
      toast.error('Failed to load target roles.');
    } finally {
      setFetchingCareers(false);
    }
  };

  // Search filter for Step 3
  const handleSearchChange = async (q) => {
    setSearchQuery(q);
    if (!q.trim()) {
      if (selectedSubDomain) {
        const res = await skillAnalyzerApi.getCareers(selectedDomain?.id, selectedSubDomain.id);
        if (res.data?.success) setCareers(res.data.data || []);
      }
      return;
    }
    try {
      setFetchingCareers(true);
      const res = await skillAnalyzerApi.searchCareers(q, selectedDomain?.id, selectedSubDomain?.id);
      if (res.data?.success) {
        setCareers(res.data.data || []);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setFetchingCareers(false);
    }
  };

  // Step 3 -> Step 4: Select Role
  const handleSelectCareer = (career) => {
    setSelectedCareer(career);
    setCustomCareerName('');
    setStep(4);
  };

  const handleSelectCustomCareer = () => {
    if (!customCareerName.trim()) {
      toast.error('Please type a custom target role name.');
      return;
    }
    setSelectedCareer({
      id: null,
      name: customCareerName.trim(),
      description: 'Custom target career role specified by user.'
    });
    setStep(4);
  };

  // Step 4 Submit: Run Analysis
  const handleSubmitAnalysis = () => {
    if (!selectedCareer && !customCareerName.trim()) {
      toast.error('Please select a target career role.');
      return;
    }

    onAnalyze({
      domainId: selectedDomain?.id || null,
      subDomainId: selectedSubDomain?.id || null,
      careerId: selectedCareer?.id || null,
      customCareerName: selectedCareer?.id ? null : (selectedCareer?.name || customCareerName.trim()),
      targetLevel,
      targetWorkType,
    });
  };

  if (!hasCareerProfile) {
    return (
      <motion.div
        initial={{ opacity: 0, y: 12 }}
        animate={{ opacity: 1, y: 0 }}
        className="rounded-2xl border border-amber-200 bg-amber-50/60 px-6 py-10 text-center shadow-sm"
      >
        <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-xl bg-amber-100 text-amber-700">
          <AlertCircle className="h-6 w-6" />
        </div>
        <h2 className="mt-4 text-xl font-bold text-slate-900">Create your Career Profile first</h2>
        <p className="mx-auto mt-2 max-w-lg text-sm leading-6 text-slate-600">
          Your profile provides the resume, education, experience, and skills needed to give a meaningful target-role analysis. Create it first, then return here to choose a career field.
        </p>
        <p className="mt-4 text-xs font-semibold text-amber-800">Use the “Create Profile” button above to continue.</p>
      </motion.div>
    );
  }

  return (
    <div className="w-full space-y-6">
      {/* Main Wizard Container */}
      <motion.div 
        initial={{ opacity: 0, y: 12 }}
        animate={{ opacity: 1, y: 0 }}
        className="bg-white border border-slate-200 rounded-2xl p-6 sm:p-8 shadow-sm space-y-6"
      >
        {/* Top Header & Breadcrumb Nav */}
        <div className="border-b border-slate-100 pb-6 space-y-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2 text-xs font-semibold text-slate-500">
              <span 
                onClick={() => setStep(1)} 
                className={`cursor-pointer hover:text-blue-600 ${step === 1 ? 'text-blue-600 font-bold' : ''}`}
              >
                1. Career Field
              </span>
              {selectedDomain && (
                <>
                  <ChevronRight className="w-3.5 h-3.5 text-slate-400" />
                  <span 
                    onClick={() => setStep(2)} 
                    className={`cursor-pointer hover:text-blue-600 ${step === 2 ? 'text-blue-600 font-bold' : ''}`}
                  >
                    2. Specialization
                  </span>
                </>
              )}
              {selectedSubDomain && (
                <>
                  <ChevronRight className="w-3.5 h-3.5 text-slate-400" />
                  <span 
                    onClick={() => setStep(3)} 
                    className={`cursor-pointer hover:text-blue-600 ${step === 3 ? 'text-blue-600 font-bold' : ''}`}
                  >
                    3. Target Role
                  </span>
                </>
              )}
              {selectedCareer && (
                <>
                  <ChevronRight className="w-3.5 h-3.5 text-slate-400" />
                  <span className="text-slate-900 font-bold">
                    4. Confirm
                  </span>
                </>
              )}
            </div>

            {step > 1 && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setStep(prev => prev - 1)}
                className="text-xs text-slate-500 hover:text-slate-900 flex items-center gap-1 font-semibold"
              >
                <ArrowLeft className="w-3.5 h-3.5" /> Back
              </Button>
            )}
          </div>

          {/* Progress Indicator */}
          <div className="w-full bg-slate-100 h-1.5 rounded-full overflow-hidden">
            <div 
              className="bg-blue-600 h-full transition-all duration-300 rounded-full"
              style={{ width: `${(step / 4) * 100}%` }}
            />
          </div>
        </div>

        {/* ─── STEP 1: CHOOSE CAREER FIELD (DOMAIN) ────────────────────────────── */}
        {step === 1 && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="space-y-5">
            <div>
              <h2 className="text-xl font-bold text-slate-900">What field are you preparing for?</h2>
              <p className="text-slate-500 text-sm font-medium mt-0.5">
                Select your broad career field or industry domain to begin.
              </p>
            </div>

            {fetchingDomains ? (
              <div className="flex items-center gap-2 text-slate-500 text-sm py-12 justify-center font-medium">
                <Loader2 className="w-5 h-5 animate-spin text-blue-600" /> Loading career fields...
              </div>
            ) : (
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
                {domains.map((d) => {
                  const IconComponent = getDomainIcon(d.id, d.name);
                  return (
                    <button
                      key={d.id}
                      type="button"
                      onClick={() => handleSelectDomain(d)}
                      className="group p-4 rounded-xl border border-slate-200 bg-white text-left transition-all hover:border-blue-600 hover:bg-blue-50/50 hover:shadow-sm flex flex-col justify-between h-28"
                    >
                      <div className="w-9 h-9 rounded-lg bg-blue-50 text-blue-600 flex items-center justify-center group-hover:bg-blue-600 group-hover:text-white transition-colors">
                        <IconComponent className="w-4 h-4" />
                      </div>
                      <div>
                        <span className="font-bold text-xs sm:text-sm text-slate-900 group-hover:text-blue-600 transition-colors block line-clamp-1">
                          {d.name}
                        </span>
                        {d.alternateNames && d.alternateNames.length > 0 && (
                          <span className="text-[11px] text-slate-400 font-medium block mt-0.5 line-clamp-1">
                            {d.alternateNames.slice(0, 2).join(' • ')}
                          </span>
                        )}
                      </div>
                    </button>
                  );
                })}
              </div>
            )}
          </motion.div>
        )}

        {/* ─── STEP 2: CHOOSE SPECIALIZATION (SUB-DOMAIN) ───────────────────────── */}
        {step === 2 && selectedDomain && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="space-y-5">
            <div>
              <div className="flex items-center gap-2 mb-1">
                <span className="bg-blue-50 text-blue-700 text-xs font-bold px-2.5 py-1 rounded border border-blue-100">
                  {selectedDomain.name}
                </span>
              </div>
              <h2 className="text-xl font-bold text-slate-900">Choose a specialization</h2>
              <p className="text-slate-500 text-sm font-medium mt-0.5">
                Select your specific sub-domain within {selectedDomain.name}.
              </p>
            </div>

            {fetchingSubDomains ? (
              <div className="flex items-center gap-2 text-slate-500 text-sm py-12 justify-center font-medium">
                <Loader2 className="w-5 h-5 animate-spin text-blue-600" /> Loading specializations...
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {subDomains.map((sd) => (
                  <button
                    key={sd.id}
                    type="button"
                    onClick={() => handleSelectSubDomain(sd)}
                    className="p-5 rounded-xl border border-slate-200 bg-white text-left transition-all hover:border-blue-600 hover:bg-blue-50/50 hover:shadow-sm group flex flex-col justify-between"
                  >
                    <div className="flex items-center justify-between mb-2">
                      <span className="font-bold text-base text-slate-900 group-hover:text-blue-600 transition-colors">
                        {sd.name}
                      </span>
                      <ChevronRight className="w-4 h-4 text-slate-400 group-hover:text-blue-600 group-hover:translate-x-1 transition-all" />
                    </div>
                    <p className="text-xs text-slate-500 font-medium leading-relaxed">
                      {sd.description}
                    </p>
                  </button>
                ))}
              </div>
            )}
          </motion.div>
        )}

        {/* ─── STEP 3: CHOOSE TARGET ROLE (CAREER) ─────────────────────────────── */}
        {step === 3 && selectedSubDomain && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="space-y-5">
            <div>
              <div className="flex items-center gap-2 mb-1">
                <span className="bg-slate-100 text-slate-700 text-xs font-semibold px-2.5 py-0.5 rounded border border-slate-200">
                  {selectedDomain?.name}
                </span>
                <span className="text-slate-400 text-xs">•</span>
                <span className="bg-blue-50 text-blue-700 text-xs font-bold px-2.5 py-0.5 rounded border border-blue-100">
                  {selectedSubDomain.name}
                </span>
              </div>
              <h2 className="text-xl font-bold text-slate-900">What role are you targeting?</h2>
              <p className="text-slate-500 text-sm font-medium mt-0.5">
                Select your target career role or search canonical role titles & aliases.
              </p>
            </div>

            {/* Role Search Input */}
            <div className="relative">
              <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-3.5" />
              <input
                type="text"
                placeholder="Search target role... (e.g. Full Stack Developer, ML Engineer, Accountant)"
                value={searchQuery}
                onChange={(e) => handleSearchChange(e.target.value)}
                className="w-full bg-white border border-slate-200 rounded-xl pl-10 pr-4 py-3 text-slate-900 text-sm font-medium placeholder-slate-400 focus:outline-none focus:border-blue-600 focus:ring-2 focus:ring-blue-500/10 transition-all"
              />
            </div>

            {fetchingCareers ? (
              <div className="flex items-center gap-2 text-slate-500 text-sm py-10 justify-center font-medium">
                <Loader2 className="w-5 h-5 animate-spin text-blue-600" /> Filtering roles...
              </div>
            ) : careers.length === 0 ? (
              <div className="bg-slate-50 p-6 rounded-xl border border-slate-200 text-center space-y-3">
                <p className="text-sm font-semibold text-slate-700">
                  No matching roles found for "{searchQuery}".
                </p>
                <div className="flex justify-center items-center gap-3">
                  <input
                    type="text"
                    placeholder="Enter custom role title..."
                    value={customCareerName}
                    onChange={(e) => setCustomCareerName(e.target.value)}
                    className="bg-white border border-slate-200 rounded-lg px-3 py-2 text-sm text-slate-900"
                  />
                  <Button size="sm" onClick={handleSelectCustomCareer} className="bg-blue-600 text-white font-bold">
                    Use Custom Role
                  </Button>
                </div>
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {careers.map((c) => (
                  <button
                    key={c.id}
                    type="button"
                    onClick={() => handleSelectCareer(c)}
                    className="p-5 rounded-xl border border-slate-200 bg-white text-left transition-all hover:border-blue-600 hover:bg-blue-50/50 hover:shadow-sm group flex flex-col justify-between"
                  >
                    <div>
                      <div className="flex items-center justify-between mb-1">
                        <span className="font-bold text-sm text-slate-900 group-hover:text-blue-600 transition-colors">
                          {c.name}
                        </span>
                        <Zap className="w-4 h-4 text-slate-300 group-hover:text-blue-600 transition-colors" />
                      </div>
                      <p className="text-xs text-slate-500 font-medium line-clamp-2 leading-relaxed">
                        {c.description}
                      </p>
                    </div>

                    {c.alternateNames && c.alternateNames.length > 0 && (
                      <div className="flex flex-wrap gap-1 mt-3 pt-2 border-t border-slate-100">
                        {c.alternateNames.slice(0, 3).map((alt, aIdx) => (
                          <span key={aIdx} className="text-[10px] font-mono bg-slate-100 text-slate-600 px-2 py-0.5 rounded">
                            {alt}
                          </span>
                        ))}
                      </div>
                    )}
                  </button>
                ))}
              </div>
            )}
          </motion.div>
        )}

        {/* ─── STEP 4: CONFIRM CAREER TARGET ───────────────────────────────────── */}
        {step === 4 && selectedCareer && (
          <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="space-y-6">
            <div>
              <h2 className="text-xl font-bold text-slate-900">Your Career Target Confirmation</h2>
              <p className="text-slate-500 text-sm font-medium mt-0.5">
                Review your selected target career path before running readiness analysis.
              </p>
            </div>

            {/* Target Summary Card */}
            <div className="bg-slate-50 border border-slate-200 rounded-xl p-6 space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pb-4 border-b border-slate-200">
                <div>
                  <span className="text-xs font-bold text-slate-400 uppercase tracking-wider block">Career Field</span>
                  <span className="text-base font-bold text-slate-900">{selectedDomain?.name || 'General'}</span>
                </div>
                <div>
                  <span className="text-xs font-bold text-slate-400 uppercase tracking-wider block">Specialization</span>
                  <span className="text-base font-bold text-slate-900">{selectedSubDomain?.name || 'General'}</span>
                </div>
                <div>
                  <span className="text-xs font-bold text-slate-400 uppercase tracking-wider block">Target Role</span>
                  <span className="text-base font-bold text-blue-600">{selectedCareer?.name}</span>
                </div>
              </div>

              {/* Preferences Grid */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 pt-2">
                <div className="space-y-2">
                  <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider">
                    Target Seniority Level
                  </label>
                  <div className="grid grid-cols-3 gap-2">
                    {['Entry / Junior', 'Mid-Level', 'Senior / Lead'].map((lvl) => (
                      <button
                        key={lvl}
                        type="button"
                        onClick={() => setTargetLevel(lvl)}
                        className={`py-2 px-2 rounded-lg text-xs font-bold border transition-all ${
                          targetLevel === lvl
                            ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
                            : 'bg-white border-slate-200 text-slate-700 hover:bg-slate-100'
                        }`}
                      >
                        {lvl}
                      </button>
                    ))}
                  </div>
                </div>

                <div className="space-y-2">
                  <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider">
                    Target Work Arrangement
                  </label>
                  <div className="grid grid-cols-4 gap-2">
                    {['Full-Time', 'Remote', 'Hybrid', 'Contract'].map((wt) => (
                      <button
                        key={wt}
                        type="button"
                        onClick={() => setTargetWorkType(wt)}
                        className={`py-2 px-1.5 rounded-lg text-xs font-bold border transition-all ${
                          targetWorkType === wt
                            ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
                            : 'bg-white border-slate-200 text-slate-700 hover:bg-slate-100'
                        }`}
                      >
                        {wt}
                      </button>
                    ))}
                  </div>
                </div>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex items-center justify-between pt-4 border-t border-slate-100">
              <Button
                type="button"
                variant="outline"
                onClick={() => setStep(1)}
                className="border-slate-300 text-slate-700 hover:bg-slate-50 font-semibold text-xs flex items-center gap-1.5"
              >
                <RefreshCw className="w-3.5 h-3.5" /> Change Career Target
              </Button>

              <Button
                type="button"
                onClick={handleSubmitAnalysis}
                disabled={loading}
                className="bg-blue-600 hover:bg-blue-700 text-white px-8 py-5 rounded-xl font-bold text-sm shadow-md transition-all flex items-center gap-2"
              >
                {loading ? (
                  <>
                    <Loader2 className="w-4 h-4 animate-spin" />
                    Analyzing Career Readiness...
                  </>
                ) : (
                  <>
                    <Sparkles className="w-4 h-4" />
                    Continue to Skill Analysis
                    <ChevronRight className="w-4 h-4" />
                  </>
                )}
              </Button>
            </div>
          </motion.div>
        )}
      </motion.div>
    </div>
  );
};

export default TargetCareerForm;
