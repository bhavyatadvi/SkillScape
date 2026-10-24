import { apiClient } from './axios';

export const peerRoomsApi = {
  getRooms: async () => {
    const response = await apiClient.get('/peerrooms');
    return response.data;
  },

  getRoom: async (roomId) => {
    const response = await apiClient.get(`/peerrooms/${roomId}`);
    return response.data;
  },

  createRoom: async (data) => {
    const response = await apiClient.post('/peerrooms', data);
    return response.data;
  },

  requestToJoin: async (roomId) => {
    const response = await apiClient.post(`/peerrooms/${roomId}/join`);
    return response.data;
  },

  approveParticipant: async (roomId, participantId) => {
    const response = await apiClient.put(`/peerrooms/${roomId}/participants/${participantId}/approve`);
    return response.data;
  },

  rejectParticipant: async (roomId, participantId) => {
    const response = await apiClient.put(`/peerrooms/${roomId}/participants/${participantId}/reject`);
    return response.data;
  },

  createTask: async (roomId, data) => {
    const response = await apiClient.post(`/peerrooms/${roomId}/tasks`, data);
    return response.data;
  },

  toggleTask: async (roomId, taskId) => {
    const response = await apiClient.put(`/peerrooms/${roomId}/tasks/${taskId}/toggle`);
    return response.data;
  },

  updateTask: async (roomId, taskId, data) => {
    const response = await apiClient.put(`/peerrooms/${roomId}/tasks/${taskId}`, data);
    return response.data;
  },

  deleteTask: async (roomId, taskId) => {
    const response = await apiClient.delete(`/peerrooms/${roomId}/tasks/${taskId}`);
    return response.data;
  },

  removeParticipant: async (roomId, participantId) => {
    const response = await apiClient.delete(`/peerrooms/${roomId}/participants/${participantId}`);
    return response.data;
  },

  saveWorkspace: async (roomId, data) => {
    const response = await apiClient.put(`/peerrooms/${roomId}/notes`, data);
    return response.data;
  },

  updateDescription: async (roomId, description) => {
    const response = await apiClient.put(`/peerrooms/${roomId}/description`, { description });
    return response.data;
  },

  updateMeetingUrl: async (roomId, meetingUrl) => {
    const response = await apiClient.put(`/peerrooms/${roomId}/meeting-link`, { meetingUrl });
    return response.data;
  },

  updateMeetingStatus: async (roomId, isMeetingLive, meetingUrl = null) => {
    const response = await apiClient.put(`/peerrooms/${roomId}/meeting-status`, { isMeetingLive, meetingUrl });
    return response.data;
  },
};

