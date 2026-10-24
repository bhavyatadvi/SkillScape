import { apiClient } from './axios';

// =============================================
// Resume API Service
// =============================================

export const resumeApi = {
  /**
   * Generate resume PDF
   */
  generate: async () => {
    const response = await apiClient.get('/resume/generate', {
      responseType: 'blob',
    });
    return response.data;
  },

  /**
   * Download generated resume
   */
  download: async () => {
    const blob = await resumeApi.generate();
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `resume-${new Date().getTime()}.pdf`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  },

  /**
   * Preview resume data
   */
  preview: async () => {
    const response = await apiClient.get('/resume/preview');
    return response.data;
  },

  /**
   * Get resume template options
   */
  getTemplates: async () => {
    const response = await apiClient.get('/resume/templates');
    return response.data;
  },
};

export default resumeApi;
