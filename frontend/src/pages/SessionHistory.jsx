import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Clock, BookOpen, CalendarClock } from 'lucide-react';

const SessionHistoryPage = () => {
  const navigate = useNavigate();

  return (
    <div className="space-y-6 p-6">
      <div className="rounded-lg border border-slate-200 bg-white p-8 shadow-sm">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h1 className="text-3xl font-semibold text-slate-900">Session History</h1>
            <p className="mt-2 text-sm text-slate-500">
              Review your past learning circle sessions, notes, and outcomes to track your progress.
            </p>
          </div>
          <Button onClick={() => navigate('/learning-circles')} className="bg-blue-600 hover:bg-blue-600">
            <CalendarClock className="mr-2 h-4 w-4" /> Back to Circles
          </Button>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card className="border-slate-200 p-6">
          <div className="flex items-center gap-3 text-slate-900">
            <Clock className="h-5 w-5 text-blue-600" />
            <h2 className="text-lg font-semibold">Past Circle Sessions</h2>
          </div>
          <p className="mt-3 text-sm text-slate-500">
            As you complete circle sessions, they will be listed here with session summaries, topics covered, and member contributions.
          </p>
          <div className="mt-6 rounded-lg border border-slate-100 bg-slate-50 p-5">
            <p className="text-sm font-semibold text-slate-700">No session history available yet.</p>
            <p className="mt-2 text-sm text-slate-500">
              Join a learning circle and run your first collaborative session to populate this history.
            </p>
          </div>
        </Card>

        <Card className="border-slate-200 p-6">
          <div className="flex items-center gap-3 text-slate-900">
            <BookOpen className="h-5 w-5 text-blue-600" />
            <h2 className="text-lg font-semibold">Session Notes & Outcomes</h2>
          </div>
          <p className="mt-3 text-sm text-slate-500">
            This section will help you revisit shared learnings, action items, and feedback from circle sessions.
          </p>
        </Card>
      </div>
    </div>
  );
};

export default SessionHistoryPage;
