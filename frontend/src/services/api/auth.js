import { apiClient } from './axios';

// =============================================
// Helper function to unwrap ApiResponse
// =============================================
const unwrapData = (response, endpoint = 'auth') => {
  const data = response.data?.Data ?? response.data?.data ?? response.data;
  console.log(`[${endpoint}] Response unwrapped:`, data);
  return data;
};

// =============================================
// Authentication API Service
// =============================================

export const authApi = {
  /**
   * Login user with email and password
   */
  login: async (credentials) => {
    const response = await apiClient.post('/auth/login', credentials);
    console.log('[Auth] Login response:', response);
    return unwrapData(response, 'auth/login');
  },

  /**
   * Register new user
   */
  register: async (credentials) => {
    const response = await apiClient.post('/auth/register', credentials);
    console.log('[Auth] Register response:', response);
    return unwrapData(response, 'auth/register');
  },

  /**
   * Logout user
   */
  logout: async () => {
    // Backend doesn't have logout endpoint, just clear local data
    return Promise.resolve();
  },

  /**
   * Get current user profile
   */
  getCurrentUser: async () => {
    const response = await apiClient.get('/auth/me');
    console.log('[Auth] Get current user response:', response);
    return unwrapData(response, 'auth/me');
  },

  /**
   * Refresh access token
   */
  refreshToken: async () => {
    const response = await apiClient.post('/auth/refresh');
    console.log('[Auth] Refresh token response:', response);
    return unwrapData(response, 'auth/refresh');
  },

  /**
   * Request password reset
   */
  forgotPassword: async (email) => {
    const response = await apiClient.post('/auth/forgot-password', { email });
    return unwrapData(response, 'auth/forgot-password');
  },

  /**
   * Reset password with token
   */
  resetPassword: async (email, token, password) => {
    const response = await apiClient.post('/auth/reset-password', { email, token, password });
    return unwrapData(response, 'auth/reset-password');
  },

  /**
   * Update user profile
   */
  updateProfile: async (data) => {
    const response = await apiClient.patch('/auth/profile', data);
    console.log('[Auth] Update profile response:', response);
    return unwrapData(response, 'auth/profile');
  },

  /**
   * Change password
   */
  changePassword: async (currentPassword, newPassword) => {
    const response = await apiClient.post('/auth/change-password', { currentPassword, newPassword });
    return unwrapData(response, 'auth/change-password');
  },

  /**
   * Upload user avatar
   */
  uploadAvatar: async (formData) => {
    const response = await apiClient.post('/auth/upload-avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    return unwrapData(response, 'auth/upload-avatar');
  },

  /**
   * Delete current user account
   */
  deleteAccount: async () => {
    const response = await apiClient.delete('/auth/me');
    return unwrapData(response, 'auth/delete');
  },
};

export default authApi;
