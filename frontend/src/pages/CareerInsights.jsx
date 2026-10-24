import React, { useState, useEffect } from 'react';
import { useSearchParams, Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { 
  Sparkles, Award, School, ShieldAlert, ArrowRight, Download, BookOpen, 
  TrendingUp, Star, CheckCircle, HelpCircle, ArrowLeft, Bookmark,
  GitPullRequest, LayoutList, Layers, ClipboardCheck, DollarSign, Loader2
} from 'lucide-react';
import { careerGuidanceApi } from '@/services/api/careerGuidance';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';

export default function CareerInsights() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const pathId = searchParams.get('id');

  const [career, setCareer] = useState(null);
  const [profile, setProfile] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [bookmarked, setBookmarked] = useState(false);
  const [userSkills, setUserSkills] = useState([]);
  const [accepting, setAccepting] = useState(false);

  const handleAcceptCareer = async (id) => {
    if (!id) return;
    try {
      setAccepting(true);
      await careerGuidanceApi.acceptCareer(id);
      toast.success('Career accepted! Redirecting to your custom roadmap...');
      navigate('/roadmap');
    } catch (err) {
      console.error(err);
      toast.error('Failed to accept career path.');
    } finally {
      setAccepting(false);
    }
  };

  // Fetch Career details
  const loadData = async () => {
    setIsLoading(true);
    try {
      if (pathId) {
        // Fetch specific path by ID
        const data = await careerGuidanceApi.getPathDetails(pathId);
        setCareer(data);
        
        // Also fetch profile to verify bookmark status
        try {
          const userProf = await careerGuidanceApi.getUserProfile();
          setProfile(userProf);
          setBookmarked(userProf.bookmarkedPaths?.includes(data.id));
        } catch {}
      } else {
        // Fetch active user profile recommendation
        const userProf = await careerGuidanceApi.getUserProfile();
        setProfile(userProf);
        setCareer(userProf.recommendedCareer);
        setBookmarked(userProf.bookmarkedPaths?.includes(userProf.recommendedCareer?.id));
      }
    } catch (err) {
      console.error(err);
      // Don't show toast if it's just 'profile not found', which is expected for new users
      if (pathId) {
        toast.error('Failed to load career insights details');
      }
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [pathId]);

  // Toggle Bookmark
  const handleToggleBookmark = async () => {
    if (!career) return;
    try {
      const isBookmarked = await careerGuidanceApi.toggleBookmark(career.id);
      setBookmarked(isBookmarked);
      toast.success(isBookmarked ? 'Career path saved to bookmarks' : 'Removed from bookmarks');
    } catch (err) {
      toast.error('Failed to toggle bookmark');
    }
  };

  // Printable report export
  const handlePrintReport = () => {
    window.print();
  };

  if (isLoading) {
    return (
      <div className="h-[calc(100vh-80px)] w-full bg-background flex flex-col items-center justify-center gap-3">
        <motion.div
          animate={{ rotate: 360 }}
          transition={{ repeat: Infinity, duration: 1, ease: 'linear' }}
          className="w-10 h-10 border-4 border-blue-600 border-t-transparent rounded-full"
        />
        <p className="text-muted-foreground text-xs font-semibold">Retrieving career insights database...</p>
      </div>
    );
  }

  if (!career) {
    return (
      <div className="min-h-[calc(100vh-80px)] w-full bg-background text-foreground flex flex-col items-center justify-center p-6 text-center">
        <div className="p-4 rounded-full bg-card border border-border text-muted-foreground mb-4">
          <ClipboardCheck className="w-10 h-10" />
        </div>
        <h2 className="text-xl font-bold text-foreground">No Active Career Profile</h2>
        <p className="text-sm text-muted-foreground max-w-sm mt-2 leading-relaxed">
          You haven't completed the career guidance quiz or chosen a specific pathway to explore.
        </p>
        <div className="mt-6 flex flex-col sm:flex-row gap-3">
          <Button asChild className="bg-white from-blue-600 to-blue-600 hover:from-blue-600 hover:to-blue-600 text-white border-0 font-bold text-xs px-6">
            <Link to="/quiz">
              Take Career Quiz
            </Link>
          </Button>
          <Button asChild variant="outline" className="border-border bg-card text-xs hover:bg-secondary text-foreground">
            <Link to="/career-explorer">
              Explore Mind Map
            </Link>
          </Button>
        </div>
      </div>
    );
  }

  // Calculate skill gap indicators
  const skillsList = career.skillsRequired || [];
  // Dummy acquired skills mapping or check profile gaps if available
  const skillGaps = profile && profile.recommendedCareer?.id === career.id ? profile.skillGap : [];
  const acquiredCount = skillsList.length - (skillGaps?.length || 0);
  const matchPercentage = skillsList.length > 0 ? Math.round((acquiredCount / skillsList.length) * 100) : 0;

  return (
    <div className="min-h-screen bg-transparent text-foreground p-4 md:p-8 relative overflow-x-hidden">
      {/* Printable Area CSS Injection */}
      <style dangerouslySetInnerHTML={{ __html: `
        @media print {
          body {
            background: white !important;
            color: black !important;
          }
          nav, sidebar, button, a, footer, .no-print {
            display: none !important;
          }
          .print-container {
            width: 100% !important;
            padding: 0 !important;
            margin: 0 !important;
            box-shadow: none !important;
            border: none !important;
            background: transparent !important;
          }
          .print-card {
            background: white !important;
            border: 1px solid #ddd !important;
            color: black !important;
            box-shadow: none !important;
          }
          .text-white {
            color: black !important;
          }
          .text-slate-400, .text-slate-500 {
            color: #555 !important;
          }
          .bg-slate-900, .bg-slate-950, .bg-slate-950\\/50 {
            background: #f8f9fa !important;
          }
          .border-slate-800, .border-slate-700 {
            border-color: #ddd !important;
          }
        }
      `}} />

      {/* Background neon blobs */}
      <div className="absolute top-[-5%] right-[-10%] w-[500px] h-[500px] bg-blue-600/5  rounded-full blur-[120px] pointer-events-none" />
      <div className="absolute bottom-[20%] left-[-10%] w-[400px] h-[400px] bg-blue-600/5  rounded-full blur-[100px] pointer-events-none" />

      <div className="max-w-5xl mx-auto space-y-8 print-container relative z-10">
        
        {/* Navigation Bar / Breadcrumbs */}
        <div className="flex items-center justify-between no-print border-b border-border/80 pb-4">
          <Button asChild variant="ghost" className="text-muted-foreground hover:text-foreground pl-0 text-xs gap-1.5">
            <Link to="/career-explorer">
              <ArrowLeft className="w-4 h-4" /> Back to Mind Map
            </Link>
          </Button>

          <div className="flex gap-2">
            <Button
              onClick={() => handleAcceptCareer(career.id)}
              disabled={accepting}
              className="bg-white from-blue-600 to-blue-700 hover:from-blue-500 hover:to-blue-600 text-white border-0 text-xs font-bold gap-1.5"
            >
              {accepting ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  Accepting...
                </>
              ) : (
                'Accept Career & Start Roadmap'
              )}
            </Button>
            <Button
              onClick={handleToggleBookmark}
              variant="outline"
              className="border-border bg-card hover:bg-secondary text-xs gap-1.5 text-foreground"
            >
              <Bookmark className={`w-4 h-4 ${bookmarked ? 'fill-indigo-650 text-blue-600  ' : 'text-muted-foreground'}`} />
              {bookmarked ? 'Bookmarked' : 'Save Path'}
            </Button>
            <Button
              onClick={handlePrintReport}
              className="bg-blue-600 hover:bg-blue-600 text-white text-xs gap-1.5 border-0"
            >
              <Download className="w-4 h-4" /> Export Report PDF
            </Button>
          </div>
        </div>

        {/* Header Hero Section */}
        <div className="p-6 md:p-8 rounded-lg bg-white from-blue-600/5 to-card border border-border/80 backdrop-blur-md shadow-xl print-card">
          <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
            <div className="space-y-3">
              <div className="flex flex-wrap gap-2 items-center">
                <span className="text-[10px] font-bold uppercase bg-blue-600/10 text-blue-600   border border-blue-600/20  px-3 py-1 rounded-full tracking-wider">
                  Level {career.level} Specialization
                </span>
                <span className="text-[10px] font-bold uppercase bg-secondary text-muted-foreground px-3 py-1 rounded-full tracking-wider">
                  {career.streamType}
                </span>
              </div>
              <h1 className="text-2xl md:text-4xl font-black text-foreground leading-tight">{career.title}</h1>
              <p className="text-muted-foreground text-sm max-w-2xl leading-relaxed">{career.description}</p>
            </div>

            {/* Quick Metrics */}
            <div className="flex flex-wrap gap-4 shrink-0 bg-background border border-border/80 p-4 rounded-lg print-card">
              <div className="text-center min-w-[80px]">
                <p className="text-[10px] text-muted-foreground uppercase font-bold mb-1">CAGR Growth</p>
                <div className="flex items-center justify-center gap-1 text-blue-600 ">
                  <TrendingUp className="w-4 h-4" />
                  <span className="text-lg font-black">{career.industryGrowth}%</span>
                </div>
              </div>
              <div className="h-10 w-px bg-border" />
              <div className="text-center min-w-[80px]">
                <p className="text-[10px] text-muted-foreground uppercase font-bold mb-1">Demand Index</p>
                <div className="flex items-center justify-center gap-1 text-fuchsia-600 ">
                  <Sparkles className="w-4 h-4" />
                  <span className="text-lg font-black">{career.demandIndex}%</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Grid: Future Scope, Certifications, Colleges */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {/* Scope Card */}
          <div className="bg-card border border-border/80 rounded-lg p-6 backdrop-blur-md flex flex-col justify-between print-card md:col-span-1">
            <div className="space-y-3">
              <h3 className="font-bold text-sm text-foreground flex items-center gap-2 border-b border-border/60 pb-3">
                <TrendingUp className="w-4 h-4 text-blue-600 " /> Market Outlook
              </h3>
              <p className="text-xs text-muted-foreground leading-relaxed font-medium">
                {career.futureScope}
              </p>
            </div>
            <div className="pt-6">
              <Button asChild variant="outline" className="w-full border-border bg-background hover:bg-secondary text-xs no-print text-foreground">
                <Link to={`/salary-forecast?id=${career.id}`}>
                  Model Salary CAGR <ArrowRight className="w-4 h-4 ml-1" />
                </Link>
              </Button>
            </div>
          </div>

          {/* Certifications Card */}
          <div className="bg-card border border-border/80 rounded-lg p-6 backdrop-blur-md print-card">
            <h3 className="font-bold text-sm text-foreground flex items-center gap-2 border-b border-border/60 pb-3">
              <Award className="w-4 h-4 text-amber-500 " /> Top Certifications
            </h3>
            <ul className="space-y-3 mt-4">
              {career.certifications && career.certifications.length > 0 && career.certifications[0] !== "" ? (
                career.certifications.map((cert, idx) => (
                  <li key={idx} className="text-xs text-muted-foreground  flex items-start gap-2 bg-background p-2.5 rounded-lg border border-border print-card">
                    <CheckCircle className="w-4 h-4 text-blue-600  mt-0.5 shrink-0" />
                    <span>{cert}</span>
                  </li>
                ))
              ) : (
                <li className="text-xs text-muted-foreground italic">No specific certifications seeded.</li>
              )}
            </ul>
          </div>

          {/* Colleges Card */}
          <div className="bg-card border border-border/80 rounded-lg p-6 backdrop-blur-md print-card">
            <h3 className="font-bold text-sm text-foreground flex items-center gap-2 border-b border-border/60 pb-3">
              <School className="w-4 h-4 text-blue-600 " /> Key Indian Colleges
            </h3>
            <div className="flex flex-wrap gap-2 mt-4">
              {career.colleges && career.colleges.length > 0 && career.colleges[0] !== "" ? (
                career.colleges.map((college, idx) => (
                  <span
                    key={idx}
                    className="text-xs bg-background text-blue-600  border border-blue-600/20 px-3 py-1.5 rounded-lg font-medium print-card"
                  >
                    {college}
                  </span>
                ))
              ) : (
                <span className="text-xs text-muted-foreground italic">No specific colleges seeded.</span>
              )}
            </div>
          </div>
        </div>

        {/* Skill Gap Analysis Section */}
        <div className="p-6 md:p-8 rounded-lg bg-card border border-border/80 backdrop-blur-md print-card">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 border-b border-border/60 pb-4 mb-6">
            <div>
              <h3 className="font-bold text-lg text-foreground flex items-center gap-2">
                <ClipboardCheck className="w-5 h-5 text-blue-600 " /> Skill Matching Analysis
              </h3>
              <p className="text-xs text-muted-foreground mt-0.5">Required skills comparison index</p>
            </div>
            
            {profile && profile.recommendedCareer?.id === career.id && (
              <div className="text-xs bg-blue-600/10 text-blue-600  border border-blue-600/20 px-3 py-1.5 rounded-lg font-semibold">
                Match Index: {matchPercentage}%
              </div>
            )}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {skillsList.map((skill, idx) => {
              // If skill isn't in skillGaps, the user has it!
              const isAcquired = profile && profile.recommendedCareer?.id === career.id ? !skillGaps.includes(skill) : false;
              return (
                <div key={idx} className="p-4 rounded-lg bg-background border border-border/80 flex items-center justify-between gap-4 print-card">
                  <div className="space-y-1">
                    <p className="text-sm font-bold text-foreground">{skill}</p>
                    <p className="text-[10px] text-muted-foreground">Tier 1 requirement</p>
                  </div>
                  <span className={`text-[10px] font-bold px-2.5 py-1 rounded-full uppercase ${
                    isAcquired 
                      ? 'bg-blue-600/10 text-blue-600  border border-blue-600/20' 
                      : 'bg-blue-600/10 text-blue-600  border border-blue-600/20'
                  }`}>
                    {isAcquired ? 'Acquired' : 'Target Skill'}
                  </span>
                </div>
              );
            })}
          </div>
        </div>

        {/* Curriculum/Learning Roadmap Timeline */}
        <div className="p-6 md:p-8 rounded-lg bg-card border border-border/80 backdrop-blur-md print-card">
          <div className="border-b border-border/60 pb-4 mb-6">
            <h3 className="font-bold text-lg text-foreground flex items-center gap-2">
              <GitPullRequest className="w-5 h-5 text-blue-600  animate-pulse" /> Structured Learning Roadmap
            </h3>
            <p className="text-xs text-muted-foreground mt-0.5">Phase-by-phase learning path curriculum</p>
          </div>

          <div className="space-y-8 relative before:absolute before:top-2 before:bottom-2 before:left-[19px] before:w-[2px] before:bg-border">
            {career.roadmap && career.roadmap.length > 0 ? (
              career.roadmap.map((phase, idx) => (
                <div key={idx} className="flex gap-6 relative z-10">
                  {/* Timeline point */}
                  <span className="w-10 h-10 rounded-full bg-background border border-border text-xs font-bold text-blue-600  flex items-center justify-center shrink-0 shadow-lg print-card">
                    {idx + 1}
                  </span>

                  <div className="flex-1 bg-background border border-border/80 rounded-lg p-5 md:p-6 print-card space-y-4">
                    <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 border-b border-border/40 pb-3">
                      <div>
                        <h4 className="font-bold text-base text-foreground">{phase.phase}</h4>
                        <p className="text-[10px] text-muted-foreground uppercase tracking-wider font-semibold mt-0.5">Estimated duration: {phase.duration}</p>
                      </div>
                    </div>

                    <div className="space-y-2">
                      <p className="text-[11px] font-bold uppercase tracking-wider text-muted-foreground">Core Topics</p>
                      <div className="flex flex-wrap gap-1.5">
                        {phase.topics?.map((topic, tIdx) => (
                          <span key={tIdx} className="text-xs bg-background border border-border text-muted-foreground  px-3 py-1 rounded-lg">
                            {topic}
                          </span>
                        ))}
                      </div>
                    </div>

                    {phase.projectSuggestion && (
                      <div className="bg-blue-600/40  border border-blue-600  rounded-lg p-4">
                        <p className="text-[10px] font-bold uppercase tracking-wider text-blue-600  mb-1">Project Milestone</p>
                        <p className="text-xs text-muted-foreground  leading-normal">{phase.projectSuggestion}</p>
                      </div>
                    )}
                  </div>
                </div>
              ))
            ) : (
              <div className="text-xs text-muted-foreground italic pl-10">No learning roadmap milestones seeded yet.</div>
            )}
          </div>
        </div>

        {/* Compare / Next Steps (No Print) */}
        <div className="p-6 md:p-8 rounded-lg bg-white from-blue-600/5 to-blue-600/10   border border-border/80 backdrop-blur-md no-print flex flex-col sm:flex-row justify-between items-center gap-6 shadow-xl print-card">
          <div className="space-y-1.5 text-center sm:text-left">
            <h3 className="font-bold text-base text-foreground flex items-center justify-center sm:justify-start gap-2">
              <Star className="w-5 h-5 text-blue-600 " /> Compare Career Paths
            </h3>
            <p className="text-xs text-muted-foreground">Compare this career side-by-side with other domains to evaluate difficulty, cagr, and education.</p>
          </div>

          <div className="flex gap-3">
            <Button
              onClick={() => handleAcceptCareer(career.id)}
              disabled={accepting}
              className="bg-white from-blue-600 to-blue-700 hover:from-blue-500 hover:to-blue-600 text-white border-0 text-xs font-bold px-6 shrink-0 gap-1.5"
            >
              {accepting ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  Accepting...
                </>
              ) : (
                'Accept Career & Start Roadmap'
              )}
            </Button>
            <Button asChild className="bg-white from-blue-600 to-blue-600 hover:from-blue-600 hover:to-blue-600 text-white border-0 text-xs font-bold px-6 shrink-0">
              <Link to={`/salary-forecast?id=${career.id}`}>
                Forecast Salary CAGR
              </Link>
            </Button>
          </div>
        </div>

      </div>
    </div>
  );
}

