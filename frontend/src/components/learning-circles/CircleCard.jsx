import React from 'react';
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from '../ui/card';
import { Badge } from '../ui/badge';
import { Users, Target, BookOpen, MessageSquare } from 'lucide-react';

const CircleCard = ({ circle, recommendation, onJoin }) => {
  return (
    <Card className={`relative overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow ${recommendation ? 'border-blue-200' : 'border-slate-200'}`}>
      {recommendation && (
        <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-blue-400 to-indigo-500" />
      )}
      <CardHeader>
        <div className="flex justify-between items-start">
          <Badge variant="outline" className="mb-2 bg-blue-50 text-blue-600 border-blue-200">
            {circle.circleType}
          </Badge>
          {circle.isDisabledByAdmin && <Badge className="bg-amber-100 text-amber-800">Disabled by admin</Badge>}
          {recommendation && (
            <Badge className="bg-blue-50 text-blue-700 border-blue-200">
              {recommendation.compatibilityScore}% Match
            </Badge>
          )}
        </div>
        <CardTitle className="text-xl text-slate-900">{circle.name}</CardTitle>
        <CardDescription className="text-slate-600 line-clamp-2">
          {circle.description}
        </CardDescription>
      </CardHeader>
      
      <CardContent className="space-y-4">
        <div className="flex items-center gap-4 text-sm text-slate-600">
          <div className="flex items-center gap-1">
            <Target className="w-4 h-4 text-slate-400" />
            {circle.careerDomain}
          </div>
          <div className="flex items-center gap-1">
            <Users className="w-4 h-4 text-slate-400" />
            {circle.memberCount} Members
          </div>
        </div>
        <div className="text-sm text-slate-500">
          Created by <span className="font-medium text-slate-700">{circle.creatorName || 'Registered member'}</span>
        </div>

        {circle.tags && (
          <div className="flex flex-wrap gap-2">
            {circle.tags.split(',').map((tag, idx) => (
              <span key={idx} className="text-xs px-2 py-1 bg-slate-100 text-slate-600 rounded-md">
                {tag.trim()}
              </span>
            ))}
          </div>
        )}
        {circle.isDisabledByAdmin && <p className="rounded-lg bg-amber-50 p-3 text-sm text-amber-900">This group has been disabled by an administrator.{circle.adminDisabledReason ? ` ${circle.adminDisabledReason}` : ''}</p>}

        {recommendation && (
          <div className="mt-4 p-3 bg-blue-50 rounded-lg border border-blue-100">
            <p className="text-xs text-blue-800 flex gap-2">
              <span className="text-blue-500">★</span> 
              {recommendation.reason}
            </p>
          </div>
        )}
      </CardContent>

      <CardFooter>
        <button 
          onClick={onJoin}
          className={`w-full py-2 rounded-md font-medium transition-colors ${
            recommendation 
              ? 'bg-blue-600 hover:bg-blue-700 text-white' 
              : 'bg-blue-600 hover:bg-blue-700 text-white'
          }`}
        >
          {circle.isDisabledByAdmin ? 'View group status' : circle.isMember || circle.isCreator ? 'Open Circle' : 'Join Circle'}
        </button>
      </CardFooter>
    </Card>
  );
};

export default CircleCard;
