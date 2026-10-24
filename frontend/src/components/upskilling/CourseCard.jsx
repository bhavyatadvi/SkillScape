import React from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { BookmarkPlus, BookmarkCheck, ExternalLink, Clock, MonitorPlay, IndianRupee, ShieldCheck, PlayCircle, CheckCircle2, Award } from 'lucide-react';

export default function CourseCard({ course, onSave, isSaved, progress, onProgressUpdate, showCertification }) {
  return (
    <Card className="h-full flex flex-col hover:shadow-md transition-shadow">
      {course.imageUrl && (
        <div className="relative h-40 w-full bg-slate-100 overflow-hidden rounded-t-xl border-b flex-shrink-0">
          <img src={course.imageUrl} alt={course.title} className="w-full h-full object-cover" />
        </div>
      )}

      <CardContent className="p-4 flex flex-col flex-grow">
        {/* Badges */}
        <div className="flex gap-1 flex-wrap mb-3">
          <Badge variant="secondary" className="bg-slate-100 text-slate-700 hover:bg-slate-200">
            {course.domain}
          </Badge>
          {course.trendingScore > 90 && (
            <Badge className="bg-blue-600 hover:bg-blue-700 text-white border-none">
              Trending
            </Badge>
          )}
        </div>
        
        {/* Provider Info */}
        <div className="flex items-center gap-2 mb-2">
          {course.provider?.logoUrl && (
            <img src={course.provider.logoUrl} alt={course.provider.name} className="h-4 object-contain max-w-[80px]" />
          )}
          <span className="text-xs font-medium text-slate-500">{course.provider?.name}</span>
          
          {course.provider?.providerType === 'Government' && (
            <ShieldCheck className="w-3.5 h-3.5 text-green-600 ml-auto" title="Verified Govt/University Provider" />
          )}
        </div>

        {/* Title */}
        <h3 className="font-bold text-lg leading-tight mb-2 line-clamp-2" title={course.title}>
          {course.title}
        </h3>

        {/* Skills */}
        <div className="flex flex-wrap gap-1 mb-4">
          {course.courseSkills?.slice(0, 3).map((skill, idx) => (
            <Badge key={idx} variant="outline" className="text-[10px] bg-slate-50 border-slate-200 text-slate-600">
              {skill.skillName}
            </Badge>
          ))}
          {course.courseSkills?.length > 3 && (
            <Badge variant="outline" className="text-[10px] bg-slate-50 border-slate-200 text-slate-600">
              +{course.courseSkills.length - 3}
            </Badge>
          )}
        </div>

        {/* Meta Info (Duration, Level, Mode) */}
        <div className="mt-auto space-y-2 mb-4">
          <div className="flex items-center text-xs text-slate-600 gap-4">
            <div className="flex items-center gap-1.5">
              <Clock className="w-3.5 h-3.5 text-slate-400" />
              {course.durationValue} {course.durationUnit}
            </div>
            <div className="flex items-center gap-1.5">
              <MonitorPlay className="w-3.5 h-3.5 text-slate-400" />
              {course.learningMode}
            </div>
          </div>
          
          <div className="flex items-center text-xs text-slate-600 gap-4">
            {course.certificateAvailable && (
              <div className="flex items-center gap-1.5 text-green-600 font-medium">
                <ShieldCheck className="w-3.5 h-3.5" />
                Certificate
              </div>
            )}
          </div>
        </div>

        {/* Extra Card Info (Reason, Progress, Certifications) */}
        <div className="flex flex-col gap-2 mt-2 mb-4">
          {course.recommendationReason && (
            <div className="rounded-md border border-blue-100 bg-blue-50 px-2 py-1.5 text-xs text-blue-700 leading-tight">
              <span className="font-semibold">Why recommended:</span> {course.recommendationReason}
            </div>
          )}
          {showCertification && course.certificationLabel && (
            <div className="text-xs text-slate-500 flex items-center">
              <Award className="mr-1.5 h-3.5 w-3.5" />
              {course.certificationLabel}
            </div>
          )}
          
          {progress?.status === 'completed' ? (
            <div className="text-xs font-medium text-green-700 flex items-center bg-green-50 px-2 py-1.5 rounded border border-green-100 w-fit">
              <CheckCircle2 className="mr-1.5 h-3.5 w-3.5" />
              Completed
            </div>
          ) : (
            onProgressUpdate && (
              <Button 
                variant="ghost" 
                size="sm" 
                className={`w-fit h-7 px-2 text-xs ${progress?.status === 'in_progress' ? 'bg-blue-50 text-blue-700 hover:bg-blue-100' : 'bg-slate-50 text-slate-600 hover:bg-slate-100'}`}
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  onProgressUpdate(course.id, progress?.status === 'in_progress' ? 'completed' : 'in_progress');
                }}
              >
                {progress?.status === 'in_progress' ? (
                  <><CheckCircle2 className="mr-1.5 h-3.5 w-3.5" /> Mark complete</>
                ) : (
                  <><PlayCircle className="mr-1.5 h-3.5 w-3.5" /> Start learning</>
                )}
              </Button>
            )
          )}
        </div>

        {/* Price & CTA */}
        <div className="flex items-center justify-between gap-2 pt-3 border-t">
          <div className="flex flex-col">
            {course.isFree ? (
              <span className="font-bold text-green-600">Free</span>
            ) : (
              <span className="font-bold flex items-center">
                <IndianRupee className="w-3.5 h-3.5" />
                {course.price.toLocaleString('en-IN')}
              </span>
            )}
            {course.financialAidAvailable && (
              <span className="text-[10px] text-slate-500">Financial Aid</span>
            )}
          </div>
          
          <div className="flex items-center gap-2">
          <Button variant="outline" size="sm"
            className="h-9 border-slate-200 px-3" 
            onClick={(event) => { event.preventDefault(); event.stopPropagation(); onSave(course.id); }}
          >
            {isSaved ? <><BookmarkCheck className="h-3.5 w-3.5" />Saved</> : <><BookmarkPlus className="h-3.5 w-3.5" />Save</>}
          </Button>
          <Button 
            className="rounded-full px-4 h-9 font-medium" 
            onClick={(event) => { event.stopPropagation(); window.open(course.url || course.courseUrl, '_blank'); }}
          >
            Visit <ExternalLink className="w-3.5 h-3.5 ml-1" />
          </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
