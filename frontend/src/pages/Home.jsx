import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import {
  Sparkles,
  ArrowRight,
  Compass,
  Map,
  ListTodo,
  Users2,
  MessageSquare,
  TrendingUp,
  CheckCircle2,
  Award,
  Zap,
  ShieldCheck,
  Brain,
  BarChart3,
  Flame,
  Star,
  ChevronRight,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useAuth } from '@/context/AuthContext';

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 16 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.4 } },
};

export const HomePage = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const features = [
    {
      icon: Compass,
      title: 'AI Career Assessment',
      description: 'Take adaptive quizzes that assess your interests, personality, and aptitude to pinpoint your ideal career trajectory.',
      badge: 'Smart Engine',
      link: '/quiz',
      color: 'from-blue-600 to-indigo-600',
    },
    {
      icon: Map,
      title: 'Interactive Career Map',
      description: 'Explore visual node-based career pathways across engineering, science, commerce, design, and emerging tech.',
      badge: 'Visual Explorer',
      link: '/career-explorer',
      color: 'from-indigo-600 to-purple-600',
    },
    {
      icon: ListTodo,
      title: 'Personalized Roadmaps',
      description: 'Follow step-by-step modular learning journeys with curated resources, hour estimates, and milestone checkboxes.',
      badge: 'Structured Paths',
      link: '/roadmap',
      color: 'from-cyan-600 to-blue-600',
    },
    {
      icon: Users2,
      title: 'Peer Study Groups',
      description: 'Collaborate with fellow learners in interactive study rooms with real-time video, discussion threads, and shared tasks.',
      badge: 'Collaborative',
      link: '/peer-rooms',
      color: 'from-emerald-600 to-teal-600',
    },
    {
      icon: MessageSquare,
      title: 'Real-Time Chat & AI Mentorship',
      description: 'Instant peer-to-peer messaging paired with an always-available AI assistant for career advice and guidance.',
      badge: 'Live & 24/7',
      link: '/messages',
      color: 'from-amber-600 to-orange-600',
    },
    {
      icon: BarChart3,
      title: 'Salary & Growth Analytics',
      description: 'Access transparent industry data on compensation brackets, hiring demand trends, and automation risk indices.',
      badge: 'Market Insights',
      link: '/career-explorer',
      color: 'from-rose-600 to-pink-600',
    },
  ];

  const steps = [
    {
      step: '01',
      title: 'Assess Your Potential',
      desc: 'Complete the adaptive aptitude and interest quiz to discover your strongest career alignments.',
    },
    {
      step: '02',
      title: 'Follow Your Custom Roadmap',
      desc: 'Master industry-relevant skills with structured modules, projects, and progressive milestones.',
    },
    {
      step: '03',
      title: 'Collaborate & Excel',
      desc: 'Join peer rooms, solve challenges together, track your streak, and reach your dream career.',
    },
  ];

  return (
    <div className="space-y-16 pb-12">
      {/* Hero Section */}
      <section className="relative overflow-hidden rounded-2xl bg-white text-slate-900 p-8 md:p-12 shadow-sm border border-slate-100">
        {/* Subtle decorative shapes */}
        <div className="absolute -top-20 -right-20 w-72 h-72 bg-slate-50 rounded-full blur-2xl pointer-events-none" />
        <div className="absolute -bottom-20 -left-20 w-72 h-72 bg-slate-50 rounded-full blur-2xl pointer-events-none" />

        <div className="relative z-10 max-w-3xl">
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
            className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-slate-50 border border-slate-100 text-sm font-medium mb-5"
          >
            <Sparkles className="h-4 w-4 text-slate-500" />
            <span className="text-slate-600">AI-driven career navigation for students & early professionals</span>
          </motion.div>

          <motion.h1
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.1 }}
            className="text-3xl sm:text-4xl font-semibold tracking-tight leading-tight mb-4 text-slate-900"
          >
            Chart your future with clarity, skill, and community
          </motion.h1>

          <motion.p
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.2 }}
            className="text-base sm:text-lg text-slate-600 mb-6 leading-relaxed max-w-2xl"
          >
            SkillNavigator provides structured career pathways, interactive maps, and collaborative study rooms to help learners progress with confidence.
          </motion.p>

          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.3 }}
            className="flex flex-wrap items-center gap-4"
          >
            <Link to="/quiz">
              <Button size="lg" className="h-12 px-6 bg-indigo-600 text-white hover:bg-indigo-700 font-semibold shadow-sm gap-2 rounded-lg">
                <Sparkles className="h-4 w-4 text-white/90" />
                Take Career Quiz
                <ArrowRight className="h-4 w-4" />
              </Button>
            </Link>

            <Link to="/roadmap">
              <Button size="lg" variant="outline" className="h-12 px-6 border border-slate-200 text-slate-700 bg-white hover:bg-slate-50 font-medium rounded-lg">
                Explore Roadmaps
              </Button>
            </Link>

            <Link to="/dashboard">
              <Button size="lg" variant="ghost" className="h-12 px-4 text-slate-700 hover:text-slate-900 hover:bg-slate-50 font-medium rounded-lg">
                Go to Dashboard <ChevronRight className="h-4 w-4 ml-1" />
              </Button>
            </Link>
          </motion.div>
        </div>
      </section>

      {/* Platform Statistics */}
      <section className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card className="border-slate-200 bg-white shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6 text-center">
            <div className="text-3xl font-extrabold text-blue-600">50+</div>
            <p className="text-xs font-semibold text-slate-700 mt-1">Career Pathways</p>
            <p className="text-[11px] text-slate-400 mt-0.5">Software, AI, Design, Science & more</p>
          </CardContent>
        </Card>

        <Card className="border-slate-200 bg-white shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6 text-center">
            <div className="text-3xl font-extrabold text-indigo-600">500+</div>
            <p className="text-xs font-semibold text-slate-700 mt-1">Curated Skills</p>
            <p className="text-[11px] text-slate-400 mt-0.5">Structured modular curriculums</p>
          </CardContent>
        </Card>

        <Card className="border-slate-200 bg-white shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6 text-center">
            <div className="text-3xl font-extrabold text-cyan-600">95%</div>
            <p className="text-xs font-semibold text-slate-700 mt-1">Quiz Accuracy</p>
            <p className="text-[11px] text-slate-400 mt-0.5">Multi-dimensional psychometric fit</p>
          </CardContent>
        </Card>

        <Card className="border-slate-200 bg-white shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-6 text-center">
            <div className="text-3xl font-extrabold text-emerald-600">24/7</div>
            <p className="text-xs font-semibold text-slate-700 mt-1">AI Assistant</p>
            <p className="text-[11px] text-slate-400 mt-0.5">Instant mentoring & advice</p>
          </CardContent>
        </Card>
      </section>

      {/* Feature Showcase Grid */}
      <section className="space-y-6">
        <div className="text-center max-w-2xl mx-auto">
          <Badge variant="outline" className="bg-blue-50 text-blue-700 border-blue-200 font-semibold mb-2">
            Core Modules
          </Badge>
          <h2 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight">
            Everything You Need for Career Readiness
          </h2>
          <p className="text-sm text-slate-500 mt-2">
            A cohesive suite of tools built to guide every step from discovering your ideal role to mastering in-demand industry skills.
          </p>
        </div>

        <motion.div
          variants={containerVariants}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
        >
          {features.map((item, idx) => (
            <motion.div key={idx} variants={itemVariants}>
              <Card className="h-full border-slate-200 bg-white hover:border-blue-300 hover:shadow-md transition-all duration-200 flex flex-col justify-between group">
                <CardContent className="p-6 flex flex-col h-full justify-between">
                  <div>
                    <div className="flex items-center justify-between mb-4">
                      <div className={`flex h-11 w-11 items-center justify-center rounded-xl bg-gradient-to-tr ${item.color} text-white shadow-sm`}>
                        <item.icon className="h-5 w-5" />
                      </div>
                      <Badge variant="secondary" className="bg-slate-100 text-slate-700 text-[11px] font-medium">
                        {item.badge}
                      </Badge>
                    </div>

                    <h3 className="text-base font-bold text-slate-900 mb-2 group-hover:text-blue-600 transition-colors">
                      {item.title}
                    </h3>
                    <p className="text-xs text-slate-600 leading-relaxed mb-6">
                      {item.description}
                    </p>
                  </div>

                  <Link to={item.link}>
                    <Button variant="ghost" size="sm" className="w-full justify-between text-xs font-semibold text-blue-600 hover:bg-blue-50 p-2 rounded-lg">
                      Explore Module
                      <ArrowRight className="h-3.5 w-3.5" />
                    </Button>
                  </Link>
                </CardContent>
              </Card>
            </motion.div>
          ))}
        </motion.div>
      </section>

      {/* How It Works */}
      <section className="bg-white border border-slate-200 rounded-2xl p-8 sm:p-10 shadow-sm">
        <div className="text-center max-w-xl mx-auto mb-8">
          <Badge variant="outline" className="bg-indigo-50 text-indigo-700 border-indigo-200 font-semibold mb-2">
            The Process
          </Badge>
          <h2 className="text-2xl font-bold text-slate-900">How SkillNavigator Works</h2>
          <p className="text-xs text-slate-500 mt-1">3 simple steps to accelerate your career readiness</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {steps.map((s, i) => (
            <div key={i} className="relative p-6 rounded-xl bg-slate-50 border border-slate-200/80">
              <div className="text-3xl font-extrabold text-blue-600/30 mb-3">{s.step}</div>
              <h3 className="text-sm font-bold text-slate-900 mb-1">{s.title}</h3>
              <p className="text-xs text-slate-600 leading-relaxed">{s.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Presentation Call-To-Action Banner */}
      <section className="rounded-2xl bg-slate-900 text-white p-8 sm:p-10 text-center flex flex-col items-center justify-center relative overflow-hidden">
        <div className="relative z-10 max-w-lg space-y-4">
          <Badge className="bg-blue-600 text-white border-transparent">Ready to Present</Badge>
          <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">
            Empowering Modern Students with Smart Guidance
          </h2>
          <p className="text-xs sm:text-sm text-slate-300 leading-relaxed">
            Jump directly into the live interactive dashboard or take the quick career assessment to see the intelligence engine in action.
          </p>
          <div className="pt-2 flex justify-center gap-3">
            <Link to="/dashboard">
              <Button className="bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl text-xs h-10 px-5">
                Open Dashboard
              </Button>
            </Link>
            <Link to="/quiz">
              <Button variant="outline" className="bg-transparent hover:bg-white/10 text-white border-slate-700 rounded-xl text-xs h-10 px-5">
                Launch Quiz
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="pt-8 border-t border-slate-200 text-center sm:flex sm:items-center sm:justify-between text-xs text-slate-500">
        <div className="flex items-center justify-center sm:justify-start gap-2 mb-4 sm:mb-0">
          <div className="flex h-6 w-6 items-center justify-center rounded-md bg-blue-600 text-white font-bold text-xs">
            <Sparkles className="h-3.5 w-3.5" />
          </div>
          <span className="font-bold text-slate-800">SkillNavigator</span>
          <span>© {new Date().getFullYear()} — Career & Skill Intelligence</span>
        </div>
        <div className="flex justify-center gap-6 text-slate-500 font-medium">
          <Link to="/dashboard" className="hover:text-blue-600">Dashboard</Link>
          <Link to="/career-explorer" className="hover:text-blue-600">Career Map</Link>
          <Link to="/roadmap" className="hover:text-blue-600">Roadmap</Link>
          <Link to="/quiz" className="hover:text-blue-600">Quiz</Link>
          <Link to="/peer-rooms" className="hover:text-blue-600">Peer Groups</Link>
        </div>
      </footer>
    </div>
  );
};

export default HomePage;
