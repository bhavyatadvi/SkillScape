import React from 'react';
import { motion } from 'framer-motion';
import { Brain } from 'lucide-react';

export default function QuizProcessingLoader({
  title = "Processing your results…",
  messages = [],
  loadingStep = 0,
}) {
  return (
    <div className="min-h-[70vh] flex items-center justify-center px-4 py-12">
      <motion.div
        initial={{ opacity: 0, scale: 0.92 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.3, ease: 'easeOut' }}
        className="bg-white rounded-2xl shadow-lg border border-slate-200 p-8 sm:p-12 max-w-md w-full text-center"
      >
        <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-6">
          <Brain className="w-8 h-8 text-blue-600 animate-pulse" />
        </div>
        <h2 className="text-2xl font-bold text-slate-900 mb-6">{title}</h2>
        <div className="space-y-3">
          {messages.map((msg, idx) => {
            const isActiveOrDone = idx <= loadingStep;
            return (
              <div
                key={idx}
                className={`flex items-center gap-3 p-3 rounded-lg transition-all duration-300 ${
                  isActiveOrDone
                    ? 'bg-blue-50 text-blue-800'
                    : 'bg-slate-50 text-slate-400'
                }`}
              >
                <div
                  className={`w-2 h-2 rounded-full flex-shrink-0 transition-colors duration-300 ${
                    isActiveOrDone ? 'bg-blue-600' : 'bg-slate-300'
                  }`}
                />
                <span className="text-sm font-medium text-left">{msg}</span>
              </div>
            );
          })}
        </div>
      </motion.div>
    </div>
  );
}
