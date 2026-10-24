import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import {
  ArrowRight,
  ArrowLeft,
  CheckCircle,
  Sparkles,
  Lightbulb,
  ArrowRight as ArrowRightIcon,
  Rocket,
  Loader2,
} from 'lucide-react';
import { toast } from 'sonner';
import { quizApi } from '@/services/api';

// Career display info (mapped to backend domain IDs)
const careerResults = {
  frontend: {
    name: 'Frontend Developer',
    icon: '🎨',
    color: 'bg-pink-500',
    description: 'You have a keen eye for design and love creating beautiful, interactive user interfaces.',
    skills: ['React', 'TypeScript', 'CSS/Tailwind', 'UI/UX Principles', 'Testing'],
    salary: '$70K - $150K',
    growth: '+25%',
  },
  backend: {
    name: 'Backend Developer',
    icon: '⚙️',
    color: 'bg-blue-500',
    description: 'You excel at building robust server-side systems and APIs that power applications.',
    skills: ['C#/Node.js', 'Databases', 'APIs', 'Security', 'System Design'],
    salary: '$80K - $160K',
    growth: '+22%',
  },
  fullstack: {
    name: 'Full Stack Developer',
    icon: '🚀',
    color: 'bg-blue-600',
    description: 'You enjoy working across the entire stack, from frontend to backend and everything in between.',
    skills: ['React', 'ASP.NET Core', 'Databases', 'DevOps Basics', 'System Architecture'],
    salary: '$85K - $170K',
    growth: '+28%',
  },
  data: {
    name: 'Data Scientist',
    icon: '📊',
    color: 'bg-blue-600',
    description: 'You have a talent for analyzing data and extracting meaningful insights to drive decisions.',
    skills: ['SQL', 'Python', 'Data Visualization', 'Statistics', 'Machine Learning'],
    salary: '$65K - $130K',
    growth: '+20%',
  },
  ml: {
    name: 'ML Engineer',
    icon: '🤖',
    color: 'bg-yellow-500',
    description: 'You are fascinated by AI and machine learning, building systems that learn and improve.',
    skills: ['Python', 'TensorFlow/PyTorch', 'Mathematics', 'Data Processing', 'MLOps'],
    salary: '$100K - $200K',
    growth: '+35%',
  },
  devops: {
    name: 'DevOps Engineer',
    icon: '☁️',
    color: 'bg-slate-600',
    description: 'You love automating processes and managing infrastructure that keeps applications running smoothly.',
    skills: ['Docker/Kubernetes', 'CI/CD', 'Cloud Platforms', 'Linux', 'Monitoring'],
    salary: '$90K - $175K',
    growth: '+30%',
  },
};

