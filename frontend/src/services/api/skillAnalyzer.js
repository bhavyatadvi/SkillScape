import apiClient from './axios';

/**
 * Career Readiness and Skill Gap Analyzer API service.
 */
export const skillAnalyzerApi = {
  /**
   * Get authenticated user's career profile status & completeness.
   */
  getProfileStatus: () =>
    apiClient.get('/skill-analyzer/profile-status'),

  /**
   * Get all career domains.
   */
  getDomains: () =>
    apiClient.get('/skill-analyzer/domains'),

  /**
   * Get sub-domains for a domain.
   * @param {string} [domainId]
   */
  getSubDomains: (domainId) =>
    apiClient.get('/skill-analyzer/subdomains', {
      params: domainId ? { domainId } : {},
    }),

  /**
   * Get careers for a given domain and/or subDomain.
   * @param {string} [domainId]
   * @param {string} [subDomainId]
   */
  getCareers: (domainId, subDomainId) =>
    apiClient.get('/skill-analyzer/careers', {
      params: {
        ...(domainId ? { domainId } : {}),
        ...(subDomainId ? { subDomainId } : {}),
      },
    }),

  /**
   * Search target roles by exact title, alternate names, or aliases.
   * @param {string} q
   * @param {string} [domainId]
   * @param {string} [subDomainId]
   */
  searchCareers: (q, domainId, subDomainId) =>
    apiClient.get('/skill-analyzer/careers/search', {
      params: {
        q,
        ...(domainId ? { domainId } : {}),
        ...(subDomainId ? { subDomainId } : {}),
      },
    }),

  /**
   * Get details for a specific target career.
   * @param {string} careerId
   */
  getCareerDetail: (careerId) =>
    apiClient.get(`/skill-analyzer/careers/${careerId}`),

  /**
   * Run full career readiness & skill gap analysis.
   * @param {Object} payload - { domainId, subDomainId, careerId, customCareerName, targetLevel, targetWorkType, notes }
   */
  analyze: (payload) =>
    apiClient.post('/skill-analyzer/analyze', payload),
};

export default skillAnalyzerApi;
