import { apiClient } from './axios';

export const chatApi = {
    // Get chat history with another user
    getHistory: (otherUserId, page = 1, limit = 50) =>
        apiClient.get(`/chat/history/${otherUserId}?page=${page}&limit=${limit}`),

    // Upload image to chat
    uploadImage: (formData) =>
        apiClient.post('/chat/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        }),

    // Get all conversations for current user
    getConversations: () =>
        apiClient.get('/chat/conversations'),

    // Delete full chat history with another user
    clearHistory: (otherUserId) =>
        apiClient.delete(`/chat/history/${otherUserId}`),

    // Mark messages from another user as read
    markAsRead: (otherUserId) =>
        apiClient.post(`/chat/mark-read/${otherUserId}`)
};
