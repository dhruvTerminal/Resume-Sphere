import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5006';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 120000,
});

// REQUEST INTERCEPTOR: Attach JWT token to every request if it exists
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ═══════════════════════════════════════
//   AUTH
// ═══════════════════════════════════════

/**
 * POST /api/auth/login
 * Body: { email: string, password: string }
 * Response: { token: string, user: { id, fullName, email } }
 */
export const loginUser = async (credentials) => {
  return await api.post('/api/auth/login', credentials);
};

/**
 * POST /api/auth/register
 * Body: { fullName: string, email: string, password: string }
 * Response: { token: string, user: { id, fullName, email } }
 * NOTE: Backend expects "fullName", not "name". This function remaps it.
 */
export const registerUser = async (userData) => {
  return await api.post('/api/auth/register', {
    fullName: userData.name || userData.fullName,
    email: userData.email,
    password: userData.password,
  });
};

// ═══════════════════════════════════════
//   RESUME
// ═══════════════════════════════════════

/**
 * POST /api/resume/upload  [JWT Required]
 * Uploads PDF or DOCX file. FormData field name must be "file".
 * Response: { resumeId: Guid, userId: Guid, fileName: string, extractedSkills: string[] }
 */
export const uploadResume = async (file) => {
  const formData = new FormData();
  formData.append('file', file);
  return await api.post('/api/resume/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
};

/**
 * GET /api/resume  [JWT Required]
 * Returns all resumes uploaded by the authenticated user.
 * Response: [{ id, userId, fileName, fileType, uploadedAt }]
 */
export const getAllResumes = async () => {
  return await api.get('/api/resume');
};

/**
 * GET /api/resume/{id}/skills  [JWT Required]
 * Returns extracted skill names for a stored resume.
 * Response: string[]
 */
export const getResumeSkills = async (resumeId) => {
  return await api.get(`/api/resume/${resumeId}/skills`);
};

// ═══════════════════════════════════════
//   JOB MATCHING
// ═══════════════════════════════════════

/**
 * POST /api/jobs/match
 * Body: { resumeId: Guid, topN: number }
 * Response: [{
 *   jobId, title, company, description, requiredSkills[],
 *   matchedSkills[], missingSkills[{skillName, explanation, jobRelevance}],
 *   matchScore, matchExplanation
 * }]
 */
export const matchJobs = async (resumeId, topN = 10) => {
  return await api.post('/api/jobs/match', { resumeId, topN });
};

/**
 * GET /api/jobs
 * Returns all available jobs in the dataset.
 * Response: [{ id, title, company, description, requiredSkills[] }]
 */
export const getJobs = async () => {
  return await api.get('/api/jobs');
};

// ═══════════════════════════════════════
//   SKILL GAP
// ═══════════════════════════════════════

/**
 * GET /api/skill-gap?resumeId=Guid&jobId=int
 * Returns detailed skill gap analysis between a resume and a specific job.
 * Response: {
 *   resumeId, jobId, jobTitle, company,
 *   resumeSkills[], matchedSkills[],
 *   missingSkills[{skillName, explanation, jobRelevance}],
 *   matchScore
 * }
 */
export const getSkillGap = async (resumeId, jobId) => {
  return await api.get(`/api/skill-gap?resumeId=${resumeId}&jobId=${jobId}`);
};

// ═══════════════════════════════════════
//   RESOURCES / LEARNING
// ═══════════════════════════════════════

/**
 * GET /api/resources/{skill}
 * Returns curated learning resources for a given skill name.
 * Response: { skill, summary, youTubeLinks[{title, url}], articleLinks[{title, url}] }
 */
export const getResourcesForSkill = async (skillName) => {
  return await api.get(`/api/resources/${encodeURIComponent(skillName)}`);
};

/**
 * GET /api/resources/supported-skills
 * Returns list of skill names that have curated resources.
 * Response: string[]
 */
export const getSupportedSkills = async () => {
  return await api.get('/api/resources/supported-skills');
};

// ═══════════════════════════════════════
//   ANALYSIS & LEARNING PLANS
// ═══════════════════════════════════════

export const runAnalysis = async (payload) => {
  return await api.post('/api/analysis/run', payload);
};

export const getAnalysis = async (id) => {
  return await api.get(`/api/analysis/${id}`);
};

export const getAnalysisHistory = async () => {
  return await api.get('/api/analysis/history');
};

export const getLearningPlan = async (analysisId) => {
  return await api.get(`/api/learning-plan/${analysisId}`);
};

export const updateProgress = async (payload) => {
  return await api.post('/api/progress/update', payload);
};

export const generateTailoredResume = async (payload) => {
  return await api.post('/api/resume/generate-tailored', payload);
};

export const getGeneratedResume = async (id) => {
  return await api.get(`/api/resume/generated/${id}`);
};

// Export default api instance for direct use in other files
export default api;