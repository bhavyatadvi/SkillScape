import { apiClient } from './axios';

// Roadmap generation may include web review, curriculum validation and AI generation.
// Keep the normal API timeout unchanged while allowing only roadmap operations enough time.
const ROADMAP_TIMEOUT = 120000;

export const roadmapApi = {
    // Get dynamically recommended roadmap based on ML quiz results
    getMyInterestRoadmap: () => apiClient.get('/roadmap/my-interest', { timeout: ROADMAP_TIMEOUT }),

    // Get all available roadmap domains the user can switch between
    getRoadmapOptions: () => apiClient.get('/roadmap/options', { timeout: ROADMAP_TIMEOUT }),

    getDismissedRoadmaps: () => apiClient.get('/roadmap/dismissed'),

    // Get roadmap for a selected domain
    getRoadmapByDomain: (domainId) => apiClient.get(`/roadmap/domain/${domainId}`, { timeout: ROADMAP_TIMEOUT }),

    // Mark a specific roadmap step as completed
    markStepComplete: (stepId) => apiClient.post(`/roadmap/mark-done/${stepId}`),

    // Fetch questions for a module quiz
    getModuleQuiz: (moduleId) => apiClient.get(`/roadmap/module/${moduleId}/quiz`),

    // Submit answers for a module quiz
    submitModuleQuiz: (moduleId, answers) => apiClient.post(`/roadmap/module/${moduleId}/quiz/submit`, { answers }),

    // Force fresh AI generation of a roadmap
    regenerateRoadmap: (careerKey) => apiClient.post(`/roadmap/regenerate/${careerKey}`, null, { timeout: ROADMAP_TIMEOUT }),

    // Fetch user's saved roadmaps ("My Roadmaps" collection)
    getMyRoadmaps: () => apiClient.get('/roadmap/my-roadmaps', { timeout: ROADMAP_TIMEOUT }),

    // Add a roadmap to "My Roadmaps"
    saveMyRoadmap: (careerKey, title) => apiClient.post(`/roadmap/my-roadmaps/${careerKey}${title ? `?title=${encodeURIComponent(title)}` : ''}`),

    // Accept and activate a recommended/custom career roadmap.
    acceptCareerRoadmap: (careerKey, careerTitle) => apiClient.post('/roadmap/accept', { careerKey, careerTitle }),

    // Remove a roadmap from "My Roadmaps"
    removeMyRoadmap: (careerKey) => apiClient.delete(`/roadmap/my-roadmaps/${careerKey}`),

    // Set default roadmap
    setDefaultMyRoadmap: (careerKey) => apiClient.patch(`/roadmap/my-roadmaps/${careerKey}/default`),

    // Freeform "Ask AI" roadmap builder
    askAIRoadmap: (goalText, situationText) => apiClient.post('/roadmap/ask-ai', { goalText, situationText }, { timeout: ROADMAP_TIMEOUT })
};
