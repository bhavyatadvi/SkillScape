import React from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { ExternalLink, BookOpen, Clock, MonitorPlay, ShieldCheck, IndianRupee, GraduationCap, Building2 } from 'lucide-react';

export default function CourseDetailsModal({ course, open, onOpenChange }) {
  if (!course) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl p-0 overflow-hidden">
        
        {/* Header / Cover */}
        <div className="relative h-48 w-full bg-slate-900 overflow-hidden">
          {course.imageUrl && (
            <img 
              src={course.imageUrl} 
              alt={course.title} 
              className="w-full h-full object-cover opacity-40 mix-blend-overlay"
            />
          )}
          <div className="absolute inset-0 bg-gradient-to-t from-slate-900 to-transparent" />
          
          <div className="absolute bottom-4 left-6 right-6 text-white">
            <div className="flex gap-2 mb-3">
              <Badge variant="secondary" className="bg-white/20 hover:bg-white/30 text-white border-none">
                {course.domain}
              </Badge>
              {course.subdomain && (
                <Badge variant="secondary" className="bg-white/20 hover:bg-white/30 text-white border-none">
                  {course.subdomain}
                </Badge>
              )}
            </div>
            <DialogTitle className="text-2xl font-bold leading-tight mb-2">
              {course.title}
            </DialogTitle>
          </div>
        </div>

        {/* Content Body */}
        <div className="p-6 overflow-y-auto max-h-[60vh]">
          
          {/* Provider & Quick Stats */}
          <div className="flex flex-wrap items-center justify-between gap-4 mb-6 pb-6 border-b">
            <div className="flex items-center gap-3">
              {course.provider?.logoUrl ? (
                <div className="bg-white p-1 rounded-md border h-12 w-12 flex items-center justify-center">
                  <img src={course.provider.logoUrl} alt={course.provider.name} className="max-h-full max-w-full object-contain" />
                </div>
              ) : (
                <Building2 className="w-10 h-10 text-slate-400" />
              )}
              <div>
                <p className="font-semibold text-slate-900">{course.provider?.name}</p>
                <p className="text-xs text-slate-500">{course.provider?.providerType} Provider</p>
              </div>
            </div>

            <div className="flex items-center gap-6">
              <div className="text-center">
                <p className="text-xs text-slate-500 uppercase tracking-wider mb-1">Price</p>
                <p className="font-semibold text-slate-900 flex items-center justify-center">
                  {course.isFree ? <span className="text-green-600">Free</span> : <><IndianRupee className="w-4 h-4 mr-0.5" />{course.price.toLocaleString('en-IN')}</>}
                </p>
              </div>
              <div className="text-center">
                <p className="text-xs text-slate-500 uppercase tracking-wider mb-1">Duration</p>
                <p className="font-semibold text-slate-900 flex items-center justify-center">
                  <Clock className="w-4 h-4 mr-1 text-slate-400" /> {course.durationValue} {course.durationUnit}
                </p>
              </div>
              <div className="text-center">
                <p className="text-xs text-slate-500 uppercase tracking-wider mb-1">Format</p>
                <p className="font-semibold text-slate-900 flex items-center justify-center">
                  <MonitorPlay className="w-4 h-4 mr-1 text-slate-400" /> {course.learningMode}
                </p>
              </div>
            </div>
          </div>

          {/* Description */}
          <div className="mb-8">
            <h3 className="text-lg font-bold mb-3 flex items-center gap-2">
              <BookOpen className="w-5 h-5 text-indigo-600" />
              About This Course
            </h3>
            <DialogDescription className="text-base text-slate-600 leading-relaxed whitespace-pre-line">
              {course.description}
            </DialogDescription>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            
            {/* Skills & Outcomes */}
            <div>
              <h3 className="text-lg font-bold mb-3 flex items-center gap-2">
                <ShieldCheck className="w-5 h-5 text-green-600" />
                Skills You Will Learn
              </h3>
              <div className="flex flex-wrap gap-2">
                {course.courseSkills?.length > 0 ? (
                  course.courseSkills.map((skill, idx) => (
                    <Badge key={idx} variant="secondary" className="bg-green-50 text-green-700 hover:bg-green-100 border border-green-200 px-3 py-1">
                      {skill.skillName}
                    </Badge>
                  ))
                ) : (
                  <p className="text-sm text-slate-500 italic">No specific skills listed.</p>
                )}
              </div>

              {course.certificateAvailable && (
                <div className="mt-6 p-4 bg-amber-50 rounded-lg border border-amber-100">
                  <h4 className="font-semibold text-amber-900 flex items-center gap-2 mb-1">
                    <ShieldCheck className="w-4 h-4" /> Earn a Certificate
                  </h4>
                  <p className="text-sm text-amber-800">
                    Upon completion, you will receive a verifiable {course.certificateType} from {course.provider?.name}.
                  </p>
                </div>
              )}
            </div>

            {/* Eligibility & Careers */}
            <div className="space-y-6">
              <div>
                <h3 className="text-lg font-bold mb-3 flex items-center gap-2">
                  <GraduationCap className="w-5 h-5 text-blue-600" />
                  Target Audience
                </h3>
                {course.educationEligibilities?.length > 0 ? (
                  <ul className="list-disc pl-5 text-sm text-slate-600 space-y-1">
                    {course.educationEligibilities.map((elig, idx) => (
                      <li key={idx}>{elig.educationLevel}</li>
                    ))}
                  </ul>
                ) : (
                  <p className="text-sm text-slate-500 italic">Open to all learners.</p>
                )}
              </div>

              <div>
                <h3 className="text-sm font-semibold text-slate-700 uppercase tracking-wider mb-3">
                  Relevant Career Paths
                </h3>
                <div className="flex flex-wrap gap-2">
                  {course.courseCareers?.length > 0 ? (
                    course.courseCareers.map((career, idx) => (
                      <Badge key={idx} variant="outline" className="text-slate-600 bg-slate-50">
                        {career.careerTitle}
                      </Badge>
                    ))
                  ) : (
                    <p className="text-sm text-slate-500 italic">Applicable across multiple fields.</p>
                  )}
                </div>
              </div>
            </div>

          </div>
        </div>

        <DialogFooter className="p-6 bg-slate-50 border-t flex items-center justify-between">
          <div className="text-xs text-slate-500">
            Source: {course.source} • Verified: {new Date(course.lastVerified).toLocaleDateString()}
          </div>
          <Button size="lg" className="px-8" onClick={() => window.open(course.url, '_blank')}>
            Enroll on {course.provider?.name} <ExternalLink className="w-4 h-4 ml-2" />
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
