import React, { useRef, useState } from 'react';
import { motion } from 'framer-motion';
import { Button } from '@/components/ui/button';
import {
  Bookmark,
  Star,
  Trash2,
  RotateCcw,
  Plus,
  CheckCircle2,
  Sparkles,
  Map,
  ArrowRightLeft,
  Brain,
} from 'lucide-react';

const normalizeKey = (val) => {
  if (!val) return '';
  return String(val)
    .toLowerCase()
    .replace(/^career[-_]?/, '')
    .replace(/[^a-z0-9]/g, '');
};

const isCareerMatch = (item, careerObj) => {
  if (!item || !careerObj) return false;
  const itemKeyNorm = normalizeKey(item.careerKey || item.domainId || item.id);
  const itemTitleNorm = normalizeKey(item.careerTitle || item.domainName || item.title);

  const careerKeyNorm = normalizeKey(careerObj.id || careerObj.careerKey || careerObj.domainId);
  const careerTitleNorm = normalizeKey(careerObj.title || careerObj.careerTitle || careerObj.name);

  return Boolean(
    (careerKeyNorm && (itemKeyNorm === careerKeyNorm || itemTitleNorm === careerKeyNorm)) ||
    (careerTitleNorm && (itemKeyNorm === careerTitleNorm || itemTitleNorm === careerTitleNorm))
  );
};

