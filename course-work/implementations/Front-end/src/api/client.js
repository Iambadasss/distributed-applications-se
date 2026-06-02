import axios from 'axios';

export const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('pft_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export function getErrorMessage(error) {
  const data = error?.response?.data;
  const translations = {
    'Budget already exists': 'Вече има бюджет за тази категория, месец и година.'
  };

  if (data?.errors) {
    return Object.values(data.errors).flat().join(' ');
  }

  if (data?.detail) {
    return data.detail;
  }

  if (data?.title) {
    return translations[data.title] || data.title;
  }

  return 'Възникна грешка при заявката.';
}

export function cleanParams(params) {
  return Object.fromEntries(
    Object.entries(params).filter(([, value]) => value !== '' && value !== null && value !== undefined)
  );
}
