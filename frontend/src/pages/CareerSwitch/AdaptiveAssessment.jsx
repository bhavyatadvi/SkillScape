import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { ArrowLeft, Brain } from 'lucide-react';
import { useCareerProfile } from '@/context/CareerProfileContext';
import { useAuth } from '@/context/AuthContext';
import { userStorageKey } from '@/lib/userStorage';
import { generateCandidateCareers, detectCurrentDomain } from '@/data/careerGraph';
import { ADAPTIVE_QUESTIONS, selectNextQuestion } from '@/data/adaptiveQuestionBank';
import {
  initializeDimensionScores,
  applyAnswerToScores,
  applyMultipleAnswers,
  rankCareers,
  getConfidenceLevel,
} from '@/data/scoringEngine';

const MIN_QUESTIONS = 6;
const MAX_QUESTIONS = 10;
const HIGH_CONFIDENCE_THRESHOLD = 82;
const CONFIDENCE_GAP_THRESHOLD = 18;

export default function AdaptiveAssessment() {
  const navigate = useNavigate();
  const location = useLocation();
  const { careerProfile: contextProfile } = useCareerProfile();
  const { user } = useAuth();

  const preQuizAnswers = location.state?.preQuizAnswers || {};
  const careerProfile = location.state?.careerProfile || contextProfile;

  const [askedIds, setAskedIds] = useState([]);
  const [currentQuestion, setCurrentQuestion] = useState(null);
  const [selectedOptions, setSelectedOptions] = useState([]);
  const [dimensionScores, setDimensionScores] = useState(initializeDimensionScores());
  const [candidateCareers, setCandidateCareers] = useState([]);
  const [rejectedDimensions, setRejectedDimensions] = useState([]);
  const [currentDomain, setCurrentDomain] = useState('general');
  const [questionNumber, setQuestionNumber] = useState(1);
  const [isFinishing, setIsFinishing] = useState(false);
  const [loadingStep, setLoadingStep] = useState(0);
  const [assessmentAnswers, setAssessmentAnswers] = useState([]);

  const loadingMessages = [
    'Analyzing your profile…',
    'Comparing career paths…',
    'Mapping your transferable skills…',
    'Checking skill gaps…',
    'Building your transition roadmap…',
  ];

  useEffect(() => {
    const domain = detectCurrentDomain(careerProfile);
    setCurrentDomain(domain);
    const careers = generateCandidateCareers(careerProfile);
    setCandidateCareers(careers);

    // Select first question
    const rejectedFromProfile = (careerProfile?.constraints || []).includes('Heavy coding') ? ['codingIntensity'] : [];
    const rejectedFromPreAssessment = (preQuizAnswers?.[4] || []).includes('Heavy coding') ? ['codingIntensity'] : [];
    const initialRejections = [...new Set([...rejectedFromProfile, ...rejectedFromPreAssessment])];
    setRejectedDimensions(initialRejections);
    const first = selectNextQuestion([], {}, domain, [], initialRejections);
    setCurrentQuestion(first);
    if (first) setAskedIds([first.id]);
  }, [careerProfile]);

  // Apply pre-quiz answers to dimension scores at start
  useEffect(() => {
    let scores = initializeDimensionScores();

    // Map pre-quiz interests to dimensions
    const interests = preQuizAnswers?.[5] || [];
    const interestToDim = {
      'Analyzing data': { dataAnalytics: 3 },
      'Research': { research: 3 },
      'Managing projects': { projectManagement: 3 },
      'Leading teams': { leadership: 3 },
      'Business strategy': { businessStrategy: 3 },
      'Healthcare': { healthcare: 5 },
      'Teaching': { peopleOrientation: 3 },
      'Working with technology': { technicalDepth: 3, codingIntensity: 2 },
      'Solving technical problems': { technicalDepth: 3 },
      'Finance': { finance: 3 },
    };
    for (const interest of interests) {
      const dimBoost = interestToDim[interest];
      if (dimBoost) {
        for (const [dim, val] of Object.entries(dimBoost)) {
          scores[dim] = (scores[dim] || 0) + val;
        }
      }
    }

    // Map constraints (not wanted)
    const notWanted = preQuizAnswers?.[4] || [];
    if (notWanted.includes('Heavy coding')) scores.codingIntensity -= 5;
    if (notWanted.includes('Sales')) scores.peopleOrientation -= 2;

    setDimensionScores(scores);
  }, [preQuizAnswers]);

  const handleOptionToggle = (option) => {
    if (!currentQuestion) return;
    if (currentQuestion.allowMultiple) {
      setSelectedOptions(prev =>
        prev.some(o => o.text === option.text)
          ? prev.filter(o => o.text !== option.text)
          : [...prev, option]
      );
    } else {
      setSelectedOptions([option]);
    }
  };

  const handleSubmitAnswer = () => {
    if (selectedOptions.length === 0) return;

    // Update dimension scores
    let updatedScores;
    if (currentQuestion.allowMultiple) {
      updatedScores = applyMultipleAnswers(dimensionScores, selectedOptions, currentQuestion.weight || 1.0);
    } else {
      updatedScores = applyAnswerToScores(dimensionScores, selectedOptions[0], currentQuestion.weight || 1.0);
    }
    setDimensionScores(updatedScores);

    const answerRejections = selectedOptions.flatMap(option => [
      ...(option.rejectsDimension ? [option.rejectsDimension] : []),
      ...(option.isNegative ? Object.keys(option.penalizeDimensions || {}) : []),
    ]);
    const nextRejectedDimensions = [...new Set([...rejectedDimensions, ...answerRejections])];
    setRejectedDimensions(nextRejectedDimensions);

    const newAskedIds = [...askedIds, currentQuestion.id];
    const newAssessmentAnswers = [...assessmentAnswers, {
      id: currentQuestion.id,
      question: currentQuestion.question,
      answers: selectedOptions.map(option => option.text),
    }];
    setAssessmentAnswers(newAssessmentAnswers);
    setAskedIds(newAskedIds);
    setSelectedOptions([]);

    const ranked = rankCareers(candidateCareers, updatedScores, preQuizAnswers, careerProfile);
    const topScore = ranked[0]?.compatibilityScore || 0;
    const secondScore = ranked[1]?.compatibilityScore || 0;
    const gap = topScore - secondScore;
    const confidence = getConfidenceLevel(ranked);

    // Check stopping condition
    const questionsAnswered = newAskedIds.length;
    const shouldStop =
      questionsAnswered >= MIN_QUESTIONS &&
      (confidence === 'High' || confidence === 'Moderate' || (topScore >= HIGH_CONFIDENCE_THRESHOLD && gap >= CONFIDENCE_GAP_THRESHOLD));

    if (shouldStop) {
      // Finish assessment
      setIsFinishing(true);
      let step = 0;
      const interval = setInterval(() => {
        step++;
        setLoadingStep(step);
        if (step >= loadingMessages.length) {
          clearInterval(interval);
          window.localStorage.setItem(userStorageKey('skillscape-career-switch-assessment', user), JSON.stringify({
            savedAt: new Date().toISOString(), preQuizAnswers, assessmentAnswers: newAssessmentAnswers,
            rankedCareers: ranked, dimensionScores: updatedScores, careerProfile, confidence,
          }));
          navigate('/career-switch/results', {
            state: {
              rankedCareers: ranked,
              dimensionScores: updatedScores,
              preQuizAnswers,
              careerProfile,
              confidence,
            }
          });
        }
      }, 800);
    } else {
      // Select next question
      const nextQ = selectNextQuestion(newAskedIds, updatedScores, currentDomain, [], nextRejectedDimensions);
      if (!nextQ || questionsAnswered >= MAX_QUESTIONS) {
        // Force finish after MAX_QUESTIONS questions
        setIsFinishing(true);
        let step = 0;
        const interval = setInterval(() => {
          step++;
          setLoadingStep(step);
        if (step >= loadingMessages.length) {
          clearInterval(interval);
          window.localStorage.setItem(userStorageKey('skillscape-career-switch-assessment', user), JSON.stringify({
            savedAt: new Date().toISOString(), preQuizAnswers, assessmentAnswers: newAssessmentAnswers,
            rankedCareers: ranked, dimensionScores: updatedScores, careerProfile, confidence,
          }));
          navigate('/career-switch/results', {
              state: {
                rankedCareers: ranked,
                dimensionScores: updatedScores,
                preQuizAnswers,
                careerProfile,
                confidence,
              }
            });
          }
        }, 800);
      } else {
        setCurrentQuestion(nextQ);
        setQuestionNumber(prev => prev + 1);
      }
    }
  };

  if (isFinishing) {
    return (
      <div className="min-h-screen pt-24 pb-12 px-4 bg-slate-50 flex items-center justify-center">
        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          className="bg-white rounded-2xl shadow-lg border border-slate-200 p-12 max-w-md w-full text-center"
        >
          <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <Brain className="w-8 h-8 text-blue-600 animate-pulse" />
          </div>
          <h2 className="text-2xl font-bold text-slate-900 mb-6">Processing your results…</h2>
          <div className="space-y-3">
            {loadingMessages.map((msg, idx) => (
              <div
                key={idx}
                className={`flex items-center gap-3 p-3 rounded-lg transition-all ${
                  idx <= loadingStep ? 'bg-blue-50 text-blue-800' : 'bg-slate-50 text-slate-400'
                }`}
              >
                <div className={`w-2 h-2 rounded-full flex-shrink-0 ${idx <= loadingStep ? 'bg-blue-600' : 'bg-slate-300'}`} />
                <span className="text-sm font-medium">{msg}</span>
              </div>
            ))}
          </div>
        </motion.div>
      </div>
    );
  }

  if (!currentQuestion) {
    return (
      <div className="min-h-screen pt-24 pb-12 px-4 bg-slate-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-slate-600">Loading assessment…</p>
        </div>
      </div>
    );
  }

  const progress = Math.min((questionNumber / (MIN_QUESTIONS + 5)) * 100, 95);

  return (
    <div className="min-h-screen pt-24 pb-12 px-4 bg-slate-50">
      <div className="max-w-5xl lg:max-w-6xl mx-auto">

        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-lg font-bold text-slate-800">Career Switch Assessment</h1>
            <p className="text-sm text-slate-500">Adaptive Assessment · Question {questionNumber} of 15+</p>
          </div>
          <div className="bg-blue-50 text-blue-700 text-sm font-semibold px-4 py-2 rounded-full border border-blue-100">
            Q{questionNumber}
          </div>
        </div>

        {/* Progress Bar */}
        <div className="w-full bg-slate-200 h-2 rounded-full overflow-hidden mb-8">
          <motion.div
            className="bg-gradient-to-r from-blue-500 to-violet-500 h-full rounded-full"
            initial={{ width: 0 }}
            animate={{ width: `${progress}%` }}
            transition={{ duration: 0.4 }}
          />
        </div>

        <AnimatePresence mode="wait">
          <motion.div
            key={currentQuestion.id}
            initial={{ opacity: 0, x: 30 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -30 }}
            transition={{ duration: 0.25 }}
            className="bg-white rounded-2xl shadow-sm border border-slate-200 p-8"
          >
            <Button
              variant="ghost"
              className="mb-6 -ml-4"
              onClick={() => navigate('/quiz')}
            >
              <ArrowLeft className="w-4 h-4 mr-2" /> Back to Career Quiz
            </Button>

            <h2 className="text-xl font-bold text-slate-900 mb-2 leading-snug">
              {currentQuestion.question}
            </h2>
            {currentQuestion.allowMultiple && (
              <p className="text-sm text-slate-500 mb-6">Select all that apply</p>
            )}
            {!currentQuestion.allowMultiple && (
              <p className="text-sm text-slate-500 mb-6">Choose the best answer</p>
            )}

            <div className={`grid gap-3 ${currentQuestion.options.length > 5 ? 'grid-cols-1 sm:grid-cols-2' : 'grid-cols-1'}`}>
              {currentQuestion.options.map((option, idx) => {
                const isSelected = selectedOptions.some(o => o.text === option.text);
                return (
                  <motion.div
                    key={idx}
                    whileTap={{ scale: 0.98 }}
                    className={`p-4 rounded-xl border-2 cursor-pointer transition-all flex items-start gap-3 ${
                      isSelected
                        ? 'border-blue-500 bg-blue-50/60 shadow-sm'
                        : option.isNegative
                        ? 'border-slate-100 bg-slate-50 hover:border-red-200 hover:bg-red-50/30'
                        : 'border-slate-100 hover:border-blue-200 hover:bg-slate-50'
                    }`}
                    onClick={() => handleOptionToggle(option)}
                  >
                    {currentQuestion.allowMultiple ? (
                      <Checkbox checked={isSelected} className="mt-0.5 pointer-events-none flex-shrink-0" />
                    ) : (
                      <div className={`w-5 h-5 rounded-full border-2 flex-shrink-0 mt-0.5 flex items-center justify-center transition-colors ${
                        isSelected ? 'border-blue-500 bg-blue-500' : 'border-slate-300'
                      }`}>
                        {isSelected && <div className="w-2 h-2 rounded-full bg-white" />}
                      </div>
                    )}
                    <span className={`text-sm font-medium leading-snug ${
                      isSelected ? 'text-blue-900' : option.isNegative ? 'text-slate-500' : 'text-slate-700'
                    }`}>
                      {option.text}
                    </span>
                  </motion.div>
                );
              })}
            </div>

            <div className="mt-8 flex justify-end">
              <Button
                size="lg"
                className="bg-blue-600 hover:bg-blue-700 text-white shadow-md shadow-blue-200/50 px-10"
                onClick={handleSubmitAnswer}
                disabled={selectedOptions.length === 0}
              >
                {questionNumber >= MIN_QUESTIONS ? 'Submit & See Results' : 'Next Question →'}
              </Button>
            </div>
          </motion.div>
        </AnimatePresence>

      </div>
    </div>
  );
}
