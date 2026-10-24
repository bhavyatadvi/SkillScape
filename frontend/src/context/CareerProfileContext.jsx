import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { useAuth } from './AuthContext';
import { careerProfileApi } from '@/services/api/careerProfile';
import { clearUserAnalysisStorage } from '@/lib/userStorage';

const CareerProfileContext = createContext(undefined);

const emptyProfile = {
  educationLevel: '', degree: '', broadField: '', specialization: '',
  institution: '', graduationYear: null, additionalQualifications: '', certifications: [],
  educationStatus: '', currentEmploymentStatus: '', currentOccupation: '', currentRole: '',
  currentIndustry: '', organizationType: '', experienceYears: null, relevantExperience: '',
  previousRoles: '', previousIndustries: '', currentSkills: [], careerSatisfaction: '',
  careerLikes: [], careerDislikes: [], workPreferences: { environment: [], style: [] },
  motivations: [], constraints: [], careerChangePreference: '', location: '',
  preferredWorkMode: '', salaryRange: '', careerGoals: '',
  profileSource: 'manual', resumeFileName: '',
};

// The API uses both ASP.NET's PascalCase `Data` and camelCase `data` response
// envelopes. Preserve an explicit null payload so "no profile" is not mistaken
// for the envelope object itself.
const unwrapProfileResponse = (response) => {
  const body = response?.data;
  if (body && Object.prototype.hasOwnProperty.call(body, 'data')) return body.data;
  if (body && Object.prototype.hasOwnProperty.call(body, 'Data')) return body.Data;
  return body;
};

export const CareerProfileProvider = ({ children }) => {
  const { user } = useAuth();

  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [profileLoadedForUserId, setProfileLoadedForUserId] = useState(null);

  const hasProfile = !!profile;

  // Fetch profile from backend whenever the user changes
  const refreshProfile = useCallback(async () => {
    if (!user) {
      setProfile(null);
      setProfileLoadedForUserId(null);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const res = await careerProfileApi.getProfile();
      const data = unwrapProfileResponse(res);
      setProfile(data ?? null);
    } catch (err) {
      if (err.status === 404 || err.response?.status === 404) {
        setProfile(null); // No profile yet — not an error condition
      } else {
        setError(err.message || 'Failed to load career profile');
        setProfile(null);
      }
    } finally {
      setProfileLoadedForUserId(user.id);
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    refreshProfile();
  }, [refreshProfile]);

  // Clear profile when user logs out
  useEffect(() => {
    if (!user) {
      setProfile(null);
      setError(null);
      setProfileLoadedForUserId(null);
    }
  }, [user]);

  const saveProfile = async (formData) => {
    setLoading(true);
    setError(null);
    try {
      if (hasProfile) {
        const res = await careerProfileApi.updateProfile(formData);
        const updated = unwrapProfileResponse(res);
        setProfile(updated);
        return updated;
      } else {
        const res = await careerProfileApi.createProfile(formData);
        const created = unwrapProfileResponse(res);
        setProfile(created);
        return created;
      }
    } catch (err) {
      setError(err.message || 'Failed to save career profile');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const updateProfile = async (formData) => {
    return saveProfile(formData);
  };

  const clearProfile = async () => {
    setLoading(true);
    try {
      await careerProfileApi.deleteProfile();
      clearUserAnalysisStorage(user);
      setProfile(null);
    } catch (err) {
      setError(err.message || 'Failed to delete career profile');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return (
    <CareerProfileContext.Provider value={{
      // New backend-driven API
      profile,
      loading,
      error,
      profileLoadedForUserId,
      hasProfile,
      saveProfile,
      updateProfile,
      refreshProfile,
      clearProfile,
      // Backward compat shim — some old pages read careerProfile from context
      careerProfile: profile ?? emptyProfile,
    }}>
      {children}
    </CareerProfileContext.Provider>
  );
};

export const useCareerProfile = () => {
  const context = useContext(CareerProfileContext);
  if (!context) {
    throw new Error('useCareerProfile must be used within a CareerProfileProvider');
  }
  return context;
};
