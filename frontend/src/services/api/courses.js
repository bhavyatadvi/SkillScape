import api from './axios';

export const courseApi = {
  searchCourses: async (filters) => {
    // Construct query parameters
    const params = new URLSearchParams();
    if (filters.query) params.append('query', filters.query);
    if (filters.domain) params.append('domain', filters.domain);
    if (filters.level) params.append('level', filters.level);
    if (filters.type) params.append('type', filters.type);
    if (filters.isFree !== undefined && filters.isFree !== null && filters.isFree !== '') params.append('isFree', filters.isFree);
    if (filters.maxPrice) params.append('maxPrice', filters.maxPrice);
    if (filters.duration) params.append('duration', filters.duration);

    const response = await api.get(`/courses/search?${params.toString()}`);
    return response.data;
  },

  getTrendingCourses: async () => {
    const response = await api.get('/courses/trending');
    return response.data;
  },

  getRecommendedCourses: async () => {
    const response = await api.get('/courses/recommended');
    return response.data;
  },

  getTrendingSkills: async () => {
    const response = await api.get('/courses/trending-skills');
    return response.data;
  },

  getCourseDetails: async (id) => {
    const response = await api.get(`/courses/${id}`);
    return response.data;
  },

  toggleSaveCourse: async (id) => {
    const response = await api.post(`/courses/${id}/save`);
    return response.data;
  },

  getSavedCourses: async () => {
    const response = await api.get('/courses/saved');
    return response.data;
  }
};
