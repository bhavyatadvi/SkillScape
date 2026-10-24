import React, { createContext, useContext, useReducer, useEffect, useCallback } from 'react';
import { authApi, setAuthToken, clearAuthData } from '@/services/api';

// Initial State
const initialState = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: true,
};

// Reducer
const authReducer = (state, action) => {
  switch (action.type) {
    case 'AUTH_START':
      return { ...state, isLoading: true };
    case 'AUTH_SUCCESS':
      return {
        ...state,
        user: action.payload.user,
        token: action.payload.token,
        isAuthenticated: true,
        isLoading: false,
      };
    case 'AUTH_FAILURE':
      return {
        ...state,
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
      };
    case 'LOGOUT':
      return {
        ...state,
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
      };
    case 'UPDATE_USER':
      return {
        ...state,
        user: state.user ? { ...state.user, ...action.payload } : null,
      };
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };
    default:
      return state;
  }
};

const AuthContext = createContext(undefined);

export const AuthProvider = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);

  useEffect(() => {
    const initAuth = async () => {
      const token = localStorage.getItem('token');
      const savedUser = localStorage.getItem('user');

      if (token && savedUser) {
        try {
          const user = JSON.parse(savedUser);
          setAuthToken(token);
          dispatch({ type: 'AUTH_SUCCESS', payload: { user, token } });
        } catch {
          clearAuthData();
          dispatch({ type: 'AUTH_FAILURE' });
        }
      } else {
        dispatch({ type: 'SET_LOADING', payload: false });
      }
    };

    initAuth();
  }, []);

  const login = useCallback(async (credentials) => {
    dispatch({ type: 'AUTH_START' });
    try {
      const data = await authApi.login(credentials);
      
      // Backend returns: { id, email, fullName, role, token, expiresAt }
      const user = {
        id: data.id,
        email: data.email,
        fullName: data.fullName,
        name: data.fullName, // Alias for compatibility
        role: data.role,
        profileCompleted: data.profileCompleted,
        expiresAt: data.expiresAt,
      };
      
      localStorage.setItem('token', data.token);
      localStorage.setItem('user', JSON.stringify(user));
      setAuthToken(data.token);

      dispatch({ type: 'AUTH_SUCCESS', payload: { user, token: data.token } });
      return data;
    } catch (error) {
      dispatch({ type: 'AUTH_FAILURE' });
      throw error;
    }
  }, []);

  const register = useCallback(async (credentials) => {
    dispatch({ type: 'AUTH_START' });
    try {
      const data = await authApi.register(credentials);
      
      // Backend returns: { id, email, fullName, role, token, expiresAt }
      const user = {
        id: data.id,
        email: data.email,
        fullName: data.fullName,
        name: data.fullName, // Alias for compatibility
        role: data.role,
        profileCompleted: data.profileCompleted,
        expiresAt: data.expiresAt,
      };
      
      localStorage.setItem('token', data.token);
      localStorage.setItem('user', JSON.stringify(user));
      setAuthToken(data.token);

      dispatch({ type: 'AUTH_SUCCESS', payload: { user, token: data.token } });
      return data;
    } catch (error) {
      dispatch({ type: 'AUTH_FAILURE' });
      throw error;
    }
  }, []);

  const logout = useCallback(() => {
    clearAuthData();
    dispatch({ type: 'LOGOUT' });
  }, []);

  const updateUser = useCallback((userData) => {
    if (state.user) {
      const updatedUser = { ...state.user, ...userData };
      localStorage.setItem('user', JSON.stringify(updatedUser));
      dispatch({ type: 'UPDATE_USER', payload: userData });
    }
  }, [state.user]);

  const hasRole = useCallback((roles) => {
    if (!state.user) return false;
    const roleArray = Array.isArray(roles) ? roles : [roles];
    return roleArray.includes(state.user.role);
  }, [state.user]);

  const value = {
    ...state,
    login,
    register,
    logout,
    updateUser,
    hasRole,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export default AuthContext;
