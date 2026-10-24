import { apiClient } from './axios';

export const notificationsApi = {
  getNotifications: async () => {
    const res = await apiClient.get('/notifications');
    return res.data;
  },
  markAllRead: async () => {
    const res = await apiClient.post('/notifications/mark-all-read');
    return res.data;
  }
};
