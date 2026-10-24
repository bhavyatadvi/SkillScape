import { apiClient } from './axios';

// =============================================
// Domains API Service
// =============================================

export const domainsApi = {
  /**
   * Get all career domains
   */
  getAll: async () => {
    const response = await apiClient.get('/domains');
    return response.data;
  },

  /**
   * Get domain by ID
   */
  getById: async (id) => {
    const response = await apiClient.get(`/domains/${id}`);
    return response.data;
  },

  /**
   * Get roadmap for a specific domain
   */
  getRoadmap: async (id) => {
    const response = await apiClient.get(`/domains/${id}/roadmap`);
    return response.data;
  },

  /**
   * Get skills for a specific domain
   */
  getSkills: async (id) => {
    const response = await apiClient.get(`/domains/${id}/skills`);
    return response.data;
  },

  /**
   * Get recommended domain for user
   */
  getRecommended: async () => {
    const response = await apiClient.get('/domains/recommended');
    return response.data;
  },
};

export default domainsApi;
