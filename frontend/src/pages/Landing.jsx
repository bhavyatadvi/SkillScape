import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import {
  Compass,
  Map as MapIcon,
  BookOpen,
  Target,
  MessageSquare,
  ArrowRight,
  BrainCircuit,
  Briefcase,
  Zap,
  CheckCircle2
} from 'lucide-react';


const LandingPage = () => {
  const navigate = useNavigate();
  const [isScrolled, setIsScrolled] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const fadeIn = {
    hidden: { opacity: 0, y: 30 },
    visible: { opacity: 1, y: 0, transition: { duration: 0.6, ease: "easeOut" } }
  };

  const staggerContainer = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: { staggerChildren: 0.1 }
    }
  };

  return (
    <div className="min-h-screen bg-transparent font-sans text-foreground selection:bg-primary selection:text-primary-foreground overflow-x-hidden relative">
      
      {/* ----------------------------------------------------- */}
      {/* NAVBAR */}
      {/* ----------------------------------------------------- */}
      <nav className={`fixed top-0 w-full z-50 transition-all duration-300 ${isScrolled ? 'bg-white/95 backdrop-blur-md shadow-sm border-b border-border' : 'bg-transparent'}`}>
        <div className="max-w-[1400px] mx-auto px-6 md:px-12 h-[76px] flex items-center justify-between">
          
          {/* Logo */}
          <div className="flex items-center cursor-pointer" onClick={() => navigate('/')}>
            <img src="/logo.png" alt="SkillScape" className="h-[60px] object-contain" />
          </div>

          {/* Nav Links */}
          <div className="hidden lg:flex items-center gap-10">
            <a href="#how-it-works" className="text-[15px] font-semibold text-muted-foreground hover:text-foreground transition-colors relative group">
              How It Works
              <span className="absolute -bottom-1 left-0 w-0 h-0.5 bg-primary transition-all group-hover:w-full"></span>
            </a>
            <a href="#features" className="text-[15px] font-semibold text-muted-foreground hover:text-foreground transition-colors relative group">
              Features
              <span className="absolute -bottom-1 left-0 w-0 h-0.5 bg-primary transition-all group-hover:w-full"></span>
            </a>
            <a href="#career-map" className="text-[15px] font-semibold text-muted-foreground hover:text-foreground transition-colors relative group">
              Career Paths
              <span className="absolute -bottom-1 left-0 w-0 h-0.5 bg-primary transition-all group-hover:w-full"></span>
            </a>
            <a href="#ai" className="text-[15px] font-semibold text-muted-foreground hover:text-foreground transition-colors relative group">
              Intelligence
              <span className="absolute -bottom-1 left-0 w-0 h-0.5 bg-primary transition-all group-hover:w-full"></span>
            </a>
          </div>

          {/* Auth Toggle */}
          <div className="flex items-center gap-4">
            <button 
              onClick={() => navigate('/login')}
              className="text-[15px] font-semibold text-foreground hover:text-primary transition-colors"
            >
              Login
            </button>
            <button 
              onClick={() => navigate('/register')}
              className="px-5 py-2.5 text-[15px] font-bold rounded-[10px] bg-primary text-primary-foreground hover:bg-primary-dark transition-all hover:scale-105 active:scale-95 shadow-[0_4px_14px_0_rgba(49,95,239,0.39)]"
            >
              Register
            </button>
          </div>
        </div>
      </nav>

      {/* ----------------------------------------------------- */}
      {/* HERO SECTION */}
      {/* ----------------------------------------------------- */}
      <section className="relative pt-36 pb-20 md:pt-48 md:pb-32 px-6 md:px-12 max-w-[1400px] mx-auto min-h-[90vh] flex items-center z-10">
        
        
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 lg:gap-12 items-center w-full relative z-10">
          
          <motion.div 
            initial="hidden" 
            animate="visible" 
            variants={staggerContainer}
            className="max-w-[600px]"
          >
            <motion.div variants={fadeIn} className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-primary/10 text-primary text-[13px] font-bold uppercase tracking-widest mb-8 border border-primary/20">
              <Zap className="w-4 h-4" />
              AI-Powered Career Guidance
            </motion.div>
            
            <motion.h1 variants={fadeIn} className="text-5xl md:text-6xl lg:text-[72px] font-extrabold tracking-[-0.02em] text-foreground leading-[1.05] mb-6">
              Find the <span className="text-primary">career path</span><br />that fits you.
            </motion.h1>
            
            <motion.p variants={fadeIn} className="text-lg md:text-[19px] text-muted-foreground mb-10 leading-[1.6]">
              Discover suitable careers, understand your skill gaps, and follow a personalized learning roadmap that adapts as your goals and skills grow.
            </motion.p>
            
            <motion.div variants={fadeIn} className="flex flex-col sm:flex-row gap-4">
              <button 
                onClick={() => navigate('/quiz')}
                className="px-8 py-4 bg-primary text-primary-foreground text-[17px] font-bold rounded-[12px] hover:bg-primary-dark transition-all hover:scale-105 active:scale-95 shadow-[0_8px_20px_-6px_rgba(49,95,239,0.6)] flex items-center justify-center gap-2"
              >
                Take the Career Quiz <ArrowRight className="w-5 h-5" />
              </button>
              <button 
                onClick={() => navigate('/career-explorer')}
                className="px-8 py-4 bg-white/70 border border-border text-foreground text-[17px] font-bold rounded-[12px] hover:bg-slate-50 transition-all hover:scale-105 active:scale-95 flex items-center justify-center shadow-sm"
              >
                Explore Career Paths
              </button>
            </motion.div>
          </motion.div>

          <motion.div 
            initial={{ opacity: 0, x: 40 }} 
            animate={{ opacity: 1, x: 0 }} 
            transition={{ duration: 0.8, ease: "easeOut", delay: 0.2 }}
            className="relative hidden lg:block"
          >
            {/* Premium UI Mockup */}
            <div className="bg-white/70 rounded-[20px] border border-border shadow-[0_20px_60px_-15px_rgba(0,0,0,0.05)] p-8 pb-0 overflow-hidden relative">
              <div className="absolute top-0 left-0 w-full h-1.5 bg-primary" />
              <div className="flex items-center gap-4 mb-10">
                <div className="w-12 h-12 rounded-[12px] bg-primary/10 flex items-center justify-center text-primary font-bold text-lg">JD</div>
                <div>
                  <div className="text-[17px] font-bold text-foreground">Your Career Map</div>
                  <div className="text-[13px] text-muted-foreground font-medium uppercase tracking-wide">Updated today</div>
                </div>
              </div>
              
              <div className="space-y-6">
                <div className="flex items-start gap-5">
                  <div className="w-10 h-10 rounded-full bg-primary text-white flex items-center justify-center shrink-0 z-10 shadow-[0_0_15px_rgba(49,95,239,0.3)]"><CheckCircle2 className="w-5 h-5" /></div>
                  <div className="bg-slate-50 border border-border rounded-[14px] p-5 flex-1">
                    <div className="text-[11px] font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Step 01</div>
                    <div className="font-bold text-[17px] text-foreground">Career Discovery</div>
                    <div className="text-[14px] text-muted-foreground mt-1">Matched with Data Science (94% compatibility)</div>
                  </div>
                </div>
                <div className="w-[2px] h-10 bg-border absolute left-[45px] top-[138px]" />
                
                <div className="flex items-start gap-5">
                  <div className="w-10 h-10 rounded-full bg-white/70 border-2 border-primary text-primary flex items-center justify-center shrink-0 z-10 relative shadow-[0_0_15px_rgba(49,95,239,0.2)]">
                    <div className="w-3 h-3 rounded-full bg-primary" />
                  </div>
                  <div className="bg-white/70 border-2 border-primary shadow-[0_10px_30px_-10px_rgba(49,95,239,0.15)] rounded-[14px] p-5 flex-1 transform -translate-y-1">
                    <div className="text-[11px] font-bold text-primary uppercase tracking-widest mb-1.5 flex justify-between items-center">
                      Current Step
                      <span className="w-2 h-2 rounded-full bg-primary animate-pulse" />
                    </div>
                    <div className="font-bold text-[17px] text-foreground">Skill Gap Analysis</div>
                    <div className="text-[14px] text-muted-foreground mt-1">3 core skills missing. Building personalized roadmap.</div>
                    <div className="mt-4 h-2 w-full bg-primary/10 rounded-full overflow-hidden">
                      <motion.div 
                        initial={{ width: 0 }}
                        animate={{ width: "60%" }}
                        transition={{ duration: 1.5, delay: 1 }}
                        className="h-full bg-primary" 
                      />
                    </div>
                  </div>
                </div>
                <div className="w-[2px] h-10 bg-border absolute left-[45px] top-[260px]" />
                
                <div className="flex items-start gap-5 pb-6">
                  <div className="w-10 h-10 rounded-full bg-white/70 border-2 border-border text-border flex items-center justify-center shrink-0 z-10">
                    <div className="w-2 h-2 rounded-full bg-border" />
                  </div>
                  <div className="bg-slate-50 border border-border rounded-[14px] p-5 flex-1 opacity-50">
                    <div className="text-[11px] font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Upcoming</div>
                    <div className="font-bold text-[17px] text-foreground">Adaptive Learning</div>
                    <div className="text-[14px] text-muted-foreground mt-1">Start Python for Data Analysis module.</div>
                  </div>
                </div>
              </div>
              
              {/* Fade out bottom */}
              <div className="absolute bottom-0 left-0 w-full h-32 bg-gradient-to-t from-white to-transparent pointer-events-none" />
            </div>
          </motion.div>
        </div>
      </section>

      {/* ----------------------------------------------------- */}
      {/* HOW IT WORKS (JOURNEY) */}
      {/* ----------------------------------------------------- */}
      <section id="how-it-works" className="py-24 md:py-32 px-6 md:px-12 bg-white/70 relative z-20">
        <div className="max-w-[1400px] mx-auto">
          <motion.div 
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: "-100px" }}
            variants={staggerContainer}
            className="text-center max-w-3xl mx-auto mb-20"
          >
            <motion.h2 variants={fadeIn} className="text-[42px] md:text-[56px] font-extrabold text-foreground tracking-[-0.02em] mb-6 leading-[1.1]">
              A career journey built around you.
            </motion.h2>
            <motion.p variants={fadeIn} className="text-[19px] text-muted-foreground leading-[1.6]">
              SkillScape connects career discovery, skill development, roadmaps and guidance into one continuous experience.
            </motion.p>
          </motion.div>

          <div className="relative">
            {/* Horizontal Line */}
            <div className="hidden md:block absolute top-[40px] left-[10%] right-[10%] h-[2px] bg-border z-0" />
            <motion.div 
              initial={{ width: 0 }}
              whileInView={{ width: "80%" }}
              viewport={{ once: true }}
              transition={{ duration: 1.5, ease: "easeInOut" }}
              className="hidden md:block absolute top-[40px] left-[10%] h-[2px] bg-primary z-0" 
            />

            <div className="grid grid-cols-1 md:grid-cols-5 gap-8 md:gap-4 relative z-10">
              {[
                { num: '01', title: 'Discover', desc: 'Take the AI Career Quiz and understand your strengths.' },
                { num: '02', title: 'Explore', desc: 'Explore career paths and understand exactly where they can lead.' },
                { num: '03', title: 'Understand', desc: 'Identify required skills and find your gaps.' },
                { num: '04', title: 'Learn', desc: 'Follow an adaptive roadmap designed around your goals.' },
                { num: '05', title: 'Grow', desc: 'Build momentum with personalized guidance and peer collaboration.' }
              ].map((step, idx) => (
                <motion.div 
                  key={idx} 
                  initial={{ opacity: 0, y: 20 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  transition={{ delay: idx * 0.15, duration: 0.5 }}
                  className="flex flex-col md:items-center md:text-center group"
                >
                  <div className="w-[80px] h-[80px] bg-white/70 border-2 border-border rounded-full flex items-center justify-center text-[20px] font-extrabold text-muted-foreground mb-6 shadow-sm group-hover:border-primary group-hover:text-primary group-hover:shadow-[0_0_20px_rgba(49,95,239,0.2)] transition-all bg-white/70 relative z-10 duration-300 transform group-hover:-translate-y-1">
                    {step.num}
                  </div>
                  <h3 className="text-[22px] font-bold text-foreground mb-3">{step.title}</h3>
                  <p className="text-[16px] text-muted-foreground leading-[1.6] max-w-[240px]">{step.desc}</p>
                </motion.div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* ----------------------------------------------------- */}
      {/* FEATURES SECTION */}
      {/* ----------------------------------------------------- */}
      <section id="features" className="py-24 md:py-32 px-6 md:px-12 max-w-[1400px] mx-auto relative">
        
        
        <motion.div 
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: "-100px" }}
          variants={staggerContainer}
          className="max-w-3xl mb-20"
        >
          <motion.h2 variants={fadeIn} className="text-[42px] md:text-[56px] font-extrabold text-foreground tracking-[-0.02em] mb-6 leading-[1.1]">
            Everything you need to navigate your career.
          </motion.h2>
          <motion.p variants={fadeIn} className="text-[19px] text-muted-foreground leading-[1.6]">
            From your first career decision to becoming job-ready, SkillScape brings the journey together in one platform.
          </motion.p>
        </motion.div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {[
            {
              title: 'AI Career Quiz',
              icon: Target,
              desc: 'Discover careers based on your interests, strengths, skills, work preferences, and goals.',
              action: 'Take the Quiz',
              route: '/quiz'
            },
            {
              title: 'Career Map',
              icon: MapIcon,
              desc: 'Explore career paths from Education to Career Domain, Specialization, and Advanced Roles.',
              action: 'Explore Career Paths',
              route: '/career-explorer'
            },
            {
              title: 'Adaptive Roadmap',
              icon: Compass,
              desc: 'Get a structured learning journey that evolves as your skills, goals and progress change.',
              action: 'View Roadmap',
              route: '/roadmap'
            },
            {
              title: 'Skill Gap Analysis',
              icon: BrainCircuit,
              desc: 'Understand the exact difference between your current skills and the skills required for your target career.',
              action: 'Analyze Skills',
              route: '/placement'
            },
            {
              title: 'AI Career Twin',
              icon: Zap,
              desc: 'Acts as a personal career assistant that understands your learning journey and provides personalized guidance.',
              action: 'View Intelligence',
              route: '/career-analytics'
            }
          ].map((feature, idx) => (
            <motion.div 
              key={idx}
              initial={{ opacity: 0, y: 30 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true, margin: "-50px" }}
              transition={{ delay: idx * 0.1, duration: 0.5 }}
              whileHover={{ y: -6, transition: { duration: 0.2 } }}
              className="bg-white/70 border border-border rounded-[24px] p-8 hover:border-primary/30 hover:shadow-[0_20px_40px_-15px_rgba(0,0,0,0.05)] transition-all flex flex-col h-full group"
            >
              <div className="w-[52px] h-[52px] bg-primary/10 rounded-[16px] flex items-center justify-center text-primary mb-8 group-hover:scale-110 transition-transform duration-300">
                <feature.icon className="w-[26px] h-[26px]" />
              </div>
              <h3 className="text-[22px] font-bold text-foreground mb-4">{feature.title}</h3>
              <p className="text-[16px] text-muted-foreground leading-[1.6] mb-8 flex-grow">{feature.desc}</p>
              <button 
                onClick={() => navigate(feature.route)}
                className="text-[15px] font-bold text-primary flex items-center gap-2 w-fit group-hover:gap-3 transition-all"
              >
                {feature.action} <ArrowRight className="w-[18px] h-[18px]" />
              </button>
            </motion.div>
          ))}
        </div>
      </section>

      {/* ----------------------------------------------------- */}
      {/* CAREER PATH SECTION */}
      {/* ----------------------------------------------------- */}
      <section id="career-map" className="py-24 md:py-32 px-6 md:px-12 bg-white/70 relative">
        <div className="max-w-[1400px] mx-auto">
          <motion.div 
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            variants={staggerContainer}
            className="text-center max-w-3xl mx-auto mb-20"
          >
            <motion.h2 variants={fadeIn} className="text-[42px] md:text-[56px] font-extrabold text-foreground tracking-[-0.02em] mb-6">
              See where your skills can take you.
            </motion.h2>
          </motion.div>

          <motion.div 
            initial={{ opacity: 0, y: 40 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.7 }}
            className="bg-white/70 border border-border rounded-[24px] p-10 md:p-16 shadow-[0_30px_60px_-15px_rgba(0,0,0,0.05)] max-w-[1100px] mx-auto relative overflow-hidden"
          >
            <div className="absolute top-0 right-0 w-[400px] h-[400px] bg-primary/5 rounded-full blur-[100px] -z-10" />

            <div className="flex flex-col md:flex-row items-start md:items-center gap-12 md:gap-16">
              
              <div className="flex-1 space-y-10">
                <div className="relative pl-8 border-l-2 border-primary">
                  <div className="absolute left-[-7px] top-1.5 w-3 h-3 rounded-full bg-primary ring-4 ring-primary/20" />
                  <div className="text-[13px] font-bold text-muted-foreground uppercase tracking-widest mb-1">Origin</div>
                  <div className="text-[20px] font-bold text-foreground">Student</div>
                </div>
                <div className="relative pl-8 border-l-2 border-primary">
                  <div className="absolute left-[-7px] top-1.5 w-3 h-3 rounded-full bg-primary ring-4 ring-primary/20" />
                  <div className="text-[13px] font-bold text-muted-foreground uppercase tracking-widest mb-1">Education</div>
                  <div className="text-[20px] font-bold text-foreground">Computer Science</div>
                </div>
                <div className="relative pl-8 border-l-2 border-primary">
                  <div className="absolute left-[-7px] top-1.5 w-3 h-3 rounded-full bg-primary ring-4 ring-primary/20" />
                  <div className="text-[13px] font-bold text-muted-foreground uppercase tracking-widest mb-1">Career Domain</div>
                  <div className="text-[20px] font-bold text-foreground">Software Development</div>
                </div>
                <div className="relative pl-8 border-l-2 border-border">
                  <div className="absolute left-[-7px] top-1.5 w-3 h-3 rounded-full bg-primary shadow-[0_0_15px_rgba(49,95,239,0.5)]" />
                  <div className="text-[13px] font-bold text-primary uppercase tracking-widest mb-1">Career Path</div>
                  <div className="text-[28px] font-extrabold text-foreground tracking-tight">Backend Development</div>
                </div>
              </div>

              <div className="w-full h-px md:w-px md:h-[400px] bg-border" />

              <div className="flex-1 w-full">
                <div className="bg-transparent rounded-[20px] p-8 border border-border">
                  <h4 className="text-[15px] font-bold text-muted-foreground uppercase tracking-widest mb-8 flex items-center gap-3">
                    <Briefcase className="w-5 h-5 text-primary" /> Progression Map
                  </h4>
                  <div className="space-y-6">
                    <div className="border-b border-border pb-6">
                      <div className="text-[14px] text-muted-foreground mb-1">Specialization</div>
                      <div className="text-[18px] font-bold text-foreground">Cloud & Distributed Systems</div>
                    </div>
                    <div className="border-b border-border pb-6">
                      <div className="text-[14px] text-muted-foreground mb-1">Advanced Role</div>
                      <div className="text-[18px] font-bold text-foreground">Senior Backend Engineer</div>
                    </div>
                    <div>
                      <div className="text-[14px] text-muted-foreground mb-2">Key Skills</div>
                      <div className="flex flex-wrap gap-2">
                        <span className="px-3 py-1 bg-white/70 border border-border rounded-full text-[13px] font-semibold text-foreground">Node.js</span>
                        <span className="px-3 py-1 bg-white/70 border border-border rounded-full text-[13px] font-semibold text-foreground">AWS</span>
                        <span className="px-3 py-1 bg-white/70 border border-border rounded-full text-[13px] font-semibold text-foreground">System Design</span>
                      </div>
                    </div>
                  </div>
                  <button 
                    onClick={() => navigate('/career-explorer')}
                    className="w-full mt-10 py-4 bg-white/70 border-2 border-border rounded-[12px] text-foreground font-bold text-[16px] hover:border-primary hover:text-primary transition-all flex items-center justify-center gap-2"
                  >
                    Explore Full Map <ArrowRight className="w-5 h-5" />
                  </button>
                </div>
              </div>

            </div>
          </motion.div>
        </div>
      </section>

      {/* ----------------------------------------------------- */}
      {/* AI / INTELLIGENCE SECTION */}
      {/* ----------------------------------------------------- */}
      <section id="ai" className="py-24 md:py-32 bg-foreground text-white relative overflow-hidden">
        {/* Dark theme watermark / blobs */}
        <div className="absolute top-0 right-0 w-[800px] h-[800px] bg-primary opacity-[0.05] blur-[120px] rounded-full pointer-events-none" />
        <div className="absolute -bottom-[20%] -left-[10%] w-[600px] h-[600px] bg-[#EEF0FF] opacity-[0.02] blur-[100px] rounded-full pointer-events-none" />
        
        <div className="max-w-[1400px] mx-auto px-6 md:px-12 relative z-10">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-20 items-center">
            
            <motion.div
              initial="hidden"
              whileInView="visible"
              viewport={{ once: true }}
              variants={staggerContainer}
            >
              <motion.h2 variants={fadeIn} className="text-[42px] md:text-[56px] font-extrabold tracking-[-0.02em] mb-6 leading-[1.1]">
                Guidance that becomes smarter as you grow.
              </motion.h2>
              <motion.p variants={fadeIn} className="text-[19px] text-slate-300 leading-[1.6] mb-12">
                SkillScape uses AI to transform your career journey from a one-time recommendation into an evolving process.
              </motion.p>
              
              <div className="space-y-10">
                <motion.div variants={fadeIn}>
                  <h3 className="text-[22px] font-bold mb-3 flex items-center gap-3">
                    <div className="w-2 h-2 rounded-full bg-primary" /> AI Career Twin
                  </h3>
                  <p className="text-slate-400 text-[16px] leading-[1.6] pl-5 border-l border-slate-800 ml-1">
                    Acts as a personal career assistant that understands your learning journey and provides personalized guidance.
                  </p>
                </motion.div>
                <motion.div variants={fadeIn}>
                  <h3 className="text-[22px] font-bold mb-3 flex items-center gap-3">
                    <div className="w-2 h-2 rounded-full bg-primary" /> Skill Gap Intelligence
                  </h3>
                  <p className="text-slate-400 text-[16px] leading-[1.6] pl-5 border-l border-slate-800 ml-1">
                    Analyzes your current capabilities against career requirements and highlights the skills you should develop.
                  </p>
                </motion.div>
                <motion.div variants={fadeIn}>
                  <h3 className="text-[22px] font-bold mb-3 flex items-center gap-3">
                    <div className="w-2 h-2 rounded-full bg-primary" /> Reverse Career Finder
                  </h3>
                  <p className="text-slate-400 text-[16px] leading-[1.6] pl-5 border-l border-transparent ml-1">
                    Start with the skills you already have and discover new, unexpected career paths that could match your profile.
                  </p>
                </motion.div>
              </div>
            </motion.div>
            
            {/* AI Visualization */}
            <motion.div 
              initial={{ opacity: 0, scale: 0.95 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ duration: 0.8 }}
              className="relative rounded-[24px] border border-slate-800 bg-[#0A1121] p-10 md:p-14 overflow-hidden shadow-2xl"
            >
              <div className="absolute inset-0 bg-gradient-to-br from-primary/10 to-transparent pointer-events-none" />
              
              <div className="space-y-6 relative z-10">
                <div className="bg-slate-900 border border-slate-800 rounded-[16px] p-5 flex items-center justify-between w-[85%]">
                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 rounded-[10px] bg-slate-800 flex items-center justify-center"><BookOpen className="w-5 h-5 text-slate-400" /></div>
                    <div>
                      <div className="text-[16px] font-bold text-white">Learning Python</div>
                      <div className="text-[13px] text-slate-400 mt-1">Completed Chapter 4</div>
                    </div>
                  </div>
                  <CheckCircle2 className="w-6 h-6 text-white opacity-50" />
                </div>
                
                <div className="w-[2px] h-8 bg-slate-800 ml-10" />
                
                <div className="bg-slate-900 border-2 border-primary shadow-[0_0_30px_rgba(49,95,239,0.2)] rounded-[16px] p-5 flex items-center justify-between ml-6">
                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 rounded-[10px] bg-primary/20 flex items-center justify-center"><Zap className="w-5 h-5 text-primary" /></div>
                    <div>
                      <div className="text-[16px] font-bold text-white">AI Recommendation</div>
                      <div className="text-[13px] text-slate-400 mt-1">Data Science path compatibility increased to 82%</div>
                    </div>
                  </div>
                </div>
                
                <div className="w-[2px] h-8 bg-slate-800 ml-[72px]" />
                
                <div className="bg-slate-900 border border-slate-800 rounded-[16px] p-5 flex items-center justify-between ml-14 w-[80%] opacity-50">
                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 rounded-[10px] bg-slate-800 flex items-center justify-center"><Target className="w-5 h-5 text-slate-400" /></div>
                    <div>
                      <div className="text-[16px] font-bold text-white">Next: Statistics 101</div>
                      <div className="text-[13px] text-slate-400 mt-1">Added to roadmap</div>
                    </div>
                  </div>
                </div>
              </div>
            </motion.div>
          </div>
        </div>
      </section>

      {/* ----------------------------------------------------- */}
      {/* FINAL CTA */}
      {/* ----------------------------------------------------- */}
      <section className="py-32 px-6 bg-primary text-center relative overflow-hidden">
        {/* Subtle watermark in CTA */}
        <Compass className="absolute -top-[50%] -left-[10%] w-[1000px] h-[1000px] text-white opacity-[0.03] pointer-events-none" />
        
        <div className="max-w-[800px] mx-auto relative z-10">
          <motion.h2 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-[42px] md:text-[56px] font-extrabold text-white tracking-[-0.02em] mb-8 leading-[1.1]"
          >
            Your career journey starts with one decision.
          </motion.h2>
          <motion.p 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: 0.1 }}
            className="text-[19px] text-white/90 leading-[1.6] mb-12 max-w-[600px] mx-auto"
          >
            Discover where you fit, understand what you need to learn, and build a path toward the career you want.
          </motion.p>
          <motion.div 
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: 0.2 }}
            className="flex flex-col sm:flex-row gap-5 justify-center"
          >
            <button 
              onClick={() => navigate('/quiz')}
              className="px-8 py-4 bg-white/70 text-primary text-[17px] font-bold rounded-[12px] hover:scale-105 active:scale-95 transition-all shadow-xl"
            >
              Take the Career Quiz
            </button>
            <button 
              onClick={() => navigate('/career-explorer')}
              className="px-8 py-4 bg-transparent border-2 border-white/30 text-white text-[17px] font-bold rounded-[12px] hover:bg-white/70/10 hover:border-white transition-all"
            >
              Explore Career Paths
            </button>
          </motion.div>
        </div>
      </section>

      {/* ----------------------------------------------------- */}
      {/* FOOTER */}
      {/* ----------------------------------------------------- */}
      <footer className="bg-foreground text-white py-20 px-6 md:px-12 border-t border-slate-800">
        <div className="max-w-[1400px] mx-auto grid grid-cols-1 md:grid-cols-5 gap-12">
          
          <div className="md:col-span-2 pr-12">
            <div className="flex items-center mb-6">
              <img src="/logo_light.png" alt="SkillScape" className="h-[60px] object-contain" />
            </div>
            <p className="text-slate-400 text-[15px] leading-[1.8] mb-6">
              AI-powered adaptive career guidance for the modern learner. Plan your education, discover paths, and build the future you want.
            </p>
          </div>
          
          <div>
            <h4 className="font-bold text-white mb-6 text-[16px]">Product</h4>
            <ul className="space-y-4">
              <li><Link to="/quiz" className="text-slate-400 hover:text-white text-[15px] transition-colors">Career Quiz</Link></li>
              <li><Link to="/career-explorer" className="text-slate-400 hover:text-white text-[15px] transition-colors">Career Map</Link></li>
              <li><Link to="/roadmap" className="text-slate-400 hover:text-white text-[15px] transition-colors">Roadmaps</Link></li>
            </ul>
          </div>
          
          <div>
            <h4 className="font-bold text-white mb-6 text-[16px]">Company</h4>
            <ul className="space-y-4">
              <li><a href="#" className="text-slate-400 hover:text-white text-[15px] transition-colors">About</a></li>
              <li><a href="#" className="text-slate-400 hover:text-white text-[15px] transition-colors">Contact</a></li>
              <li><a href="#" className="text-slate-400 hover:text-white text-[15px] transition-colors">Privacy</a></li>
            </ul>
          </div>
          
          <div>
            <h4 className="font-bold text-white mb-6 text-[16px]">Connect</h4>
            <ul className="space-y-4">
              <li><a href="#" className="text-slate-400 hover:text-white text-[15px] transition-colors">LinkedIn</a></li>
              <li><a href="#" className="text-slate-400 hover:text-white text-[15px] transition-colors">Instagram</a></li>
              <li><a href="#" className="text-slate-400 hover:text-white text-[15px] transition-colors">GitHub</a></li>
            </ul>
          </div>
          
        </div>
        
        <div className="max-w-[1400px] mx-auto mt-20 pt-8 border-t border-slate-800 text-slate-500 text-[14px]">
          &copy; {new Date().getFullYear()} SkillScape. All rights reserved.
        </div>
      </footer>

    </div>
  );
};

export default LandingPage;
