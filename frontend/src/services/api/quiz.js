import { apiClient } from './axios';

// =============================================
// Quiz API Service
// =============================================

export const quizApi = {
  /**
   * Get all quiz questions
   */
  getQuestions: async () => {
    const response = await apiClient.get('/quiz/questions');
    return response.data?.data;
  },

  /**
   * Get next quiz question based on current answers
   */
  getNextQuestion: async (answers) => {
    const response = await apiClient.post('/quiz/next', answers);
    return response.data?.data;
  },

  /**
   * Submit quiz answers and get result
   */
  submit: async (answers) => {
    const response = await apiClient.post('/quiz/submit', answers);
    return response.data?.data;
  },

  /**
   * Confirm and save the quiz result
   */
  confirm: async (data) => {
    const response = await apiClient.post('/quiz/confirm', data);
    return response.data?.data;
  },

  /**
   * Get user's quiz result
   */
  getResult: async (userId) => {
    const response = await apiClient.get(`/quiz/result/${userId}`);
    return response.data?.data;
  },

  /**
   * Get latest quiz result for current user
   */
  getMyResult: async () => {
    const response = await apiClient.get('/quiz/result');
    return response.data;
  },

  /**
   * Get all quiz results history for current user
   */
  getResults: async () => {
    const response = await apiClient.get('/quiz/results');
    return response.data;
  },

  /**
   * Get a specific quiz result by ID
   */
  getResultById: async (resultId) => {
    const response = await apiClient.get(`/quiz/results/${resultId}`);
    return response.data;
  },

  /**
   * Delete a specific quiz result by ID
   */
  deleteResult: async (resultId) => {
    const response = await apiClient.delete(`/quiz/results/${resultId}`);
    return response.data;
  },
};

export default quizApi;
