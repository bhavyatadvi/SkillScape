import { apiClient } from './axios';

export const peerMatchingApi = {
  getProfile: async () => {
    const response = await apiClient.get('/peermatching/profile');
    return response.data;
  },

  updateProfile: async (data) => {
    const response = await apiClient.put('/peermatching/profile', data);
    return response.data;
  },

  getRecommendations: async () => {
    const response = await apiClient.get('/peermatching/recommendations');
    return response.data;
  },

  createCircleRoom: async (data) => {
    const response = await apiClient.post('/peermatching/create-circle-room', data);
    return response.data;
  },
};
