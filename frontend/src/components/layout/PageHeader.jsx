import React from 'react';
import { cn } from '@/lib/utils';

export const PageHeader = ({ icon: Icon, title, description, eyebrow, action, className }) => {
  return (
    <div className={cn("mb-8", className)}>
      {eyebrow && (
        <div className="text-xs font-semibold text-[#2563EB] uppercase tracking-widest mb-3">
          {eyebrow}
        </div>
      )}
      
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
        <div className="flex items-start gap-3.5">
          {Icon && (
            <div className="w-10 h-10 bg-[#EFF6FF] text-[#2563EB] rounded-[10px] flex items-center justify-center shrink-0">
              <Icon className="w-5 h-5" />
            </div>
          )}
          
          <div className="flex flex-col gap-1">
            <h1 className="text-2xl sm:text-[28px] font-bold text-[#0F172A] leading-tight tracking-tight">
              {title}
            </h1>
            {description && (
              <p className="text-[15px] text-[#64748B] leading-relaxed max-w-4xl">
                {description}
              </p>
            )}
          </div>
        </div>
        
        {action && (
          <div className="flex-shrink-0">
            {action}
          </div>
        )}
      </div>
    </div>
  );
};

export default PageHeader;
