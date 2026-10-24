import { apiClient } from './axios';

// =============================================
// Progress API Service
// =============================================

export const progressApi = {
  /**
   * Get user's progress
   */
  getMyProgress: async () => {
    const response = await apiClient.get('/progress/me');
    return response.data;
  },

  /**
   * Mark a skill as completed
   */
  completeSkill: async (skillId) => {
    const response = await apiClient.post('/progress/complete-skill', { skillId });
    return response.data;
  },

  /**
   * Get user's badges
   */
  getBadges: async () => {
    const response = await apiClient.get('/progress/badges');
    return response.data;
  },

  /**
   * Get user's completed skills
   */
  getCompletedSkills: async () => {
    const response = await apiClient.get('/progress/completed-skills');
    return response.data;
  },

  /**
   * Get overall statistics
   */
  getStats: async () => {
    const response = await apiClient.get('/progress/stats');
    return response.data;
  },

  /**
   * Get leaderboard rankings
   */
  getLeaderboard: async (limit = 10) => {
    const response = await apiClient.get(`/progress/leaderboard?limit=${limit}`);
    return response.data;
  },

  /**
   * Update progress for a domain
   */
  updateDomainProgress: async (domainId, progress) => {
    const response = await apiClient.put(`/progress/domain/${domainId}`, progress);
    return response.data;
  },
};

export default progressApi;