const QuizPage = () => {
  const [currentQuestionData, setCurrentQuestionData] = useState(null);
  const [currentResponses, setCurrentResponses] = useState([]);
  const [currentSelection, setCurrentSelection] = useState(null);
  const [showResult, setShowResult] = useState(false);
  const [quizResult, setQuizResult] = useState(null);
  const [selectedDomain, setSelectedDomain] = useState(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();

  const [phaseKey, setPhaseKey] = useState('');
  const [step, setStep] = useState('selector'); // 'selector', 'quiz', 'result'

  const startQuiz = async (selectedPhaseKey) => {
    if (selectedPhaseKey === 'GovernmentJobs') {
      navigate('/government-jobs');
      return;
    }
    if (selectedPhaseKey === 'CareerSwitch') {
      navigate('/career-switch/pre-assessment');
      return;
    }

    try {
      setLoading(true);
      setPhaseKey(selectedPhaseKey);
      const question = await quizApi.getNextQuestion({ responses: [], phaseKey: selectedPhaseKey });
      setCurrentQuestionData(question);
      setStep('quiz');
    } catch (error) {
      console.error('Failed to fetch first quiz question:', error);
      toast.error('Failed to load quiz');
    } finally {
      setLoading(false);
    }
  };

  const progress = (currentResponses.length / 7) * 100; // Hardcoded to 7 as per backend

  const handleAnswer = (optionId) => {
    setCurrentSelection(optionId);
  };

  const submitQuiz = async (finalResponses) => {
    try {
      setSubmitting(true);
      const result = await quizApi.submit({ responses: finalResponses, phaseKey: phaseKey });
      setQuizResult(result);
      setSelectedDomain(result?.recommendedDomainId);
      setShowResult(true);
      setStep('result');
      toast.success('Quiz completed! Here\'s your career recommendation');
      return true;
    } catch (error) {
      console.error('Failed to submit quiz:', error);
      toast.error(error?.message || 'Failed to submit quiz');
      return false;
    } finally {
      setSubmitting(false);
    }
  };

  const handleNext = async () => {
    if (!currentSelection) {
      toast.error('Please select an answer');
      return;
    }

    try {
      setSubmitting(true);
      const newResponses = [...currentResponses, { questionId: currentQuestionData.id, optionId: currentSelection }];
      setCurrentResponses(newResponses);
      setCurrentSelection(null);

      const nextQuestion = await quizApi.getNextQuestion({ responses: newResponses, phaseKey: phaseKey });

      if (nextQuestion) {
        setCurrentQuestionData(nextQuestion);
      } else {
        // Quiz is over (7 questions reached or no more questions)
        const success = await submitQuiz(newResponses);
        if (!success) {
          // Rollback the local state so the user isn't stuck on "Question 8"
          setCurrentResponses(currentResponses);
        }
      }
    } catch (error) {
      console.error('Failed to load next question', error);
      toast.error('Error loading next question');
    } finally {
      setSubmitting(false);
    }
  };

  const handleConfirmResult = async () => {
    try {
      setSubmitting(true);
      const domainToSave = selectedDomain || quizResult.recommendedDomainId;
      await quizApi.confirm({ recommendedDomainId: domainToSave });
      toast.success('Skills and Domain saved to your profile!');
      navigate(`/roadmap?domain=${domainToSave}`);
    } catch (error) {
      console.error('Failed to confirm quiz result:', error);
      toast.error('Failed to save your result');
    } finally {
      setSubmitting(false);
    }
  };

  const getCareerInfo = (domainId) => {
    return careerResults[domainId] || careerResults.fullstack;
  };

  // Show loading state
  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin mx-auto text-accent" />
          <p className="mt-4 text-muted-foreground">Loading quiz questions...</p>
        </div>
      </div>
    );
  }

  if (step === 'selector') {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] px-4">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold mb-4">Choose Your Stage</h1>
          <p className="text-muted-foreground">Select your current academic or career stage to personalize the quiz.</p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 max-w-2xl w-full">
          {['After10th', 'After12th', 'AfterGraduation', 'AfterPostGraduation', 'CareerSwitch', 'GovernmentJobs'].map(phase => (
            <Button
              key={phase}
              variant="outline"
              className="h-auto py-6 text-lg justify-start px-6"
              onClick={() => startQuiz(phase)}
            >
              {phase.replace(/([A-Z0-9])/g, ' $1').trim()}
            </Button>
          ))}
        </div>
      </div>
    );
  }

  // No question loaded
  if (!currentQuestionData && step !== 'result') {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Card className="max-w-md">
          <CardContent className="p-6 text-center">
            <p className="text-muted-foreground mb-4">No quiz questions available</p>
            <Button onClick={() => window.location.reload()}>Reload</Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (step === 'result' && quizResult) {
    const career = getCareerInfo(quizResult.recommendedDomainId);
    // Get top 3 careers from scores
    let sortedCareers = Object.entries(quizResult.scores || {})
      .filter(([careerId]) => careerId !== quizResult.recommendedDomainId)
      .sort(([, a], [, b]) => b - a);

    // Pad with other careers if the backend didn't return them
    const otherCareers = Object.keys(careerResults).filter(id => id !== quizResult.recommendedDomainId).slice(0, 3);
    while (sortedCareers.length < 3 && otherCareers.length > 0) {
      const toAdd = otherCareers.shift();
      if (!sortedCareers.find(([id]) => id === toAdd)) {
        sortedCareers.push([toAdd, Math.floor(Math.random() * 30) + 50]);
      }
    }
    sortedCareers = sortedCareers.slice(0, 3);

    const maxScore = Math.max(...Object.values(quizResult.scores || {}), 100, ...sortedCareers.map(s => s[1]));

    return (
      <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="max-w-3xl mx-auto">
        <div className="text-center mb-8">
          <motion.div
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ type: 'spring', stiffness: 200, delay: 0.2 }}
            className="inline-flex h-24 w-24 items-center justify-center rounded-lg bg-blue-600 mb-6"
          >
            <Sparkles className="h-12 w-12 text-accent-foreground" />
          </motion.div>
          <h1 className="text-3xl font-bold text-foreground mb-2">Your Career Path Revealed! 🎉</h1>
          <p className="text-muted-foreground">Based on your answers, here's your personalized recommendation</p>
        </div>

        <motion.div initial={{ y: 20, opacity: 0 }} animate={{ y: 0, opacity: 1 }} transition={{ delay: 0.3 }}>
          <Card
            className={`shadow-elevated mb-6 overflow-hidden cursor-pointer transition-all ${selectedDomain === quizResult.recommendedDomainId ? 'border-2 border-accent ring-2 ring-accent/20' : 'border border-accent/20'}`}
            onClick={() => setSelectedDomain(quizResult.recommendedDomainId)}
          >
            <div className={`h-2 ${career.color}`} />
            <CardContent className="p-8">
              <div className="flex items-center gap-4 mb-6">
                <div className={`flex h-20 w-20 items-center justify-center rounded-lg ${career.color}/10`}>
                  <span className="text-5xl">{career.icon}</span>
                </div>
                <div>
                  <Badge className="mb-2 bg-accent/10 text-accent">Best Match</Badge>
                  <h2 className="text-2xl font-bold text-foreground">{career.name}</h2>
                </div>
              </div>

              <p className="text-lg text-muted-foreground mb-6">
                {quizResult.recommendationReason || career.description}
              </p>

              <div className="grid grid-cols-2 gap-6 mb-6">
                <div className="p-4 rounded-lg bg-secondary/50">
                  <p className="text-sm text-muted-foreground mb-1">Salary Range (Predicted)</p>
                  <p className="text-xl font-bold text-foreground">{quizResult.salaryRange || career.salary}</p>
                </div>
                <div className="p-4 rounded-lg bg-secondary/50 flex flex-col justify-end">
                  <p className="text-sm text-muted-foreground mb-1">5-Year Trend (StackOverflow)</p>
                  {quizResult.fiveYearTrend && Object.keys(quizResult.fiveYearTrend).length > 0 ? (
                    <div className="flex items-end gap-1 h-8 mt-2">
                      {Object.entries(quizResult.fiveYearTrend)
                        .sort(([a], [b]) => Number(a) - Number(b))
                        .map(([year, count]) => {
                          const maxCount = Math.max(...Object.values(quizResult.fiveYearTrend), 1);
                          const heightPercent = Math.max((count / maxCount) * 100, 5);
                          return (
                            <div key={year} className="group relative flex-1 flex flex-col justify-end h-full">
                              <div className="bg-accent/80 hover:bg-accent rounded-t-sm w-full transition-all" style={{ height: `${heightPercent}%` }}></div>
                              <div className="absolute opacity-0 group-hover:opacity-100 -top-8 left-1/2 -translate-x-1/2 bg-popover text-popover-foreground text-[10px] py-1 px-2 rounded pointer-events-none whitespace-nowrap z-10 transition-opacity">
                                {year}: {count.toLocaleString()}
                              </div>
                            </div>
                          );
                        })}
                    </div>
                  ) : (
                    <p className="text-xl font-bold text-success">{career.growth}</p>
                  )}
                </div>
              </div>

              <div>
                <p className="text-sm font-medium text-muted-foreground mb-3">Key Skills to Learn</p>
                <div className="flex flex-wrap gap-2">
                  {(quizResult.recommendedSkills?.length > 0 ? quizResult.recommendedSkills : career.skills).map((skill) => (
                    <Badge key={skill} variant="secondary" className="px-3 py-1">{skill}</Badge>
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>
        </motion.div>

        <motion.div initial={{ y: 20, opacity: 0 }} animate={{ y: 0, opacity: 1 }} transition={{ delay: 0.5 }}>
          <h3 className="text-lg font-semibold text-foreground mb-4">Other Great Matches</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
            {sortedCareers.map(([careerId, score]) => {
              const info = getCareerInfo(careerId);
              return (
                <Card
                  key={careerId}
                  className={`card-hover cursor-pointer transition-all ${selectedDomain === careerId ? 'border-2 border-accent ring-2 ring-accent/20' : 'border-border/50'}`}
                  onClick={() => setSelectedDomain(careerId)}
                >
                  <CardContent className="p-5">
                    <div className="flex items-center gap-3 mb-3">
                      <span className="text-3xl">{info.icon}</span>
                      <div>
                        <p className="font-medium text-foreground">{info.name}</p>
                        <p className="text-xs text-muted-foreground">{Math.round((score / maxScore) * 100)}% match</p>
                      </div>
                    </div>
                    <Progress value={(score / maxScore) * 100} className="h-2" />
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </motion.div>

        <div className="flex gap-4 justify-center mt-8">
          <Button variant="outline" size="lg" onClick={() => { setCurrentResponses([]); setCurrentSelection(null); setStep('selector'); setQuizResult(null); }} disabled={submitting}>
            <ArrowLeft className="mr-2 h-5 w-5" />
            Disagree (Retake)
          </Button>
          <Button size="lg" className="bg-blue-600 text-white" onClick={handleConfirmResult} disabled={submitting}>
            {submitting ? (
              <><Loader2 className="mr-2 h-5 w-5 animate-spin" />Saving...</>
            ) : (
              <>Agree & Save <CheckCircle className="ml-2 h-5 w-5" /></>
            )}
          </Button>
        </div>
      </motion.div>
    );
  }

  return (
    <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="max-w-2xl mx-auto">
      <div className="text-center mb-8">
        <div className="inline-flex h-12 w-12 items-center justify-center rounded-lg bg-accent/10 mb-4">
          <Lightbulb className="h-6 w-6 text-accent" />
        </div>
        <h1 className="text-2xl font-bold text-foreground mb-2">Discover Your Perfect Career</h1>
        <p className="text-muted-foreground">Answer a few questions to find your ideal tech career path</p>
      </div>

      <div className="mb-8">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm text-muted-foreground">Question {currentResponses.length + 1} of 7</span>
          <Badge variant="secondary">{currentQuestionData.category}</Badge>
        </div>
        <Progress value={progress} className="h-2" />
      </div>

      <AnimatePresence mode="wait">
        <motion.div key={currentQuestionData.id} initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -20 }} transition={{ duration: 0.3 }}>
          <Card className="shadow-soft border-border/50 mb-8">
            <CardContent className="p-8">
              <h2 className="text-xl font-semibold text-foreground mb-6">{currentQuestionData.text || currentQuestionData.question}</h2>

              <div className="space-y-3">
                {currentQuestionData?.options?.map((option) => (
                  <motion.button
                    key={option.id}
                    onClick={() => handleAnswer(option.id)}
                    whileHover={{ scale: 1.01 }}
                    whileTap={{ scale: 0.99 }}
                    className={`w-full p-4 rounded-lg border-2 text-left transition-all ${currentSelection === option.id ? 'border-accent bg-accent/5' : 'border-border hover:border-accent/50 hover:bg-secondary/50'}`}
                  >
                    <div className="flex items-center gap-3">
                      <div className={`flex h-8 w-8 items-center justify-center rounded-full border-2 transition-colors ${currentSelection === option.id ? 'border-accent bg-accent text-accent-foreground' : 'border-border'}`}>
                        {currentSelection === option.id && <CheckCircle className="h-5 w-5" />}
                      </div>
                      <span className="font-medium text-foreground">{option.text}</span>
                    </div>
                  </motion.button>
                ))}
              </div>
            </CardContent>
          </Card>
        </motion.div>
      </AnimatePresence>

      <div className="flex items-center justify-between mt-8">
        <div></div>
        <Button onClick={handleNext} className="bg-blue-600" disabled={submitting}>
          {submitting ? (
            <><Loader2 className="mr-2 h-5 w-5 animate-spin" /><span>Processing...</span></>
          ) : (
            <><span>Next</span><ArrowRight className="ml-2 h-5 w-5" /></>
          )}
        </Button>
      </div>
    </motion.div>
  );
};

export default QuizPage;

