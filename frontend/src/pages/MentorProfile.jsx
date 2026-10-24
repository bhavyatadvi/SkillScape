import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useAuth } from '@/context/AuthContext';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Star,
  MapPin,
  Briefcase,
  Award,
  Calendar,
  Clock,
  CheckCircle,
  Loader2,
  ArrowLeft,
  Linkedin,
} from 'lucide-react';
import { mentorsApi } from '@/services/api';
import { toast } from 'sonner';

const itemVariants = {
  hidden: { opacity: 0, y: 12 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.36 } },
};

const MentorProfilePage = () => {
  const { mentorId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [mentor, setMentor] = useState(null);
  const [enrollmentStatus, setEnrollmentStatus] = useState(null); // null, 'Pending', 'Approved'
  const [enrollment, setEnrollment] = useState(null); // Full enrollment object
  const [loading, setLoading] = useState(true);
  const [enrolling, setEnrolling] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [processing, setProcessing] = useState(false);
  const [selectedPaymentMethod, setSelectedPaymentMethod] = useState('');
  const [wallet, setWallet] = useState(null);

  const fetchEnrollmentStatus = async () => {
    if (user?.role !== 'Student') return;
    try {
      const enrollmentResponse = await mentorsApi.isEnrolledWithMentor(mentorId);
      const enrollmentData = enrollmentResponse?.data;
      if (enrollmentData && typeof enrollmentData === 'object') {
        const approvalStatus = enrollmentData.approvalStatus ?? enrollmentData.ApprovalStatus ?? null;
        setEnrollment(enrollmentData);
        setEnrollmentStatus(approvalStatus);
      } else {
        setEnrollment(null);
        setEnrollmentStatus(null);
      }
    } catch (error) {
      setEnrollment(null);
      setEnrollmentStatus(null);
    }
  };

  useEffect(() => {
    const fetchMentorProfile = async () => {
      try {
        setLoading(true);
        // Fetch mentor details
        const response = await mentorsApi.getById(mentorId);
        setMentor(response?.data ?? null);

        // Check if student is enrolled
        if (user?.role === 'Student') {
          await fetchEnrollmentStatus();

          // Fetch wallet balance
          try {
            const walletResponse = await mentorsApi.getStudentWallet();
            setWallet(walletResponse?.data?.balance ?? 0);
          } catch (error) {
            console.error('Failed to fetch wallet', error);
          }
        }
      } catch (error) {
        toast.error('Failed to load mentor profile');
        console.error(error);
      } finally {
        setLoading(false);
      }
    };

    if (mentorId) {
      fetchMentorProfile();
    }
  }, [mentorId, user?.id]);

  // Refetch enrollment status when page becomes visible
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (!document.hidden && user?.role === 'Student') {
        fetchEnrollmentStatus();
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange);
  }, [mentorId, user?.id]);

  // Poll enrollment status while pending
  useEffect(() => {
    if (enrollmentStatus !== 'Pending' || user?.role !== 'Student') return;

    const pollInterval = setInterval(() => {
      fetchEnrollmentStatus();
    }, 3000); // Check every 3 seconds

    return () => clearInterval(pollInterval);
  }, [enrollmentStatus, mentorId, user?.id]);

  const handleEnroll = async () => {
    try {
      setEnrolling(true);
      const response = await mentorsApi.enrollWithMentor(mentorId);
      const enrollmentData = response?.data;
      setEnrollment(enrollmentData);
      setEnrollmentStatus(enrollmentData?.approvalStatus || 'Pending');
      toast.success('Enrollment request submitted! Waiting for mentor approval.');
    } catch (error) {
      toast.error(
        error.response?.data?.message || 'Failed to enroll with mentor'
      );
    } finally {
      setEnrolling(false);
    }
  };

  const handlePayment = async () => {
    if (!selectedPaymentMethod) {
      toast.error('Please select a payment method');
      return;
    }

    try {
      setProcessing(true);
      const response = await mentorsApi.processPayment(
        enrollment.id,
        selectedPaymentMethod,
        mentor.sessionPrice || 500
      );
      toast.success('Payment completed successfully!');
      setShowPaymentModal(false);
      setEnrollmentStatus('Approved');
    } catch (error) {
      toast.error(error.response?.data?.message || 'Payment failed');
    } finally {
      setProcessing(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Loader2 className="w-8 h-8 animate-spin text-cyan-500" />
      </div>
    );
  }

  if (!mentor) {
    return (
      <div className="flex flex-col items-center justify-center h-screen">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white mb-4">
          Mentor not found
        </h1>
        <Button onClick={() => navigate('/mentors')}>Back to Mentors</Button>
      </div>
    );
  }

  return (
    <motion.div
      className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 dark:from-slate-950 dark:to-slate-900 p-4 md:p-8"
      initial="hidden"
      animate="visible"
    >
      {/* Back Button */}
      <motion.button
        variants={itemVariants}
        onClick={() => navigate('/mentors')}
        className="flex items-center gap-2 text-cyan-500 hover:text-cyan-600 dark:text-cyan-400 dark:hover:text-cyan-300 mb-6 transition"
      >
        <ArrowLeft className="w-4 h-4" />
        Back to Mentors
      </motion.button>

      {/* Mentor Header */}
      <motion.div variants={itemVariants} className="mb-8">
        <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
          <CardContent className="p-6">
            <div className="flex flex-col md:flex-row gap-6">
              {/* Avatar */}
              <div className="flex-shrink-0">
                <div className="w-32 h-32 rounded-full bg-gradient-to-br from-cyan-400 to-blue-500 flex items-center justify-center">
                  <span className="text-4xl font-bold text-white">
                    {mentor.userName?.charAt(0).toUpperCase()}
                  </span>
                </div>
              </div>

              {/* Info */}
              <div className="flex-1">
                <div className="flex flex-col md:flex-row md:items-start md:justify-between gap-4 mb-4">
                  <div>
                    <h1 className="text-3xl font-bold text-slate-900 dark:text-white mb-2">
                      {mentor.userName}
                    </h1>
                    <p className="text-lg text-slate-600 dark:text-slate-400 mb-3">
                      {mentor.expertise}
                    </p>
                    <div className="flex flex-wrap gap-2">
                      <Badge className="bg-cyan-100 text-cyan-800">
                        {mentor.yearsOfExperience} years experience
                      </Badge>
                      <Badge className="bg-purple-100 text-purple-800">
                        {mentor.totalStudentsAssigned} students
                      </Badge>
                    </div>
                  </div>

                  {/* Enroll/Payment Button */}
                  {user?.role === 'Student' && (
                    <div className="flex gap-3">
                      {!enrollmentStatus ? (
                        <Button
                          onClick={handleEnroll}
                          disabled={enrolling}
                          className="bg-cyan-500 hover:bg-cyan-600 text-white flex-1"
                        >
                          {enrolling ? (
                            <>
                              <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                              Enrolling...
                            </>
                          ) : (
                            'Enroll Now'
                          )}
                        </Button>
                      ) : enrollmentStatus === 'Pending' ? (
                        <Button
                          disabled
                          className="bg-yellow-500 hover:bg-yellow-600 text-white flex-1"
                        >
                          <Clock className="w-4 h-4 mr-2" />
                          Pending Approval
                        </Button>
                      ) : enrollmentStatus === 'Approved' && !enrollment?.isPaid ? (
                        <Button
                          onClick={() => navigate('/my-mentors')}
                          className="bg-green-500 hover:bg-green-600 text-white flex-1"
                        >
                          💳 Pay Per Session
                        </Button>
                      ) : enrollment?.isPaid ? (
                        <Button
                          disabled
                          className="bg-green-500 hover:bg-green-600 text-white flex-1"
                        >
                          <CheckCircle className="w-4 h-4 mr-2" />
                          Paid ✓
                        </Button>
                      ) : null}
                    </div>
                  )}
                </div>

                {/* Stats */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div>
                    <div className="flex items-center gap-1 mb-1">
                      <Briefcase className="w-4 h-4 text-purple-500" />
                      <span className="text-sm font-medium">Hourly Rate</span>
                    </div>
                    <p className="text-2xl font-bold text-slate-900 dark:text-white">
                      ₹{mentor.hourlyRate}/hr
                    </p>
                  </div>
                  <div>
                    <div className="flex items-center gap-1 mb-1">
                      <Award className="w-4 h-4 text-green-500" />
                      <span className="text-sm font-medium">Verification</span>
                    </div>
                    <Badge className={`${
                      mentor.isApproved 
                        ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' 
                        : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
                    }`}>
                      {mentor.isApproved ? '✓ Verified Mentor' : '⏳ Pending Verification'}
                    </Badge>
                  </div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </motion.div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column - About & Skills */}
        <motion.div variants={itemVariants} className="lg:col-span-2">
          {/* About */}
          <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 mb-6">
            <CardHeader>
              <CardTitle>About</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-slate-600 dark:text-slate-400 mb-4">
                {mentor.bio|| 'An experienced mentor dedicated to helping students succeed in their careers.'}
              </p>
              {mentor.currentCompany && (
                <div className="flex items-center gap-2 mb-3">
                  <Briefcase className="w-4 h-4 text-slate-500" />
                  <span className="text-slate-700 dark:text-slate-300">
                    Works at {mentor.currentCompany}
                  </span>
                </div>
              )}
              {mentor.linkedInUrl && (
                <a
                  href={mentor.linkedInUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-2 text-cyan-500 hover:text-cyan-600 dark:text-cyan-400 dark:hover:text-cyan-300 transition"
                >
                  <Linkedin className="w-4 h-4" />
                  LinkedIn Profile
                </a>
              )}
            </CardContent>
          </Card>

          {/* Skills */}
          <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
            <CardHeader>
              <CardTitle>Expertise & Skills</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {Array.isArray(mentor.skills) && mentor.skills.length > 0 ? (
                  mentor.skills.map((skill, idx) => (
                    <Badge
                      key={idx}
                      className="bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200"
                    >
                      {skill.trim()}
                    </Badge>
                  ))
                ) : mentor.skillsCsv?.split(',').map((skill, idx) => (
                  <Badge
                    key={idx}
                    className="bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200"
                  >
                    {skill.trim()}
                  </Badge>
                ))}
              </div>
            </CardContent>
          </Card>
        </motion.div>

        {/* Right Column - Availability & Contact */}
        <motion.div variants={itemVariants}>
          {/* Availability Schedule */}
          <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 mb-6">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="w-5 h-5 text-orange-500" />
                Availability
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {mentor.availabilitySchedule
                ?.split(',')
                .map((day, idx) => (
                  <div
                    key={idx}
                    className="flex items-center justify-between p-2 bg-slate-50 dark:bg-slate-700 rounded"
                  >
                    <span className="text-sm font-medium text-slate-700 dark:text-slate-300">
                      {day.trim()}
                    </span>
                    <Badge className="bg-green-100 text-green-800 text-xs">
                      Available
                    </Badge>
                  </div>
                ))}
              {!mentor.availabilitySchedule && (
                <p className="text-sm text-slate-500 dark:text-slate-500">
                  Check availability with mentor
                </p>
              )}
            </CardContent>
          </Card>

          {/* Quick Info */}
          <Card className="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700">
            <CardHeader>
              <CardTitle>Quick Info</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div>
                <p className="text-xs text-slate-600 dark:text-slate-400 mb-1">
                  Session Price
                </p>
                <p className="text-2xl font-bold text-slate-900 dark:text-white">
                  ₹{mentor.sessionPrice}
                </p>
              </div>
              <div>
                <p className="text-xs text-slate-600 dark:text-slate-400 mb-1">
                  Availability Status
                </p>
                <Badge
                  className={`${
                    mentor.isAvailable
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  }`}
                >
                  {mentor.isAvailable ? 'Available' : 'Unavailable'}
                </Badge>
              </div>
            </CardContent>
          </Card>
        </motion.div>
      </div>

      {/* Payment Modal */}
      {showPaymentModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            exit={{ scale: 0.9, opacity: 0 }}
            className="bg-white dark:bg-slate-900 rounded-lg p-6 w-96 shadow-lg"
          >
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-bold text-slate-900 dark:text-white">Select Payment Method</h2>
              <button
                onClick={() => setShowPaymentModal(false)}
                className="text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
              >
                ✕
              </button>
            </div>

            <div className="space-y-3 mb-6">
              {/* Card Payment */}
              <button
                onClick={() => setSelectedPaymentMethod('Card')}
                className={`w-full p-4 border-2 rounded-lg transition-all ${
                  selectedPaymentMethod === 'Card'
                    ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
                    : 'border-slate-200 dark:border-slate-700 hover:border-blue-300'
                }`}
              >
                <div className="flex items-center gap-3">
                  <div className="w-8 h-6 bg-gradient-to-r from-blue-500 to-blue-600 rounded flex items-center justify-center">
                    <span className="text-white text-xs font-bold">💳</span>
                  </div>
                  <div className="text-left">
                    <p className="font-semibold text-slate-900 dark:text-white">Credit/Debit Card</p>
                    <p className="text-xs text-slate-500 dark:text-slate-400">Visa, Mastercard, Amex</p>
                  </div>
                </div>
              </button>

              {/* UPI Payment */}
              <button
                onClick={() => setSelectedPaymentMethod('UPI')}
                className={`w-full p-4 border-2 rounded-lg transition-all ${
                  selectedPaymentMethod === 'UPI'
                    ? 'border-purple-500 bg-purple-50 dark:bg-purple-900/20'
                    : 'border-slate-200 dark:border-slate-700 hover:border-purple-300'
                }`}
              >
                <div className="flex items-center gap-3">
                  <div className="w-8 h-6 bg-gradient-to-r from-purple-500 to-purple-600 rounded flex items-center justify-center">
                    <span className="text-white text-xs font-bold">📱</span>
                  </div>
                  <div className="text-left">
                    <p className="font-semibold text-slate-900 dark:text-white">UPI</p>
                    <p className="text-xs text-slate-500 dark:text-slate-400">Google Pay, PhonePe, Paytm</p>
                  </div>
                </div>
              </button>

              {/* Net Banking */}
              <button
                onClick={() => setSelectedPaymentMethod('NetBanking')}
                className={`w-full p-4 border-2 rounded-lg transition-all ${
                  selectedPaymentMethod === 'NetBanking'
                    ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                    : 'border-slate-200 dark:border-slate-700 hover:border-green-300'
                }`}
              >
                <div className="flex items-center gap-3">
                  <div className="w-8 h-6 bg-gradient-to-r from-green-500 to-green-600 rounded flex items-center justify-center">
                    <span className="text-white text-xs font-bold">🏦</span>
                  </div>
                  <div className="text-left">
                    <p className="font-semibold text-slate-900 dark:text-white">Net Banking</p>
                    <p className="text-xs text-slate-500 dark:text-slate-400">All major Indian banks</p>
                  </div>
                </div>
              </button>
            </div>

            {/* Payment Info */}
            <div className="bg-slate-100 dark:bg-slate-800 rounded-lg p-4 mb-6">
              <div className="flex justify-between mb-2">
                <span className="text-slate-600 dark:text-slate-400">Session Price:</span>
                <span className="font-semibold text-slate-900 dark:text-white">₹{enrollment?.amount || mentor?.sessionPrice}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-600 dark:text-slate-400">Available Balance:</span>
                <span className="font-semibold text-green-600">₹{wallet}</span>
              </div>
              {wallet < (enrollment?.amount || mentor?.sessionPrice) && (
                <p className="text-red-600 text-xs mt-3">⚠️ Insufficient balance in your wallet</p>
              )}
            </div>

            {/* Action Buttons */}
            <div className="flex gap-3">
              <button
                onClick={() => setShowPaymentModal(false)}
                className="flex-1 px-4 py-2 border border-slate-300 dark:border-slate-600 rounded-lg text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handlePayment}
                disabled={!selectedPaymentMethod || processing || wallet < (enrollment?.amount || mentor?.sessionPrice)}
                className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-slate-300 dark:disabled:bg-slate-700 disabled:cursor-not-allowed text-white rounded-lg transition-colors font-semibold flex items-center justify-center gap-2"
              >
                {processing ? (
                  <>
                    <span className="animate-spin">⏳</span>
                    Processing...
                  </>
                ) : (
                  <>
                    💳 Pay ₹{enrollment?.amount || mentor?.sessionPrice}
                  </>
                )}
              </button>
            </div>
          </motion.div>
        </div>
      )}
    </motion.div>
  );
};

export default MentorProfilePage;
