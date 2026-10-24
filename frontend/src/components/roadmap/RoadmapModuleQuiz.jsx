import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { roadmapApi } from '@/services/api';
import { toast } from 'sonner';
import {
  CheckCircle2,
  XCircle,
  HelpCircle,
  Loader2,
  Award,
  RotateCcw,
  Sparkles,
  X
} from 'lucide-react';

export default function RoadmapModuleQuiz({ moduleId, moduleTitle, isOpen, onClose, onQuizPassed }) {
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [quizData, setQuizData] = useState(null);
  const [userAnswers, setUserAnswers] = useState({});
  const [evaluationResult, setEvaluationResult] = useState(null);

  useEffect(() => {
    if (isOpen && moduleId) {
      fetchQuiz();
    } else {
      setQuizData(null);
      setUserAnswers({});
      setEvaluationResult(null);
    }
  }, [isOpen, moduleId]);

  const fetchQuiz = async () => {
    try {
      setLoading(true);
      setEvaluationResult(null);
      setUserAnswers({});
      const res = await roadmapApi.getModuleQuiz(moduleId);
      setQuizData(res.data?.data);
    } catch (err) {
      toast.error(err.message || 'Failed to load module quiz.');
    } finally {
      setLoading(false);
    }
  };

  const handleOptionSelect = (questionIndex, optionIndex) => {
    if (evaluationResult) return;
    setUserAnswers(prev => ({
      ...prev,
      [questionIndex]: optionIndex
    }));
  };

  const handleSubmit = async () => {
    const questions = quizData?.questions || [];
    if (Object.keys(userAnswers).length < questions.length) {
      toast.error('Please answer all questions before submitting.');
      return;
    }

    const answersList = questions.map((_, idx) => userAnswers[idx] ?? -1);

    try {
      setSubmitting(true);
      const res = await roadmapApi.submitModuleQuiz(moduleId, answersList);
      const result = res.data?.data;
      setEvaluationResult(result);

      if (result?.passed) {
        toast.success(`Passed! You scored ${result.scorePercentage}% 🎉`);
        if (onQuizPassed) onQuizPassed();
      } else {
        toast.error(`Scored ${result?.scorePercentage}%. Need 70% to pass.`);
      }
    } catch (err) {
      toast.error(err.message || 'Quiz submission failed.');
    } finally {
      setSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 backdrop-blur-sm p-4 overflow-y-auto">
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        exit={{ opacity: 0, scale: 0.95 }}
        className="relative w-full max-w-2xl bg-white rounded-2xl shadow-2xl border border-slate-200 overflow-hidden my-8"
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 bg-slate-50 border-b border-slate-200">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-100 text-blue-600">
              <HelpCircle className="h-5 w-5" />
            </div>
            <div>
              <span className="text-xs font-bold uppercase tracking-wider text-blue-600">End-of-Module Quiz</span>
              <h3 className="text-lg font-bold text-slate-900 truncate max-w-md">{moduleTitle}</h3>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-200 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content Body */}
        <div className="p-6 max-h-[70vh] overflow-y-auto space-y-6">
          {loading ? (
            <div className="flex flex-col items-center justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin text-blue-600 mb-3" />
              <p className="text-sm font-medium text-slate-500">Preparing your assessment questions...</p>
            </div>
          ) : evaluationResult ? (
            /* Result Banner & Review */
            <div className="space-y-6">
              <div className={`p-6 rounded-xl border text-center ${
                evaluationResult.passed
                  ? 'bg-emerald-50/60 border-emerald-200 text-emerald-900'
                  : 'bg-rose-50/60 border-rose-200 text-rose-900'
              }`}>
                <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-white shadow-md mb-3">
                  {evaluationResult.passed ? (
                    <Award className="h-8 w-8 text-emerald-600" />
                  ) : (
                    <XCircle className="h-8 w-8 text-rose-600" />
                  )}
                </div>
                <h4 className="text-2xl font-black mb-1">
                  {evaluationResult.passed ? 'Module Passed!' : 'Try Again'}
                </h4>
                <p className="text-sm font-semibold mb-3">
                  You scored <span className="text-xl font-bold">{evaluationResult.scorePercentage}%</span> ({evaluationResult.correctCount}/{evaluationResult.totalQuestions} Correct)
                </p>
                <p className="text-xs max-w-md mx-auto opacity-90">{evaluationResult.message}</p>
              </div>

              <div className="space-y-4">
                <h5 className="text-xs font-bold text-slate-400 uppercase tracking-wider">Question Breakdown & Explanations</h5>
                {evaluationResult.evaluatedQuestions?.map((q, qIdx) => {
                  const userChoice = userAnswers[qIdx];
                  const isCorrect = userChoice === q.correctOptionIndex;

                  return (
                    <div key={q.questionId || qIdx} className="p-4 rounded-xl border border-slate-200 bg-slate-50/50 space-y-3">
                      <div className="flex items-start gap-2">
                        {isCorrect ? (
                          <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0 mt-0.5" />
                        ) : (
                          <XCircle className="w-5 h-5 text-rose-600 shrink-0 mt-0.5" />
                        )}
                        <p className="text-sm font-bold text-slate-900">{qIdx + 1}. {q.questionText}</p>
                      </div>

                      <div className="grid grid-cols-1 gap-2 pl-7">
                        {q.options?.map((opt, optIdx) => {
                          let optStyle = 'bg-white border-slate-200 text-slate-700';
                          if (optIdx === q.correctOptionIndex) {
                            optStyle = 'bg-emerald-100 border-emerald-300 text-emerald-900 font-medium';
                          } else if (optIdx === userChoice && !isCorrect) {
                            optStyle = 'bg-rose-100 border-rose-300 text-rose-900';
                          }

                          return (
                            <div key={optIdx} className={`px-3 py-2 rounded-lg border text-xs flex items-center justify-between ${optStyle}`}>
                              <span>{opt}</span>
                              {optIdx === q.correctOptionIndex && <span className="text-[10px] font-bold text-emerald-700 uppercase">Correct Answer</span>}
                              {optIdx === userChoice && !isCorrect && <span className="text-[10px] font-bold text-rose-700 uppercase">Your Choice</span>}
                            </div>
                          );
                        })}
                      </div>

                      {q.explanation && (
                        <p className="text-xs text-slate-600 bg-blue-50/50 p-2.5 rounded-lg border border-blue-100 pl-7">
                          <span className="font-semibold text-blue-700">Explanation:</span> {q.explanation}
                        </p>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          ) : (
            /* Question Stream */
            <div className="space-y-6">
              {quizData?.questions?.map((q, qIdx) => (
                <div key={q.questionId || qIdx} className="p-4 rounded-xl border border-slate-200 bg-slate-50/50">
                  <p className="text-sm font-bold text-slate-900 mb-3">
                    <span className="text-blue-600 mr-2">Q{qIdx + 1}.</span> {q.questionText}
                  </p>

                  <div className="space-y-2">
                    {q.options?.map((opt, optIdx) => {
                      const isSelected = userAnswers[qIdx] === optIdx;
                      return (
                        <button
                          key={optIdx}
                          type="button"
                          onClick={() => handleOptionSelect(qIdx, optIdx)}
                          className={`w-full text-left p-3 rounded-lg border text-xs font-medium transition-all ${
                            isSelected
                              ? 'bg-blue-50 border-blue-600 text-blue-900 ring-1 ring-blue-600 shadow-sm'
                              : 'bg-white border-slate-200 text-slate-700 hover:border-slate-300 hover:bg-slate-100/50'
                          }`}
                        >
                          <div className="flex items-center gap-3">
                            <div className={`w-4 h-4 rounded-full border flex items-center justify-center shrink-0 ${
                              isSelected ? 'border-blue-600 bg-blue-600 text-white' : 'border-slate-300'
                            }`}>
                              {isSelected && <div className="w-1.5 h-1.5 rounded-full bg-white" />}
                            </div>
                            <span>{opt}</span>
                          </div>
                        </button>
                      );
                    })}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 bg-slate-50 border-t border-slate-200 flex items-center justify-between">
          {evaluationResult ? (
            <div className="flex items-center gap-3 ml-auto">
              {!evaluationResult.passed && (
                <Button variant="outline" onClick={fetchQuiz} className="border-slate-300">
                  <RotateCcw className="w-4 h-4 mr-2" /> Retake Quiz
                </Button>
              )}
              <Button onClick={onClose} className="bg-blue-600 text-white hover:bg-blue-700">
                {evaluationResult.passed ? 'Continue Roadmap' : 'Close'}
              </Button>
            </div>
          ) : (
            <div className="flex items-center justify-between w-full">
              <span className="text-xs font-medium text-slate-500">
                {Object.keys(userAnswers).length} of {quizData?.questions?.length || 0} Answered
              </span>
              <div className="flex gap-2">
                <Button variant="ghost" onClick={onClose}>Cancel</Button>
                <Button
                  onClick={handleSubmit}
                  disabled={submitting || loading || Object.keys(userAnswers).length < (quizData?.questions?.length || 0)}
                  className="bg-blue-600 hover:bg-blue-700 text-white"
                >
                  {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Sparkles className="w-4 h-4 mr-2" />}
                  Submit & Grade Quiz
                </Button>
              </div>
            </div>
          )}
        </div>
      </motion.div>
    </div>
  );
}
