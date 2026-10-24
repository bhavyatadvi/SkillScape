import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { 
  CheckCircle2, AlertTriangle, Layers
} from 'lucide-react';

export const SkillGapTable = ({ matchedSkills = [], missingSkills = [] }) => {
  const [activeTab, setActiveTab] = useState('all'); // 'all' | 'gaps' | 'matched'

  const allSkills = [
    ...missingSkills.map(s => ({ ...s, isGap: true })),
    ...matchedSkills.map(s => ({ ...s, isGap: false })),
  ];

  const filteredSkills = allSkills.filter(s => {
    if (activeTab === 'gaps') return s.isGap;
    if (activeTab === 'matched') return !s.isGap;
    return true;
  });

  const getPriorityBadge = (priority) => {
    switch (priority?.toLowerCase()) {
      case 'high':
      case 'critical':
        return 'bg-rose-50 border-rose-200 text-rose-700 font-semibold';
      case 'medium':
        return 'bg-amber-50 border-amber-200 text-amber-700 font-semibold';
      default:
        return 'bg-slate-100 border-slate-200 text-slate-700 font-semibold';
    }
  };

  const getEvidenceBadge = (source) => {
    switch (source?.toLowerCase()) {
      case 'from resume':
        return 'bg-blue-50 border-blue-200 text-blue-700 font-semibold';
      case 'from projects':
        return 'bg-indigo-50 border-indigo-200 text-indigo-700 font-semibold';
      case 'self assessed':
        return 'bg-emerald-50 border-emerald-200 text-emerald-700 font-semibold';
      default:
        return 'bg-slate-100 border-slate-200 text-slate-600 font-semibold';
    }
  };

  return (
    <motion.div 
      initial={{ opacity: 0, y: 15 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-white border border-slate-200 rounded-xl p-8 shadow-sm space-y-6"
    >
      {/* Header & Tabs */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 pb-6">
        <div>
          <h2 className="text-xl font-extrabold text-slate-900 flex items-center gap-2">
            <Layers className="w-5 h-5 text-blue-600" />
            Detailed Skill Gap & Proficiency Matrix
          </h2>
          <p className="text-slate-500 text-sm font-medium mt-0.5">
            Comparing your verified profile skills against target career requirements.
          </p>
        </div>

        {/* Tab Buttons */}
        <div className="flex items-center bg-slate-100 p-1 rounded-xl border border-slate-200">
          <button
            onClick={() => setActiveTab('all')}
            className={`px-4 py-2 rounded-lg text-xs font-bold transition-all ${
              activeTab === 'all'
                ? 'bg-blue-600 text-white shadow-sm'
                : 'text-slate-600 hover:text-slate-900'
            }`}
          >
            All Skills ({allSkills.length})
          </button>
          <button
            onClick={() => setActiveTab('gaps')}
            className={`px-4 py-2 rounded-lg text-xs font-bold transition-all ${
              activeTab === 'gaps'
                ? 'bg-rose-600 text-white shadow-sm'
                : 'text-slate-600 hover:text-slate-900'
            }`}
          >
            Skill Gaps ({missingSkills.length})
          </button>
          <button
            onClick={() => setActiveTab('matched')}
            className={`px-4 py-2 rounded-lg text-xs font-bold transition-all ${
              activeTab === 'matched'
                ? 'bg-emerald-600 text-white shadow-sm'
                : 'text-slate-600 hover:text-slate-900'
            }`}
          >
            Matched ({matchedSkills.length})
          </button>
        </div>
      </div>

      {/* Skills Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-left text-sm text-slate-700">
          <thead className="bg-slate-50 text-slate-700 uppercase text-xs font-bold border-b border-slate-200">
            <tr>
              <th className="py-3.5 px-4">Skill Name</th>
              <th className="py-3.5 px-4">Category</th>
              <th className="py-3.5 px-4">Your Level / Required</th>
              <th className="py-3.5 px-4">Priority / Status</th>
              <th className="py-3.5 px-4">Evidence Source</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filteredSkills.length === 0 ? (
              <tr>
                <td colSpan="5" className="text-center py-8 text-slate-400 font-medium">
                  No skills found in this view.
                </td>
              </tr>
            ) : (
              filteredSkills.map((s, idx) => (
                <tr key={idx} className="hover:bg-slate-50/80 transition-colors">
                  {/* Skill Name */}
                  <td className="py-4 px-4 font-bold text-slate-900">
                    <div className="flex items-center gap-2.5">
                      {s.isGap ? (
                        <AlertTriangle className="w-4 h-4 text-rose-500 shrink-0" />
                      ) : (
                        <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0" />
                      )}
                      <span>{s.skillName}</span>
                      {s.isRequired && (
                        <span className="text-[10px] bg-blue-50 border border-blue-200 text-blue-700 px-1.5 py-0.5 rounded font-mono font-bold">
                          REQUIRED
                        </span>
                      )}
                    </div>
                  </td>

                  {/* Category */}
                  <td className="py-4 px-4">
                    <span className="px-2.5 py-1 rounded-md text-xs font-semibold bg-slate-100 border border-slate-200 text-slate-700">
                      {s.category || 'Technical'}
                    </span>
                  </td>

                  {/* Proficiency Level Comparison Bar */}
                  <td className="py-4 px-4 min-w-[200px]">
                    <div className="space-y-1">
                      <div className="flex justify-between text-xs font-mono font-bold">
                        <span className={s.isGap ? 'text-rose-600' : 'text-emerald-600'}>
                          Level {s.userProficiencyLevel || 0}
                        </span>
                        <span className="text-slate-400">
                          Req: Level {s.requiredProficiencyLevel}
                        </span>
                      </div>
                      <div className="h-2 w-full bg-slate-100 rounded-full overflow-hidden relative border border-slate-200">
                        {/* Target marker line */}
                        <div 
                          className="absolute top-0 bottom-0 border-l-2 border-slate-400 z-10"
                          style={{ left: `${(s.requiredProficiencyLevel / 5) * 100}%` }}
                        />
                        {/* User proficiency bar */}
                        <div 
                          className={`h-full ${s.isGap ? 'bg-rose-500' : 'bg-emerald-600'}`}
                          style={{ width: `${((s.userProficiencyLevel || 0) / 5) * 100}%` }}
                        />
                      </div>
                    </div>
                  </td>

                  {/* Priority / Status */}
                  <td className="py-4 px-4">
                    {s.isGap ? (
                      <span className={`px-2.5 py-1 rounded-md text-xs border ${getPriorityBadge(s.priorityRank)}`}>
                        {s.priorityRank ? `${s.priorityRank} Priority` : 'Gap Identified'}
                      </span>
                    ) : (
                      <span className="px-2.5 py-1 rounded-md text-xs font-bold bg-emerald-50 border border-emerald-200 text-emerald-700">
                        Proficiency Met
                      </span>
                    )}
                  </td>

                  {/* Evidence Source */}
                  <td className="py-4 px-4">
                    <span className={`px-2.5 py-1 rounded-md text-xs border ${getEvidenceBadge(s.evidenceSource)}`}>
                      {s.evidenceSource || (s.isGap ? 'Gap Identification' : 'Inferred')}
                    </span>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </motion.div>
  );
};

export default SkillGapTable;
