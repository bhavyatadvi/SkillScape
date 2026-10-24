import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { BookOpen, ChevronRight, Compass, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/context/AuthContext";
import { userStorageKey } from "@/lib/userStorage";

const read = (key) => { try { return JSON.parse(window.localStorage.getItem(key) || "null"); } catch { return null; } };
const readList = (key) => { try { return JSON.parse(window.localStorage.getItem(key) || "[]"); } catch { return []; } };

export default function LearningPlans() {
  const { user } = useAuth();
  const [quizPlans, setQuizPlans] = useState([]);
  const [govData, setGovData] = useState(null);
  const plansKey = userStorageKey("skillscape-quiz-learning-plans", user);

  useEffect(() => {
    setQuizPlans(readList(plansKey));
    setGovData(read(userStorageKey("skillscape-government-learning", user)));
  }, [plansKey, user]);

  const removeQuizPlan = (idx) => {
    const updated = quizPlans.filter((_, i) => i !== idx);
    window.localStorage.setItem(plansKey, JSON.stringify(updated));
    setQuizPlans(updated);
  };

  const govPlan = govData?.selectedCareer ? [{
    type: "Government Jobs",
    data: govData,
    link: "/government-jobs/results",
    removable: false,
  }] : [];

  const quizPlansList = quizPlans
    .filter((p) => !(govData?.selectedCareer && p.assessmentType === 'Government Jobs' && p.selectedCareer?.id === govData.selectedCareer?.id))
    .map((p, i) => ({
    type: p.assessmentType || "Career Assessment",
    data: p,
    link: p.assessmentType === 'Skill Analyzer' 
      ? '/roadmap' 
      : `/roadmap?domain=${encodeURIComponent(p.selectedCareer?.id || p.selectedCareer?.title || '')}`,
    removable: true,
    idx: i,
  }));

  const plans = [...govPlan, ...quizPlansList].sort(
    (a, b) => new Date(b.data.savedAt || 0) - new Date(a.data.savedAt || 0)
  );

  return (
    <div className="max-w-6xl mx-auto pb-12">
      <p className="text-xs font-semibold tracking-widest uppercase text-[#94A3B8]">Learning</p>
      <h1 className="text-[32px] font-bold tracking-tight text-[#0F172A] mt-2">My learning plans</h1>
      <p className="text-[#64748B] mt-2">Review your saved quiz results and choose the plan you want to pursue.</p>
      {plans.length ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-7">
          {plans.map((plan, index) => {
            const career = plan.data.selectedCareer;
            if (!career) return null;
            const name = career.title || career.name || career.careerName || "Learning recommendation";
            const score = career.matchScore || career.compatibilityScore || career.score;
            const exam = career.exam;
            const salary = career.expectedSalaryRange || career.expectedSalaryLpa || career.salaryMin;
            const duration = career.expectedTimeToJobReady || career.typicalDurationMonths || career.preparationMonths;
            return (
              <div key={plan.type + index} className="bg-white border border-[#E2E8F0] rounded-[10px] p-5">
                <div className="flex items-start justify-between gap-4">
                  <div className="flex-1">
                    <p className="text-xs font-semibold uppercase tracking-widest text-[#94A3B8]">{plan.type}</p>
                    <h2 className="text-lg font-bold text-[#0F172A] mt-2">{name}</h2>
                    {exam && <p className="text-sm text-[#64748B] mt-1">{exam}</p>}
                    <div className="flex flex-wrap gap-2 mt-2">
                      {salary && <span className="text-xs bg-green-50 border border-green-200 text-green-700 px-2 py-1 rounded-full font-medium">💰 {typeof salary === 'number' ? `₹${salary}+ LPA` : salary}</span>}
                      {duration && <span className="text-xs bg-blue-50 border border-blue-200 text-blue-700 px-2 py-1 rounded-full font-medium">⏱ {typeof duration === 'number' ? `${duration} months` : duration}</span>}
                    </div>
                  </div>
                  <div className="flex flex-col items-end gap-2">
                    {score ? <span className="text-sm font-semibold text-[#2563EB] bg-[#EFF6FF] border border-[#DBEAFE] rounded px-2 py-1">{Math.round(score)}%</span> : null}
                    {plan.removable && (
                      <button onClick={() => removeQuizPlan(plan.idx)} className="text-slate-400 hover:text-red-500 transition-colors">
                        <Trash2 className="w-4 h-4" />
                      </button>
                    )}
                  </div>
                </div>
                <Link to={plan.link} state={plan.removable ? { savedPlan: plan.data } : undefined}>
                  <Button variant="outline" className="mt-4 border-[#E2E8F0] text-[#2563EB] hover:bg-[#EFF6FF] font-semibold">
                    Open Learning Roadmap <ChevronRight className="w-4 h-4 ml-2" />
                  </Button>
                </Link>
                {index === 0 && <p className="text-xs text-[#2563EB] mt-3">Current path on your Dashboard</p>}
              </div>
            );
          })}
        </div>
      ) : (
        <div className="bg-white border border-[#E2E8F0] rounded-[10px] p-8 text-center mt-7">
          <Compass className="w-8 h-8 text-[#2563EB] mx-auto mb-3" />
          <h2 className="font-bold text-[#0F172A]">No saved learning plans yet</h2>
          <p className="text-sm text-[#64748B] mt-2">Complete a quiz or a skill analysis and add its result to your roadmap to see it here.</p>
          <Link to="/placement">
            <Button className="mt-5 bg-[#2563EB]">
              <BookOpen className="w-4 h-4 mr-2" />Analyze your skills
            </Button>
          </Link>
        </div>
      )}
    </div>
  );
}
