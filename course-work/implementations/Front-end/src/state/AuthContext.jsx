import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { api } from '../api/client';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem('pft_token'));
  const [user, setUser] = useState(() => {
    const saved = localStorage.getItem('pft_user');
    return saved ? JSON.parse(saved) : null;
  });
  const [loading, setLoading] = useState(Boolean(token));

  useEffect(() => {
    if (!token) {
      setLoading(false);
      return;
    }

    api.get('/auth/me')
      .then((response) => {
        setUser(response.data);
        localStorage.setItem('pft_user', JSON.stringify(response.data));
      })
      .catch(() => {
        localStorage.removeItem('pft_token');
        localStorage.removeItem('pft_user');
        setToken(null);
        setUser(null);
      })
      .finally(() => setLoading(false));
  }, [token]);

  function saveSession(authResponse) {
    localStorage.setItem('pft_token', authResponse.accessToken);
    localStorage.setItem('pft_user', JSON.stringify(authResponse.user));
    setToken(authResponse.accessToken);
    setUser(authResponse.user);
  }

  async function login(payload) {
    const response = await api.post('/auth/login', payload);
    saveSession(response.data);
  }

  async function register(payload) {
    const response = await api.post('/auth/register', payload);
    saveSession(response.data);
  }

  async function refreshUser() {
    const response = await api.get('/auth/me');
    setUser(response.data);
    localStorage.setItem('pft_user', JSON.stringify(response.data));
  }

  function logout() {
    localStorage.removeItem('pft_token');
    localStorage.removeItem('pft_user');
    setToken(null);
    setUser(null);
  }

  const value = useMemo(() => ({
    token,
    user,
    loading,
    isAuthenticated: Boolean(token),
    login,
    register,
    refreshUser,
    logout
  }), [token, user, loading]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider.');
  }

  return context;
}
