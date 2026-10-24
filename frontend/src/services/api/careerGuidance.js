import { apiClient } from './axios';

// Helper function to unwrap response Data
const unwrapData = (response, endpoint = 'careerGuidance') => {
  const data = response.data?.Data ?? response.data?.data ?? response.data;
  console.log(`[${endpoint}] Response unwrapped:`, data);
  return data;
};

export const careerGuidanceApi = {
  startAdaptiveQuiz: async (phaseKey, profile = {}, subPhaseKey = '', assessmentType = '') => {
    // If assessmentType is not explicitly provided, default to the phaseKey for backward compatibility
    const actualAssessmentType = assessmentType || phaseKey;
    const normalizedProfile = Object.fromEntries(
      Object.entries(profile).map(([key, value]) => [
        key,
        Array.isArray(value) ? value.join(',') : value == null ? '' : String(value)
      ])
    );
    const response = await apiClient.post('/quiz/start', {
      assessmentType: actualAssessmentType,
      phase: phaseKey,
      educationProfile: {
        phaseKey,
        subPhaseKey,
        fields: { ...normalizedProfile, ...(subPhaseKey ? { subPhaseKey } : {}) }
      }
    }, { timeout: 30000 }); // Extended timeout: LocalDB can take ~15 s to wake from idle
    return unwrapData(response, 'startAdaptiveQuiz');
  },

  answerAdaptiveQuiz: async (sessionId, questionId, optionIndex) => {
    const response = await apiClient.post(`/quiz/${sessionId}/answer`, {
      questionId,
      answerId: String(optionIndex)
    }, { timeout: 60000 });
    return unwrapData(response, 'answerAdaptiveQuiz');
  },

  completeAdaptiveQuiz: async (sessionId) => {
    const response = await apiClient.post(`/quiz/${sessionId}/complete`, null, { timeout: 60000 });
    return unwrapData(response, 'completeAdaptiveQuiz');
  },

  getLatestResult: async () => {
    const response = await apiClient.get('/quiz/result', { timeout: 30000 });
    return unwrapData(response, 'getLatestResult');
  },

  finalizeDecision: async (sessionId, chosenCareerId, careerTitle = null) => {
    const response = await apiClient.post('/quiz/finalize-decision', { sessionId, chosenCareerId, careerTitle });
    return unwrapData(response, 'finalizeDecision');
  },

  generateDynamicRoadmap: async (sessionId, payload) => {
    const response = await apiClient.post(`/quiz/${sessionId}/roadmap`, payload);
    return unwrapData(response, 'generateDynamicRoadmap');
  },

  /**
   * Get career paths hierarchy tree by academic stage/stream
   */
  getPathsTree: async (streamType) => {
    const response = await apiClient.get('/career-guidance/paths', {
      params: { streamType }
    });
    return unwrapData(response, 'getPathsTree');
  },

  /**
   * Get details of a single career path
   */
  getPathDetails: async (id) => {
    const response = await apiClient.get(`/career-guidance/paths/${id}`);
    return unwrapData(response, 'getPathDetails');
  },

  /**
   * Get adaptive quiz questions filtered by stream
   */
  getQuizQuestions: async (streamType) => {
    const response = await apiClient.get('/career-guidance/quiz/questions', {
      params: { streamType }
    });
    return unwrapData(response, 'getQuizQuestions');
  },

  /**
   * Submit quiz answers and retrieve recommendation profile
   */
  submitQuizAnswers: async (payload) => {
    const response = await apiClient.post('/career-guidance/quiz/submit', payload);
    return unwrapData(response, 'submitQuizAnswers');
  },

  /**
   * Get user career recommendation profile
   */
  getUserProfile: async () => {
    const response = await apiClient.get('/career-guidance/profile');
    return unwrapData(response, 'getUserProfile');
  },

  /**
   * Toggle bookmarking a career path
   */
  toggleBookmark: async (pathId) => {
    const response = await apiClient.post(`/career-guidance/bookmarks/${pathId}`);
    return unwrapData(response, 'toggleBookmark');
  },

  /**
   * Accept a career path and copy its roadmap to the user's active profile
   */
  acceptCareer: async (id) => {
    const response = await apiClient.post(`/career-guidance/accept/${id}`);
    return unwrapData(response, 'acceptCareer');
  },

  /**
   * Get user bookmarked career paths
   */
  getBookmarkedPaths: async () => {
    const response = await apiClient.get('/career-guidance/bookmarks');
    return unwrapData(response, 'getBookmarkedPaths');
  },

  /**
   * Forecast salary based on target year, location and skill level sliders (Legacy and New)
   */
  forecastSalary: async (payload) => {
    // Determine which API to call based on the payload structure
    // New endpoint uses "EducationTrack" and "GraduationYear"
    if (payload.educationTrack || payload.graduationYear) {
      const response = await apiClient.post('/salary-intelligence/forecast', payload);
      return unwrapData(response, 'forecastSalary (V2)');
    } else {
      // Legacy endpoint
      const response = await apiClient.post('/career-guidance/forecast', payload);
      return unwrapData(response, 'forecastSalary (Legacy)');
    }
  },

  /**
   * Batch Compare careers (Salary Intelligence V2)
   */
  compareCareersSalary: async (payload) => {
    const response = await apiClient.post('/salary-intelligence/compare', payload);
    return unwrapData(response, 'compareCareersSalary');
  },

  /**
   * Retrieve aggregated metrics and trends charts data
   */
  getAnalytics: async () => {
    const response = await apiClient.get('/career-guidance/analytics');
    return unwrapData(response, 'getAnalytics');
  },

  /**
   * Compare two career paths side-by-side
   */
  compareCareers: async (id1, id2) => {
    const response = await apiClient.get('/career-guidance/compare', {
      params: { id1, id2 }
    });
    return unwrapData(response, 'compareCareers');
  }
};

export default careerGuidanceApi;
