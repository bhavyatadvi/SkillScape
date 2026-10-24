import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { useCareerProfile } from '@/context/CareerProfileContext';
import PageHeader from '@/components/layout/PageHeader';
import {
  FileText, PenLine, User, GraduationCap, Briefcase,
  Wrench, Target, Edit2, RefreshCw, Loader2,
  CheckCircle2, AlertCircle, Plus, Trash2, Sparkles, Award
} from 'lucide-react';
import { toast } from 'sonner';

/* ─── Defensive Helpers ───────────────────────────── */

function ensureArray(val) {
  if (!val) return [];
  if (Array.isArray(val)) return val;
  if (typeof val === 'string') {
    const trimmed = val.trim();
    if (!trimmed) return [];
    try {
      const parsed = JSON.parse(trimmed);
      if (Array.isArray(parsed)) return parsed;
    } catch {
      return [trimmed];
    }
  }
  return [];
}

function calcCompletion(profile) {
  if (!profile) return 0;
  const skills = ensureArray(profile.currentSkills);
  const motivations = ensureArray(profile.motivations);
  const constraints = ensureArray(profile.constraints);
  const envPref = ensureArray(profile.workPreferences?.environment);

  const checks = [
    !!(profile.educationLevel || profile.degree || profile.broadField),
    !!(profile.currentEmploymentStatus || profile.currentOccupation),
    skills.length > 0,
    motivations.length > 0 || constraints.length > 0 || envPref.length > 0,
    !!(profile.careerGoals || profile.careerChangePreference || profile.location),
  ];
  return Math.round((checks.filter(Boolean).length / checks.length) * 100);
}

function SectionCard({ icon: Icon, title, children }) {
  return (
    <div className="card-hover rounded-xl border border-slate-200 bg-white p-5 shadow-soft">
      <div className="mb-4 flex items-center gap-2.5">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-50">
          <Icon className="w-4 h-4 text-blue-600" />
        </div>
        <span className="text-sm font-bold text-slate-800">{title}</span>
      </div>
      {children}
    </div>
  );
}

/* ─── No-profile State ─────────────────────────────── */

function NoProfile() {
  const navigate = useNavigate();
  return (
    <div className="mx-auto max-w-6xl pb-16">
      <div className="mb-8">
        <PageHeader icon={User} eyebrow="Career foundation" title="Build your Career Profile" description="Keep your education, experience, skills, and goals in one professional profile that powers SkillScape." />
      </div>
      <div className="mx-auto max-w-4xl">
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}>
          <div className="mb-8 rounded-2xl border border-blue-100 bg-gradient-to-br from-blue-50 to-white p-6 text-center sm:p-8">
            <div className="mb-4 inline-flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-accent text-white shadow-glow-accent">
              <Sparkles className="h-6 w-6" />
            </div>
            <h2 className="mb-2 text-xl font-bold text-slate-900">One profile. Better guidance everywhere.</h2>
            <p className="mx-auto max-w-xl text-sm leading-relaxed text-slate-600">
              Add your career evidence once, then let Career Switch, Skill Analyzer, and learning recommendations reuse it.
            </p>
          </div>

          <p className="text-center text-sm font-semibold text-slate-500 mb-6 uppercase tracking-wider">
            Choose how you'd like to create your profile
          </p>

          <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
            {/* Upload Resume card */}
            <motion.div
              whileHover={{ y: -2, boxShadow: '0 8px 30px rgba(37,99,235,0.12)' }}
              className="card-hover cursor-pointer rounded-2xl border border-blue-200 bg-white p-7 shadow-soft"
              onClick={() => navigate('/career-profile/resume')}
            >
              <div className="w-12 h-12 bg-blue-600 rounded-xl flex items-center justify-center mb-5 shadow-md shadow-blue-200">
                <FileText className="w-6 h-6 text-white" />
              </div>
              <h2 className="mb-2 text-lg font-bold text-slate-900">Upload Resume</h2>
              <p className="text-sm text-slate-500 mb-5 leading-relaxed">
                Automatically extract your education, experience, and skills from your PDF or Word resume.
              </p>
              <Button className="w-full bg-blue-600 text-white shadow-md shadow-blue-200 hover:bg-blue-700">
                <FileText className="w-4 h-4 mr-2" /> Upload Resume
              </Button>
            </motion.div>

            {/* Fill manually card */}
            <motion.div
              whileHover={{ y: -2, boxShadow: '0 8px 30px rgba(15,23,42,0.07)' }}
              className="card-hover cursor-pointer rounded-2xl border border-slate-200 bg-white p-7 shadow-soft"
              onClick={() => navigate('/career-profile/edit', { state: { mode: 'manual' } })}
            >
              <div className="w-12 h-12 bg-slate-800 rounded-xl flex items-center justify-center mb-5 shadow-md shadow-slate-200">
                <PenLine className="w-6 h-6 text-white" />
              </div>
              <h2 className="text-lg font-bold text-slate-900 mb-2">Fill Form Manually</h2>
              <p className="text-sm text-slate-500 mb-5 leading-relaxed">
                Complete your career profile step-by-step. Covers education, skills, preferences, and goals.
              </p>
              <Button variant="outline" className="w-full border-slate-200 bg-slate-50 text-slate-700 hover:bg-slate-100">
                <PenLine className="w-4 h-4 mr-2" /> Fill Manually
              </Button>
            </motion.div>
          </div>
        </motion.div>
      </div>
    </div>
  );
}

