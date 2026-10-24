import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Tabs, TabsContent } from '@/components/ui/tabs';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Users,
  Star,
  TrendingUp,
  Calendar,
  BookOpen,
  MessageSquare,
  BarChart3,
  Award,
  Clock,
  AlertCircle,
  CheckCircle,
  Zap,
  Target,
  Loader2,
} from 'lucide-react';
import { mentorsApi } from '@/services/api';
import { toast } from 'sonner';

const containerVariants = {
  hidden: { opacity: 0 },
  visible: { opacity: 1, transition: { staggerChildren: 0.08 } },
};

const itemVariants = {
  hidden: { opacity: 0, y: 12 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.36 } },
};

const MentorDashboard = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [mentorStats, setMentorStats] = useState(null);
  const [assignedStudents, setAssignedStudents] = useState([]);
  const [upcomingSessions, setUpcomingSessions] = useState([]);
  const [recentFeedback, setRecentFeedback] = useState([]);
  const [mentorshipProgresses, setMentorshipProgresses] = useState([]);
  const [paymentStatements, setPaymentStatements] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [handlingRequestId, setHandlingRequestId] = useState('');
  const [selectedStudentPayments, setSelectedStudentPayments] = useState(null);
  const [isStudentPaymentsOpen, setIsStudentPaymentsOpen] = useState(false);
  const [savingProfile, setSavingProfile] = useState(false);
  const [mentorProfileForm, setMentorProfileForm] = useState({
    expertise: '',
    expertiseArea: '',
    yearsOfExperience: 1,
    currentCompany: '',
    bio: '',
    skills: '',
    linkedInUrl: '',
    availabilitySchedule: '',
    sessionPrice: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchMentorData = async () => {
      try {
        setLoading(true);
        const [dashboardResponse, requestsResponse, myProfileResponse] = await Promise.all([
          mentorsApi.getDashboard(),
          mentorsApi.getReceivedRequests(),
          mentorsApi.getMyProfile(),
        ]);

        if (dashboardResponse && dashboardResponse.data) {
          const dashboardData = dashboardResponse.data;

          setMentorStats(dashboardData.stats);
          setAssignedStudents(dashboardData.assignedStudents || []);
          setUpcomingSessions(dashboardData.upcomingSessions || []);
          setRecentFeedback(dashboardData.recentFeedback || []);
          setMentorshipProgresses(dashboardData.mentorshipProgresses || []);
          setPaymentStatements(dashboardData.paymentStatements || []);
        }

        setPendingRequests(requestsResponse?.data || []);

        const myProfile = myProfileResponse?.data;
        if (myProfile) {
          setMentorProfileForm({
            expertise: myProfile.expertise || '',
            expertiseArea: myProfile.expertiseArea || myProfile.expertise || '',
            yearsOfExperience: myProfile.yearsOfExperience || 1,
            currentCompany: myProfile.currentCompany || '',
            bio: myProfile.bio || '',
            skills: Array.isArray(myProfile.skills) ? myProfile.skills.join(', ') : '',
            linkedInUrl: myProfile.linkedInUrl || '',
            availabilitySchedule: myProfile.availabilitySchedule || '',
            sessionPrice: myProfile.sessionPrice || myProfile.hourlyRate || 0,
          });
        }
      } catch (error) {
        toast.error('Failed to load mentor dashboard');
        console.error(error);
      } finally {
        setLoading(false);
      }
    };

    if (user?.role === 'Mentor') {
      fetchMentorData();
    }
  }, [user?.id]);

  const refreshDashboardData = async () => {
    const dashboardResponse = await mentorsApi.getDashboard();
    if (dashboardResponse?.data) {
      const dashboardData = dashboardResponse.data;
      setMentorStats(dashboardData.stats);
      setAssignedStudents(dashboardData.assignedStudents || []);
      setUpcomingSessions(dashboardData.upcomingSessions || []);
      setRecentFeedback(dashboardData.recentFeedback || []);
      setMentorshipProgresses(dashboardData.mentorshipProgresses || []);
      setPaymentStatements(dashboardData.paymentStatements || []);
    }
  };

  const handleRequestAction = async (requestId, status) => {
    try {
      setHandlingRequestId(requestId);
      await mentorsApi.updateRequestStatus(requestId, status);
      toast.success(status === 'Accepted' ? 'Request accepted' : 'Request rejected');
      setPendingRequests((prev) => prev.filter((request) => request.id !== requestId));
      await refreshDashboardData();
    } catch (error) {
      toast.error(error?.response?.data?.message || 'Failed to update request');
    } finally {
      setHandlingRequestId('');
    }
  };

  const handleMentorProfileSave = async () => {
    try {
      setSavingProfile(true);
      const skills = mentorProfileForm.skills
        .split(',')
        .map((skill) => skill.trim())
        .filter(Boolean);

      await mentorsApi.saveProfile({
        expertise: mentorProfileForm.expertise,
        expertiseArea: mentorProfileForm.expertiseArea,
        yearsOfExperience: Number(mentorProfileForm.yearsOfExperience) || 1,
        currentCompany: mentorProfileForm.currentCompany || null,
        bio: mentorProfileForm.bio || null,
        skills,
        linkedInUrl: mentorProfileForm.linkedInUrl || null,
        availabilitySchedule: mentorProfileForm.availabilitySchedule || null,
        sessionPrice: Number(mentorProfileForm.sessionPrice) || 0,
      });

      toast.success('Mentor profile updated successfully');
      await refreshDashboardData();
    } catch (error) {
      toast.error(error?.response?.data?.message || 'Failed to update mentor profile');
    } finally {
      setSavingProfile(false);
    }
  };

  const isMentorHome = location.pathname === '/mentor-dashboard' || location.pathname === '/mentor-dashboard/';
  const activeSection = (() => {
    if (isMentorHome) return 'overview';

    const section = location.pathname.split('/')[2] || 'overview';
    if (section === 'schedule') return 'availability';
    return section;
  })();

  const sectionMeta = {
    overview: {
      title: 'Dashboard Overview',
      subtitle: 'Track your mentorship performance and outcomes at a glance.',
    },
    requests: {
      title: 'Pending Requests',
      subtitle: 'Review and respond to incoming student mentorship requests.',
    },
    students: {
      title: 'Assigned Students',
      subtitle: 'Monitor student progress, activity, and learning milestones.',
    },
    payments: {
      title: 'Payments',
      subtitle: 'View payment statements and session-wise earnings details.',
    },
    sessions: {
      title: 'Sessions',
      subtitle: 'Manage upcoming mentorship sessions and calendar actions.',
    },
    feedback: {
      title: 'Feedback',
      subtitle: 'Check student reviews and rating trends for your mentorship.',
    },
    availability: {
      title: 'Schedule',
      subtitle: 'Update your mentor profile, pricing, and availability slots.',
    },
  };

  const currentSectionMeta = sectionMeta[activeSection] || sectionMeta.overview;

  const getStudentPaymentStatements = (student) => {
    const studentName = (student?.name || '').trim().toLowerCase();
    return paymentStatements.filter(
      (statement) => (statement.studentName || '').trim().toLowerCase() === studentName
    );
  };

  const openStudentPayments = (student) => {
    setSelectedStudentPayments({
      student,
      payments: getStudentPaymentStatements(student),
    });
    setIsStudentPaymentsOpen(true);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Loader2 className="w-8 h-8 animate-spin text-cyan-500" />
      </div>
    );
  }

  if (!mentorStats) {
    return (
      <div className="flex items-center justify-center h-screen">
        <AlertCircle className="w-8 h-8 mr-2 text-red-500" />
        <span>Failed to load dashboard</span>
      </div>
    );
  }

  return (
    <motion.div
      className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 dark:from-slate-950 dark:to-slate-900 p-4 md:p-8"
      initial="hidden"
      animate="visible"
      variants={containerVariants}
    >
      {/* Header */}
      {isMentorHome && (
      <motion.div className="mb-8" variants={itemVariants}>
        <h1 className="text-4xl font-bold text-slate-900 dark:text-white mb-2">
          Welcome back, {user?.fullName || 'Mentor'}! 👋
        </h1>
        <p className="text-slate-600 dark:text-slate-400">
          Manage your students and track your mentorship progress
        </p>
      </motion.div>
      )}

      {/* Key Metrics - Feature 1, 13, 15 */}
      {isMentorHome && (
      <motion.div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8" variants={containerVariants}>
        {/* Total Students Assigned - Feature 13 */}
        <motion.div variants={itemVariants}>
          <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-slate-600 dark:text-slate-400">
                Students Assigned
              </CardTitle>
              <Users className="h-4 w-4 text-cyan-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-slate-900 dark:text-white">
                {mentorStats.totalStudents}
              </div>
              <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                {mentorStats.pendingRequests > 0
                  ? `${mentorStats.pendingRequests} pending request${mentorStats.pendingRequests > 1 ? 's' : ''}`
                  : 'No pending requests'}
              </p>
            </CardContent>
          </Card>
        </motion.div>

        {/* Monthly Earnings - Feature 15 */}
        <motion.div variants={itemVariants}>
          <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-slate-600 dark:text-slate-400">
                This Month
              </CardTitle>
              <span className="text-xs font-semibold text-green-600 dark:text-green-400">Rs</span>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-slate-900 dark:text-white">
                Rs {mentorStats.thisMonthEarnings}
              </div>
              <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                Rs {mentorStats.totalEarnings} lifetime earnings
              </p>
            </CardContent>
          </Card>
        </motion.div>

        {/* Total Sessions - Feature 6 */}
        <motion.div variants={itemVariants}>
          <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-slate-600 dark:text-slate-400">
                Sessions Completed
              </CardTitle>
              <Calendar className="h-4 w-4 text-purple-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-slate-900 dark:text-white">
                {mentorStats.totalSessions}
              </div>
              <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                {mentorStats.completionRate}% completion rate
              </p>
            </CardContent>
          </Card>
        </motion.div>
      </motion.div>
      )}

      {!isMentorHome && (
      <motion.div className="mb-6" variants={itemVariants}>
        <h2 className="text-3xl font-bold text-slate-900 dark:text-white mb-1">
          {currentSectionMeta.title}
        </h2>
        <p className="text-slate-600 dark:text-slate-400">
          {currentSectionMeta.subtitle}
        </p>
      </motion.div>
      )}

      {/* Tabs for different sections */}
      <motion.div variants={itemVariants}>
        <Tabs value={activeSection} className="w-full">

          {/* Overview Tab - Feature 12: Mentorship Effectiveness Metrics */}
          <TabsContent value="overview" className="space-y-4 mt-6">
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {/* Student Progress Distribution */}
              <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Target className="w-5 h-5 text-cyan-500" />
                    Mentorship Effectiveness
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <div className="flex justify-between mb-2">
                      <span className="text-sm font-medium">Student Completion Rate</span>
                      <span className="text-sm font-bold">{mentorStats.completionRate}%</span>
                    </div>
                    <Progress value={mentorStats.completionRate} className="h-2" />
                  </div>
                  <div>
                    <div className="flex justify-between mb-2">
                      <span className="text-sm font-medium">Satisfaction Rate</span>
                      <span className="text-sm font-bold">{mentorStats.satisfactionRate}%</span>
                    </div>
                    <Progress value={mentorStats.satisfactionRate} className="h-2" />
                  </div>
                </CardContent>
              </Card>

              {/* Quick Stats */}
              <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Award className="w-5 h-5 text-purple-500" />
                    Achievement Tracking
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-sm">Students Reached Level 10</span>
                    <Badge className="bg-green-100 text-green-800">{mentorStats.studentsReachedLevel10}</Badge>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm">Skills Mastered by Students</span>
                    <Badge className="bg-blue-100 text-blue-800">{mentorStats.skillsMasteredByStudents}</Badge>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm">Career Path Completions</span>
                    <Badge className="bg-purple-100 text-purple-800">{mentorStats.careerPathCompletions}</Badge>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm">Badges Earned by Students</span>
                    <Badge className="bg-yellow-100 text-yellow-800">{mentorStats.badgesEarnedByStudents}</Badge>
                  </div>
                </CardContent>
              </Card>
            </div>

            {isMentorHome && (
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <BarChart3 className="w-5 h-5 text-green-500" />
                      Student Performance Analytics
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div>
                      <p className="text-sm font-medium mb-2">Average Student Level</p>
                      <p className="text-3xl font-bold text-slate-900 dark:text-white">{mentorStats.avgStudentLevel}</p>
                    </div>
                    <div>
                      <p className="text-sm font-medium mb-2">Avg Quiz Performance</p>
                      <p className="text-3xl font-bold text-slate-900 dark:text-white">{mentorStats.avgQuizPerformance}%</p>
                    </div>
                    <div>
                      <p className="text-sm font-medium mb-2">Avg Hours per Student</p>
                      <p className="text-3xl font-bold text-slate-900 dark:text-white">{mentorStats.avgHoursPerStudent}h</p>
                    </div>
                    <div>
                      <p className="text-sm font-medium mb-2">Course Completion Rate</p>
                      <Progress value={mentorStats.courseCompletionRate} className="h-2" />
                      <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">{mentorStats.courseCompletionRate}%</p>
                    </div>
                  </CardContent>
                </Card>

                <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <TrendingUp className="w-5 h-5 text-blue-500" />
                      Growth Metrics
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="flex items-center justify-between">
                      <span className="text-sm">This Month Growth</span>
                      <span className="text-2xl font-bold text-green-500">+{mentorStats.thisMonthGrowth}%</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Student Retention</span>
                      <span className="text-2xl font-bold text-blue-500">{mentorStats.studentRetention}%</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Skill Recommendations Used</span>
                      <span className="text-2xl font-bold text-purple-500">{mentorStats.skillRecommendationsUsed}%</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Career Path Transitions</span>
                      <span className="text-2xl font-bold text-orange-500">{mentorStats.careerPathTransitions}</span>
                    </div>
                  </CardContent>
                </Card>
              </div>
            )}
          </TabsContent>

          {/* Pending Requests Module */}
          <TabsContent value="requests" className="space-y-4 mt-6">
            <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <AlertCircle className="w-5 h-5 text-amber-500" />
                  Pending Student Requests
                </CardTitle>
              </CardHeader>
              <CardContent>
                {pendingRequests.length === 0 ? (
                  <p className="text-slate-600 dark:text-slate-400">No pending student requests</p>
                ) : (
                  <div className="space-y-3">
                    {pendingRequests.map((request) => (
                      <div
                        key={request.id}
                        className="p-4 border border-slate-200 dark:border-slate-700 rounded-lg flex items-center justify-between gap-4"
                      >
                        <div>
                          <p className="font-semibold text-slate-900 dark:text-white">{request.studentName || 'Student'}</p>
                          <div className="mt-1 flex items-center gap-2 flex-wrap">
                            <Badge className="bg-purple-100 text-purple-800">Student Request</Badge>
                            <p className="text-sm text-slate-600 dark:text-slate-400">Topic: {request.topic}</p>
                          </div>
                          <p className="text-xs text-slate-500 dark:text-slate-500 mt-1">
                            Requested on {new Date(request.createdAt).toLocaleDateString()}
                          </p>
                        </div>
                        <div className="flex gap-2">
                          <Button
                            size="sm"
                            className="bg-green-600 hover:bg-green-700 text-white"
                            disabled={handlingRequestId === request.id}
                            onClick={() => handleRequestAction(request.id, 'Accepted')}
                          >
                            Accept
                          </Button>
                          <Button
                            size="sm"
                            variant="outline"
                            className="border-red-300 text-red-600 hover:bg-red-50"
                            disabled={handlingRequestId === request.id}
                            onClick={() => handleRequestAction(request.id, 'Rejected')}
                          >
                            Reject
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          {/* Students Tab - Feature 2, 4, 10 */}
          <TabsContent value="students" className="space-y-4 mt-6">
            <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Users className="w-5 h-5 text-cyan-500" />
                  Assigned Students
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                  {assignedStudents.map((student) => (
                    <div
                      key={student.id}
                      className="p-4 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700/50 transition h-full"
                    >
                      <div className="flex items-start justify-between mb-3">
                        <div>
                          <h4 className="font-semibold text-slate-900 dark:text-white text-base">
                            {student.name}
                          </h4>
                          <p className="text-xs text-slate-500 dark:text-slate-400 break-all">
                            {student.email}
                          </p>
                        </div>
                        <Badge
                          className={`${
                            student.status === 'Active'
                              ? 'bg-green-100 text-green-800'
                              : 'bg-yellow-100 text-yellow-800'
                          }`}
                        >
                          {student.status}
                        </Badge>
                      </div>

                      <div className="grid grid-cols-2 gap-2 mb-3 text-sm">
                        <div>
                          <p className="text-xs text-slate-600 dark:text-slate-400">Level</p>
                          <p className="font-semibold text-slate-900 dark:text-white">
                            {student.currentLevel}
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-slate-600 dark:text-slate-400">Learning Stage</p>
                          <p className="font-semibold text-slate-900 dark:text-white">
                            {student.roadmapStage}
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-slate-600 dark:text-slate-400">Quiz Score</p>
                          <p className="font-semibold text-slate-900 dark:text-white">
                            {student.quizScore}%
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-slate-600 dark:text-slate-400">Hours Logged</p>
                          <p className="font-semibold text-slate-900 dark:text-white">
                            {student.hoursSpent}h
                          </p>
                        </div>
                      </div>

                      <div className="mb-3">
                        <div className="flex justify-between mb-1 text-xs">
                          <span className="text-slate-600 dark:text-slate-400">Overall Progress</span>
                          <span className="font-semibold">{student.progress}%</span>
                        </div>
                        <Progress value={student.progress} className="h-2" />
                      </div>

                      <p className="text-xs text-slate-500 dark:text-slate-500">
                        Last active: {student.lastActive}
                      </p>

                      <Button
                        size="sm"
                        variant="outline"
                        className="mt-3 w-full"
                        onClick={() => openStudentPayments(student)}
                      >
                        View Payments
                      </Button>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Payments Module */}
          <TabsContent value="payments" className="space-y-4 mt-6">
            <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  Payment Statements
                </CardTitle>
              </CardHeader>
              <CardContent>
                {paymentStatements.length === 0 ? (
                  <p className="text-sm text-slate-600 dark:text-slate-400">No payments yet.</p>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm border-collapse">
                      <thead>
                        <tr className="border-b border-slate-200 dark:border-slate-700">
                          <th className="text-left py-2 pr-3">Student</th>
                          <th className="text-left py-2 pr-3">Amount</th>
                          <th className="text-left py-2 pr-3">Session Price</th>
                          <th className="text-left py-2 pr-3">Sessions</th>
                          <th className="text-left py-2 pr-3">Method</th>
                          <th className="text-left py-2 pr-3">Status</th>
                          <th className="text-left py-2">Paid On</th>
                        </tr>
                      </thead>
                      <tbody>
                        {paymentStatements.map((payment) => (
                          <tr key={payment.transactionId} className="border-b border-slate-100 dark:border-slate-800">
                            <td className="py-2 pr-3 font-medium text-slate-900 dark:text-white">{payment.studentName}</td>
                            <td className="py-2 pr-3">Rs {payment.amount}</td>
                            <td className="py-2 pr-3">Rs {payment.sessionPrice}</td>
                            <td className="py-2 pr-3">{payment.sessionsCovered}</td>
                            <td className="py-2 pr-3">{payment.paymentMethod || '-'}</td>
                            <td className="py-2 pr-3">{payment.status}</td>
                            <td className="py-2">{new Date(payment.completedAt || payment.createdAt).toLocaleString()}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          {/* Sessions Tab - Feature 6, 7 */}
          <TabsContent value="sessions" className="space-y-4 mt-6">
            <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Calendar className="w-5 h-5 text-purple-500" />
                  Upcoming Sessions
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {upcomingSessions.length > 0 ? (
                    upcomingSessions.map((session) => {
                      const scheduledTime = new Date(session.scheduledDateTime);
                      const timeString = scheduledTime.toLocaleDateString() + ', ' + scheduledTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                      const isPaid = !!session.isPaid;
                      const paymentStatus = session.paymentStatus || (isPaid ? 'Paid' : 'Unpaid');
                      
                      return (
                      <div
                          key={session.id}
                          className="p-4 border border-slate-200 dark:border-slate-700 rounded-lg flex items-start justify-between"
                        >
                          <div className="flex-1">
                            <h4 className="font-semibold text-slate-900 dark:text-white">
                              {session.studentName}
                            </h4>
                            <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                              {session.notes || 'Session'}
                            </p>
                            <div className="flex gap-4 mt-2 text-xs text-slate-500 dark:text-slate-500">
                              <span className="flex items-center gap-1">
                                <Clock className="w-3 h-3" />
                                {timeString}
                              </span>
                              <span>{session.durationMinutes} minutes</span>
                            </div>
                          </div>
                          <div className="flex flex-col gap-2 items-end">
                            <Badge
                              className={`${
                                session.status === 'Confirmed'
                                  ? 'bg-green-100 text-green-800'
                                  : 'bg-yellow-100 text-yellow-800'
                              }`}
                            >
                              {session.status}
                            </Badge>
                            <Badge
                              className={isPaid ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800'}
                            >
                              {isPaid ? 'Paid' : paymentStatus}
                            </Badge>
                          </div>
                        </div>
                      );
                    })
                  ) : (
                    <p className="text-slate-600 dark:text-slate-400">No upcoming sessions</p>
                  )}
                </div>
                <Button 
                  className="w-full mt-4 bg-cyan-500 hover:bg-cyan-600 text-white"
                  onClick={() => navigate('/sessions')}
                >
                  View Full Calendar
                </Button>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Feedback Tab - Feature 7, 8 */}
          <TabsContent value="feedback" className="space-y-4 mt-6">
            <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <MessageSquare className="w-5 h-5 text-blue-500" />
                  Student Feedback & Reviews
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {recentFeedback.length > 0 ? (
                    recentFeedback.map((feedback) => (
                      <div
                        key={feedback.id}
                        className="p-4 border border-slate-200 dark:border-slate-700 rounded-lg"
                      >
                        <div className="flex items-start justify-between mb-2">
                          <h4 className="font-semibold text-slate-900 dark:text-white">
                            {feedback.studentName}
                          </h4>
                          <div className="flex gap-1">
                            {[...Array(5)].map((_, i) => (
                              <Star
                                key={i}
                                className={`w-4 h-4 ${
                                  i < Math.floor(feedback.rating)
                                    ? 'fill-yellow-400 text-yellow-400'
                                    : i < feedback.rating
                                    ? 'fill-yellow-200 text-yellow-400'
                                    : 'text-slate-300'
                                }`}
                              />
                            ))}
                          </div>
                        </div>
                        <p className="text-sm text-slate-600 dark:text-slate-400 mb-2">
                          {feedback.comment || 'No comment provided'}
                        </p>
                        <p className="text-xs text-slate-500 dark:text-slate-500">{feedback.date}</p>
                      </div>
                    ))
                  ) : (
                    <p className="text-slate-600 dark:text-slate-400">No feedback yet</p>
                  )}
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Analytics Tab - Feature 11, 12 */}
          <TabsContent value="analytics" className="space-y-4 mt-6">
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <BarChart3 className="w-5 h-5 text-green-500" />
                    Student Performance Analytics
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <p className="text-sm font-medium mb-2">Average Student Level</p>
                    <p className="text-3xl font-bold text-slate-900 dark:text-white">{mentorStats.avgStudentLevel}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium mb-2">Avg Quiz Performance</p>
                    <p className="text-3xl font-bold text-slate-900 dark:text-white">{mentorStats.avgQuizPerformance}%</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium mb-2">Avg Hours per Student</p>
                    <p className="text-3xl font-bold text-slate-900 dark:text-white">{mentorStats.avgHoursPerStudent}h</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium mb-2">Course Completion Rate</p>
                    <Progress value={mentorStats.courseCompletionRate} className="h-2" />
                    <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">{mentorStats.courseCompletionRate}%</p>
                  </div>
                </CardContent>
              </Card>

              <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <TrendingUp className="w-5 h-5 text-blue-500" />
                    Growth Metrics
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span className="text-sm">This Month Growth</span>
                    <span className="text-2xl font-bold text-green-500">+{mentorStats.thisMonthGrowth}%</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm">Student Retention</span>
                    <span className="text-2xl font-bold text-blue-500">{mentorStats.studentRetention}%</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm">Skill Recommendations Used</span>
                    <span className="text-2xl font-bold text-purple-500">{mentorStats.skillRecommendationsUsed}%</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm">Career Path Transitions</span>
                    <span className="text-2xl font-bold text-orange-500">{mentorStats.careerPathTransitions}</span>
                  </div>
                </CardContent>
              </Card>
            </div>
          </TabsContent>

          {/* Availability Tab - Feature 14 */}
          <TabsContent value="availability" className="space-y-4 mt-6">
            <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Clock className="w-5 h-5 text-orange-500" />
                  Mentor Profile & Availability
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <p className="text-sm font-medium text-slate-700 dark:text-slate-300">Expertise</p>
                    <Input
                      value={mentorProfileForm.expertise}
                      onChange={(e) => setMentorProfileForm((prev) => ({ ...prev, expertise: e.target.value }))}
                      placeholder="e.g. Data Science"
                    />
                  </div>

                  <div className="space-y-2">
                    <p className="text-sm font-medium text-slate-700 dark:text-slate-300">Expertise Area</p>
                    <Input
                      value={mentorProfileForm.expertiseArea}
                      onChange={(e) => setMentorProfileForm((prev) => ({ ...prev, expertiseArea: e.target.value }))}
                      placeholder="e.g. Full Stack Development"
                    />
                  </div>

                  <div className="space-y-2">
                    <p className="text-sm font-medium text-slate-700 dark:text-slate-300">Years of Experience</p>
                    <Input
                      type="number"
                      min={1}
                      value={mentorProfileForm.yearsOfExperience}
                      onChange={(e) => setMentorProfileForm((prev) => ({ ...prev, yearsOfExperience: e.target.value }))}
                    />
                  </div>

                  <div className="space-y-2">
                    <p className="text-sm font-medium text-slate-700 dark:text-slate-300">Current Company</p>
                    <Input
                      value={mentorProfileForm.currentCompany}
                      onChange={(e) => setMentorProfileForm((prev) => ({ ...prev, currentCompany: e.target.value }))}
                      placeholder="e.g. Microsoft"
                    />
                  </div>

                  <div className="space-y-2">
                    <p className="text-sm font-medium text-slate-700 dark:text-slate-300">Session Price (Rs)</p>
                    <Input
                      type="number"
                      min={0}
                      value={mentorProfileForm.sessionPrice}
                      onChange={(e) => setMentorProfileForm((prev) => ({ ...prev, sessionPrice: e.target.value }))}
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <p className="text-sm font-medium text-slate-700 dark:text-slate-300">Skills (comma separated)</p>
                  <Input
                    value={mentorProfileForm.skills}
                    onChange={(e) => setMentorProfileForm((prev) => ({ ...prev, skills: e.target.value }))}
                    placeholder="React, Node.js, SQL"
                  />
                </div>

                <div className="space-y-2">
                  <p className="text-sm font-medium text-slate-700 dark:text-slate-300">LinkedIn URL</p>
                  <Input
                    value={mentorProfileForm.linkedInUrl}
                    onChange={(e) => setMentorProfileForm((prev) => ({ ...prev, linkedInUrl: e.target.value }))}
                    placeholder="https://linkedin.com/in/your-profile"
                  />
                </div>

                <div className="space-y-2">
                  <p className="text-sm font-medium text-slate-700 dark:text-slate-300">Availability Schedule</p>
                  <Input
                    value={mentorProfileForm.availabilitySchedule}
                    onChange={(e) => setMentorProfileForm((prev) => ({ ...prev, availabilitySchedule: e.target.value }))}
                    placeholder="Mon-Fri: 6 PM - 9 PM"
                  />
                </div>

                <div className="space-y-2">
                  <p className="text-sm font-medium text-slate-700 dark:text-slate-300">Bio</p>
                  <Textarea
                    value={mentorProfileForm.bio}
                    onChange={(e) => setMentorProfileForm((prev) => ({ ...prev, bio: e.target.value }))}
                    placeholder="Describe your mentoring style and expertise"
                    className="min-h-[120px]"
                  />
                </div>

                <Button
                  className="w-full bg-cyan-500 hover:bg-cyan-600 text-white"
                  onClick={handleMentorProfileSave}
                  disabled={savingProfile}
                >
                  {savingProfile ? 'Saving...' : 'Save Profile Changes'}
                </Button>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </motion.div>

      <Dialog open={isStudentPaymentsOpen} onOpenChange={setIsStudentPaymentsOpen}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>
              Payment Details - {selectedStudentPayments?.student?.name || 'Student'}
            </DialogTitle>
            <DialogDescription>
              Mentor view of payment records for this student.
            </DialogDescription>
          </DialogHeader>

          {selectedStudentPayments?.payments?.length ? (
            <div className="overflow-x-auto">
              <table className="w-full text-sm border-collapse">
                <thead>
                  <tr className="border-b border-slate-200 dark:border-slate-700">
                    <th className="text-left py-2 pr-3">Amount</th>
                    <th className="text-left py-2 pr-3">Session Price</th>
                    <th className="text-left py-2 pr-3">Sessions</th>
                    <th className="text-left py-2 pr-3">Method</th>
                    <th className="text-left py-2 pr-3">Status</th>
                    <th className="text-left py-2">Paid On</th>
                  </tr>
                </thead>
                <tbody>
                  {selectedStudentPayments.payments.map((payment) => (
                    <tr key={payment.transactionId} className="border-b border-slate-100 dark:border-slate-800">
                      <td className="py-2 pr-3">Rs {payment.amount}</td>
                      <td className="py-2 pr-3">Rs {payment.sessionPrice}</td>
                      <td className="py-2 pr-3">{payment.sessionsCovered}</td>
                      <td className="py-2 pr-3">{payment.paymentMethod || '-'}</td>
                      <td className="py-2 pr-3">{payment.status}</td>
                      <td className="py-2">{new Date(payment.completedAt || payment.createdAt).toLocaleString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-sm text-slate-600 dark:text-slate-400">No payment records available for this student.</p>
          )}
        </DialogContent>
      </Dialog>
    </motion.div>
  );
};

export default MentorDashboard;
