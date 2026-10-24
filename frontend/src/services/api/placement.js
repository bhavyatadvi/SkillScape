import { apiClient } from './axios';

export const placementService = {
  getAvailableRoles: async () => {
    const response = await apiClient.get('/placement/roles');
    return response.data.data;
  },

  getAssessmentQuestions: async (role) => {
    const response = await apiClient.get(`/placement/assessment/${encodeURIComponent(role)}`);
    return response.data.data;
  },

  submitAssessment: async (targetRole, answers) => {
    const response = await apiClient.post('/placement/assessment/submit', {
      targetRole,
      answers
    });
    return response.data.data;
  },

  getMyHistory: async () => {
    const response = await apiClient.get('/placement/history');
    return response.data.data;
  },

  getAssessmentResult: async (assessmentId) => {
    const response = await apiClient.get(`/placement/assessment/result/${assessmentId}`);
    return response.data.data;
  }
};
