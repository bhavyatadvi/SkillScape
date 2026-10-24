import apiClient from './axios';

/**
 * Career Profile API service.
 * Uses the existing authenticated axios instance — JWT is automatically included.
 */

export const careerProfileApi = {
  /**
   * Get the authenticated user's central Career Profile.
   * Returns null-like (404 error) if not yet created.
   */
  getProfile: () =>
    apiClient.get('/career-profile'),

  /**
   * Create a new Career Profile for the authenticated user.
   * @param {Object} profile - SaveCareerProfileRequest shape
   */
  createProfile: (profile) =>
    apiClient.post('/career-profile', profile),

  /**
   * Update the authenticated user's existing Career Profile.
   * @param {Object} profile - SaveCareerProfileRequest shape
   */
  updateProfile: (profile) =>
    apiClient.put('/career-profile', profile),

  /**
   * Delete the authenticated user's Career Profile.
   */
  deleteProfile: () =>
    apiClient.delete('/career-profile'),

  /**
   * Upload a resume file (PDF/DOCX) and receive extracted Career Profile data.
   * Does NOT save to DB — frontend is responsible for review + save.
   * @param {File} file - The resume file object
   */
  uploadResume: (file) => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.post('/career-profile/resume', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 60000, // 60s timeout for AI parsing
    });
  },
};

export default careerProfileApi;