/* ─── Profile-exists State ─────────────────────────── */

function ProfileSummary({ profile }) {
  const navigate = useNavigate();
  const { clearProfile } = useCareerProfile();
  const [deleting, setDeleting] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const completion = calcCompletion(profile);
  const skillsList = ensureArray(profile?.currentSkills);
  const certificationsList = ensureArray(profile?.certifications);
  const motivationsList = ensureArray(profile?.motivations);

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await clearProfile();
      toast.success('Career profile deleted successfully!');
    } catch (err) {
      toast.error(err?.message || 'Failed to delete career profile.');
    } finally {
      setDeleting(false);
      setShowConfirm(false);
    }
  };

  return (
    <div className="mx-auto max-w-6xl pb-16">
      <div className="mx-auto max-w-5xl">
        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}>

          {/* Delete Confirmation Modal */}
          {showConfirm && (
            <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4">
              <div className="bg-white rounded-2xl p-6 max-w-md w-full shadow-xl space-y-4">
                <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center text-red-600 mx-auto">
                  <Trash2 className="w-6 h-6" />
                </div>
                <h3 className="text-lg font-bold text-slate-900 text-center">Delete Career Profile?</h3>
                <p className="text-sm text-slate-500 text-center leading-relaxed">
                  This will permanently delete your stored education, experience, skills, and goals. You will need to recreate your profile or upload a resume again.
                </p>
                <div className="flex gap-3 pt-2">
                  <Button variant="outline" className="flex-1" onClick={() => setShowConfirm(false)} disabled={deleting}>
                    Cancel
                  </Button>
                  <Button variant="destructive" className="flex-1 bg-red-600 hover:bg-red-700 text-white" onClick={handleDelete} disabled={deleting}>
                    {deleting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Trash2 className="w-4 h-4 mr-2" />}
                    Delete
                  </Button>
                </div>
              </div>
            </div>
          )}

          {/* Title row */}
          <div className="mb-6 flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
            <div>
              <p className="mb-2 text-xs font-semibold uppercase tracking-widest text-blue-600">Career foundation</p>
              <h1 className="text-3xl font-bold tracking-tight text-slate-900">Career Profile</h1>
            </div>
            <div className="flex gap-2 flex-wrap items-center">
              <Button
                variant="outline"
                className="border-blue-200 bg-blue-50 text-blue-700 hover:bg-blue-100"
                onClick={() => navigate('/career-profile/edit', { state: { section: 'skills', focus: 'certifications' } })}
              >
                <Award className="w-4 h-4 mr-2" /> Add Certification
              </Button>
              <Button
                onClick={() => navigate('/career-profile/edit')}
                className="bg-blue-600 text-white shadow-md shadow-blue-200 hover:bg-blue-700"
              >
                <Edit2 className="w-4 h-4 mr-2" /> Edit Career Profile
              </Button>
              <Button variant="outline" onClick={() => navigate('/career-profile/resume')}>
                <RefreshCw className="w-4 h-4 mr-2" /> Update Resume
              </Button>
              <Button
                variant="outline"
                className="text-red-600 border-red-200 hover:bg-red-50 hover:text-red-700"
                onClick={() => setShowConfirm(true)}
              >
                <Trash2 className="w-4 h-4 mr-2" /> Delete Profile
              </Button>
            </div>
          </div>

          {/* Profile health */}
          <div className="mb-6 grid gap-4 rounded-2xl border border-slate-200 bg-white p-5 shadow-soft md:grid-cols-[auto_1fr_auto] md:items-center">
            <div className={`flex h-16 w-16 items-center justify-center rounded-2xl ${completion >= 80 ? 'bg-emerald-50 text-emerald-600' : 'bg-blue-50 text-blue-600'}`}>
              <span className="text-xl font-black">{completion}%</span>
            </div>
            <div>
              <p className="font-bold text-slate-900">Profile completeness</p>
              <p className="mt-1 text-sm text-slate-500">A stronger profile makes your readiness analysis and recommendations more useful.</p>
              <div className="mt-3 h-2.5 overflow-hidden rounded-full bg-slate-100">
                <motion.div initial={{ width: 0 }} animate={{ width: `${completion}%` }} transition={{ duration: 0.8, ease: 'easeOut' }} className={`h-full rounded-full ${completion >= 80 ? 'bg-emerald-500' : completion >= 50 ? 'bg-blue-600' : 'bg-amber-500'}`} />
              </div>
            </div>
            <div className="grid grid-cols-3 gap-3 text-center">
              <div className="rounded-xl bg-slate-50 px-3 py-2"><p className="text-lg font-black text-slate-900">{skillsList.length}</p><p className="text-[11px] font-medium text-slate-500">Skills</p></div>
              <div className="rounded-xl bg-slate-50 px-3 py-2"><p className="text-lg font-black text-slate-900">{certificationsList.length}</p><p className="text-[11px] font-medium text-slate-500">Certificates</p></div>
              <div className="rounded-xl bg-slate-50 px-3 py-2"><p className="text-lg font-black text-slate-900">{profile?.experienceYears ?? 0}</p><p className="text-[11px] font-medium text-slate-500">Years exp.</p></div>
            </div>
          </div>

          {/* Summary Cards */}
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">

            {/* Education */}
            <SectionCard icon={GraduationCap} title="Education">
              {(profile?.degree || profile?.educationLevel || profile?.broadField) ? (
                <div className="space-y-1">
                  <p className="font-semibold text-slate-800 text-sm">
                    {[profile.degree, profile.specialization || profile.broadField].filter(Boolean).join(' — ')}
                  </p>
                  {profile.institution && (
                    <p className="text-slate-500 text-xs">{profile.institution}</p>
                  )}
                  {profile.graduationYear && (
                    <p className="text-slate-400 text-xs">Graduated {profile.graduationYear}</p>
                  )}
                </div>
              ) : (
                <p className="text-slate-400 text-sm italic">Not filled</p>
              )}
            </SectionCard>

            {/* Current Career */}
            <SectionCard icon={Briefcase} title="Current Career">
              {(profile?.currentEmploymentStatus || profile?.currentOccupation || profile?.currentRole) ? (
                <div className="space-y-1">
                  <p className="font-semibold text-slate-800 text-sm">
                    {profile.currentOccupation || profile.currentRole || profile.currentEmploymentStatus}
                  </p>
                  {profile.currentIndustry && (
                    <p className="text-slate-500 text-xs">{profile.currentIndustry}</p>
                  )}
                  {profile.experienceYears != null && (
                    <p className="text-slate-400 text-xs">{profile.experienceYears} years experience</p>
                  )}
                </div>
              ) : (
                <p className="text-slate-400 text-sm italic">Not filled</p>
              )}
            </SectionCard>

            {/* Skills */}
            <SectionCard icon={Wrench} title="Skills">
              {skillsList.length > 0 ? (
                <div className="flex flex-wrap gap-1.5">
                  {skillsList.slice(0, 8).map((s, i) => {
                    const label = typeof s === 'string' ? s : (s?.name || s?.label || s?.title || JSON.stringify(s));
                    return (
                      <span key={i} className="bg-blue-50 text-blue-700 text-xs px-2 py-0.5 rounded-md font-medium">
                        {label}
                      </span>
                    );
                  })}
                  {skillsList.length > 8 && (
                    <span className="text-slate-400 text-xs">+{skillsList.length - 8} more</span>
                  )}
                </div>
              ) : (
                <p className="text-slate-400 text-sm italic">No skills added</p>
              )}
            </SectionCard>

            <SectionCard icon={Award} title="Certifications">
              {certificationsList.length ? (
                <div className="space-y-2">
                  <div className="flex flex-wrap gap-1.5">
                    {certificationsList.slice(0, 6).map((certificate, index) => (
                      <span key={`${certificate}-${index}`} className="rounded-md bg-indigo-50 px-2 py-1 text-xs font-medium text-indigo-700">
                        {typeof certificate === 'string' ? certificate : certificate?.name || certificate?.title || 'Certificate'}
                      </span>
                    ))}
                  </div>
                  {certificationsList.length > 6 && <p className="text-xs text-slate-400">+{certificationsList.length - 6} more</p>}
                  <button type="button" onClick={() => navigate('/career-profile/edit', { state: { section: 'skills', focus: 'certifications' } })} className="text-xs font-semibold text-blue-600 hover:text-blue-700">
                    Manage certifications →
                  </button>
                </div>
              ) : (
                <div>
                  <p className="mb-2 text-sm italic text-slate-400">No completed certifications added yet.</p>
                  <button type="button" onClick={() => navigate('/career-profile/edit', { state: { section: 'skills', focus: 'certifications' } })} className="text-sm font-semibold text-blue-600 hover:text-blue-700">
                    + Add a certification
                  </button>
                </div>
              )}
            </SectionCard>

            {/* Goals */}
            <SectionCard icon={Target} title="Career Goals">
              {(profile?.careerGoals || motivationsList.length > 0) ? (
                <div className="space-y-2">
                  {profile.careerGoals && (
                    <p className="text-slate-700 text-sm line-clamp-2">{profile.careerGoals}</p>
                  )}
                  {motivationsList.length > 0 && (
                    <div className="flex flex-wrap gap-1.5">
                      {motivationsList.slice(0, 4).map((m, i) => {
                        const label = typeof m === 'string' ? m : (m?.name || m?.title || String(m));
                        return (
                          <span key={i} className="bg-purple-50 text-purple-700 text-xs px-2 py-0.5 rounded-md font-medium">
                            {label}
                          </span>
                        );
                      })}
                    </div>
                  )}
                </div>
              ) : (
                <p className="text-slate-400 text-sm italic">Not filled</p>
              )}
            </SectionCard>

          </div>
        </motion.div>
      </div>
    </div>
  );
}

/* ─── Main Component with Fallback ─────────────────── */

export default function CareerProfile() {
  const { profile, loading, error } = useCareerProfile();

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <div className="text-center space-y-3">
          <Loader2 className="w-8 h-8 animate-spin text-blue-600 mx-auto" />
          <p className="text-sm text-slate-500">Loading career profile...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <div className="text-center p-6 bg-white rounded-2xl shadow-sm border border-slate-200 max-w-sm">
          <AlertCircle className="w-10 h-10 text-red-400 mx-auto mb-3" />
          <p className="text-slate-600 text-sm mb-4">{error}</p>
          <Button variant="outline" size="sm" onClick={() => window.location.reload()}>
            Retry
          </Button>
        </div>
      </div>
    );
  }

  try {
    return profile ? <ProfileSummary profile={profile} /> : <NoProfile />;
  } catch (err) {
    console.error('Render error in CareerProfile:', err);
    return <NoProfile />;
  }
}
