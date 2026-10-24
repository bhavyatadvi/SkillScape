import { apiClient } from './axios';

// =============================================
// Mentors API Service
// =============================================

export const mentorsApi = {
  /**
   * Get all mentors
   */
  getAll: async (filters = {}) => {
    const response = await apiClient.get('/mentors', { params: filters });
    return response.data;
  },

  /**
   * Get detailed mentor profiles for users (same source as mentor profile edit table)
   */
  getAllDetailedForUsers: async () => {
    const response = await apiClient.get('/mentors/users/detailed');
    return response.data;
  },

  /**
   * Apply as mentor
   */
  applyAsMentor: async (mentorData) => {
    const response = await apiClient.post('/mentors/apply', mentorData);
    return response.data;
  },

  /**
   * Get mentor by ID
   */
  getById: async (id) => {
    const response = await apiClient.get(`/mentors/${id}`);
    return response.data;
  },

  /**
   * Send a mentor request
   */
  requestMentor: async (mentorIdOrPayload, message = '') => {
    const payload =
      typeof mentorIdOrPayload === 'object' && mentorIdOrPayload !== null
        ? mentorIdOrPayload
        : { mentorId: mentorIdOrPayload, message };

    const response = await apiClient.post('/mentors/request', payload);
    return response.data;
  },

  /**
   * Get mentor requests (for students)
   */
  getMyRequests: async () => {
    const response = await apiClient.get('/mentors/my-requests');
    return response.data;
  },

  /**
   * Get received requests (for mentors)
   */
  getReceivedRequests: async () => {
    const response = await apiClient.get('/mentors/requests/pending');
    return response.data;
  },

  /**
   * Accept a mentor request
   */
  acceptRequest: async (requestId) => {
    const response = await apiClient.patch(`/mentors/requests/${requestId}`, {
      status: 'Accepted',
    });
    return response.data;
  },

  /**
   * Decline a mentor request
   */
  declineRequest: async (requestId) => {
    const response = await apiClient.patch(`/mentors/requests/${requestId}`, {
      status: 'Rejected',
    });
    return response.data;
  },

  /**
   * Update mentor request status
   */
  updateRequestStatus: async (requestId, status, scheduledAt = null) => {
    const response = await apiClient.patch(`/mentors/requests/${requestId}`, {
      status,
      scheduledAt,
    });
    return response.data;
  },

  /**
   * Search mentors by expertise
   */
  searchByExpertise: async (expertise) => {
    const response = await apiClient.get('/mentors/search', {
      params: { expertise },
    });
    return response.data;
  },

  /**
   * Get my notifications
   */
  getNotifications: async () => {
    const response = await apiClient.get('/mentors/notifications');
    return response.data;
  },

  /**
   * Mark notification as read
   */
  markNotificationAsRead: async (notificationId) => {
    const response = await apiClient.post(`/mentors/notifications/${notificationId}/read`);
    return response.data;
  },

  /**
   * Mark all notifications as read
   */
  markAllNotificationsAsRead: async () => {
    const response = await apiClient.post('/mentors/notifications/read-all');
    return response.data;
  },

  /**
   * Get mentor dashboard data
   */
  getDashboard: async () => {
    const response = await apiClient.get('/mentors/dashboard');
    return response.data;
  },

  /**
   * Get current mentor profile
   */
  getMyProfile: async () => {
    const response = await apiClient.get('/mentors/my-profile');
    return response.data;
  },

  /**
   * Save mentor profile
   */
  saveProfile: async (payload) => {
    const response = await apiClient.post('/mentors/create-profile', payload);
    return response.data;
  },

  /**
   * Enroll with mentor
   */
  enrollWithMentor: async (mentorId) => {
    const response = await apiClient.post(`/mentors/${mentorId}/enroll`);
    return response.data;
  },

  /**
   * Get enrolled mentors
   */
  getEnrolledMentors: async () => {
    const response = await apiClient.get('/mentors/student/enrolled');
    return response.data;
  },

  /**
   * Check if enrolled with mentor
   */
  isEnrolledWithMentor: async (mentorId) => {
    const response = await apiClient.get(`/mentors/${mentorId}/is-enrolled`);
    return response.data;
  },

  /**
   * Approve or reject enrollment
   */
  approveEnrollment: async (enrollmentId, isApproved, rejectionReason = '') => {
    const response = await apiClient.post(`/mentors/enrollments/${enrollmentId}/approve`, {
      isApproved,
      rejectionReason,
    });
    return response.data;
  },

  /**
   * Process payment for enrollment
   */
  processPayment: async (enrollmentId, paymentMethod, amount, sessionId = null) => {
    const response = await apiClient.post(`/mentors/enrollments/${enrollmentId}/pay`, {
      enrollmentId,
      paymentMethod,
      amount,
      sessionId,
    });
    return response.data;
  },

  /**
   * Add session feedback by student
   */
  addFeedback: async (payload) => {
    const response = await apiClient.post('/feedback/add', payload);
    return response.data;
  },

  /**
   * Get current student's submitted feedback
   */
  getMyFeedback: async () => {
    const response = await apiClient.get('/feedback/my');
    return response.data;
  },

  /**
   * Get student wallet balance
   */
  getStudentWallet: async () => {
    const response = await apiClient.get('/mentors/student-wallet');
    return response.data;
  },
};

export default mentorsApi;
