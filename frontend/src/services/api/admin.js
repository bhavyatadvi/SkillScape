import { apiClient } from './axios';

const unwrap = (response) => {
  return response.data?.Data ?? response.data?.data ?? response.data;
};

export const adminApi = {
  getDashboard: async () => {
    const response = await apiClient.get('/admin/dashboard');
    return unwrap(response);
  },

  getUsers: async (params = {}) => {
    const response = await apiClient.get('/admin/users', { params });
    return unwrap(response) || [];
  },

  blockUser: async (payload) => {
    const response = await apiClient.put('/admin/block-user', payload);
    return unwrap(response);
  },

  updateUserRole: async (payload) => {
    const response = await apiClient.put('/admin/users/role', payload);
    return unwrap(response);
  },

  getAnalytics: async () => {
    const response = await apiClient.get('/admin/analytics');
    return unwrap(response);
  },

  getQuizQuestions: async () => {
    const response = await apiClient.get('/admin/quiz/questions');
    return unwrap(response) || [];
  },

  createQuizQuestion: async (payload) => {
    const response = await apiClient.post('/admin/quiz/questions', payload);
    return unwrap(response);
  },

  updateQuizQuestion: async (questionId, payload) => {
    const response = await apiClient.put(`/admin/quiz/questions/${questionId}`, payload);
    return unwrap(response);
  },

  deleteQuizQuestion: async (questionId) => {
    const response = await apiClient.delete(`/admin/quiz/questions/${questionId}`);
    return unwrap(response);
  },

  getRoadmapModules: async (domainId) => {
    const response = await apiClient.get('/admin/roadmap/modules', {
      params: domainId ? { domainId } : {},
    });
    return unwrap(response) || [];
  },

  createRoadmapModule: async (payload) => {
    const response = await apiClient.post('/admin/roadmap/modules', payload);
    return unwrap(response);
  },

  updateRoadmapModule: async (moduleId, payload) => {
    const response = await apiClient.put(`/admin/roadmap/modules/${moduleId}`, payload);
    return unwrap(response);
  },

  deleteRoadmapModule: async (moduleId) => {
    const response = await apiClient.delete(`/admin/roadmap/modules/${moduleId}`);
    return unwrap(response);
  },

  getResumes: async () => {
    const response = await apiClient.get('/admin/resumes');
    return unwrap(response) || [];
  },

  getResumeByUserId: async (userId) => {
    const response = await apiClient.get(`/admin/resumes/${userId}`);
    return unwrap(response);
  },

  getProgressSnapshot: async () => {
    const response = await apiClient.get('/admin/progress/snapshot');
    return unwrap(response) || [];
  },

  getPeerRooms: async () => {
    const response = await apiClient.get('/admin/peer-rooms');
    return unwrap(response) || [];
  },

  getPeerRoomDetails: async (roomId) => {
    const response = await apiClient.get(`/admin/peer-rooms/${roomId}`);
    return unwrap(response);
  },

  setPeerRoomModeration: async (roomId, payload) => {
    const response = await apiClient.put(`/admin/peer-rooms/${roomId}/moderation`, payload);
    return unwrap(response);
  },

  getHierarchicalQuestions: async () => {
    const response = await apiClient.get('/admin/hierarchical-questions');
    return unwrap(response) || [];
  },

  createHierarchicalQuestion: async (data) => {
    const response = await apiClient.post('/admin/hierarchical-questions', data);
    return unwrap(response);
  },

  updateHierarchicalQuestion: async (id, data) => {
    const response = await apiClient.put(`/admin/hierarchical-questions/${id}`, data);
    return unwrap(response);
  },

  deleteHierarchicalQuestion: async (id) => {
    const response = await apiClient.delete(`/admin/hierarchical-questions/${id}`);
    return unwrap(response);
  },
};

export default adminApi;
