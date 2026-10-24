import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { apiClient } from '../services/api';
import CircleCard from '../components/learning-circles/CircleCard';
import CreateCircleModal from '../components/learning-circles/CreateCircleModal';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Brain, Users, Sparkles, Compass, Plus } from 'lucide-react';
import PageHeader from '../components/layout/PageHeader';

const LearningCircles = () => {
  const [recommended, setRecommended] = useState([]);
  const [explore, setExplore] = useState([]);
  const [myCircles, setMyCircles] = useState([]);
  const [requiresQuiz, setRequiresQuiz] = useState(false);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [showMyCircles, setShowMyCircles] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [recRes, exploreRes, myRes] = await Promise.all([
          apiClient.get('/learningcircles/recommended'),
          apiClient.get('/learningcircles/explore'),
          apiClient.get('/learningcircles/my')
        ]);
        
        // ASP.NET's JSON naming policy can vary between environments; accept both
        // camelCase and the DTO's original PascalCase response property names.
        setRequiresQuiz(recRes.data.requiresQuiz ?? recRes.data.RequiresQuiz ?? false);
        setRecommended(recRes.data.recommendations ?? recRes.data.Recommendations ?? []);
        setExplore(exploreRes.data || []);
        setMyCircles(myRes.data || []);
      } catch (error) {
        console.error("Error fetching learning circles", error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const handleJoin = async (circleId) => {
    if (myCircles.some(circle => circle.id === circleId)) {
      navigate(`/learning-circles/${circleId}`);
      return;
    }
    try {
      const response = await apiClient.post(`/learningcircles/${circleId}/join`);
      
      if (response.data.status === 'AlreadyMember') {
        navigate(`/learning-circles/${circleId}`);
      } else {
        toast.success(response.data.message || 'Join request sent! Pending creator approval.');
      }
    } catch (error) {
      console.error("Failed to join circle", error);
      if (error.response?.data?.status === 'AlreadyMember') {
         navigate(`/learning-circles/${circleId}`);
      } else {
         toast.error(error.response?.data?.message || error.response?.data || 'Failed to send join request');
      }
    }
  };

  if (loading) return <div className="p-8 text-center">Loading AI Communities...</div>;

  return (
    <div className="container mx-auto p-4 md:p-8 space-y-8 animate-fade-in">
      <PageHeader 
        icon={Brain}
        title="AI Learning Circles"
        description="Connect with peers sharing your career goals and interests."
      />

      {requiresQuiz && (
        <Card className="bg-white border-indigo-100 shadow-sm">
          <CardContent className="p-6 flex flex-col md:flex-row items-center justify-between">
            <div className="space-y-2">
              <h3 className="text-lg font-semibold text-slate-800 flex items-center gap-2">
                <Compass className="text-indigo-500" /> AI Matching Disabled
              </h3>
              <p className="text-slate-600">Complete your Career Guidance Assessment to receive personalized AI-powered peer recommendations.</p>
            </div>
            <button 
              onClick={() => navigate('/quiz')}
              className="mt-4 md:mt-0 px-6 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-md font-medium transition-colors"
            >
              Take Assessment
            </button>
          </CardContent>
        </Card>
      )}

      {!requiresQuiz && (
        <section className="space-y-4">
          <h2 className="text-2xl font-semibold flex items-center gap-2 text-slate-800">
            Recommended For You
          </h2>
          
          {recommended.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {recommended.map(rec => (
                <CircleCard 
                  key={rec.circle.id} 
                  circle={rec.circle} 
                  recommendation={rec}
                  onJoin={() => handleJoin(rec.circle.id)} 
                />
              ))}
            </div>
          ) : (
            <div className="p-8 text-center bg-white rounded-xl border border-dashed border-slate-300">
              <p className="text-slate-500 mb-4">No public circles match your career goals yet. You can browse all circles below or create one for your interests.</p>
              <button 
                onClick={() => setIsModalOpen(true)}
                className="px-4 py-2 bg-indigo-50 text-indigo-600 hover:bg-indigo-100 font-medium rounded-lg transition-colors"
              >
                Create a New Circle
              </button>
            </div>
          )}
        </section>
      )}

      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-2xl font-semibold flex items-center gap-2 text-slate-800">
            <Users className="text-blue-500" /> {showMyCircles ? 'Your Peer Circles' : 'Explore All Circles'}
          </h2>
          <div className="flex items-center gap-3">
            <button 
              onClick={() => setShowMyCircles(!showMyCircles)}
              className="flex items-center gap-2 px-4 py-2 bg-indigo-50 text-indigo-600 rounded-lg shadow-sm border border-indigo-200 hover:bg-indigo-100 transition-colors"
            >
              {showMyCircles ? 'Show All' : 'Your Peer Circles'}
            </button>
            <button 
              onClick={() => setIsModalOpen(true)}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg shadow-sm hover:bg-blue-700 transition-colors"
            >
              <Plus className="w-4 h-4" /> Create Circle
            </button>
          </div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {(showMyCircles ? myCircles : explore).map(circle => (
            <CircleCard 
              key={circle.id} 
              circle={circle} 
              onJoin={() => handleJoin(circle.id)} 
            />
          ))}
          {showMyCircles && myCircles.length === 0 && (
            <div className="col-span-full py-12 text-center bg-white rounded-xl border border-dashed border-slate-300">
              <p className="text-slate-500 mb-4">You haven't created any peer circles yet.</p>
              <button 
                onClick={() => setIsModalOpen(true)}
                className="px-4 py-2 bg-indigo-50 text-indigo-600 hover:bg-indigo-100 font-medium rounded-lg transition-colors"
              >
                Create your first circle
              </button>
            </div>
          )}
        </div>
      </section>

      <CreateCircleModal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
        onCreated={(newCircle) => setExplore([newCircle, ...explore])}
      />
    </div>
  );
};

export default LearningCircles;
