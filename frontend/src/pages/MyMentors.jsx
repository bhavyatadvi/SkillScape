import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Calendar, Clock, Video, User, ArrowLeft, ChevronRight, Loader2, CreditCard, CheckCircle, MessageSquare, Star } from 'lucide-react';
import { mentorsApi, sessionsApi } from '@/services/api';
import { toast } from 'sonner';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Textarea } from '@/components/ui/textarea';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

const MyMentors = () => {
  const [enrolledMentors, setEnrolledMentors] = useState([]);
  const [selectedMentor, setSelectedMentor] = useState(null);
  const [mentorSessions, setMentorSessions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [sessionsLoading, setSessionsLoading] = useState(false);
  const [selectedEnrollment, setSelectedEnrollment] = useState(null);
  const [processingPaymentSessionId, setProcessingPaymentSessionId] = useState(null);
  const [isPaymentGatewayOpen, setIsPaymentGatewayOpen] = useState(false);
  const [paymentSession, setPaymentSession] = useState(null);
  const [selectedPaymentMethod, setSelectedPaymentMethod] = useState('UPI');
  const [gatewayState, setGatewayState] = useState('select');
  const [feedbackBySessionId, setFeedbackBySessionId] = useState({});
  const [isFeedbackModalOpen, setIsFeedbackModalOpen] = useState(false);
  const [feedbackSession, setFeedbackSession] = useState(null);
  const [feedbackRating, setFeedbackRating] = useState(0);
  const [feedbackReviewText, setFeedbackReviewText] = useState('');
  const [submittingFeedback, setSubmittingFeedback] = useState(false);

  useEffect(() => {
    fetchEnrolledMentors();
  }, []);

  const fetchEnrolledMentors = async () => {
    try {
      setLoading(true);
      const response = await mentorsApi.getEnrolledMentors();
      setEnrolledMentors(response.data || []);
    } catch (err) {
      console.error('Failed to fetch enrolled mentors:', err);
      toast.error('Failed to load mentors');
    } finally {
      setLoading(false);
    }
  };

  const fetchMentorSessions = async (mentorId) => {
    try {
      setSessionsLoading(true);
      const response = await sessionsApi.getMySessions();
      
      // Filter sessions for the selected mentor
      const filteredSessions = (response.data || []).filter(
        (session) => session.mentorId === mentorId
      );
      
      // Sort by date
      filteredSessions.sort((a, b) => new Date(a.scheduledDateTime) - new Date(b.scheduledDateTime));
      
      setMentorSessions(filteredSessions);
    } catch (err) {
      console.error('Failed to fetch sessions:', err);
      toast.error('Failed to load sessions');
    } finally {
      setSessionsLoading(false);
    }
  };

  const fetchMyFeedback = async () => {
    try {
      const response = await mentorsApi.getMyFeedback();
      const feedbackList = response.data || [];
      const feedbackMap = feedbackList.reduce((acc, feedback) => {
        acc[feedback.sessionId] = feedback;
        return acc;
      }, {});
      setFeedbackBySessionId(feedbackMap);
    } catch (err) {
      console.error('Failed to fetch my feedback:', err);
    }
  };

  const handleMentorClick = async (mentor) => {
    setSelectedMentor(mentor);
    try {
      const enrollmentResponse = await mentorsApi.isEnrolledWithMentor(mentor.id);
      setSelectedEnrollment(enrollmentResponse?.data || null);
    } catch {
      setSelectedEnrollment(null);
    }
    await fetchMentorSessions(mentor.id);
    await fetchMyFeedback();
  };

  const handleBackToMentors = () => {
    setSelectedMentor(null);
    setSelectedEnrollment(null);
    setMentorSessions([]);
    setFeedbackSession(null);
    setFeedbackRating(0);
    setFeedbackReviewText('');
    setIsFeedbackModalOpen(false);
  };

  const openFeedbackModal = (session) => {
    setFeedbackSession(session);
    setFeedbackRating(0);
    setFeedbackReviewText('');
    setIsFeedbackModalOpen(true);
  };

  const submitFeedback = async () => {
    if (!feedbackSession) return;
    if (feedbackRating < 1 || feedbackRating > 5) {
      toast.error('Please select a rating between 1 and 5 stars');
      return;
    }

    try {
      setSubmittingFeedback(true);
      await mentorsApi.addFeedback({
        sessionId: feedbackSession.id,
        rating: feedbackRating,
        reviewText: feedbackReviewText?.trim() || null,
      });

      toast.success('Feedback submitted successfully');
      setFeedbackBySessionId(prev => ({
        ...prev,
        [feedbackSession.id]: {
          sessionId: feedbackSession.id,
          rating: feedbackRating,
          reviewText: feedbackReviewText?.trim() || null,
          createdAt: new Date().toISOString(),
        },
      }));
      setIsFeedbackModalOpen(false);
      setFeedbackSession(null);
      setFeedbackRating(0);
      setFeedbackReviewText('');
      await fetchMentorSessions(selectedMentor.id);
      await fetchEnrolledMentors();
    } catch (err) {
      toast.error(err?.message || 'Failed to submit feedback');
    } finally {
      setSubmittingFeedback(false);
    }
  };

  const getSessionFee = (session) => session.sessionFee ?? selectedMentor?.sessionPrice ?? selectedMentor?.hourlyRate ?? 500;

  const handleSessionPayment = async (session) => {
    if (session.isPaid) return;

    setPaymentSession(session);
    setSelectedPaymentMethod('UPI');
    setGatewayState('select');
    setIsPaymentGatewayOpen(true);
  };

  const processGatewayPayment = async () => {
    if (!selectedEnrollment?.id || !paymentSession) {
      toast.error('Enrollment not found for payment');
      return;
    }

    try {
      setGatewayState('processing');
      setProcessingPaymentSessionId(paymentSession.id);

      // Simulate payment gateway processing
      await new Promise((resolve) => setTimeout(resolve, 2200));

      await mentorsApi.processPayment(
        selectedEnrollment.id,
        selectedPaymentMethod,
        getSessionFee(paymentSession),
        paymentSession.id
      );

      setGatewayState('success');
      toast.success(`Payment of ₹${getSessionFee(paymentSession)} completed`);
      await fetchMentorSessions(selectedMentor.id);
      const enrollmentResponse = await mentorsApi.isEnrolledWithMentor(selectedMentor.id);
      setSelectedEnrollment(enrollmentResponse?.data || null);

      setTimeout(() => {
        setIsPaymentGatewayOpen(false);
        setPaymentSession(null);
        setGatewayState('select');
      }, 900);
    } catch (err) {
      setGatewayState('select');
      toast.error(err?.message || 'Payment failed');
    } finally {
      setProcessingPaymentSessionId(null);
    }
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    const now = new Date();
    const tomorrow = new Date(now);
    tomorrow.setDate(tomorrow.getDate() + 1);

    const isToday = date.toDateString() === now.toDateString();
    const isTomorrow = date.toDateString() === tomorrow.toDateString();

    if (isToday) return 'Today';
    if (isTomorrow) return 'Tomorrow';
    
    return date.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' });
  };

  const formatTime = (dateString) => {
    return new Date(dateString).toLocaleTimeString('en-US', { 
      hour: 'numeric', 
      minute: '2-digit', 
      hour12: true 
    });
  };

  const getSessionStatus = (session) => {
    const now = new Date();
    const sessionDate = new Date(session.scheduledDateTime);
    
    if (session.status === 'Cancelled') {
      return { label: 'Cancelled', color: 'bg-red-100 text-red-800 border-red-300' };
    }
    if (session.status === 'Completed') {
      return { label: 'Completed', color: 'bg-green-100 text-green-800 border-green-300' };
    }
    if (session.status === 'Pending') {
      return { label: 'Pending Approval', color: 'bg-yellow-100 text-yellow-800 border-yellow-300' };
    }
    if (sessionDate < now) {
      return { label: 'Past', color: 'bg-gray-100 text-gray-800 border-gray-300' };
    }
    return { label: 'Upcoming', color: 'bg-blue-100 text-blue-800 border-blue-300' };
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin mx-auto text-primary" />
          <p className="mt-4 text-muted-foreground">Loading your mentors...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background p-6">
      <div className="max-w-7xl mx-auto">
        <AnimatePresence mode="wait">
          {!selectedMentor ? (
            <motion.div
              key="mentors-list"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -20 }}
              transition={{ duration: 0.3 }}
            >
              <div className="mb-6">
                <h1 className="text-3xl font-bold text-foreground flex items-center gap-3">
                  <User className="w-8 h-8 text-primary" />
                  My Mentors
                </h1>
                <p className="text-muted-foreground mt-2">
                  View your enrolled mentors and their scheduled sessions
                </p>
              </div>

              {enrolledMentors.length === 0 ? (
                <Card className="shadow-soft border-border/50">
                  <CardContent className="py-12 text-center">
                    <User className="w-16 h-16 text-muted-foreground/50 mx-auto mb-4" />
                    <p className="text-muted-foreground text-lg">No mentors enrolled yet</p>
                    <p className="text-muted-foreground/70 text-sm mt-2">
                      Browse mentors and enroll to start your learning journey
                    </p>
                    <Button 
                      className="mt-4" 
                      onClick={() => window.location.href = '/mentors'}
                    >
                      Browse Mentors
                    </Button>
                  </CardContent>
                </Card>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                  {enrolledMentors.map((mentor) => (
                    <motion.div
                      key={mentor.id}
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                    >
                      <Card 
                        className="shadow-soft border-border/50 hover:shadow-md transition-all cursor-pointer h-full"
                        onClick={() => handleMentorClick(mentor)}
                      >
                        <CardContent className="p-6">
                          <div className="flex items-start gap-4 mb-4">
                            <Avatar className="h-16 w-16">
                              <AvatarImage src={mentor.profileImageUrl} alt={mentor.userName} />
                              <AvatarFallback className="bg-primary/10 text-primary text-lg">
                                {mentor.userName?.charAt(0).toUpperCase()}
                              </AvatarFallback>
                            </Avatar>
                            <div className="flex-1 min-w-0">
                              <h3 className="font-semibold text-lg text-foreground truncate">
                                {mentor.userName}
                              </h3>
                              <p className="text-sm text-muted-foreground truncate">
                                {mentor.expertise}
                              </p>
                              <div className="flex items-center gap-2 mt-2">
                                <Badge variant="secondary" className="text-xs">
                                  ⭐ {mentor.avgRating?.toFixed(1) || 'N/A'}
                                </Badge>
                                <Badge variant="outline" className="text-xs">
                                  ₹{mentor.hourlyRate}/hr
                                </Badge>
                              </div>
                            </div>
                          </div>
                          
                          {mentor.skills && mentor.skills.length > 0 && (
                            <div className="flex flex-wrap gap-2 mb-4">
                              {mentor.skills.slice(0, 3).map((skill, index) => (
                                <Badge key={index} variant="outline" className="text-xs">
                                  {skill}
                                </Badge>
                              ))}
                              {mentor.skills.length > 3 && (
                                <Badge variant="outline" className="text-xs">
                                  +{mentor.skills.length - 3}
                                </Badge>
                              )}
                            </div>
                          )}

                          <div className="flex items-center justify-between pt-4 border-t border-border">
                            <span className="text-sm text-muted-foreground">
                              View Sessions
                            </span>
                            <ChevronRight className="w-5 h-5 text-primary" />
                          </div>
                        </CardContent>
                      </Card>
                    </motion.div>
                  ))}
                </div>
              )}
            </motion.div>
          ) : (
            <motion.div
              key="sessions-view"
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
              transition={{ duration: 0.3 }}
            >
              <div className="mb-6">
                <Button
                  variant="ghost"
                  onClick={handleBackToMentors}
                  className="mb-4"
                >
                  <ArrowLeft className="w-4 h-4 mr-2" />
                  Back to Mentors
                </Button>

                <div className="flex items-center gap-4 mb-4">
                  <Avatar className="h-16 w-16">
                    <AvatarImage src={selectedMentor.profileImageUrl} alt={selectedMentor.userName} />
                    <AvatarFallback className="bg-primary/10 text-primary text-xl">
                      {selectedMentor.userName?.charAt(0).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                  <div>
                    <h1 className="text-3xl font-bold text-foreground">
                      {selectedMentor.userName}
                    </h1>
                    <p className="text-muted-foreground">
                      {selectedMentor.expertise}
                    </p>
                  </div>
                </div>

                <h2 className="text-xl font-semibold text-foreground flex items-center gap-2 mt-6">
                  <Calendar className="w-6 h-6 text-primary" />
                  Scheduled Sessions
                </h2>
              </div>

              {sessionsLoading ? (
                <div className="flex items-center justify-center py-12">
                  <Loader2 className="h-8 w-8 animate-spin text-primary" />
                </div>
              ) : mentorSessions.length === 0 ? (
                <Card className="shadow-soft border-border/50">
                  <CardContent className="py-12 text-center">
                    <Calendar className="w-16 h-16 text-muted-foreground/50 mx-auto mb-4" />
                    <p className="text-muted-foreground text-lg">No sessions scheduled</p>
                    <p className="text-muted-foreground/70 text-sm mt-2">
                      Book a session with this mentor to get started
                    </p>
                  </CardContent>
                </Card>
              ) : (
                <div className="space-y-4">
                  {mentorSessions.map((session) => {
                    const status = getSessionStatus(session);
                    const sessionDate = new Date(session.scheduledDateTime);
                    const isFeedbackEligible = sessionDate <= new Date() && session.status !== 'Cancelled' && session.status !== 'Pending';
                    const submittedFeedback = feedbackBySessionId[session.id];
                    return (
                      <motion.div
                        key={session.id}
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.2 }}
                      >
                        <Card className="shadow-soft border-border/50 hover:shadow-md transition-shadow">
                          <CardContent className="p-6">
                            <div className="flex items-start justify-between gap-4">
                              <div className="flex-1">
                                <div className="flex items-center gap-3 mb-3">
                                  <Badge className={`${status.color} border`}>
                                    {status.label}
                                  </Badge>
                                  <h3 className="font-semibold text-lg text-foreground">
                                    {session.notes || 'Mentorship Session'}
                                  </h3>
                                </div>

                                <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground mb-4">
                                  <div className="flex items-center gap-2 bg-secondary/50 px-3 py-2 rounded-lg">
                                    <Calendar className="w-4 h-4 text-primary" />
                                    <span className="font-medium">{formatDate(session.scheduledDateTime)}</span>
                                  </div>
                                  <div className="flex items-center gap-2 bg-secondary/50 px-3 py-2 rounded-lg">
                                    <Clock className="w-4 h-4 text-primary" />
                                    <span className="font-medium">{formatTime(session.scheduledDateTime)}</span>
                                  </div>
                                  <Badge variant="outline" className="text-xs">
                                    Duration: {session.durationMinutes} min
                                  </Badge>
                                  <Badge variant="outline" className="text-xs">
                                    Fee: ₹{getSessionFee(session)}
                                  </Badge>
                                </div>

                                <div className="flex items-center gap-2 mb-4">
                                  <Badge
                                    className={session.isPaid ? 'bg-emerald-100 text-emerald-800 border border-emerald-200' : 'bg-rose-100 text-rose-800 border border-rose-200'}
                                  >
                                    {session.isPaid ? 'Paid' : (session.paymentStatus || 'Unpaid')}
                                  </Badge>
                                  {session.status === 'Approved' && !session.isPaid && (
                                    <Button
                                      size="sm"
                                      className="bg-cyan-500 hover:bg-cyan-600 text-white"
                                      onClick={() => handleSessionPayment(session)}
                                      disabled={processingPaymentSessionId === session.id}
                                    >
                                      {processingPaymentSessionId === session.id ? (
                                        <>
                                          <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                                          Processing...
                                        </>
                                      ) : (
                                        <>💳 Pay Now</>
                                      )}
                                    </Button>
                                  )}

                                  {isFeedbackEligible && !submittedFeedback && (
                                    <Button
                                      size="sm"
                                      variant="outline"
                                      className="border-amber-300 text-amber-700 hover:bg-amber-50"
                                      onClick={() => openFeedbackModal(session)}
                                    >
                                      <MessageSquare className="w-4 h-4 mr-2" />
                                      Add Feedback
                                    </Button>
                                  )}
                                </div>

                                {submittedFeedback && (
                                  <div className="mb-4 p-3 rounded-lg border border-amber-200 bg-amber-50/60">
                                    <div className="flex items-center justify-between gap-3">
                                      <p className="text-sm font-medium text-foreground">Your Feedback</p>
                                      <div className="flex items-center gap-1">
                                        {[1, 2, 3, 4, 5].map((star) => (
                                          <Star
                                            key={star}
                                            className={`w-4 h-4 ${star <= submittedFeedback.rating ? 'fill-amber-400 text-amber-400' : 'text-slate-300'}`}
                                          />
                                        ))}
                                      </div>
                                    </div>
                                    {submittedFeedback.reviewText && (
                                      <p className="text-sm text-muted-foreground mt-2">{submittedFeedback.reviewText}</p>
                                    )}
                                  </div>
                                )}

                                <div className="text-sm text-muted-foreground">
                                  <p className="mb-1">
                                    <span className="font-medium text-foreground">Session ID:</span> {session.id.substring(0, 8)}
                                  </p>
                                  {session.notes && (
                                    <p className="mb-1">
                                      <span className="font-medium text-foreground">Notes:</span> {session.notes}
                                    </p>
                                  )}
                                  <p>
                                    <span className="font-medium text-foreground">Created:</span>{' '}
                                    {new Date(session.createdAt).toLocaleDateString()}
                                  </p>
                                </div>

                                {session.meetingLink && session.status === 'Approved' && (
                                  <a
                                    href={session.meetingLink}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="mt-4"
                                  >
                                    <Button className="bg-primary hover:bg-primary/90">
                                      <Video className="w-4 h-4 mr-2" />
                                      Join Meeting
                                    </Button>
                                  </a>
                                )}
                              </div>
                            </div>
                          </CardContent>
                        </Card>
                      </motion.div>
                    );
                  })}
                </div>
              )}
            </motion.div>
          )}
        </AnimatePresence>

        <Dialog
          open={isPaymentGatewayOpen}
          onOpenChange={(open) => {
            if (gatewayState === 'processing') return;
            setIsPaymentGatewayOpen(open);
            if (!open) {
              setPaymentSession(null);
              setGatewayState('select');
            }
          }}
        >
          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <CreditCard className="w-5 h-5 text-primary" />
                Payment Gateway Simulation
              </DialogTitle>
              <DialogDescription>
                Complete secure payment for this session.
              </DialogDescription>
            </DialogHeader>

            {paymentSession && (
              <div className="space-y-4">
                <div className="rounded-lg border border-border p-3 bg-secondary/30 text-sm">
                  <p><span className="font-medium">Mentor:</span> {selectedMentor?.userName}</p>
                  <p><span className="font-medium">Session:</span> {formatDate(paymentSession.scheduledDateTime)} at {formatTime(paymentSession.scheduledDateTime)}</p>
                  <p><span className="font-medium">Amount:</span> ₹{getSessionFee(paymentSession)}</p>
                </div>

                {gatewayState === 'select' && (
                  <>
                    <div className="space-y-2">
                      <p className="text-sm font-medium">Select payment method</p>
                      <div className="grid grid-cols-3 gap-2">
                        {['UPI', 'Card', 'NetBanking'].map((method) => (
                          <Button
                            key={method}
                            type="button"
                            variant={selectedPaymentMethod === method ? 'default' : 'outline'}
                            onClick={() => setSelectedPaymentMethod(method)}
                            className="h-9"
                          >
                            {method}
                          </Button>
                        ))}
                      </div>
                    </div>

                    <div className="flex justify-end gap-2 pt-2">
                      <Button variant="outline" onClick={() => setIsPaymentGatewayOpen(false)}>
                        Cancel
                      </Button>
                      <Button onClick={processGatewayPayment} className="bg-cyan-500 hover:bg-cyan-600 text-white">
                        Pay ₹{getSessionFee(paymentSession)}
                      </Button>
                    </div>
                  </>
                )}

                {gatewayState === 'processing' && (
                  <div className="flex flex-col items-center gap-3 py-6">
                    <Loader2 className="w-8 h-8 animate-spin text-primary" />
                    <p className="text-sm text-muted-foreground">Processing payment securely...</p>
                  </div>
                )}

                {gatewayState === 'success' && (
                  <div className="flex flex-col items-center gap-3 py-6">
                    <CheckCircle className="w-8 h-8 text-emerald-500" />
                    <p className="text-sm font-medium">Payment completed successfully</p>
                  </div>
                )}
              </div>
            )}
          </DialogContent>
        </Dialog>

        <Dialog
          open={isFeedbackModalOpen}
          onOpenChange={(open) => {
            if (submittingFeedback) return;
            setIsFeedbackModalOpen(open);
            if (!open) {
              setFeedbackSession(null);
              setFeedbackRating(0);
              setFeedbackReviewText('');
            }
          }}
        >
          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <MessageSquare className="w-5 h-5 text-primary" />
                Submit Feedback
              </DialogTitle>
              <DialogDescription>
                Rate your mentorship session with {selectedMentor?.userName}.
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-4">
              {feedbackSession && (
                <div className="rounded-lg border border-border p-3 bg-secondary/30 text-sm">
                  <p><span className="font-medium">Session:</span> {formatDate(feedbackSession.scheduledDateTime)} at {formatTime(feedbackSession.scheduledDateTime)}</p>
                  <p><span className="font-medium">Topic:</span> {feedbackSession.notes || 'Mentorship Session'}</p>
                </div>
              )}

              <div>
                <p className="text-sm font-medium mb-2">Your Rating</p>
                <div className="flex items-center gap-2">
                  {[1, 2, 3, 4, 5].map((star) => (
                    <button
                      key={star}
                      type="button"
                      className="p-1"
                      onClick={() => setFeedbackRating(star)}
                      disabled={submittingFeedback}
                    >
                      <Star
                        className={`w-7 h-7 transition-colors ${star <= feedbackRating ? 'fill-amber-400 text-amber-400' : 'text-slate-300 hover:text-amber-300'}`}
                      />
                    </button>
                  ))}
                  <span className="text-sm text-muted-foreground ml-2">
                    {feedbackRating > 0 ? `${feedbackRating}/5` : 'Select rating'}
                  </span>
                </div>
              </div>

              <div>
                <p className="text-sm font-medium mb-2">Review (optional)</p>
                <Textarea
                  placeholder="Share your experience with this mentor"
                  value={feedbackReviewText}
                  onChange={(e) => setFeedbackReviewText(e.target.value)}
                  disabled={submittingFeedback}
                  className="min-h-[110px]"
                />
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <Button
                  variant="outline"
                  onClick={() => setIsFeedbackModalOpen(false)}
                  disabled={submittingFeedback}
                >
                  Cancel
                </Button>
                <Button
                  onClick={submitFeedback}
                  disabled={submittingFeedback || feedbackRating < 1}
                  className="bg-cyan-500 hover:bg-cyan-600 text-white"
                >
                  {submittingFeedback ? (
                    <>
                      <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                      Submitting...
                    </>
                  ) : (
                    'Submit Feedback'
                  )}
                </Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
};

export default MyMentors;