export default function MyRoadmapsSidebar({
  savedRoadmaps = [],
  availableRoadmaps = [],
  selectedDomainId,
  careerSwitchPath = null,
  skillAnalyzerPlan = null,
  quizLearningPlans = [],
  onSelectRoadmap,
  onSaveRoadmap,
  onRemoveRoadmap,
  onSetDefaultRoadmap,
  onRegenerateRoadmap,
  onOpenAskAI
}) {
  // Local pending key — set immediately on click so the UI responds instantly
  // without waiting for parent re-renders from the async API call.
  const [pendingKey, setPendingKey] = useState(null);
  const isBusyRef = useRef(false);

  const checkCareerSwitch = (item) => {
    return (
      isCareerMatch(item, careerSwitchPath?.selectedCareer) ||
      quizLearningPlans.some(
        (p) => p.assessmentType === 'Career Switch' && isCareerMatch(item, p.selectedCareer)
      )
    );
  };

  const checkSkillAnalyzer = (item) => {
    return (
      isCareerMatch(item, skillAnalyzerPlan?.selectedCareer) ||
      quizLearningPlans.some(
        (p) => p.assessmentType === 'Skill Analyzer' && isCareerMatch(item, p.selectedCareer)
      )
    );
  };

  const hasCareerSwitchRoadmap = savedRoadmaps.some(checkCareerSwitch);
  const hasSkillAnalyzerRoadmap = savedRoadmaps.some(checkSkillAnalyzer);

  const handleSelect = async (careerKey) => {
    if (isBusyRef.current) return;
    isBusyRef.current = true;
    setPendingKey(careerKey);
    try {
      await onSelectRoadmap(careerKey);
    } finally {
      isBusyRef.current = false;
      setPendingKey(null);
    }
  };

  return (
    <div className="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm">

      <div className="flex flex-col lg:flex-row gap-8">

        {/* Saved Roadmaps Section */}
        <div className="flex-1 space-y-4">
          <div className="flex items-center justify-between pb-3 border-b border-slate-100">
            <div className="flex items-center gap-2">
              <Bookmark className="w-4 h-4 text-blue-600" />
              <h3 className="font-bold text-slate-900 text-sm">My Roadmaps</h3>
            </div>
            <div className="flex items-center gap-2">
              {hasCareerSwitchRoadmap && (
                <span
                  className="hidden sm:inline-flex items-center gap-1 text-[10px] font-semibold px-2 py-0.5 rounded-full bg-indigo-50 text-indigo-700 border border-indigo-100"
                  title="Accepted career switch path"
                >
                  <ArrowRightLeft className="w-2.5 h-2.5" />
                  <span>Career Switch</span>
                </span>
              )}
              {hasSkillAnalyzerRoadmap && (
                <span
                  className="hidden sm:inline-flex items-center gap-1 text-[10px] font-semibold px-2 py-0.5 rounded-full bg-purple-50 text-purple-700 border border-purple-100"
                  title="Saved skill analyzer roadmap"
                >
                  <Brain className="w-2.5 h-2.5" />
                  <span>Skill Analyzer</span>
                </span>
              )}
              <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-blue-50 text-blue-700 border border-blue-100 uppercase tracking-widest">
                {savedRoadmaps.length} Saved
              </span>
            </div>
          </div>

          {savedRoadmaps.length === 0 ? (
            <div className="py-8 text-center bg-slate-50/50 rounded-xl border border-dashed border-slate-200">
              <p className="text-sm font-medium text-slate-500">No saved roadmaps yet.</p>
              <p className="text-xs text-slate-400 mt-1">Add from Available Templates →</p>
            </div>
          ) : (
            <div className="flex flex-wrap gap-2.5 pt-1">
              {savedRoadmaps.map((item) => {
                // pendingKey gives instant visual feedback before parent re-renders
                const effectiveSelected = pendingKey ?? selectedDomainId;
                const isSelected =
                  item.careerKey === effectiveSelected ||
                  (item.careerKey &&
                    effectiveSelected &&
                    item.careerKey.replace(/_/g, '-') === effectiveSelected.replace(/_/g, '-'));
                const progress = Math.round(item.progressPercentage || 0);
                const isComplete = progress >= 100;
                const isCareerSwitch = checkCareerSwitch(item);
                const isSkillAnalyzer = checkSkillAnalyzer(item);

                const badgeTooltip = `${item.careerTitle}${
                  isCareerSwitch
                    ? ' · Accepted career switch path'
                    : isSkillAnalyzer
                      ? ' · Saved skill analyzer roadmap'
                      : ''
                }`;

                return (
                  // Plain div with CSS transitions — avoids framer-motion animation
                  // conflicts that caused rapid re-renders and visual blinking.
                  <div
                    key={item.id || item.careerKey}
                    role="button"
                    tabIndex={0}
                    className={`group relative overflow-hidden cursor-pointer rounded-full border transition-colors duration-150 select-none ${
                      isSelected
                        ? 'border-blue-400 shadow-md shadow-blue-100'
                        : isComplete
                          ? 'border-emerald-300'
                          : isCareerSwitch
                            ? 'border-indigo-200 hover:border-indigo-400 hover:shadow-sm'
                            : isSkillAnalyzer
                              ? 'border-purple-200 hover:border-purple-400 hover:shadow-sm'
                              : 'border-slate-200 hover:border-blue-300 hover:shadow-sm'
                    }`}
                    onClick={() => handleSelect(item.careerKey)}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        handleSelect(item.careerKey);
                      }
                    }}
                    title={badgeTooltip}
                  >
                    {/* Progress fill background */}
                    <div
                      className="absolute inset-0"
                      style={{
                        width: `${progress}%`,
                        background: isComplete
                          ? 'linear-gradient(90deg, rgba(16,185,129,0.35), rgba(16,185,129,0.15))'
                          : isSelected
                            ? 'linear-gradient(90deg, rgba(37,99,235,0.35), rgba(99,102,241,0.15))'
                            : 'linear-gradient(90deg, rgba(148,163,184,0.35), rgba(148,163,184,0.15))',
                      }}
                    />

                    {/* Badge content */}
                    <div className="relative flex items-center gap-2 px-3.5 py-2 pr-20">
                      {/* Status icon */}
                      {isComplete ? (
                        <CheckCircle2 className="w-3.5 h-3.5 shrink-0 text-emerald-500" />
                      ) : item.isDefault ? (
                        <Star className="w-3.5 h-3.5 shrink-0 fill-amber-400 text-amber-400" />
                      ) : (
                        <div className={`w-2 h-2 rounded-full shrink-0 ${isSelected ? 'bg-blue-500' : 'bg-slate-300'}`} />
                      )}

                      {/* Title */}
                      <span className={`text-xs font-semibold whitespace-nowrap max-w-[150px] truncate leading-none ${
                        isSelected
                          ? 'text-blue-800'
                          : isComplete
                            ? 'text-emerald-800'
                            : 'text-slate-700'
                      }`}>
                        {item.careerTitle}
                      </span>

                      {/* Source Indicator Icon on the side of the roadmap badge */}
                      {isCareerSwitch && (
                        <span
                          className="inline-flex items-center justify-center w-4 h-4 rounded-full bg-indigo-50 text-indigo-600 border border-indigo-200/80 shrink-0"
                          title="Accepted career switch path"
                        >
                          <ArrowRightLeft className="w-2.5 h-2.5" />
                        </span>
                      )}

                      {isSkillAnalyzer && (
                        <span
                          className="inline-flex items-center justify-center w-4 h-4 rounded-full bg-purple-50 text-purple-600 border border-purple-200/80 shrink-0"
                          title="Saved skill analyzer roadmap"
                        >
                          <Brain className="w-2.5 h-2.5" />
                        </span>
                      )}

                      {/* Progress % */}
                      <span className={`text-[10px] font-bold ml-0.5 ${
                        isComplete ? 'text-emerald-600' : isSelected ? 'text-blue-600' : 'text-slate-400'
                      }`}>
                        {progress}%
                      </span>

                      {/* Keep actions out of the flex layout so hover never resizes
                          the chip or moves neighboring chips under the pointer. */}
                      <div
                        className={`absolute right-1 top-1/2 z-10 flex -translate-y-1/2 items-center gap-0.5 rounded-full bg-white/95 px-1 shadow-sm transition-opacity duration-150 ${
                          isSelected
                            ? 'opacity-100'
                            : 'pointer-events-none opacity-0 group-hover:pointer-events-auto group-hover:opacity-100 group-focus-within:pointer-events-auto group-focus-within:opacity-100'
                        }`}
                        onClick={(e) => e.stopPropagation()}
                      >
                        {!item.isDefault && (
                          <button
                            type="button"
                            onClick={() => onSetDefaultRoadmap(item.careerKey)}
                            className="p-1 rounded-full text-amber-500 hover:bg-amber-50 transition-colors"
                            title="Set as Default"
                          >
                            <Star className="w-3 h-3" />
                          </button>
                        )}
                        <button
                          type="button"
                          onClick={() => onRegenerateRoadmap(item.careerKey)}
                          className="p-1 rounded-full text-blue-500 hover:bg-blue-50 transition-colors"
                          title="Refresh"
                        >
                          <RotateCcw className="w-3 h-3" />
                        </button>
                        <button
                          type="button"
                          onClick={() => onRemoveRoadmap(item.careerKey)}
                          className="p-1 rounded-full text-rose-500 hover:bg-rose-50 transition-colors"
                          title="Remove"
                        >
                          <Trash2 className="w-3 h-3" />
                        </button>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {/* Vertical Divider */}
        <div className="hidden lg:block w-px bg-slate-100 mx-1" />

        {/* Available Templates Section */}
        <div className="flex-[0.55] min-w-[220px] space-y-3">
          <div className="flex items-center gap-2 pb-3 border-b border-slate-100">
            <Map className="w-4 h-4 text-indigo-500" />
            <h4 className="font-bold text-slate-900 text-sm">Available Templates</h4>
          </div>

          <div className="flex flex-wrap gap-2 pt-1">
            {availableRoadmaps.length === 0 ? (
              <p className="text-xs text-slate-400 italic">All templates are saved.</p>
            ) : (
              availableRoadmaps.map((item) => (
                <motion.button
                  key={item.domainId}
                  whileHover={{ scale: 1.04 }}
                  whileTap={{ scale: 0.96 }}
                  onClick={() => onSaveRoadmap(item.domainId, item.domainName)}
                  className="group flex items-center gap-1.5 px-3.5 py-2 rounded-full border border-dashed border-slate-300 bg-white hover:border-indigo-400 hover:bg-indigo-50 transition-all duration-200 shadow-sm"
                  title={`Add ${item.domainName}`}
                >
                  <Plus className="w-3 h-3 text-slate-400 group-hover:text-indigo-600 transition-colors" />
                  <span className="text-[11px] font-semibold text-slate-600 group-hover:text-indigo-800 transition-colors">
                    {item.domainName}
                  </span>
                </motion.button>
              ))
            )}
          </div>

          <div className="pt-3">
            <Button
              onClick={onOpenAskAI}
              variant="outline"
              size="sm"
              className="border-dashed border-blue-300 bg-blue-50/50 hover:bg-blue-100 text-blue-700 text-xs font-semibold rounded-full gap-1.5"
            >
              <Sparkles className="w-3 h-3 text-blue-500" />
              Create Custom AI Roadmap
            </Button>
          </div>
        </div>

      </div>
    </div>
  );
}
