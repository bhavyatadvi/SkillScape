import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Users, Video, Sparkles } from 'lucide-react';

const MyCirclePage = () => {
  const navigate = useNavigate();

  return (
    <div className="space-y-6 p-6">
      <div className="rounded-lg border border-slate-200 bg-white p-8 shadow-sm">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h1 className="text-3xl font-semibold text-slate-900">My Circle</h1>
            <p className="mt-2 text-sm text-slate-500">
              Access your active learning circle, review current members, and continue collaboration.
            </p>
          </div>
          <div className="flex flex-wrap gap-3">
            <Button onClick={() => navigate('/learning-circles')} className="bg-blue-600 hover:bg-blue-600">
              <Sparkles className="mr-2 h-4 w-4" /> Find Peers
            </Button>
            <Button onClick={() => navigate('/learning-circles')} variant="outline">
              <Video className="mr-2 h-4 w-4" /> Open Circles
            </Button>
          </div>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card className="border-slate-200 p-6">
          <div className="flex items-center gap-3 text-slate-900">
            <Users className="h-5 w-5 text-blue-600" />
            <h2 className="text-lg font-semibold">Current Circle</h2>
          </div>
          <p className="mt-3 text-sm text-slate-500">
            View the learning circle you are currently active in, including recent updates, shared notes, and tasks.
          </p>
          <div className="mt-6 rounded-lg border border-slate-100 bg-slate-50 p-5">
            <p className="text-sm font-semibold text-slate-700">No active circle detected yet.</p>
            <p className="mt-2 text-sm text-slate-500">
              Start a new circle from the Find Peers page or join a recommended learning circle to get started.
            </p>
          </div>
        </Card>

        <Card className="border-slate-200 p-6">
          <div className="flex items-center gap-3 text-slate-900">
            <Video className="h-5 w-5 text-blue-600" />
            <h2 className="text-lg font-semibold">Circle Workspace</h2>
          </div>
          <p className="mt-3 text-sm text-slate-500">
            When you have joined a circle, this page will help you quickly navigate to your circle's chat, notes, and task board.
          </p>
          <div className="mt-6 space-y-3">
            <Button onClick={() => navigate('/learning-circles')} className="w-full bg-blue-600 hover:bg-blue-600">
              Go to Learning Circles Workspace
            </Button>
          </div>
        </Card>
      </div>
    </div>
  );
};

export default MyCirclePage;
