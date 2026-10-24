import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  Sparkles,
  TrendingUp,
  Flame,
  Target,
  Crown,
  Medal,
  Trophy,
  Award,
  Star,
  Zap,
  Lock,
  Loader2,
} from 'lucide-react';

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Separator } from '@/components/ui/separator';
import { progressApi } from '@/services/api';
import { toast } from 'sonner';



const ProgressPage = () => {
  const [userStats, setUserStats] = useState(null);
  const [badges, setBadges] = useState([]);
  const [leaderboard, setLeaderboard] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchProgressData = async () => {
      try {
        setLoading(true);
        setError(null);

        const [statsRes, badgesRes, leaderboardRes] = await Promise.all([
          progressApi.getMyProgress(),
          progressApi.getBadges(),
          progressApi.getLeaderboard(10)
        ]);

        setUserStats(statsRes.data);
        setBadges(badgesRes.data || []);
        setLeaderboard(leaderboardRes.data || []);
      } catch (err) {
        console.error('Failed to fetch progress data:', err);
        setError(err.message || 'Failed to load progress data');
        toast.error('Failed to load progress data');
      } finally {
        setLoading(false);
      }
    };

    fetchProgressData();
  }, []);

  // Show loading state
  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin mx-auto text-accent" />
          <p className="mt-4 text-muted-foreground">Loading your progress...</p>
        </div>
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Card className="max-w-md">
          <CardContent className="p-6 text-center">
            <p className="text-destructive mb-4">{error}</p>
            <Button onClick={() => window.location.reload()}>Try Again</Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!userStats) {
    return null;
  }

  const xpProgress = (userStats.totalXP / userStats.xpToNextLevel) * 100;

  // Determine rank based on level
  const getRank = (level) => {
    if (level >= 15) return 'Diamond';
    if (level >= 10) return 'Platinum';
    if (level >= 5) return 'Gold';
    if (level >= 3) return 'Silver';
    return 'Bronze';
  };

  const rank = getRank(userStats.level);

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      className="max-w-7xl mx-auto space-y-8 pb-10"
    >
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-foreground mb-2">Your Progress</h1>
        <p className="text-muted-foreground">
          Track your achievements, badges, and climb the leaderboard
        </p>
      </div>

      {/* Top Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          icon={<Sparkles className="h-6 w-6 text-accent" />}
          value={userStats.totalXP.toLocaleString()}
          label="Total XP"
        />
        <StatCard
          icon={<TrendingUp className="h-6 w-6 text-accent" />}
          value={`Level ${userStats.level}`}
          label="Current Level"
        />
        <StatCard
          icon={<Flame className="h-6 w-6 text-warning" />}
          value={userStats.currentStreak}
          label="Day Streak 🔥"
        />
        <StatCard
          icon={<Target className="h-5 w-5 text-foreground" />}
          value={userStats.completedSkills}
          label="Skills Done"
        />
      </div>

      {/* Level Progress Banner */}
      <Card className="shadow-soft border-l-4 border-l-accent overflow-hidden relative">
        <div className="absolute top-0 right-0 p-4">
          <Badge variant="secondary" className="bg-warning/20 text-warning-foreground hover:bg-warning/30 gap-1">
            <Crown className="h-3 w-3" /> {rank} Rank
          </Badge>
        </div>
        <CardContent className="p-8">
          <div className="mb-4">
            <h2 className="text-2xl font-bold flex items-center gap-2">
              <div className="bg-accent p-2 rounded-lg text-accent-foreground">
                <Crown className="h-5 w-5" />
              </div>
              Level {userStats.level}
            </h2>
            <p className="text-muted-foreground mt-1 ml-11">
              {userStats.xpToNextLevel - userStats.totalXP} XP to Level {userStats.level + 1}
            </p>
          </div>

          <div className="space-y-2">
            <Progress value={xpProgress} className="h-4 bg-muted" />
            <div className="flex justify-end text-xs font-medium text-muted-foreground">
              {userStats.totalXP} / {userStats.xpToNextLevel} XP
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Badges Section */}
        <div className="lg:col-span-2 h-full">
          <Card className="h-full border-none shadow-none bg-transparent">
            <CardHeader className="px-0 pt-0">
              <div className="flex items-center gap-2">
                <Award className="h-5 w-5 text-teal-500" />
                <h3 className="font-bold text-lg text-foreground">Badges</h3>
              </div>
            </CardHeader>
            <CardContent className="px-0">
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                {badges.length > 0 ? badges.map((badge) => {
                  // Rarity color mapping
                  const rarityColors = {
                    Common: { bg: "bg-gray-50", border: "border-gray-100", text: "text-gray-900" },
                    Rare: { bg: "bg-cyan-50", border: "border-cyan-100", text: "text-cyan-900" },
                    Epic: { bg: "bg-purple-50", border: "border-purple-100", text: "text-purple-900" },
                    Legendary: { bg: "bg-yellow-50", border: "border-yellow-100", text: "text-yellow-900" },
                  };

                  const earned = badge.earned;
                  const rarity = badge.rarity || 'Common';
                  const colors = rarityColors[rarity] || rarityColors.Common;
                  const colorClasses = earned ? `${colors.bg} ${colors.border} ${colors.text}` : 'bg-white border-gray-100 text-gray-300';

                  return (
                    <Card
                      key={badge.id}
                      title={badge.description}
                      className={`shadow-sm border transition-all ${earned ? 'hover:-translate-y-1 ' + colorClasses : colorClasses}`}
                    >
                      <CardContent className="p-4 flex flex-col items-center text-center h-full justify-center gap-3">
                        <div className="relative">
                          <span className={`text-4xl filter drop-shadow-sm ${!earned ? 'grayscale opacity-20' : ''}`}>{badge.iconUrl || '🏆'}</span>
                          {!earned && (
                            <div className="absolute -bottom-2 left-1/2 -translate-x-1/2">
                              <Lock className="h-6 w-6 text-gray-400 fill-gray-300" />
                            </div>
                          )}
                        </div>

                        <div>
                          <h4 className={`font-semibold text-sm line-clamp-1 ${!earned ? 'text-gray-300' : ''}`}>{badge.name}</h4>
                          <p className={`text-xs mt-1 ${!earned ? 'text-gray-200' : 'opacity-70'}`}>
                            {earned ? (badge.earnedAt ? new Date(badge.earnedAt).toLocaleDateString() : 'Earned') : badge.description}
                          </p>
                        </div>
                      </CardContent>
                    </Card>
                  )
                }) : (
                  <div className="col-span-full text-center py-8">
                    <Award className="mx-auto h-12 w-12 text-muted-foreground opacity-50" />
                    <p className="mt-4 text-sm text-muted-foreground">No badges yet. Complete skills to earn badges!</p>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Leaderboard Section */}
        <div className="h-full">
          <Card className="h-full border-none shadow-none bg-transparent">
            <CardHeader className="px-0 pt-0">
              <div className="flex items-center gap-2">
                <Trophy className="h-5 w-5 text-orange-500" />
                <h3 className="font-bold text-lg text-foreground">Leaderboard</h3>
              </div>
            </CardHeader>
            <CardContent className="px-0 h-full">
              <Card className="shadow-sm border-border/50 bg-white h-full">
                <CardContent className="p-0">
                  {leaderboard.map((user, index) => (
                    <div key={user.rank}>
                      <div className={`flex items-center justify-between p-4 ${user.isUser ? 'bg-teal-50/80 border-l-2 border-l-teal-500' : 'hover:bg-gray-50/50'}`}>
                        <div className="flex items-center gap-3">
                          <div className={`
                                                flex items-center justify-center w-8 h-8 rounded-full text-xs font-bold shadow-sm
                                                ${user.rank === 1 ? 'bg-yellow-400 text-white' :
                              user.rank === 2 ? 'bg-gray-300 text-white' :
                                user.rank === 3 ? 'bg-orange-400 text-white' : 'text-muted-foreground bg-gray-100'}
                                            `}>
                            {user.rank}
                          </div>

                          <div className="h-10 w-10 flex items-center justify-center text-2xl bg-gray-100 rounded-full border-2 border-white shadow-sm">
                            {user.avatar}
                          </div>

                          <div className="flex flex-col">
                            <span className={`text-sm font-semibold ${user.isUser ? 'text-teal-900' : 'text-foreground'}`}>
                              {user.name}
                            </span>
                            <span className="text-[10px] text-muted-foreground">Level {user.level}</span>
                          </div>
                        </div>
                        <div className="text-right">
                          <div className="text-sm font-bold text-foreground">
                            {user.xp.toLocaleString()}
                          </div>
                          <div className="text-[10px] text-muted-foreground uppercase">XP</div>
                        </div>
                      </div>
                      {index < leaderboard.length - 1 && !user.isUser && !leaderboard[index + 1].isUser && <Separator className="opacity-50" />}
                    </div>
                  ))}
                </CardContent>
              </Card>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Domain Progress Section */}
      {userStats.domainProgressions && userStats.domainProgressions.length > 0 && (
        <div className="space-y-6">
          <div className="flex items-center gap-2 mb-4">
            <Star className="h-5 w-5 text-warning" />
            <h3 className="font-semibold text-lg">Domain Progress</h3>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {userStats.domainProgressions.map((domain) => {
              return (
                <Card key={domain.id} className="shadow-sm border-border/50">
                  <CardContent className="p-5 space-y-4">
                    <div className="flex justify-between items-start">
                      <div>
                        <h4 className="font-semibold text-sm">{domain.domainName}</h4>
                        <p className="text-xs text-muted-foreground">
                          {domain.skillsCompleted} of {domain.totalSkills} skills
                        </p>
                      </div>
                      {domain.progressPercentage >= 100 && <Zap className="h-4 w-4 text-accent fill-accent" />}
                    </div>

                    <div className="space-y-2">
                      <Progress value={domain.progressPercentage} className="h-2" />
                    </div>

                    <div className="flex justify-between items-center text-xs">
                      <span className="text-muted-foreground">
                        {Math.round(domain.progressPercentage)}% Complete
                      </span>
                      <span className="font-semibold text-accent">
                        {domain.xpInDomain} XP
                      </span>
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      )}

    </motion.div>
  );
};

const StatCard = ({ icon, value, label }) => (
  <Card className="shadow-sm border-border/50 flex items-center justify-center p-6 bg-card text-card-foreground">
    <div className="flex flex-col items-center text-center space-y-2">
      <div className="p-2 bg-secondary rounded-full mb-1">
        {icon}
      </div>
      <div className="text-2xl font-bold">{value}</div>
      <div className="text-xs text-muted-foreground uppercase tracking-wider">{label}</div>
    </div>
  </Card>
);

export default ProgressPage;
