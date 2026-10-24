import { apiClient } from './axios';

// =============================================
// Sessions API
// =============================================

export const sessionsApi = {
  // Get all sessions for the logged-in user (mentor or student)
  getMySessions: async () => {
    const response = await apiClient.get('/session/my-sessions');
    return response.data;
  },

  // Get upcoming sessions from enrolled mentors (student only)
  getUpcomingSessions: async () => {
    const response = await apiClient.get('/session/upcoming');
    return response.data;
  },

  // Book a new session
  bookSession: async (data) => {
    const response = await apiClient.post('/session/book', data);
    return response.data;
  },

  // Mentor creates a session for a student
  createMentorSession: async (data) => {
    const response = await apiClient.post('/session/mentor-create', data);
    return response.data;
  },

  // Approve/reject a session (mentor only)
  approveSession: async (data) => {
    const response = await apiClient.put('/session/approve', data);
    return response.data;
  },

  // Report a complaint about a session
  reportComplaint: async (data) => {
    const response = await apiClient.post('/session/complaint', data);
    return response.data;
  },

  // Get a specific session by ID
  getSessionById: async (sessionId) => {
    const response = await apiClient.get(`/session/${sessionId}`);
    return response.data;
  },

  // Update a session (mentor only)
  updateSession: async (sessionId, data) => {
    const response = await apiClient.post(`/session/update/${sessionId}`, data);
    return response.data;
  },

  // Delete/cancel a session (mentor only)
  deleteSession: async (sessionId) => {
    const response = await apiClient.delete(`/session/${sessionId}`);
    return response.data;
  },
};
