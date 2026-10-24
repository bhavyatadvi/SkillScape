const CAREER_DISPLAY_NAMES = {
  'com.acc.prof': 'Accounting & Finance Professional',
  'com.acc': 'Accounting Professional',
  'com.acco': 'Accounting Professional',
  role_social_worker: 'Social Worker',
  role_community_development_officer: 'Community Development Officer',
  role_career_counsellor: 'Career Counsellor',
  role_journalist: 'Journalist',
  role_editor: 'Editor',
  role_social_researcher: 'Social Researcher',
  role_sociologist: 'Sociologist',
  role_public_policy_analyst: 'Public Policy Analyst',
  role_development_officer: 'Development Officer',
  role_programme_officer: 'Programme Officer',
  role_public_administration_officer: 'Public Administration Officer',
  role_curriculum_developer: 'Curriculum Developer',
  role_instructional_designer: 'Instructional Designer',
  role_translator: 'Translator',
  role_linguist: 'Linguist',
  role_clinical_psychologist: 'Clinical Psychologist',
  role_counselling_psychologist: 'Counselling Psychologist',
  role_mental_health_counsellor: 'Mental Health Counsellor',
  role_industrial_organizational_psychologist: 'Industrial-Organizational Psychologist',
  role_psychology_researcher: 'Psychology Researcher',
  role_people_analytics_specialist: 'People Analytics Specialist',
  role_psychometrist: 'Psychometrist',
  role_content_strategist: 'Content Strategist',
  role_social_media_manager: 'Social Media Manager',
  role_economic_analyst: 'Economic Analyst',
  role_archivist: 'Archivist',
  role_museum_educator: 'Museum Educator',
  role_ethics_researcher: 'Ethics Researcher',
  role_social_research_analyst: 'Social Research Analyst',
  role_geospatial_analyst: 'Geospatial Analyst',
  role_urban_planner: 'Urban Planner',
};

export function getCareerDisplayName(value) {
  const raw = String(value || '').trim();
  if (!raw) return '';
  const key = raw.toLowerCase();
  if (CAREER_DISPLAY_NAMES[key]) return CAREER_DISPLAY_NAMES[key];
  if (key.startsWith('role_')) {
    return key.slice(5).split(/[_-]+/).filter(Boolean)
      .map(word => word[0].toUpperCase() + word.slice(1)).join(' ');
  }
  return raw;
}

export function formatCareerReferences(value) {
  const raw = String(value || '');
  return raw.replace(/\brole_[a-z0-9_]+\b/gi, roleId => getCareerDisplayName(roleId));
}

export default CAREER_DISPLAY_NAMES;
